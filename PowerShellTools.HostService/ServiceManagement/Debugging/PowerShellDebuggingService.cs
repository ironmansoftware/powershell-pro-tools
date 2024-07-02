using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Remoting;
using System.Management.Automation.Runspaces;
using System.Reflection;
using System.Runtime.InteropServices;
using System.ServiceModel;
using System.Text.RegularExpressions;
using System.Threading;
using PowerShellTools.Common;
using PowerShellTools.Common.Debugging;
using PowerShellTools.Common.Logging;
using PowerShellTools.Common.ServiceManagement.DebuggingContract;

namespace PowerShellTools.HostService.ServiceManagement.Debugging
{
    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Multiple, UseSynchronizationContext = false)]
    [PowerShellServiceHostBehavior]
    public partial class PowerShellDebuggingService : IPowerShellDebuggingService
    {
        private static Runspace _runspace;
        private PowerShell _currentPowerShell;
        private IDebugEngineCallback _callback;
        private string _debuggingCommand;
        private IEnumerable<PSObject> _varaiables;
        private IEnumerable<PSObject> _callstack;
        private Collection<PSVariable> _localVariables;
        private Dictionary<string, Object> _propVariables;
        private Dictionary<string, string> _mapLocalToRemote;
        private Dictionary<string, string> _mapRemoteToLocal;
        private HashSet<PowerShellBreakpointRecord> _psBreakpointTable;
        private readonly AutoResetEvent _pausedEvent = new AutoResetEvent(false);
        private readonly AutoResetEvent _debugCommandEvent = new AutoResetEvent(false);
        private readonly AutoResetEvent _attachRequestEvent = new AutoResetEvent(false);
        private object _executeDebugCommandLock = new object();
        private string _debugCommandOutput;
        private bool _debugOutput;
        private static readonly Regex _rgx = new Regex(DebugEngineConstants.ExecutionCommandFileReplacePattern);
        private static readonly Regex validStackLine = new Regex(DebugEngineConstants.ValidCallStackLine, RegexOptions.Compiled);
        private DebuggerResumeAction _resumeAction;
        private Version _installedPowerShellVersion;
        private PowerShellDebuggingServiceAttachUtilities _attachUtilities;
        private bool _useSSL;
        private int _currentPid;
        private static readonly ILog Log = LogManager.GetLogger(typeof(PowerShellDebuggingService));

        // Needs to be initilaized from its corresponding VS option page over the wcf channel.
        // For now we dont have anything needed from option page, so we just initialize here.
        private PowerShellRawHostOptions _rawHostOptions = new PowerShellRawHostOptions();

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

        public PowerShellDebuggingService()
        {
            ServiceCommon.Log("Initializing debugging engine service ...");
            HostUi = new HostUi(this);
            _localVariables = new Collection<PSVariable>();
            _propVariables = new Dictionary<string, object>();
            _mapLocalToRemote = new Dictionary<string, string>();
            _mapRemoteToLocal = new Dictionary<string, string>();
            _psBreakpointTable = new HashSet<PowerShellBreakpointRecord>();
            _debugOutput = true;
            _installedPowerShellVersion = DependencyUtilities.GetInstalledPowerShellVersion();
            _attachUtilities = new PowerShellDebuggingServiceAttachUtilities(this);
            _forceStop = false;
            _currentPid = Process.GetCurrentProcess().Id;
            InitializeRunspace(this);
        }

        /// <summary>
        /// The runspace used by the current PowerShell host.
        /// </summary>
        public static Runspace Runspace
        {
            get
            {
                return _runspace;
            }
            set
            {
                _runspace = value;
            }
        }

        /// <summary>
        /// Call back service used to talk to VS side
        /// </summary>
        public IDebugEngineCallback CallbackService
        {
            get
            {
                return _callback;
            }
            set
            {
                _callback = value;
            }
        }

        /// <summary>
        /// PowerShell raw host UI options 
        /// </summary>
        public PowerShellRawHostOptions RawHostOptions
        {
            get
            {
                return _rawHostOptions;
            }
            set
            {
                _rawHostOptions = value;
            }
        }

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

            SetRunspace(_runspace);
        }

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
                using (_currentPowerShell = PowerShell.Create())
                {
                    // see if the process is a PS host
                    if (InvokeScript(_currentPowerShell, string.Format("Get-PSHostProcessInfo -Id {0}", pid)).Any())
                    {
                        var process = Process.GetProcessById((int)pid);
                        if (process != null)
                        {
                            ServiceCommon.Log(string.Format("IsAttachable: {1}; id: {1}", process.ProcessName, process.Id));
                            return true;
                        }
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
            if (_callback == null)
            {
                _callback = OperationContext.Current.GetCallbackChannel<IDebugEngineCallback>();
            }

            // scenario before entering, used to determine if we are local attaching
            DebugScenario preScenario = GetDebugScenario();

            // attaching leverages cmdlets introduced in PSv5, this must be on local machine to attach to a local process
            if (preScenario == DebugScenario.Local && _installedPowerShellVersion < RequiredPowerShellVersionForProcessAttach)
            {
                ServiceCommon.Log(string.Format("User asked to attach to process while running inadequete PowerShell version {0}", _installedPowerShellVersion.ToString()));
                return string.Format(Resources.ProcessAttachVersionErrorBody, _installedPowerShellVersion.ToString());
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
                    _pausedEvent.Reset();
                    _forceStop = false;

                    // debug the runspace, for the vast majority of cases the 1st runspace is the one to attach to
                    InvokeScript(_currentPowerShell, "Debug-Runspace -Id 1");

                    if (_currentPowerShell.HadErrors)
                    {
                        return Resources.ProcessDebugError;
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
                    return Resources.ProcessDebugError;
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
                    return Resources.ProcessDebugError;
                }
            }
            catch (Exception exception)
            {
                // any other sort of exception is not expected
                ServiceCommon.Log(string.Format("Unexpected exception while debugging runspace; {0}", exception.ToString()));
                return Resources.ProcessDebugError;
            }
            return result;
        }

        /// <summary>
        /// Detaches the HostService from a local runspace
        /// </summary>
        public bool DetachFromRunspace()
        {
            if (_callback == null)
            {
                _callback = OperationContext.Current.GetCallbackChannel<IDebugEngineCallback>();
            }

            try
            {
                // attempt to gracefully detach the deugger
                ClearBreakpoints();
                if (_runspace.RunspaceAvailability != RunspaceAvailability.Busy)
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
                    _pausedEvent.Set();
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

            // Retrieve callback context so credentials window can display
            if (_callback == null)
            {
                _callback = OperationContext.Current.GetCallbackChannel<IDebugEngineCallback>();
            }

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
                        errorMessage = Resources.LocalHostRemoteDebugError;
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
                        errorMessage = string.Format(Resources.EnumRemoteConnectionError, remoteName);
                        return null;
                    }

                    // Check remote PowerShell version
                    Version remoteVersion = InvokeScript(_currentPowerShell, "$PSVersionTable.PSVersion").ElementAt(0).BaseObject as Version;
                    if (remoteVersion != null && (remoteVersion < RequiredPowerShellVersionForProcessAttach))
                    {
                        InvokeScript(_currentPowerShell, string.Format(DebugEngineConstants.ExitRemoteSessionDefaultCommand));
                        errorMessage = string.Format(Resources.EnumRemoteVersionError, remoteVersion.ToString());
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
                errorMessage = string.Format(Resources.EnumRemoteConnectionError, remoteName);
                return null;
            }
            return validProcesses;
        }

        /// <summary>
        /// Attaches the HostService to a remote runspace already in execution
        /// </summary>
        public string AttachToRemoteRunspace(uint pid, string remoteName)
        {
            if (_callback == null)
            {
                _callback = OperationContext.Current.GetCallbackChannel<IDebugEngineCallback>();
            }

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
                        return string.Format(Resources.ConnectionError, remoteName);
                    }

                    _needToCopyRemoteScript = true;
                }

            }
            catch (Exception ex)
            {
                ServiceCommon.Log(string.Format("Error connecting to remote machine; {0}", ex.ToString()));
                return string.Format(Resources.ConnectionError, remoteName);
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
            _pausedEvent.Set();

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
                if (_runspace.RunspaceAvailability == RunspaceAvailability.Available)
                {
                    using (var pipeline = (_runspace.CreatePipeline()))
                    {
                        var command = new Command("Set-PSBreakpoint");

                        string file = bp.ScriptFullPath;
                        if (GetDebugScenario() != DebugScenario.Local && _mapLocalToRemote.ContainsKey(bp.ScriptFullPath))
                        {
                            file = _mapLocalToRemote[bp.ScriptFullPath];
                        }

                        command.Parameters.Add("Script", file);

                        command.Parameters.Add("Line", bp.Line);

                        pipeline.Commands.Add(command);

                        breakpoints = pipeline.Invoke();
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

                if (_runspace.RunspaceAvailability == RunspaceAvailability.Available)
                {
                    using (var pipeline = (_runspace.CreatePipeline()))
                    {
                        var command = new Command("Remove-PSBreakpoint");

                        string file = bp.ScriptFullPath;
                        if (GetDebugScenario() != DebugScenario.Local && _mapLocalToRemote.ContainsKey(bp.ScriptFullPath))
                        {
                            file = _mapLocalToRemote[bp.ScriptFullPath];
                        }

                        command.Parameters.Add("Id", id);

                        pipeline.Commands.Add(command);

                        pipeline.Invoke();
                    }

                    foreach (var p in _psBreakpointTable.Where(b => b.PSBreakpoint.Equals(bp)))
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

                if (_runspace.RunspaceAvailability == RunspaceAvailability.Available)
                {
                    using (var pipeline = (_runspace.CreatePipeline()))
                    {
                        string cmd = enable ? "Enable-PSBreakpoint" : "Disable-PSBreakpoint";

                        var command = new Command(cmd);

                        string file = bp.ScriptFullPath;
                        if (GetDebugScenario() != DebugScenario.Local && _mapLocalToRemote.ContainsKey(bp.ScriptFullPath))
                        {
                            file = _mapLocalToRemote[bp.ScriptFullPath];
                        }

                        command.Parameters.Add("Id", id);

                        pipeline.Commands.Add(command);

                        pipeline.Invoke();
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
                if (_runspace.RunspaceAvailability == RunspaceAvailability.Available)
                {
                    IEnumerable<PSObject> breakpoints;

                    using (var pipeline = (_runspace.CreatePipeline()))
                    {
                        var command = new Command("Get-PSBreakpoint");
                        pipeline.Commands.Add(command);
                        breakpoints = pipeline.Invoke();
                    }

                    if (!breakpoints.Any()) return;

                    using (var pipeline = (_runspace.CreatePipeline()))
                    {
                        var command = new Command("Remove-PSBreakpoint");
                        command.Parameters.Add("Breakpoint", breakpoints);
                        pipeline.Commands.Add(command);

                        pipeline.Invoke();
                    }
                }
                else if (IsDebuggerActive(_runspace.Debugger))
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
            RunspaceAvailability state = _runspace.RunspaceAvailability;
            ServiceCommon.Log("Checking runspace availability: " + state.ToString());

            return state;
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
                _pausedEvent.Reset();

                // Retrieve callback context
                if (_callback == null)
                {
                    _callback = OperationContext.Current.GetCallbackChannel<IDebugEngineCallback>();
                }

                if (_callback == null)
                {
                    ServiceCommon.Log("No instance context retrieved.");
                    return false;
                }

                bool error = false;
                if (GetDebugScenario() != DebugScenario.Local && Regex.IsMatch(commandLine, DebugEngineConstants.ExecutionCommandPattern))
                {
                    string localFile = _rgx.Match(commandLine).Value;

                    if (_mapLocalToRemote.ContainsKey(localFile))
                    {
                        commandLine = _rgx.Replace(commandLine, _mapLocalToRemote[localFile]);
                    }
                    else
                    {
                        _callback.OutputStringLine(string.Format(Resources.Error_LocalScriptInRemoteSession, localFile));

                        ServiceCommon.Log(Resources.Error_LocalScriptInRemoteSession + Environment.NewLine, localFile);

                        return false;
                    }
                }

                

                lock (ServiceCommon.RunspaceLock)
                {
                    if (_runspace.RunspaceAvailability == RunspaceAvailability.Available)
                    {
                        commandExecuted = true;

                        using (_currentPowerShell = PowerShell.Create())
                        {
                            _currentPowerShell.Runspace = _runspace;
                            _currentPowerShell.AddScript(commandLine);
                            
                            _currentPowerShell.AddCommand("out-default");
                            _currentPowerShell.Commands.Commands[0].MergeMyResults(PipelineResultTypes.Error, PipelineResultTypes.Output);

                            AppRunning = true;

                            _currentPowerShell.Invoke();

                            AppRunning = false;

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
        public Collection<Variable> GetScopedVariable()
        {
            Collection<Variable> variables = new Collection<Variable>();

            foreach (var psobj in _varaiables)
            {
                PSVariable psVar = null;
                if (GetDebugScenario() == DebugScenario.Local)
                {
                    // Local debugging variable
                    psVar = psobj.BaseObject as PSVariable;

                    if (psVar != null)
                    {
                        //if (psVar.Value is PSObject &&
                        //    !(((PSObject)psVar.Value).ImmediateBaseObject is PSCustomObject))
                        //{
                        //    psVar = new PSVariable(
                        //        (string)psVar.Name,
                        //        ((PSObject)psVar.Value).ImmediateBaseObject,
                        //        ScopedItemOptions.None);
                        //}

                        variables.Add(new Variable(psVar));
                    }
                }
                else
                {
                    // Remote debugging variable
                    dynamic dyVar = (dynamic)psobj;

                    if (dyVar.Value == null)
                    {
                        variables.Add(
                            new Variable(
                                dyVar.Name,
                                string.Empty,
                                string.Empty,
                                false,  // isEnumerale
                                false,  // isPSObject
                                false));  // isEnum
                    }
                    else
                    {
                        // Variable was wrapped into Deserialized.PSObject, which contains a deserialized representation of public properties of the corresponding remote, live objects.
                        if (dyVar.Value is PSObject)
                        {
                            // Non-primitive types
                            if (((PSObject)dyVar.Value).ImmediateBaseObject is string)
                            {
                                // BaseObject is string indicates the original object is real PSObject
                                psVar = new PSVariable(
                                    (string)dyVar.Name,
                                    (PSObject)dyVar.Value,
                                    ScopedItemOptions.None);
                            }
                            else
                            {
                                // Otherwise we should look into its BaseObject to obtain the original object
                                psVar = new PSVariable(
                                    (string)dyVar.Name,
                                    ((PSObject)dyVar.Value).ImmediateBaseObject,
                                    ScopedItemOptions.None);
                            }

                            variables.Add(new Variable(psVar));
                        }
                        else
                        {
                            // Primitive types
                            psVar = new PSVariable(
                                (string)dyVar.Name,
                                dyVar.Value.ToString(),
                                ScopedItemOptions.None);
                            variables.Add(
                                new Variable(
                                    psVar.Name,
                                    psVar.Value.ToString(),
                                    dyVar.Value.GetType().ToString(),
                                    false,  // isEnumerale
                                    false,  // isPSObject
                                    false));  // isEnum
                        }
                    }
                }

                if (psVar != null)
                {
                    PSVariable existingVar = _localVariables.FirstOrDefault(v => v.Name == psVar.Name);
                    if (existingVar != null)
                    {
                        _localVariables.Remove(existingVar);
                    }

                    _localVariables.Add(psVar);
                }
            }

            return variables;
        }


        /// <summary>
        /// Expand IEnumerable to retrieve all elements
        /// </summary>
        /// <param name="varName">IEnumerable object name</param>
        /// <returns>Collection of variable to client</returns>
        public Collection<Variable> GetExpandedIEnumerableVariable(string varName)
        {
            ServiceCommon.Log("Client tries to watch an IEnumerable variable, dump its content ...");

            Collection<Variable> expandedVariable = new Collection<Variable>();

            object psVariable = RetrieveVariable(varName);

            if (psVariable != null && psVariable is IEnumerable)
            {
                int i = 0;
                foreach (var item in (IEnumerable)psVariable)
                {
                    object obj = item;
                    var psObj = obj as PSObject;
                    if (psObj != null && GetDebugScenario() != DebugScenario.Local && !(psObj.ImmediateBaseObject is string))
                    {
                        obj = psObj.ImmediateBaseObject;
                    }

                    expandedVariable.Add(
                        new Variable(
                            String.Format("[{0}]", i),
                            obj.ToString(),
                            obj.GetType().ToString(),
                            obj is IEnumerable,
                            obj is PSObject,
                            obj is Enum));

                    if (!obj.GetType().IsPrimitive)
                    {
                        string key = string.Format("{0}\\{1}", varName, String.Format("[{0}]", i));
                        _propVariables[key] = obj;
                    }

                    i++;
                }
            }

            return expandedVariable;
        }

        /// <summary>
        /// Expand object to retrieve all properties
        /// </summary>
        /// <param name="varName">Object name</param>
        /// <returns>Collection of variable to client</returns>
        public Collection<Variable> GetObjectVariable(string varName)
        {
            ServiceCommon.Log("Client tries to watch an object variable, dump its content ...");

            Collection<Variable> expandedVariable = new Collection<Variable>();

            object psVariable = RetrieveVariable(varName);

            if (psVariable != null && !(psVariable is IEnumerable) && !(psVariable is PSObject))
            {
                

                var props = psVariable.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

                foreach (var propertyInfo in props)
                {
                    try
                    {
                        object val = propertyInfo.GetValue(psVariable, null);
                        if (val != null)
                        {
                            expandedVariable.Add(
                                new Variable(
                                    propertyInfo.Name,
                                    val.ToString(),
                                    val.GetType().ToString(),
                                    val is IEnumerable,
                                    val is PSObject,
                                    val is Enum));

                            if (!val.GetType().IsPrimitive)
                            {
                                string key = string.Format("{0}\\{1}", varName, propertyInfo.Name);
                                _propVariables[key] = val;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        ServiceCommon.Log("Property infomation is not able to be retrieved through reflection due to exception: {0} {2} InnerException: {1}", ex.Message, ex.InnerException, Environment.NewLine);
                    }
                }
            }

            return expandedVariable;
        }

        /// <summary>
        /// Expand PSObject to retrieve all its properties
        /// </summary>
        /// <param name="varName">PSObject name</param>
        /// <returns>Collection of variable to client</returns>
        public Collection<Variable> GetPSObjectVariable(string varName)
        {
            ServiceCommon.Log("Client tries to watch an PSObject variable, dump its content ...");

            Collection<Variable> propsVariable = new Collection<Variable>();

            object psVariable = RetrieveVariable(varName);

            if (psVariable != null && psVariable is PSObject)
            {
                foreach (var prop in ((PSObject)psVariable).Properties)
                {
                    if (propsVariable.Any(m => m.VarName == prop.Name))
                    {
                        continue;
                    }

                    object val;
                    try
                    {
                        Runspace.DefaultRunspace = _runspace;
                        val = prop.Value;
                        var psObj = val as PSObject;
                        if (psObj != null && GetDebugScenario() != DebugScenario.Local && !(psObj.ImmediateBaseObject is string))
                        {
                            val = psObj.ImmediateBaseObject;
                        }
                    }
                    catch
                    {
                        val = "Failed to evaluate value.";
                    }

                    propsVariable.Add(
                        new Variable(
                            prop.Name,
                            val == null ? String.Empty : val.ToString(),
                            val == null ? prop.TypeNameOfValue : val.GetType().ToString(),
                            val is IEnumerable,
                            val is PSObject,
                            val is Enum));

                    if (val != null  && !val.GetType().IsPrimitive)
                    {
                        string key = string.Format("{0}\\{1}", varName, prop.Name);
                        _propVariables[key] = val;
                    }
                }
            }

            return propsVariable;
        }

        /// <summary>
        /// Respond client request for callstack frames of current execution context
        /// </summary>
        /// <returns>Collection of callstack to client</returns>
        public IEnumerable<CallStack> GetCallStack()
        {
            ServiceCommon.Log("Obtaining the callstack");
            List<CallStack> callStackFrames = new List<CallStack>();
            DebugScenario scenario = GetDebugScenario();

            foreach (var psobj in _callstack)
            {
                if (scenario == DebugScenario.Local)
                {
                    // standard debugging scenario
                    var frame = psobj.BaseObject as CallStackFrame;
                    if (frame != null)
                    {
                        callStackFrames.Add(
                            new CallStack(
                                frame.ScriptName,
                                frame.FunctionName,
                                frame.Position.StartLineNumber,
                                frame.Position.EndLineNumber,
                                frame.Position.StartColumnNumber,
                                frame.Position.EndColumnNumber));
                    }
                }
                else if (scenario == DebugScenario.RemoteSession)
                {
                    // remote session debugging
                    dynamic psFrame = (dynamic)psobj;

                    callStackFrames.Add(
                        new CallStack(
                            psFrame.ScriptName == null ? string.Empty : _mapRemoteToLocal[(string)psFrame.ScriptName.ToString()],
                            (string)psFrame.FunctionName.ToString(),
                            (int)psFrame.ScriptLineNumber));
                }
                else if (scenario == DebugScenario.RemoteAttach || scenario == DebugScenario.LocalAttach)
                {
                    // local and remote process attach debugging
                    string currentCall = psobj.ToString();
                    Match match = validStackLine.Match(currentCall);
                    if (match.Success)
                    {
                        String funcall = match.Groups[1].Value;
                        String script = match.Groups[3].Value;
                        int lineNum = int.Parse(match.Groups[4].Value);

                        callStackFrames.Add(new CallStack(script, funcall, lineNum));
                    }
                }

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
                _currentPowerShell.Runspace = _runspace;
                _currentPowerShell.AddCommand("prompt");

                string prompt = _currentPowerShell.Invoke<string>().FirstOrDefault();
                if (GetDebugScenario() != DebugScenario.Local)
                {
                    prompt = string.Format("[{0}] {1}", _runspace.ConnectionInfo.ComputerName, prompt);
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
            lock (_executeDebugCommandLock)
            {
                ServiceCommon.Log("Client asks for resuming debugger");
                _resumeAction = resumeAction;
                _pausedEvent.Set();
            }
        }

        /// <summary>
        /// Apply the raw host options on powershell host
        /// </summary>
        /// <param name="options">PS raw UI host options</param>
        public void SetOption(PowerShellRawHostOptions options)
        {
            _rawHostOptions = options;
        }

        /// <summary>
        /// Returns the connection info for the current runspace based on certain characteristics of the runspace.
        /// Note: if you overwrite the _currentPowerShell object after attaching to a process, this will no longer
        /// return RemoteAttach.
        /// </summary>
        public DebugScenario GetDebugScenario()
        {
            if (_runspace.ConnectionInfo == null)
            {
                return DebugScenario.Local;
            }
            else if (_runspace.ConnectionInfo is WSManConnectionInfo)
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
            else if (_runspace.ConnectionInfo != null && !(_runspace.ConnectionInfo is WSManConnectionInfo))
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
            PSObject profiles = _runspace.SessionStateProxy.PSVariable.Get("profile").Value as PSObject;

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
    }
}
