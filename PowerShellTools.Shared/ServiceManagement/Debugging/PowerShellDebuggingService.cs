using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Host;
using System.Management.Automation.Remoting;
using System.Management.Automation.Runspaces;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Microsoft.PowerShell;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PowerShellTools.Common;
using PowerShellTools.Common.Debugging;
using PowerShellTools.Common.Logging;
using PowerShellTools.Common.ServiceManagement.DebuggingContract;
using PowerShellToolsPro;

namespace PowerShellTools.HostService.ServiceManagement.Debugging
{
    public class PowerShellDebuggingService : PSHost, IHostSupportsInteractiveSession, IPowerShellDebuggingService
    {
        private PowerShell _currentPowerShell;
        private IEnumerable<Variable> _varaiables;
        private IEnumerable<PSObject> _callstack;
        private Collection<PSVariable> _localVariables;
        private Dictionary<string, object> _propVariables;
        private Dictionary<string, string> _mapLocalToRemote;
        private Dictionary<string, string> _mapRemoteToLocal;
        private HashSet<PowerShellBreakpointRecord> _psBreakpointTable;
        private readonly AutoResetEvent _attachRequestEvent = new AutoResetEvent(false);
        private Version _installedPowerShellVersion;
        private PowerShellDebuggingServiceAttachUtilities _attachUtilities;
        private bool _useSSL;
        private int _currentPid;
        private BlockingCollection<DebuggingCommandRequest> _debuggingCommandRequests;
        private Dictionary<string, List<Variable>> _variableCache;
        private static readonly ILog Log = LogManager.GetLogger(typeof(PowerShellDebuggingService));

        /// <summary>
        /// Minimal powershell version required for remote session debugging
        /// </summary>
        private static readonly Version RequiredPowerShellVersionForRemoteSessionDebugging = new Version(4, 0);

        /// <summary>
        /// Minimal powershell version required for process attach debugging
        /// </summary>
        private static readonly Version RequiredPowerShellVersionForProcessAttach = new Version(5, 0);

        /// <summary>
        /// Whether or not we need to copy the script associated with a remote process. This is set to true before in AttachToRemoteRunspace
        /// and false inside of Debugger_DebuggerStop. Needed so if code is changed on a remote machine while we are debugging, we don't overwrite
        /// our local copy and cause VS to reload the script which breaks the debugging experience.
        /// </summary>
        private static bool _needToCopyRemoteScript = false;

        /// <summary>
        /// Marks whether or not we decided to/were forced to use _currentPowerShell.stop() in order to detach while debugging.
        /// </summary>
        private bool _forceStop;

        /// <summary>
        /// User credentials that we save after enumerating remote processes.
        /// </summary>
        private PSCredential _savedCredential;
        private int _targetProcessId;

        public PowerShellDebuggingService(IDebugEngineCallback callback, int processId, bool sta = false)
        {
            ServiceCommon.Log("Initializing debugging engine service ...");
            HostUi = new HostUi(this);
            _localVariables = new Collection<PSVariable>();
            _propVariables = new Dictionary<string, object>();
            _mapLocalToRemote = new Dictionary<string, string>();
            _mapRemoteToLocal = new Dictionary<string, string>();
            _psBreakpointTable = new HashSet<PowerShellBreakpointRecord>();
            _installedPowerShellVersion = DependencyUtilities.GetInstalledPowerShellVersion();
            _attachUtilities = new PowerShellDebuggingServiceAttachUtilities(this);
            _forceStop = false;
            _currentPid = Process.GetCurrentProcess().Id;
            CallbackService = callback;
            _targetProcessId = processId;
            InitializeRunspace(processId, this, sta);
        }

        /// <summary>
        /// The runspace used by the current PowerShell host.
        /// </summary>
        public static Runspace Runspace { get; set; }

        public event EventHandler ExecuteFinished;
        public event EventHandler DebuggerStopped;

        /// <summary>
        /// Call back service used to talk to VS side
        /// </summary>
        public IDebugEngineCallback CallbackService { get; set; }

        /// <summary>
        /// PowerShell raw host UI options
        /// </summary>
        public PowerShellRawHostOptions RawHostOptions { get; set; } = new PowerShellRawHostOptions();

        #region Debugging service calls

        /// <summary>
        /// Initialization of the PowerShell runspace
        /// </summary>
        public void SetRunspace(bool overrideExecutionPolicy)
        {
            if (overrideExecutionPolicy)
            {
                SetupExecutionPolicy();
            }

            SetRunspace(Runspace);
        }

        private DateTime cacheDate = DateTime.MinValue;
        private List<int> validPids = new List<int>();

        /// <summary>
        /// Examines a process' modules for powershell.exe which indicates the process should be attachable.
        /// </summary>
        /// <param name="pid">Process id of the process to examine</param>
        /// <returns>True if powershell.exe is a module of the process, false otherwise.</returns>
        public bool IsAttachable(uint pid)
        {
            // do not let users attach to PowerShell Tools directly
            if (pid == _currentPid)
            {
                return false;
            }

            // make sure we are in a local scenario and that an adequate version of PowerShell is installed
            if (GetDebugScenario() == DebugScenario.Local && _installedPowerShellVersion >= RequiredPowerShellVersionForProcessAttach)
            {
                if (DateTime.UtcNow - cacheDate > TimeSpan.FromMinutes(1))
                {
                    using (_currentPowerShell = PowerShell.Create())
                    {
                        validPids.Clear();
                        foreach (var item in InvokeScript(_currentPowerShell, "Get-PSHostProcessInfo | Select-Object -ExpandProperty ProcessId"))
                        {
                            validPids.Add((int)item.BaseObject);
                        }
                        cacheDate = DateTime.UtcNow;
                    }
                }

                // see if the process is a PS host
                if (validPids.Any(m => m == (int)pid))
                {
                    var process = Process.GetProcessById((int)pid);
                    if (process != null)
                    {
                        ServiceCommon.Log(string.Format("IsAttachable: {1}; id: {1}", process.ProcessName, process.Id));
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Attaches the HostService to a local runspace already in execution
        /// </summary>
        public string AttachToRunspace(uint pid)
        {
            string result = string.Empty;

            // scenario before entering, used to determine if we are local attaching
            DebugScenario preScenario = GetDebugScenario();

            // attaching leverages cmdlets introduced in PSv5, this must be on local machine to attach to a local process
            if (preScenario == DebugScenario.Local && _installedPowerShellVersion < RequiredPowerShellVersionForProcessAttach)
            {
                ServiceCommon.Log(string.Format("User asked to attach to process while running inadequete PowerShell version {0}", _installedPowerShellVersion.ToString()));
                return string.Format("Unable to attach with current PowerShell version ({0}). Please ensure that PowerShell v5 or higher is installed.", _installedPowerShellVersion.ToString());
            }

            try
            {
                using (_currentPowerShell = PowerShell.Create())
                {
                    // enter into to-attach process which will swap out the current runspace
                    _attachRequestEvent.Reset();
                    InvokeScript(_currentPowerShell, string.Format("Enter-PSHostProcess -Id {0}", pid.ToString()));
                    result = _attachUtilities.VerifyAttachToRunspace(preScenario, _attachRequestEvent);

                    if (!string.IsNullOrEmpty(result))
                    {
                        return result;
                    }

                    // rehook event handlers and reset _pausedEvent and _forceStop
                    AddEventHandlers();
                    _debuggingCommandRequests = new BlockingCollection<DebuggingCommandRequest>();
                    _forceStop = false;

                    // debug the runspace, for the vast majority of cases the 1st runspace is the one to attach to
                    InvokeScript(_currentPowerShell, "Debug-Runspace -Id 1");

                    if (_currentPowerShell.HadErrors)
                    {
                        return "Error while attempting to debug the specified process.";
                    }
                }
            }
            catch (RemoteException remoteException)
            {
                if (_forceStop)
                {
                    // exception is expected if user asks to stop debugging while script is running, no need to notify
                    ServiceCommon.Log(string.Format("Forced to detach via stop command; {0}", remoteException.ToString()));
                }
                else
                {
                    // some actions, such as closing a remote process mid debugging may cause an unexpected remote exception
                    ServiceCommon.Log(string.Format("Unexpected remote exception while debugging runspace; {0}", remoteException.ToString()));
                    return "Error while attempting to debug the specified process.";
                }
            }
            catch (PSRemotingDataStructureException remotingDataStructureException)
            {
                if (_forceStop)
                {
                    // exception is expected if we have to stop during cleanup
                    ServiceCommon.Log(string.Format("Forced to detach via stop command; {0}", remotingDataStructureException.ToString()));
                }
                else
                {
                    // some actions, such as closing a remote process mid debugging may cause an unexpected remote exception
                    ServiceCommon.Log(string.Format("Unexpected remote exception while debugging runspace; {0}", remotingDataStructureException.ToString()));
                    return "Error while attempting to debug the specified process.";
                }
            }
            catch (Exception exception)
            {
                // any other sort of exception is not expected
                ServiceCommon.Log(string.Format("Unexpected exception while debugging runspace; {0}", exception.ToString()));
                return "Error while attempting to debug the specified process.";
            }
            return result;
        }

        /// <summary>
        /// Detaches the HostService from a local runspace
        /// </summary>
        public bool DetachFromRunspace()
        {
            try
            {
                // attempt to gracefully detach the deugger
                ClearBreakpoints();
                if (Runspace.RunspaceAvailability != RunspaceAvailability.Busy)
                {
                    ExecuteDebuggingCommand("detach", false);
                }
                else
                {
                    _forceStop = true;
                    _currentPowerShell.Stop();
                }
            }
            catch (Exception ex)
            {
                // if program is running/a problem is encountered, we must use stop to force the debugger to detach
                ServiceCommon.Log(string.Format("Script currently in execution, must use stop to end debugger; {0}", ex.ToString()));
                _forceStop = true;
                _currentPowerShell.Stop();
            }

            // scenario before exiting, used to determine if we are local detaching
            DebugScenario preScenario = GetDebugScenario();

            using (_currentPowerShell = PowerShell.Create())
            {
                _attachRequestEvent.Reset();
                InvokeScript(_currentPowerShell, "Exit-PSHostProcess");

                if (!_attachUtilities.VerifyDetachFromRunspace(preScenario, _attachRequestEvent))
                {
                    return false;
                }
                else
                {
                    // rehook event handlers and make sure _pausedEvent is woken up
                    AddEventHandlers();
                    _debuggingCommandRequests = new BlockingCollection<DebuggingCommandRequest>();
                    return true;
                }
            }
        }

        /// <summary>
        /// Finds all processes on a remote computer which have loaded the powershell.exe module
        /// </summary>
        /// <param name="remoteName">Name of the remote machine</param>
        /// <param name="errorMessage">Error message to be presented to user if failure occurs</param>
        /// <returns>List of valid processes, each represented by a KeyValuePair of pid to process name</returns>
        public List<KeyValuePair<uint, string>> EnumerateRemoteProcesses(string remoteName, ref string errorMessage, bool useSSL)
        {
            _useSSL = useSSL;
            List<KeyValuePair<uint, string>> validProcesses = new List<KeyValuePair<uint, string>>();

            try
            {
                using (_currentPowerShell = PowerShell.Create())
                {
                    Tuple<string, int> parts = _attachUtilities.GetNameAndPort(remoteName);
                    remoteName = parts.Item1;
                    int port = parts.Item2;

                    if (_attachUtilities.RemoteIsLoopback(remoteName))
                    {
                        // user gave a loopback address which we do not allow
                        ServiceCommon.Log("User entered a loopback address as their qualifier.");
                        errorMessage = "Cannot connect to the localhost with remote debugging. Use the default transport to attach to local processes.";
                        return null;
                    }

                    // Grab user credentials and initiate remote session with the remote machine
                    PSObject psobj = InvokeScript(_currentPowerShell, DebugEngineConstants.GetCredentialsCommand).FirstOrDefault();
                    if (psobj != null)
                    {
                        _savedCredential = psobj.BaseObject as PSCredential;
                    }
                    else
                    {
                        // user hit cancel
                        errorMessage = string.Empty;
                        return null;
                    }
                    EnterCredentialedRemoteSession(_currentPowerShell, remoteName, port, _useSSL);

                    if (GetDebugScenario() == DebugScenario.Local)
                    {
                        // bad credentials or couldn't connect to machine
                        ServiceCommon.Log("User entered wrong credentials, or we could not reach the remote machine.");
                        errorMessage = string.Format("Unable to connect to {0}. Retry?", remoteName);
                        return null;
                    }

                    // Check remote PowerShell version
                    Version remoteVersion = InvokeScript(_currentPowerShell, "$PSVersionTable.PSVersion").ElementAt(0).BaseObject as Version;
                    if (remoteVersion != null && (remoteVersion < RequiredPowerShellVersionForProcessAttach))
                    {
                        InvokeScript(_currentPowerShell, string.Format(DebugEngineConstants.ExitRemoteSessionDefaultCommand));
                        errorMessage = string.Format("Unable to attach with current remote PowerShell version ({0}). Please ensure that PowerShell v5 or higher is installed.", remoteVersion.ToString());
                        return null;
                    }

                    // grab all attachable processes and add each process' name and pid to the list to be returned
                    foreach (PSObject obj in InvokeScript(_currentPowerShell, "Get-PSHostProcessInfo"))
                    {
                        uint pid = (uint)((int)obj.Members["ProcessId"].Value);
                        string name = (string)obj.Members["ProcessName"].Value;
                        validProcesses.Add(new KeyValuePair<uint, string>(pid, name));
                    }

                    // Exit the remote session and return results back to RemoteEnumDebugProcess
                    InvokeScript(_currentPowerShell, string.Format(DebugEngineConstants.ExitRemoteSessionDefaultCommand));
                }
            }
            catch (Exception ex)
            {
                ServiceCommon.Log(string.Format("Error connecting to remote machine; {0}", ex.ToString()));
                errorMessage = string.Format("Unable to connect to {0}. Retry?", remoteName);
                return null;
            }
            return validProcesses;
        }

        /// <summary>
        /// Attaches the HostService to a remote runspace already in execution
        /// </summary>
        public string AttachToRemoteRunspace(uint pid, string remoteName)
        {
            try
            {
                using (_currentPowerShell = PowerShell.Create())
                {
                    // enter into a remote session
                    Tuple<string, int> parts = _attachUtilities.GetNameAndPort(remoteName);
                    remoteName = parts.Item1;
                    int port = parts.Item2;

                    EnterCredentialedRemoteSession(_currentPowerShell, remoteName, port, _useSSL);

                    if (!_attachUtilities.VerifyAttachToRemoteRunspace())
                    {
                        // bad credentials, couldn't connect to machine, user hit cancel on the auth dialog
                        ServiceCommon.Log("Unable to connect to remote machine.");
                        return string.Format("Unable to connect to {0}.", remoteName);
                    }

                    _needToCopyRemoteScript = true;
                }

            }
            catch (Exception ex)
            {
                ServiceCommon.Log(string.Format("Error connecting to remote machine; {0}", ex.ToString()));
                return string.Format("Unable to connect to {0}.", remoteName);
            }

            // now that we are in the remote session we can attach to the runspace
            return AttachToRunspace(pid);
        }

        /// <summary>
        /// Detaches the HostService from a remote runspace
        /// </summary>
        public bool DetachFromRemoteRunspace()
        {
            // detach from the runspace
            if (!DetachFromRunspace())
            {
                return false;
            }

            using (_currentPowerShell = PowerShell.Create())
            {
                // exit the remote session and delete saved credentials
                InvokeScript(_currentPowerShell, DebugEngineConstants.ExitRemoteSessionDefaultCommand);
                _savedCredential = null;

                if (!_attachUtilities.VerifyDetachFromRemoteRunspace())
                {
                    // very unlikely for this to happen, but we should make sure to handle the case anyway
                    ServiceCommon.Log("Unable to disconnect from the remote machine.");
                    return false;
                }
            }

            // rehook event handlers
            AddEventHandlers();
            return true;
        }

        /// <summary>
        /// Cleans up the host service from any given point in either remote or local attach scenarios.
        /// Used after any sort of error state is detected.
        /// </summary>
        /// <returns>Scenario after attempting to cleanup the environement.</returns>
        public DebugScenario CleanupAttach()
        {
            DebugScenario scenario = GetDebugScenario();
            _forceStop = true;
            try
            {
                switch (scenario)
                {
                    case DebugScenario.RemoteAttach:
                        // 1. detach the debugger, 2. exit the process, 3. exit the session
                        _currentPowerShell.Stop();
                        using (_currentPowerShell = PowerShell.Create())
                        {
                            InvokeScript(_currentPowerShell, "Exit-PSHostProcess");
                            InvokeScript(_currentPowerShell, string.Format(DebugEngineConstants.ExitRemoteSessionDefaultCommand));
                        }
                        break;
                    case DebugScenario.RemoteSession:
                        // 1. exit the process, 2. exit the session
                        using (_currentPowerShell = PowerShell.Create())
                        {
                            InvokeScript(_currentPowerShell, "Exit-PSHostProcess");
                            InvokeScript(_currentPowerShell, string.Format(DebugEngineConstants.ExitRemoteSessionDefaultCommand));
                        }
                        break;
                    case DebugScenario.LocalAttach:
                        // 1. detach the debugger, 2. exit the process
                        _currentPowerShell.Stop();
                        using (_currentPowerShell = PowerShell.Create())
                        {
                            InvokeScript(_currentPowerShell, "Exit-PSHostProcess");
                        }
                        break;
                }
            }
            catch (Exception e)
            {
                ServiceCommon.Log(string.Format("CleanupAttach exception while in {0}; {1}", scenario.ToString(), e.ToString()));
            }

            _savedCredential = null;
            AddEventHandlers();
            _debuggingCommandRequests = new BlockingCollection<DebuggingCommandRequest>();

            return GetDebugScenario();
        }

        /// <summary>
        /// Client respond with resume action to service
        /// </summary>
        /// <param name="action">Resumeaction from client</param>
        /// <returns>Output from debugging command</returns>
        public string ExecuteDebuggingCommandOutDefault(string debuggingCommand)
        {
            return ExecuteDebuggingCommand(debuggingCommand, true); // also print output to user
        }

        /// <summary>
        /// Client respond with resume action to service
        /// </summary>
        /// <param name="action">Resumeaction from client</param>
        /// <returns>Output from debugging command</returns>
        public string ExecuteDebuggingCommandOutNull(string debuggingCommand)
        {
            return ExecuteDebuggingCommand(debuggingCommand, false); // dont need print output to user
        }

        /// <summary>
        /// Sets breakpoint for the current runspace.
        /// </summary>
        /// <param name="bp">Breakpoint to set</param>
        public void SetBreakpoint(PowerShellBreakpoint bp)
        {
            IEnumerable<PSObject> breakpoints;

            ServiceCommon.Log("Setting breakpoint ...");
            try
            {
                if (Runspace.RunspaceAvailability == RunspaceAvailability.Available)
                {
                    using (var powershell = PowerShell.Create())
                    {
                        powershell.Runspace = Runspace;
                        string file = bp.ScriptFullPath;
                        if (GetDebugScenario() != DebugScenario.Local && _mapLocalToRemote.ContainsKey(bp.ScriptFullPath))
                        {
                            file = _mapLocalToRemote[bp.ScriptFullPath];
                        }

                        powershell.AddCommand("Set-PSBreakpoint").AddParameter("Line", bp.Line).AddParameter("Script", file);

                        breakpoints = powershell.Invoke();
                    }

                    var pobj = breakpoints.FirstOrDefault();
                    if (pobj != null)
                    {
                        _psBreakpointTable.Add(
                            new PowerShellBreakpointRecord(
                                bp,
                                ((LineBreakpoint)pobj.BaseObject).Id));
                    }
                }
                else
                {
                    ServiceCommon.Log("Setting breakpoint failed due to busy runspace.");
                }
            }
            catch (InvalidOperationException)
            {
                ServiceCommon.Log("Invalid breakpoint location!");
            }
        }

        /// <summary>
        /// Remove breakpoint for the current runspace.
        /// </summary>
        /// <param name="bp">Breakpoint to set</param>
        public void RemoveBreakpoint(PowerShellBreakpoint bp)
        {
            int id = GetPSBreakpointId(bp);

            if (id >= 0)
            {
                ServiceCommon.Log("Removing breakpoint ...");

                if (Runspace.RunspaceAvailability == RunspaceAvailability.Available)
                {
                    using (var powershell = PowerShell.Create())
                    {
                        string file = bp.ScriptFullPath;
                        if (GetDebugScenario() != DebugScenario.Local && _mapLocalToRemote.ContainsKey(bp.ScriptFullPath))
                        {
                            file = _mapLocalToRemote[bp.ScriptFullPath];
                        }

                        powershell.AddCommand("Remove-PSBreakpoint").AddParameter("Id", id);
                        powershell.Invoke();
                    }

                    var itemsToRemove = _psBreakpointTable.Where(b => b.PSBreakpoint.Equals(bp)).ToList();

                    foreach (var p in itemsToRemove)
                    {
                        _psBreakpointTable.Remove(p);
                    }
                }
                else
                {
                    ServiceCommon.Log("Removing breakpoint failed due to busy runspace.");
                }
            }
        }

        /// <summary>
        /// Alternative removal of breakpoint for current runspace, does so by executing a debugging command.
        /// </summary>
        /// <param name="id"></param>
        public void RemoveBreakpointById(int id)
        {
            ExecuteDebuggingCommandOutNull(string.Format(DebugEngineConstants.RemovePSBreakpoint, id));
            _psBreakpointTable.RemoveWhere(bp => bp.Id == id);
        }

        /// <summary>
        /// Enable/Disable breakpoint for the current runspace.
        /// </summary>
        /// <param name="bp">Breakpoint to set</param>
        public void EnableBreakpoint(PowerShellBreakpoint bp, bool enable)
        {
            int id = GetPSBreakpointId(bp);

            if (id >= 0)
            {
                ServiceCommon.Log(string.Format("{0} breakpoint ...", enable ? "Enabling" : "Disabling"));

                if (Runspace.RunspaceAvailability == RunspaceAvailability.Available)
                {
                    using (var powershell = PowerShell.Create())
                    {
                        powershell.Runspace = Runspace;
                        string cmd = enable ? "Enable-PSBreakpoint" : "Disable-PSBreakpoint";

                        string file = bp.ScriptFullPath;
                        if (GetDebugScenario() != DebugScenario.Local && _mapLocalToRemote.ContainsKey(bp.ScriptFullPath))
                        {
                            file = _mapLocalToRemote[bp.ScriptFullPath];
                        }

                        powershell.AddCommand(cmd).AddParameter("Id", id);
                        powershell.Invoke();
                    }
                }
                else
                {
                    ServiceCommon.Log(string.Format("{0} breakpoint failed due to busy runspace.", enable ? "Enabling" : "Disabling"));
                }
            }
            else
            {
                ServiceCommon.Log("Can not locate the breakpoint!");
            }
        }

        /// <summary>
        /// Clears existing breakpoints for the current runspace.
        /// </summary>
        public void ClearBreakpoints()
        {
            ServiceCommon.Log("Clearing all breakpoints");

            try
            {
                if (Runspace.RunspaceAvailability == RunspaceAvailability.Available)
                {
                    IEnumerable<PSObject> breakpoints;

                    using (var powershell = PowerShell.Create())
                    {
                        powershell.Runspace = Runspace;
                        powershell.AddCommand("Get-PSBreakpoint");
                        breakpoints = powershell.Invoke();
                    }

                    if (!breakpoints.Any()) return;

                    using (var powershell = PowerShell.Create())
                    {
                        powershell.Runspace = Runspace;
                        powershell.AddCommand("Remove-PSBreakpoint").AddParameter("Breakpoint", breakpoints);
                        powershell.Invoke();
                    }
                }
                else if (IsDebuggerActive(Runspace.Debugger))
                {
                    foreach (PowerShellBreakpointRecord bp in _psBreakpointTable)
                    {
                        ExecuteDebuggingCommandOutNull(string.Format(DebugEngineConstants.RemovePSBreakpoint, bp.Id));
                    }

                    _psBreakpointTable.Clear();
                }
                else
                {
                    ServiceCommon.Log("Clearing all breakpoints failed due to busy runspace.");
                }
            }
            catch (Exception ex)
            {
                ServiceCommon.Log(string.Format("ClearBreakpoints exception: {0}", ex.ToString()));
            }
        }

        /// <summary>
        /// Get powershell breakpoint Id
        /// </summary>
        /// <param name="bp">Powershell breakpoint</param>
        /// <returns>Id of breakpoint if found, otherwise -1</returns>
        public int GetPSBreakpointId(PowerShellBreakpoint bp)
        {
            ServiceCommon.Log("Getting PSBreakpoint ...");
            var bpr = _psBreakpointTable.FirstOrDefault(b => b.PSBreakpoint.Equals(bp));

            return bpr != null ? bpr.Id : -1;
        }

        /// <summary>
        /// Get runspace availability
        /// </summary>
        /// <returns>runspace availability enum</returns>
        public RunspaceAvailability GetRunspaceAvailability()
        {
            RunspaceAvailability state = Runspace.RunspaceAvailability;
            ServiceCommon.Log("Checking runspace availability: " + state.ToString());

            return state;
        }

        public Collection<T> Execute<T>(string commandLine)
        {
            ServiceCommon.Log("Start executing ps script ...");

            Collection<T> result = default(Collection<T>);
            var commandExecuted = false;

            try
            {
                _debuggingCommandRequests = new BlockingCollection<DebuggingCommandRequest>();

                lock(ServiceCommon.RunspaceLock)
                {
                    if (Runspace.RunspaceAvailability == RunspaceAvailability.Available)
                    {
                        commandExecuted = true;

                        using (_currentPowerShell = PowerShell.Create())
                        {
                            _currentPowerShell.Runspace = Runspace;
                            _currentPowerShell.AddScript(commandLine);

                            _currentPowerShell.AddCommand("out-default");
                            _currentPowerShell.Commands.Commands[0].MergeMyResults(PipelineResultTypes.Error, PipelineResultTypes.Output);

                            AppRunning = true;

                            result = _currentPowerShell.Invoke<T>();

                            AppRunning = false;
                        }
                    }
                    else
                    {
                        ServiceCommon.Log("Execution skipped due to busy runspace.");
                    }
                }

                return result;
            }
            catch (TypeLoadException ex)
            {
                ServiceCommon.Log("Type,  Exception: {0}", ex.Message);
                OnTerminatingException(ex);

                return result;
            }
            catch (Exception ex)
            {
                ServiceCommon.Log("Terminating error,  Exception: {0}", ex.Message);
                OnTerminatingException(ex);

                return result;
            }
            finally
            {
                if (commandExecuted)
                {
                    DebuggerFinished();
                }

            }
        }

        /// <summary>
        /// Execute the specified command line from client
        /// </summary>
        /// <param name="commandLine">Command line to execute</param>
        public bool Execute(string commandLine)
        {
            ServiceCommon.Log("Start executing ps script ...");

            bool commandExecuted = false;

            try
            {
                _debuggingCommandRequests = new BlockingCollection<DebuggingCommandRequest>();

                bool error = false;
                lock(ServiceCommon.RunspaceLock)
                {
                    if (Runspace.RunspaceAvailability == RunspaceAvailability.Available)
                    {
                        commandExecuted = true;

                        using (_currentPowerShell = PowerShell.Create())
                        {
                            _currentPowerShell.Runspace = Runspace;
                            _currentPowerShell.AddScript(commandLine);

                            _currentPowerShell.AddCommand("out-default");
                            _currentPowerShell.Commands.Commands[0].MergeMyResults(PipelineResultTypes.Error, PipelineResultTypes.Output);

                            AppRunning = true;

                            _currentPowerShell.Invoke();

                            error = _currentPowerShell.HadErrors;
                        }
                    }
                    else
                    {
                        ServiceCommon.Log("Execution skipped due to busy runspace.");
                    }
                }

                return !error;
            }
            catch (TypeLoadException ex)
            {
                ServiceCommon.Log("Type,  Exception: {0}", ex.Message);
                OnTerminatingException(ex);

                return false;
            }
            catch (Exception ex)
            {
                ServiceCommon.Log("Terminating error,  Exception: {0}", ex.Message);
                OnTerminatingException(ex);

                return false;
            }
            finally
            {
                AppRunning = false;
                if (commandExecuted)
                {
                    DebuggerFinished();
                }
            }
        }

        /// <summary>
        /// Stop the current executiong
        /// </summary>
        public void Stop()
        {
            ReleaseWaitHandler();

            Log.Debug("Stop()");
            try
            {
                if (_currentPowerShell != null)
                {
                    Log.Debug("Stopping current PowerShell");
                    _currentPowerShell.Stop();
                    Log.Debug("Stop complete.");
                }
            }
            catch (Exception ex)
            {
                Log.Debug("Exception when stopping current PowerShell execution.", ex);
            }
        }

        /// <summary>
        /// Get all local scoped variables for client
        /// </summary>
        /// <returns>Collection of variable to client</returns>
        public IEnumerable<Variable> GetScopedVariable()
        {
            try
            {
                RefreshScopedVariable();
            }
            catch (Exception ex)
            {
                ServiceCommon.Log("Failed to get variables. " + ex.Message);
            }

            return _varaiables;
        }

        public IEnumerable<PowerShellProTools.Host.Module> GetModules()
        {
            return GetObjects<PowerShellProTools.Host.Module>("Get-Module -ListAvailable | ForEach-Object { [PowerShellProTools.Host.Module]$_ } | ConvertTo-Json -Depth 1 -WarningAction SilentlyContinue");
        }

        public IEnumerable<PowerShellProTools.Host.Module> GetImportedModules()
        {
            return GetObjects<PowerShellProTools.Host.Module>("Get-Module | ForEach-Object { [PowerShellProTools.Host.Module]$_ } | ConvertTo-Json -Depth 1 -WarningAction SilentlyContinue");
        }

        public void ImportModule(PowerShellProTools.Host.Module module)
        {
            ExecuteCommand($"Import-Module -Name '{module.Name}' -RequiredVersion '{module.Version}'");
        }

        public IEnumerable<PowerShellToolsPro.Cmdlets.VSCode.PSJob> GetJobs()
        {
            return GetObjects<PowerShellToolsPro.Cmdlets.VSCode.PSJob>("Get-Job | ForEach-Object { [PowerShellToolsPro.Cmdlets.VSCode.PSJob]$_ } | ConvertTo-Json -Depth 1 -WarningAction SilentlyContinue");
        }

        public void StopJob(PowerShellToolsPro.Cmdlets.VSCode.PSJob job)
        {
            ExecuteCommand($"Stop-Job -Id {job.Id}");
        }

        public void ReceiveJob(PowerShellToolsPro.Cmdlets.VSCode.PSJob job)
        {
            ExecuteCommand($"Receive-Job -Id {job.Id}");
        }

        private IEnumerable<T> GetObjects<T>(string command)
        {
            if (Runspace.Debugger.InBreakpoint)
            {
                PSCommand psCommand = new PSCommand();
                psCommand.AddScript(command);
                var output = new PSDataCollection<PSObject>();
                psCommand.Commands[0].MergeMyResults(PipelineResultTypes.Error, PipelineResultTypes.Output);
                Runspace.Debugger.ProcessCommand(psCommand, output);
                var json = output.FirstOrDefault()?.BaseObject.ToString();

                return JsonConvert.DeserializeObject<IEnumerable<T>>(json);
            }
            else
            {
                using (var powershell = PowerShell.Create())
                {
                    powershell.Runspace = Runspace;
                    powershell.AddScript(command);
                    var json = powershell.Invoke<string>().FirstOrDefault();
                    return JsonConvert.DeserializeObject<IEnumerable<T>>(json);
                }
            }
        }

        private void ExecuteCommand(string command)
        {
            if (Runspace.Debugger.InBreakpoint)
            {
                PSCommand psCommand = new PSCommand();
                psCommand.AddScript(command);
                var output = new PSDataCollection<PSObject>();
                psCommand.Commands[0].MergeMyResults(PipelineResultTypes.Error, PipelineResultTypes.Output);
                Runspace.Debugger.ProcessCommand(psCommand, output);
            }
            else
            {
                using (var powershell = PowerShell.Create())
                {
                    powershell.Runspace = Runspace;
                    powershell.AddScript(command);
                    powershell.Invoke();
                }
            }
        }

        /// <summary>
        /// Respond client request for callstack frames of current execution context
        /// </summary>
        /// <returns>Collection of callstack to client</returns>
        public IEnumerable<CallStack> GetCallStack()
        {
            ServiceCommon.Log("Obtaining the callstack");
            List<CallStack> callStackFrames = new List<CallStack>();

            var callstackString = _callstack.FirstOrDefault().BaseObject.ToString();
            var frames = JsonConvert.DeserializeObject<JArray>(callstackString);

            foreach (var frame in frames)
            {
                var position = frame["Position"];
                callStackFrames.Add(
                    new CallStack(
                        frame["ScriptName"].Value<string>(),
                        frame["FunctionName"].Value<string>(),
                        position["StartLineNumber"].Value<int>(),
                        position["EndLineNumber"].Value<int>(),
                        position["StartColumnNumber"].Value<int>(),
                        position["EndColumnNumber"].Value<int>()));
            }

            return callStackFrames;
        }

        /// <summary>
        /// Get prompt string
        /// </summary>
        /// <returns>Prompt string</returns>
        public string GetPrompt()
        {
            using (_currentPowerShell = PowerShell.Create())
            {
                _currentPowerShell.Runspace = Runspace;
                _currentPowerShell.AddCommand("prompt");

                string prompt = _currentPowerShell.Invoke<string>().FirstOrDefault();
                if (GetDebugScenario() != DebugScenario.Local)
                {
                    prompt = string.Format("[{0}] {1}", Runspace.ConnectionInfo.ComputerName, prompt);
                }

                return prompt;
            }
        }

        /// <summary>
        /// Client set resume action for debugger
        /// </summary>
        /// <param name="resumeAction">DebuggerResumeAction</param>
        public void SetDebuggerResumeAction(DebuggerResumeAction resumeAction)
        {
            _debuggingCommandRequests.Add(new DebuggingCommandRequest
            {
                ResumeAction = resumeAction
            });
        }

        /// <summary>
        /// Apply the raw host options on powershell host
        /// </summary>
        /// <param name="options">PS raw UI host options</param>
        public void SetOption(PowerShellRawHostOptions options)
        {
            RawHostOptions = options;
        }

        /// <summary>
        /// Returns the connection info for the current runspace based on certain characteristics of the runspace.
        /// Note: if you overwrite the _currentPowerShell object after attaching to a process, this will no longer
        /// return RemoteAttach.
        /// </summary>
        public DebugScenario GetDebugScenario()
        {
            if (Runspace.ConnectionInfo == null || Runspace.ConnectionInfo is NamedPipeConnectionInfo)
            {
                return DebugScenario.Local;
            }
            else if (Runspace.ConnectionInfo is WSManConnectionInfo)
            {
                if (_currentPowerShell != null)
                {
                    if (_currentPowerShell.Commands.Commands.FirstOrDefault(c => c.CommandText.StartsWith("Debug-Runspace", StringComparison.OrdinalIgnoreCase)) != null)
                    {
                        return DebugScenario.RemoteAttach;
                    }
                }
                return DebugScenario.RemoteSession;
            }
            else if (Runspace.ConnectionInfo != null && !(Runspace.ConnectionInfo is WSManConnectionInfo))
            {
                return DebugScenario.LocalAttach;
            }
            return DebugScenario.Unknown;
        }

        public string GetTrueFileName(string file)
        {
            if (_mapLocalToRemote.ContainsKey(file))
            {
                return _mapLocalToRemote[file];
            }
            else
            {
                return file;
            }
        }

        public void LoadProfiles()
        {
            ServiceCommon.Log("Loading PowerShell Profiles");

            PSObject profiles;
            using (var powerShell = PowerShell.Create())
            {
                powerShell.Runspace = Runspace;
                powerShell.AddCommand("Get-Variable").AddParameter("Name", "profile").AddParameter("ValueOnly").AddParameter("ErrorAction", "SilentlyContinue");
                profiles = powerShell.Invoke<PSObject>().FirstOrDefault();
            }

            string sourceProfilesCommand = string.Empty;

            // if a file exists at each location of the profiles, add sourcing that profile to the command
            foreach (string[] profile in DebugEngineConstants.PowerShellProfiles)
            {
                PSMemberInfo profileMember = profiles.Members[profile[0]];
                var profilePath = (string)profileMember.Value;
                var profileFile = new FileInfo(profilePath);

                if (profileFile.Exists)
                {
                    ServiceCommon.Log(string.Format("Profile file for {0} found at {1}.", profile[0], profilePath));
                    sourceProfilesCommand += string.Format(". '{0}';{1}", profilePath, Environment.NewLine);
                }
            }

            if (!string.IsNullOrEmpty(sourceProfilesCommand))
            {
                Execute(sourceProfilesCommand);
            }
        }

        #endregion

        private const string ProfileVariableName = "profile";
        private static object RunspaceLock = new object();

        private void SetRunspace(Runspace runspace)
        {
            lock (RunspaceLock)
            {
                if (Runspace != null)
                {
                    UnloadRunspace(Runspace);
                }

                Runspace = runspace;
                LoadRunspace(Runspace);
            }
        }

        private void LoadRunspace(Runspace runspace)
        {
            if (Runspace != null)
            {
                if (GetDebugScenario() == DebugScenario.Local || _installedPowerShellVersion >= RequiredPowerShellVersionForRemoteSessionDebugging)
                {
                    runspace.Debugger.DebuggerStop += Debugger_DebuggerStop;
                    runspace.Debugger.BreakpointUpdated += Debugger_BreakpointUpdated;
                }

                runspace.StateChanged += _runspace_StateChanged;
            }
        }

        private void UnloadRunspace(Runspace runspace)
        {
            if (Runspace != null)
            {
                if (GetDebugScenario() == DebugScenario.Local || _installedPowerShellVersion >= RequiredPowerShellVersionForRemoteSessionDebugging)
                {
                    runspace.Debugger.DebuggerStop -= Debugger_DebuggerStop;
                    runspace.Debugger.BreakpointUpdated -= Debugger_BreakpointUpdated;
                }
            }

            runspace.StateChanged -= _runspace_StateChanged;
        }

        public int GetVariableDetailsCount(string path)
        {
            if (Runspace.Debugger.InBreakpoint)
            {
                PSCommand psCommand = new PSCommand();
                psCommand.AddScript($"(Get-PoshToolsVariable -Path '{path}' | Measure-Object).Count");
                var output = new PSDataCollection<PSObject>();

                Runspace.Debugger.ProcessCommand(psCommand, output);

                return (int)output.FirstOrDefault()?.BaseObject;
            }
            else
            {
                using (var ps = PowerShell.Create())
                {
                    ps.Runspace = Runspace;
                    ps.AddScript($"(Get-PoshToolsVariable -Path '{path}' | Measure-Object).Count");
                    return ps.Invoke<int>().FirstOrDefault();
                }
            }
        }


        public IEnumerable<Variable> GetVariableDetails(string path, int skip, int take)
        {
            var variables = new List<Variable>();
            if (Runspace.Debugger.InBreakpoint)
            {
                PSCommand psCommand = new PSCommand();

                var select = $"| Select-Object -First {take} -Skip {skip}";
                if (skip == -1)
                {
                    select = String.Empty;
                }
                psCommand.AddScript($"Get-PoshToolsVariable -Path '{path}' {select} | ConvertTo-Json -Depth 1 -ErrorAction SilentlyContinue -WarningAction SilentlyContinue");
                var output = new PSDataCollection<PSObject>();

                Runspace.Debugger.ProcessCommand(psCommand, output);

                var json = output.FirstOrDefault()?.BaseObject.ToString();

                if (string.IsNullOrEmpty(json)) return variables;

                if (json.StartsWith("["))
                {
                    variables = JsonConvert.DeserializeObject<List<Variable>>(json);
                }
                else
                {
                    variables = new List<Variable> { JsonConvert.DeserializeObject<Variable>(json) };
                }
            }
            else
            {
                string json;
                using(var ps = PowerShell.Create())
                {
                    ps.Runspace = Runspace;
                    var select = $"| Select-Object -First {take} -Skip {skip}";
                    if (skip == -1)
                    {
                        select = String.Empty;
                    }
                    ps.AddScript($"Get-PoshToolsVariable -Path '{path}' {select} | ConvertTo-Json -Depth 1 -ErrorAction SilentlyContinue -WarningAction SilentlyContinue");
                    json = ps.Invoke<string>().FirstOrDefault();
                }

                if (string.IsNullOrEmpty(json)) return variables;

                if (json.StartsWith("["))
                {
                    variables = JsonConvert.DeserializeObject<List<Variable>>(json);
                }
                else
                {
                    variables = new List<Variable> { JsonConvert.DeserializeObject<Variable>(json) };
                }
            }

            return variables;
        }

        private dynamic CallPoshToolsServer(Request request)
        {
            try
            {
                var pipeName = $"psp_{_targetProcessId}";
                using (var client = new NamedPipeClientStream(pipeName))
                {
                    client.Connect(1000);
                    var streamReader = new StreamReader(client);
                    var streamWriter = new StreamWriter(client);

                    var json = JsonConvert.SerializeObject(request);
                    var packet = Convert.ToBase64String(Encoding.UTF8.GetBytes(json));

                    streamWriter.WriteLine(packet);
                    var responsePacket = streamReader.ReadLine();

                    var decodedRequest = Encoding.UTF8.GetString(Convert.FromBase64String(responsePacket));
                    dynamic response = JObject.Parse(decodedRequest);

                    return response.Data;
                }
            }
            catch
            {
                return null;
            }

        }

        private void RefreshScopedVariable()
        {
            if (Runspace.Debugger.InBreakpoint)
            {
                var psCommand = new PSCommand();
                psCommand.AddScript("Get-Variable -Exclude @('foreach', 'switch') | Out-PoshToolsVariable -PassThru");
                var output = new PSDataCollection<PSObject>();
                psCommand.Commands[0].MergeMyResults(PipelineResultTypes.Error, PipelineResultTypes.Output);
                Runspace.Debugger.ProcessCommand(psCommand, output);
                _varaiables = output.OfType<PSObject>().Select(m => new Variable(m.Members[nameof(Variable.VarName)].Value.ToString(), m.Members[nameof(Variable.VarValue)].Value)
                {
                    HasChildren = (bool)m.Members[nameof(Variable.HasChildren)].Value,
                    Path = m.Members[nameof(Variable.Path)].Value.ToString(),
                    Type = m.Members[nameof(Variable.Type)].Value.ToString()
                });
            }
            else
            {
                using (var powershell = PowerShell.Create())
                {
                    powershell.Runspace = Runspace;
                    powershell.AddScript("Get-Variable -Exclude @('foreach', 'switch') | Out-PoshToolsVariable -PassThru");
                    _varaiables = powershell.Invoke<Variable>();
                }
            }
        }

        private void RefreshCallStack()
        {
            ServiceCommon.Log("Debuggger stopped, let us retreive all call stack frames");
            PSCommand psCommand = new PSCommand();
            psCommand.AddScript("Get-PSCallstack | ConvertTo-Json -WarningAction SilentlyContinue");
            var output = new PSDataCollection<PSObject>();
            DebuggerCommandResults results = Runspace.Debugger.ProcessCommand(psCommand, output);
            _callstack = output;
        }

        private void OnTerminatingException(Exception ex)
        {
            ServiceCommon.Log("OnTerminatingException");
            UnloadRunspace(Runspace);
            CallbackService.TerminatingException(new PowerShellRunTerminatingException(ex));
        }

        private void DebuggerFinished()
        {
            ServiceCommon.Log("DebuggerFinished");

            ClearBreakpoints();
            _psBreakpointTable.Clear();

            CallbackService.OutputStringLine(string.Empty);
            CallbackService.RefreshPrompt();

            if (Runspace != null)
            {
                UnloadRunspace(Runspace);
            }

            if (_currentPowerShell != null)
            {
                _currentPowerShell.Stop();
                _currentPowerShell = null;
            }

            ReleaseWaitHandler();

            _localVariables.Clear();
            _propVariables.Clear();

            CallbackService.DebuggerFinished();
            ExecuteFinished?.Invoke(null, new EventArgs());
        }

        private bool IsDebuggerActive(System.Management.Automation.Debugger debugger)
        {
            if (_installedPowerShellVersion >= RequiredPowerShellVersionForRemoteSessionDebugging)
            {
                // IsActive denotes debugger being stopped and the presence of breakpoints
                return debugger.IsActive;
            }

            return false;
        }

        private void InitializeRunspace(int processId, PSHost psHost, bool sta)
        {
            ServiceCommon.Log("Initializing run space with debugger");

            /*
                 This is necessary because PowerShell Remoting does not allow the end user to select the threading state for their connection.
                 We can work around this by overrding the default "UseCurrentThread" threading state to "Default"
                 UseCurrentThread does not work with STA threads.
             */
            InitialSessionState iss = InitialSessionState.CreateDefault();

            var assemblyBasePath = Path.GetDirectoryName(GetType().GetTypeInfo().Assembly.Location);
            iss.ImportPSModulesFromPath(Path.Combine(assemblyBasePath, "Modules"));

            if (processId != -1)
            {
                var ci = new NamedPipeConnectionInfo(processId);
                var tt = TypeTable.LoadDefaultTypeFiles();

                using (var primer = RunspaceFactory.CreateRunspace(ci, psHost, tt))
                {
                    primer.Open();
                    using (var powerShell = PowerShell.Create())
                    {
                        powerShell.Runspace = primer;
                        powerShell.AddScript(Common.Constants.NamedPipeRunspacePrimer);

                        try
                        {
                            powerShell.Invoke();
                        }
                        catch (Exception ex)
                        {
                            ServiceCommon.Log("Failed to set threading mode. " + ex.Message);
                        }
                    }
                }

                Runspace = RunspaceFactory.CreateRunspace(ci, psHost, tt);
                typeof(Runspace).GetProperty("ApartmentState").SetValue(Runspace, ApartmentState.STA);

                Runspace.Open();

                if (Runspace.Debugger == null)
                {
                    throw new Exception("PowerShell Tools for Visual Studio does not support constrained language module.");
                }

                Runspace.Debugger.SetDebugMode(DebugModes.RemoteScript | DebugModes.LocalScript);
            }
            else
            {
                Runspace = RunspaceFactory.CreateRunspace(psHost);
                Runspace.Open();
            }

            while (Runspace.RunspaceStateInfo.State != RunspaceState.Opened)
            {
                Thread.Sleep(10);
            }

            ImportCommon();
            ProvideProfileVariable();
            ServiceCommon.Log("Done initializing runspace");
        }

        private void ImportCommon()
        {
            var commonModule = Path.Combine(Path.GetDirectoryName(GetType().GetTypeInfo().Assembly.Location), "PowerShellProTools.SharedCommands.dll");
            using (PowerShell ps = PowerShell.Create())
            {
                ps.Runspace = Runspace;
                ps.AddCommand("Import-Module").AddParameter("Name", commonModule);
                ps.Invoke();

                if (ps.HadErrors)
                {
                    Log.Error("Failed to load common module.");
                    foreach (var error in ps.Streams.Error)
                    {
                        Log.Error(error.ToString());
                    }
                }
            }

            using (var ps = PowerShell.Create())
            {
                ps.Runspace = Runspace;
                ps.AddCommand("Remove-Variable").AddParameter("Name", "PSSenderInfo").AddParameter("Force");
                ps.Invoke();
            }

            using (var ps = PowerShell.Create())
            {
                ps.Runspace = Runspace;
                ps.AddScript($"$DTE = [PowerShellTools.DteManager]::GetDTE({Process.GetCurrentProcess().Id})");
                ps.Invoke();
            }
        }

        private void SetupExecutionPolicy()
        {
            SetExecutionPolicy(ExecutionPolicy.RemoteSigned, ExecutionPolicyScope.Process);
        }

        private void SetExecutionPolicy(ExecutionPolicy policy, ExecutionPolicyScope scope)
        {
            ExecutionPolicy machinePolicy = ExecutionPolicy.Undefined;
            ExecutionPolicy userPolicy = ExecutionPolicy.Undefined;

            ServiceCommon.Log("Setting execution policy");
            using (PowerShell ps = PowerShell.Create())
            {
                ps.Runspace = Runspace;

                ps.AddCommand("Get-ExecutionPolicy")
                        .AddParameter("Scope", "MachinePolicy");

                foreach (var result in ps.Invoke())
                {
                    machinePolicy = ((ExecutionPolicy)result.BaseObject);
                    break;
                }

                ps.Commands.Clear();

                ps.AddCommand("Get-ExecutionPolicy")
                        .AddParameter("Scope", "UserPolicy");

                foreach (var result in ps.Invoke())
                {
                    userPolicy = ((ExecutionPolicy)result.BaseObject);
                    break;
                }

                ps.Commands.Clear();

                if (machinePolicy != ExecutionPolicy.Undefined || userPolicy != ExecutionPolicy.Undefined)
                    return;

                ps.Commands.Clear();

                ps.AddCommand("Set-ExecutionPolicy")
                    .AddParameter("ExecutionPolicy", policy)
                    .AddParameter("Scope", scope)
                    .AddParameter("Force");

                try
                {
                    //If a more restritive scope causes this to fail, this can throw
                    //an exception and cause the host to crash. This causes it to be
                    //recreated over and over again. This leads to performance issues in VS.
                    ps.Invoke();
                }
                catch (Exception ex)
                {
                    ServiceCommon.Log("Failed to set execution policy. {0}", ex.Message);
                }
            }
        }

        private void ReleaseWaitHandler()
        {
            if (!_debuggingCommandRequests.IsAddingCompleted)
            {
                _debuggingCommandRequests.Add(new DebuggingCommandRequest
                {
                    ResumeAction = DebugEngineConstants.Debugger_Stop
                });
            }
        }

        /// <summary>
        /// Provides the $dte Variable if we are in a local runspace and if the $dte variable has not yet been set.
        /// </summary>
        private static void ProvideDteVariable(Runspace runspace)
        {
            ServiceCommon.Log("Providing $dte variable to the local runspace.");

            using (var powerShell = PowerShell.Create())
            {
                powerShell.Runspace = runspace;
                powerShell.AddScript("$Global:DTE = [PowerShellTools.HostService.ServiceCommon]::Dte");
                powerShell.Invoke();

                if (powerShell.HadErrors)
                {
                    foreach (var error in powerShell.Streams.Error)
                    {
                        ServiceCommon.Log(error.ToString());
                    }
                }
            }
        }

        private static void ProvideProfileVariable()
        {
            PSObject profiles;
            using (var powerShell = PowerShell.Create())
            {
                powerShell.Runspace = Runspace;
                powerShell.AddCommand("Get-Variable").AddParameter("Name", ProfileVariableName).AddParameter("ValueOnly").AddParameter("ErrorAction", "SilentlyContinue");
                profiles = powerShell.Invoke<PSObject>().FirstOrDefault();
            }

            bool windowsPowerShell;
            using (var powerShell = PowerShell.Create())
            {
                powerShell.Runspace = Runspace;
                powerShell.AddCommand("Get-Variable").AddParameter("Name", "PSVersionTable").AddParameter("ValueOnly").AddParameter("ErrorAction", "SilentlyContinue");
                var psversiontable = powerShell.Invoke<Hashtable>().FirstOrDefault();
                windowsPowerShell = psversiontable["PSVersion"].ToString().StartsWith("5");
            }

            // Provide profile as PS variable if not yet defined
            if (profiles == null)
            {
                ServiceCommon.Log("Providing $profile variable to the local runspace.");

                string psHome;
                using (var powerShell = PowerShell.Create())
                {
                    powerShell.Runspace = Runspace;
                    powerShell.Runspace = Runspace;
                    powerShell.AddCommand("Get-Variable").AddParameter("Name", "PsHome").AddParameter("ValueOnly").AddParameter("ErrorAction", "SilentlyContinue");
                    psHome = powerShell.Invoke<string>().FirstOrDefault();
                }

                string userPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), windowsPowerShell ? "WindowsPowerShell" : "PowerShell");

                PSObject profile = new PSObject(Path.Combine(userPath, DebugEngineConstants.PowerShellToolsProfileFileName));

                // 1. All Users, All Hosts, 2. All Users, Current Host, 3. Current User All Hosts, 4. Current User Current Host
                profile.Members.Add(new PSNoteProperty(DebugEngineConstants.PowerShellProfiles[0][0], Path.Combine(psHome, DebugEngineConstants.PowerShellProfiles[0][1])));
                profile.Members.Add(new PSNoteProperty(DebugEngineConstants.PowerShellProfiles[1][0], Path.Combine(psHome, DebugEngineConstants.PowerShellProfiles[1][1])));
                profile.Members.Add(new PSNoteProperty(DebugEngineConstants.PowerShellProfiles[2][0], Path.Combine(userPath, DebugEngineConstants.PowerShellProfiles[2][1])));
                profile.Members.Add(new PSNoteProperty(DebugEngineConstants.PowerShellProfiles[3][0], Path.Combine(userPath, DebugEngineConstants.PowerShellProfiles[3][1])));

                using (var powerShell = PowerShell.Create())
                {
                    powerShell.Runspace = Runspace;
                    powerShell.Runspace = Runspace;
                    powerShell.AddCommand("Set-Variable").AddParameter("Name", ProfileVariableName).AddParameter("Value", profile).AddParameter("ErrorAction", "SilentlyContinue").AddParameter("Option", "Constant");
                    powerShell.Invoke();
                }
            }
        }

        private string ExecuteDebuggingCommand(string debuggingCommand, bool output)
        {
            var request = new DebuggingCommandRequest
            {
                Command = debuggingCommand,
                WriteOutput = output
            };

            if (_debuggingCommandRequests.IsAddingCompleted)
            {
                return null;
            }

            _debuggingCommandRequests.Add(request);

            request.Completed.WaitOne();

            return request.CommandOutput;
        }

        /// <summary>
        /// Opens the script a remote process is running.
        /// </summary>
        /// <param name="scriptName"></param>
        /// <returns></returns>
        private string OpenRemoteAttachedFile(string scriptName)
        {
            if (!_needToCopyRemoteScript && _mapRemoteToLocal.ContainsKey(scriptName))
            {
                return _mapRemoteToLocal[scriptName];
            }

            PSCommand psCommand = new PSCommand();
            psCommand.AddScript(string.Format("Get-Content \"{0}\"", scriptName));
            PSDataCollection<PSObject> result = new PSDataCollection<PSObject>();
            Runspace.Debugger.ProcessCommand(psCommand, result);

            string[] remoteText = new string[result.Count()];

            for (int i = 0; i < remoteText.Length; i++)
            {
                remoteText[i] = result.ElementAt(i).BaseObject as string;
            }

            // create new directory and corressponding file path/name
            string tmpFileName = Path.GetTempFileName();
            string dirPath = tmpFileName.Remove(tmpFileName.LastIndexOf('.'));
            string fullFileName = Path.Combine(dirPath, new FileInfo(scriptName).Name);

            // check to see if we have already copied the script over, and if so, overwrite
            if (_mapRemoteToLocal.ContainsKey(scriptName))
            {
                fullFileName = _mapRemoteToLocal[scriptName];
            }
            else
            {
                Directory.CreateDirectory(dirPath);
            }

            _mapRemoteToLocal[scriptName] = fullFileName;
            _mapLocalToRemote[fullFileName] = scriptName;

            File.WriteAllLines(fullFileName, remoteText);

            return fullFileName;
        }

        /// <summary>
        /// Re-adds all of the various event handlers to the runspace
        /// </summary>
        private void AddEventHandlers()
        {
            Runspace.Debugger.DebuggerStop += Debugger_DebuggerStop;
            Runspace.Debugger.BreakpointUpdated += Debugger_BreakpointUpdated;
            Runspace.StateChanged += _runspace_StateChanged;
            Runspace.AvailabilityChanged += _runspace_AvailabilityChanged;
        }

        /// <summary>
        /// Invokes given script on the provided PowerShell object after setting its runspace object. If powerShell is null, then the method will
        /// instantiate it inside of a using.
        /// </summary>
        /// <param name="powerShell">This should usually be _currentPowerShell. Make sure to enclose uses of _currentPowerShell and this method
        /// inside of a using.</param>
        /// <param name="script">Script to invoke.</param>
        /// <returns>Returns the result of the invoke</returns>
        private Collection<PSObject> InvokeScript(PowerShell powerShell, string script)
        {
            if (powerShell == null)
            {
                // if user passes in a null PowerShell object, we will instantiate it for them
                using (powerShell = PowerShell.Create())
                {
                    powerShell.Runspace = Runspace;
                    powerShell.AddScript(script);
                    return powerShell.Invoke();
                }
            }

            powerShell.Commands.Clear();
            powerShell.Runspace = Runspace;
            powerShell.AddScript(script);
            return powerShell.Invoke();
        }

        /// <summary>
        /// Uses _savedCredential in order to enter into a remote session with a remote machine. If _savedCredential is null, method will instead
        /// use InvokeScript to prompt/enter into a session without saving credentials.
        /// </summary>
        /// <param name="powerShell">This should be an already instanstiated PowerShell object, and should be inside of a using. If
        /// powerShell is null, method will return without performing any action.</param>
        /// <param name="remoteName">Machine to connect to.</param>
        private void EnterCredentialedRemoteSession(PowerShell powerShell, string remoteName, int port, bool useSSL)
        {
            if (powerShell == null)
            {
                // callee is expected to passs in an already inalized PowerShell object to this method
                return;
            }

            if (_savedCredential == null)
            {
                InvokeScript(powerShell, string.Format(DebugEngineConstants.EnterRemoteSessionDefaultCommand, remoteName));
                return;
            }

            PSCommand enterSession = new PSCommand();
            enterSession.AddCommand("Enter-PSSession").AddParameter("ComputerName", remoteName).AddParameter("Credential", _savedCredential);

            if (port != -1)
            {
                // check for user specified port
                enterSession.AddParameter("-Port", port);
            }
            if (useSSL)
            {
                // if told to use SSL from options dialog, add the SSL parameter
                enterSession.AddParameter("-UseSSL");
            }

            powerShell.Runspace = Runspace;
            powerShell.Commands.Clear();
            powerShell.Commands = enterSession;
            powerShell.Invoke();
        }

        /// <summary>
        /// Runspace state change event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _runspace_StateChanged(object sender, RunspaceStateEventArgs e)
        {
            ServiceCommon.Log("Runspace State Changed: {0}", e.RunspaceStateInfo.State);

            switch (e.RunspaceStateInfo.State)
            {
                case RunspaceState.Broken:
                case RunspaceState.Closed:
                case RunspaceState.Disconnected:
                    CallbackService.DebuggerFinished();
                    break;
            }
        }

        /// <summary>
        /// Runspace availability change handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _runspace_AvailabilityChanged(Object sender, RunspaceAvailabilityEventArgs e)
        {
            ServiceCommon.Log("Runspace Availability Changed: {0}", e.RunspaceAvailability.ToString());
        }

        /// <summary>
        /// Breakpoint updates (such as enabled/disabled)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Debugger_BreakpointUpdated(object sender, BreakpointUpdatedEventArgs e)
        {
            // Not in-use for now, leave it as place holder for future support on powershell debugging command in REPL
            ServiceCommon.Log("Breakpoint updated: {0} {1}", e.UpdateType, e.Breakpoint);

            if (CallbackService != null)
            {
                var lbp = e.Breakpoint as LineBreakpoint;
                CallbackService.BreakpointUpdated(new DebuggerBreakpointUpdatedEventArgs(new PowerShellBreakpoint(e.Breakpoint.Script, lbp.Line, lbp.Column), e.UpdateType));
            }
        }

        /// <summary>
        /// Debugging output event handler
        /// </summary>
        /// <param name="value">String to output</param>
        public void NotifyOutputString(string value)
        {
            ServiceCommon.LogCallbackEvent("Callback to client for string output in VS");

            CallbackService.OutputString(value);
        }

        /// <summary>
        /// Debugging output event handler, to show progress status.
        /// </summary>
        /// <param name="sourceId">The id of the record with progress.</param>
        /// <param name="record">The record itself.</param>
        public void NotifyOutputProgress(long sourceId, ProgressRecord record)
        {
            ServiceCommon.LogCallbackEvent("Callback to client to show progress");

            CallbackService.OutputProgress(sourceId, record);
        }

        /// <summary>
        /// PS debugger stopped event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Debugger_DebuggerStop(object sender, DebuggerStopEventArgs e)
        {
            _debuggingCommandRequests = new BlockingCollection<DebuggingCommandRequest>();
            _variableCache = new Dictionary<string, List<Variable>>();

            ServiceCommon.Log("Debugger stopped ...");
            DebugScenario currScenario = GetDebugScenario();

            try
            {
                RefreshCallStack();
            }
            catch (Exception ex)
            {
                ServiceCommon.Log("Failed to refresh variables and call stack." + ex.Message);
            }

            ServiceCommon.LogCallbackEvent("Callback to client, and wait for debuggee to resume");
            if (e.Breakpoints.Count > 0)
            {
                LineBreakpoint bp = (LineBreakpoint)e.Breakpoints[0];

                string file = bp.Script;
                if (currScenario != DebugScenario.Local && _mapRemoteToLocal.ContainsKey(bp.Script))
                {
                    file = _mapRemoteToLocal[bp.Script];
                }

                // breakpoint is always hit for this case
                CallbackService.DebuggerStopped(new DebuggerStoppedEventArgs(file, bp.Line, bp.Column, true, false));

            }
            else
            {

                string file;
                int lineNum, column;

                switch (currScenario)
                {
                    case DebugScenario.LocalAttach:
                        file = e.InvocationInfo.ScriptName;
                        lineNum = e.InvocationInfo.ScriptLineNumber;
                        column = e.InvocationInfo.OffsetInLine;

                        // the stop which occurs after attaching is not associated with a breakpoint and should result in the process' script being opened
                        CallbackService.DebuggerStopped(new DebuggerStoppedEventArgs(file, lineNum, column, false, true));
                        break;
                    case DebugScenario.RemoteAttach:
                        // copy the remote file over to host machine
                        file = OpenRemoteAttachedFile(e.InvocationInfo.ScriptName);
                        lineNum = e.InvocationInfo.ScriptLineNumber;
                        column = e.InvocationInfo.OffsetInLine;

                        // the stop which occurs after attaching is not associated with a breakpoint and should result in the remote process' script being opened
                        CallbackService.DebuggerStopped(new DebuggerStoppedEventArgs(file, lineNum, column, false, true));
                        _needToCopyRemoteScript = false;
                        break;
                    default:
                        CallbackService.DebuggerStopped(new DebuggerStoppedEventArgs());
                        break;
                }

            }

            bool resumed = false;
            while (!resumed)
            {
                DebuggerStopped?.Invoke(null, new EventArgs());

                var request = _debuggingCommandRequests.Take();

                try
                {
                    currScenario = GetDebugScenario();
                    if (!string.IsNullOrEmpty(request.Command))
                    {
                        if (currScenario == DebugScenario.Local)
                        {
                            // local debugging
                            var output = new Collection<PSObject>();

                            PSCommand psCommand = new PSCommand();
                            psCommand.AddScript(request.Command);
                            psCommand.AddCommand("Out-String");
                            var debuggingOutput = new PSDataCollection<PSObject>();

                            debuggingOutput.DataAdded += (s, args) =>
                            {
                                var dataCollection = s as PSDataCollection<PSObject>;
                                var item = dataCollection[args.Index];
                                ProcessDebuggingCommandResults(item, request);
                            };

                            Runspace.Debugger.ProcessCommand(psCommand, debuggingOutput);
                        }
                        else
                        {
                            // remote session and local attach debugging
                            ProcessRemoteDebuggingCommandResults(ExecuteDebuggingCommand(request), request);
                        }
                    }
                    else
                    {
                        ServiceCommon.Log(string.Format("Debuggee resume action is {0}", request.ResumeAction));
                        e.ResumeAction = request.ResumeAction;
                        resumed = true; // debugger resumed executing
                        _debuggingCommandRequests.CompleteAdding();
                    }
                }
                catch (Exception ex)
                {
                    NotifyOutputString(ex.Message);
                }

                // Notify the debugging command execution call that debugging command was complete.
                request.Completed.Set();
            }

            if(_debuggingCommandRequests.Any())
            {
                foreach(var r in _debuggingCommandRequests.GetConsumingEnumerable())
                {
                    r.Completed.Set();
                }
            }
        }

        private PSDataCollection<PSObject> ExecuteDebuggingCommand(DebuggingCommandRequest request)
        {
            PSCommand psCommand = new PSCommand();
            psCommand.AddScript(request.Command);
            psCommand.Commands[0].MergeMyResults(PipelineResultTypes.Error, PipelineResultTypes.Output);
            var output = new PSDataCollection<PSObject>();
            output.DataAdded += (sender, args) => {
                var list = sender as PSDataCollection<PSObject>;
                StringBuilder outputString = new StringBuilder();
                foreach (PSObject obj in list)
                {
                    outputString.AppendLine(obj.ToString());
                }

                if (request.WriteOutput)
                {
                    NotifyOutputString(outputString.ToString());
                }
            };

            Runspace.Debugger.ProcessCommand(psCommand, output);

            return output;
        }

        private void ProcessDebuggingCommandResults(PSObject output, DebuggingCommandRequest request)
        {
            if (output != null)
            {
                if (request.WriteOutput)
                {
                    var newLine = string.Empty;
                    if (!output.ToString().TrimStart().StartsWith("[DBG]"))
                    {
                        newLine = Environment.NewLine;
                    }

                    NotifyOutputString(output.ToString() + newLine);
                }

                var pobj = output;

                if (pobj != null && pobj.BaseObject is string)
                {
                    request.CommandOutput = (string)pobj.BaseObject;
                }
                else if (pobj != null && pobj.BaseObject is LineBreakpoint)
                {
                    LineBreakpoint bp = (LineBreakpoint)pobj.BaseObject;
                    if (bp != null)
                    {
                        _psBreakpointTable.Add(
                            new PowerShellBreakpointRecord(
                                new PowerShellBreakpoint(bp.Script, bp.Line, bp.Column),
                                bp.Id));
                    }
                }
            }
        }

        private void ProcessRemoteDebuggingCommandResults(PSDataCollection<PSObject> output, DebuggingCommandRequest request)
        {
            var pobj = output.FirstOrDefault();

            if (pobj != null && pobj.BaseObject is string)
            {
                request.CommandOutput = (string)pobj.BaseObject;
            }
            else if (pobj != null && pobj.BaseObject is LineBreakpoint)
            {
                LineBreakpoint bp = (LineBreakpoint)pobj.BaseObject;
                if (bp != null)
                {
                    _psBreakpointTable.Add(
                        new PowerShellBreakpointRecord(
                            new PowerShellBreakpoint(bp.Script, bp.Line, bp.Column),
                            bp.Id));
                }
            }
        }

        /// <summary>
        /// Handling the remote file open event
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="args">remote file name</param>
        private async void HandleRemoteSessionForwardedEvent(object sender, PSEventArgs args)
        {
            if (args.SourceIdentifier.Equals("PSISERemoteSessionOpenFile", StringComparison.OrdinalIgnoreCase))
            {
                string text = null;
                byte[] array = null;
                try
                {
                    if (args.SourceArgs.Length == 2)
                    {
                        text = (args.SourceArgs[0] as string);
                        array = (byte[])(args.SourceArgs[1] as PSObject).BaseObject;
                    }
                    if (!string.IsNullOrEmpty(text) && array != null)
                    {
                        string tmpFileName = Path.GetTempFileName();
                        string dirPath = tmpFileName.Remove(tmpFileName.LastIndexOf('.'));
                        Directory.CreateDirectory(dirPath);
                        string fullFileName = Path.Combine(dirPath, new FileInfo(text).Name);

                        _mapRemoteToLocal[text] = fullFileName;
                        _mapLocalToRemote[fullFileName] = text;

                        File.WriteAllBytes(fullFileName, array);

                        await CallbackService.OpenRemoteFileAsync(fullFileName);
                    }
                }
                catch (Exception ex)
                {
                    ServiceCommon.Log("Failed to create local copy for downloaded file due to exception: {0}", ex.Message);
                }
            }
        }

        private void SetRemoteScriptDebugMode40(System.Management.Automation.Runspaces.Runspace runspace)
        {
            runspace.Debugger.SetDebugMode(DebugModes.RemoteScript);
        }

        const int STD_INPUT_HANDLE = -10;

        [DllImport("kernel32.dll")]
        static extern IntPtr GetStdHandle(int handle);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern UInt32 WaitForSingleObject(IntPtr hHandle, UInt32 dwMilliseconds);

        /// <summary>
        /// App running flag indicating if there is app runing on PSHost
        /// </summary>
        private bool _appRunning = false;

        /// <summary>
        /// The identifier of this PSHost implementation.
        /// </summary>
        private Guid myId = Guid.NewGuid();

        /// <summary>
        /// A reference to the runspace used to start an interactive session.
        /// </summary>
        private Runspace _pushedRunspace = null;

        /// <summary>
        /// Thread lock
        /// </summary>
        private object _synLock = new object();

        /// <summary>
        /// Gets a string that contains the name of this host implementation.
        /// Keep in mind that this string may be used by script writers to
        /// identify when your host is being used.
        /// </summary>
        public override string Name
        {
            get { return "PowerShell Tools for Visual Studio Host"; }
        }

        /// <summary>
        /// This implementation always returns the GUID allocated at
        /// instantiation time.
        /// </summary>
        public override Guid InstanceId
        {
            get { return this.myId; }
        }

        public HostUi HostUi { get; private set; }

        public override PSHostUserInterface UI
        {
            get { return HostUi; }
        }

        /// <summary>
        /// Gets the version object for this application. Typically this
        /// should match the version resource in the application.
        /// </summary>
        public override Version Version
        {
            get
            {
                return Assembly.GetExecutingAssembly().GetName().Version;
            }
        }

        /// <summary>
        /// App running flag indicating if there is app runing on PSHost
        /// </summary>
        public bool AppRunning
        {
            get
            {
                return _appRunning;
            }
            set
            {
                lock (_synLock)
                {
                    _appRunning = value;

                    // Start monitoring thread
                    if (value)
                    {
                        //Task.Run(() =>
                        //{
                        //    //MonitorUserInputRequest();
                        //});
                    }
                }
            }
        }

        /// <summary>
        /// This API Instructs the host to interrupt the currently running
        /// pipeline and start a new nested input loop. In this example this
        /// functionality is not needed so the method throws a
        /// NotImplementedException exception.
        /// </summary>
        public override void EnterNestedPrompt()
        {
            throw new NotImplementedException(
                  "The method or operation is not implemented.");
        }

        /// <summary>
        /// This API instructs the host to exit the currently running input loop.
        /// In this example this functionality is not needed so the method
        /// throws a NotImplementedException exception.
        /// </summary>
        public override void ExitNestedPrompt()
        {
            throw new NotImplementedException(
                  "The method or operation is not implemented.");
        }

        /// <summary>
        /// This API is called before an external application process is
        /// started. Typically it is used to save state so that the parent
        /// can restore state that has been modified by a child process (after
        /// the child exits). In this example this functionality is not
        /// needed so the method returns nothing.
        /// </summary>
        public override void NotifyBeginApplication()
        {
            ServiceCommon.Log(@"For some reason, we don't get notified from this API anymore during execution when we have wpf application based PSHost process, 
leaving this log/comment here in case someone is wondering. It should never been logged unless there is a debugger attached to this process.
Also leaving the implementation here as a reference because that is going to be more ideal way if we have the above issue solved by powershell team in future.");
            // AppRunning = true;
        }

        /// <summary>
        /// This API is called after an external application process finishes.
        /// Typically it is used to restore state that a child process has
        /// altered. In this example, this functionality is not needed so
        /// the method returns nothing.
        /// </summary>
        public override void NotifyEndApplication()
        {
            ServiceCommon.Log(@"For some reason, we don't get notified from this API anymore during execution when we have wpf application based PSHost process, 
leaving this log/comment here in case someone is wondering. It should never been logged unless there is a debugger attached to this process.
Also leaving the implementation here as a reference because that is going to be more ideal way if we have the above issue solved by powershell team in future.");
            // AppRunning = false;
        }

        /// <summary>
        /// Indicate to the host application that exit has
        /// been requested. Pass the exit code that the host
        /// application should use when exiting the process.
        /// </summary>
        /// <param name="exitCode">The exit code that the
        /// host application should use.</param>
        public override void SetShouldExit(int exitCode)
        {

        }

        /// <summary>
        /// The culture information of the thread that created
        /// this object.
        /// </summary>
        private CultureInfo originalCultureInfo =
            System.Threading.Thread.CurrentThread.CurrentCulture;

        /// <summary>
        /// The UI culture information of the thread that created
        /// this object.
        /// </summary>
        private CultureInfo originalUICultureInfo =
            System.Threading.Thread.CurrentThread.CurrentUICulture;

        /// <summary>
        /// Gets the culture information to use. This implementation
        /// returns a snapshot of the culture information of the thread
        /// that created this object.
        /// </summary>
        public override System.Globalization.CultureInfo CurrentCulture
        {
            get { return this.originalCultureInfo; }
        }

        /// <summary>
        /// Gets the UI culture information to use. This implementation
        /// returns a snapshot of the UI culture information of the thread
        /// that created this object.
        /// </summary>
        public override System.Globalization.CultureInfo CurrentUICulture
        {
            get { return this.originalUICultureInfo; }
        }

        #region IHostSupportsInteractiveSession

        public bool IsRunspacePushed
        {
            get { return _pushedRunspace != null; }
        }

        public void PopRunspace()
        {
            // determine if you need to wake up detach runspace thread after popping
            bool needToWake = (GetDebugScenario() == DebugScenario.LocalAttach);

            if (_pushedRunspace != null)
            {
                Runspace.StateChanged -= Runspace_StateChanged;
                UnregisterRemoteFileOpenEvent(Runspace);
                Runspace = _pushedRunspace;
                _pushedRunspace = null;
            }

            CallbackService.SetRemoteRunspace(false);

            if (needToWake)
            {
                // wake up DetachFromRunspace
                _attachRequestEvent.Set();
            }
        }


        public void PushRunspace(System.Management.Automation.Runspaces.Runspace runspace)
        {
            _pushedRunspace = Runspace;
            Runspace = runspace;

            Runspace.StateChanged += Runspace_StateChanged;

            if (_installedPowerShellVersion < RequiredPowerShellVersionForRemoteSessionDebugging)
            {
                CallbackService.OutputStringLine(string.Format("Warning: PowerShell v4.0 or later is needed to debug during remote script execution. You can install the latest PowerShell from: {0}", Common.Constants.PowerShellInstallFWLink));
            }
            else
            {
                if (Runspace.Debugger != null)
                {
                    SetRemoteScriptDebugMode40(Runspace);
                }
                else
                {
                    CallbackService.OutputStringLine(string.Format("Warning: PowerShell v4.0 or later is needed to debug during remote script execution. You can install the latest PowerShell from: {0}", Common.Constants.PowerShellInstallFWLink));
                }
            }

            CallbackService.SetRemoteRunspace(true);

            RegisterRemoteFileOpenEvent(runspace);

            if (GetDebugScenario() == DebugScenario.LocalAttach)
            {
                // wake up AttachToRunspace
                _attachRequestEvent.Set();
            }
        }

        private void Runspace_StateChanged(object sender, RunspaceStateEventArgs e)
        {
            ServiceCommon.Log("Remote runspace State Changed: {0}", e.RunspaceStateInfo.State);

            switch (e.RunspaceStateInfo.State)
            {
                case RunspaceState.Broken:
                case RunspaceState.Closed:
                case RunspaceState.Disconnected:
                    PopRunspace();
                    break;
            }
        }

        Runspace IHostSupportsInteractiveSession.Runspace
        {
            get { return PowerShellDebuggingService.Runspace; }
        }

        #endregion IHostSupportsInteractiveSession

        #region private helpers

        /// <summary>
        /// Register psedit command for remote file open event
        /// </summary>
        /// <param name="remoteRunspace"></param>
        public void RegisterRemoteFileOpenEvent(Runspace remoteRunspace)
        {
            remoteRunspace.Events.ReceivedEvents.PSEventReceived += new PSEventReceivedEventHandler(this.HandleRemoteSessionForwardedEvent);
            if (remoteRunspace.RunspaceStateInfo.State != RunspaceState.Opened || remoteRunspace.RunspaceAvailability != RunspaceAvailability.Available)
            {
                return;
            }
            using (PowerShell powerShell = PowerShell.Create())
            {
                powerShell.Runspace = remoteRunspace;
                powerShell.AddScript(DebugEngineConstants.RegisterPSEditScript).AddParameter(DebugEngineConstants.RegisterPSEditParameterName, DebugEngineConstants.PSEditFunctionScript);

                try
                {
                    powerShell.Invoke();
                }
                catch (RemoteException)
                {
                }
            }
        }

        /// <summary>
        /// Unregister psedit function
        /// </summary>
        /// <param name="remoteRunspace"></param>
        public void UnregisterRemoteFileOpenEvent(Runspace remoteRunspace)
        {
            remoteRunspace.Events.ReceivedEvents.PSEventReceived -= new PSEventReceivedEventHandler(this.HandleRemoteSessionForwardedEvent);
            if (remoteRunspace.RunspaceStateInfo.State != RunspaceState.Opened || remoteRunspace.RunspaceAvailability != RunspaceAvailability.Available)
            {
                return;
            }
            using (PowerShell powerShell = PowerShell.Create())
            {
                powerShell.Runspace = remoteRunspace;
                powerShell.AddScript(DebugEngineConstants.UnregisterPSEditScript);

                try
                {
                    powerShell.Invoke();
                }
                catch (RemoteException)
                {
                }
            }
        }

        /// <summary>
        /// Monitoring thread for user input request
        /// Get a handle of the console console input file object,
        /// and check whether it's signalled by calling WaiForSingleobject with zero timeout.
        /// If it's not signalled, the process issued a pending Read on the handle
        /// </summary>
        /// <remarks>
        /// Will be started once app begins to run on remote PowerShell host service
        /// Stopped once app exits
        /// </remarks>
        private void MonitorUserInputRequest()
        {
            while (_appRunning)
            {
                IntPtr handle = GetStdHandle(STD_INPUT_HANDLE);
                UInt32 ret = WaitForSingleObject(handle, 0);

                if (ret != 0)
                {
                    // Tactic Fix (TODO: github issue https://github.com/Microsoft/poshtools/issues/479)
                    // Give a bit of time for case where app crashed on readline/readkey
                    // We dont want to put any dirty content into stdin stream buffer
                    // Which can only be flushed out till the next readline/readkey
                    Thread.Sleep(50);

                    if (_appRunning)
                    {
                        var task = CallbackService.RequestUserInputOnStdIn();
                    }
                    else
                    {
                        break;
                    }
                }

                Thread.Sleep(50);
            }
        }

        #endregion
    }

    public class DebuggingCommandRequest
    {
        public string Command { get; set; }
        public DebuggerResumeAction ResumeAction { get; set; }
        public string CommandOutput { get; set; }
        public bool WriteOutput { get; set; }
        public ManualResetEvent Completed { get; private set; } = new ManualResetEvent(false);
    }
}
