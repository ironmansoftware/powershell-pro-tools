using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Host;
using System.Management.Automation.Runspaces;
using System.Reflection;
using System.ServiceModel;
using System.Text;
using System.Threading;
using EnvDTE80;
using Microsoft.PowerShell;
using PowerShellTools.Common.Debugging;
using PowerShellTools.Common.ServiceManagement.DebuggingContract;

namespace PowerShellTools.HostService.ServiceManagement.Debugging
{
    /// <summary>
    /// Utility functions for powershell debugging service
    /// </summary>
    public partial class PowerShellDebuggingService
    {
        // Potential TODO: Refactor this class into either a static Utilities class

        private const string DteVariableName = "dte";
        private const string ProfileVariableName = "profile";

        private void SetRunspace(Runspace runspace)
        {
            if (_runspace != null)
            {
                UnloadRunspace(_runspace);
            }

            _runspace = runspace;
            LoadRunspace(_runspace);

            ProvideDteVariable(_runspace);
        }

        private void LoadRunspace(Runspace runspace)
        {
            if (_runspace != null)
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
            if (_runspace != null)
            {
                if (GetDebugScenario() == DebugScenario.Local || _installedPowerShellVersion >= RequiredPowerShellVersionForRemoteSessionDebugging)
                {
                    runspace.Debugger.DebuggerStop -= Debugger_DebuggerStop;
                    runspace.Debugger.BreakpointUpdated -= Debugger_BreakpointUpdated;
                }
            }

            runspace.StateChanged -= _runspace_StateChanged;
        }

        private void RefreshScopedVariable()
        {
            ServiceCommon.Log("Debuggger stopped, let us retreive all local variable in scope");
            using (var pipeline = (_runspace.CreateNestedPipeline()))
            {
                var command = new Command("Get-Variable");
                pipeline.Commands.Add(command);
                _varaiables = pipeline.Invoke();
            }
        }

        private void RefreshCallStack()
        {
            ServiceCommon.Log("Debuggger stopped, let us retreive all call stack frames");
            using (var pipeline = (_runspace.CreateNestedPipeline()))
            {
                var command = new Command("Get-PSCallstack");
                pipeline.Commands.Add(command);
                _callstack = pipeline.Invoke();
            }
        }

        private void OnTerminatingException(Exception ex)
        {
            ServiceCommon.Log("OnTerminatingException");
            UnloadRunspace(_runspace);
            if (_callback != null)
            {
                _callback.TerminatingException(new PowerShellRunTerminatingException(ex));
            }
        }

        private void DebuggerFinished()
        {
            ServiceCommon.Log("DebuggerFinished");

            ClearBreakpoints();
            _psBreakpointTable.Clear();

            if (_callback != null)
            {
                _callback.OutputStringLine(string.Empty);
                _callback.RefreshPrompt();
            }

            if (_runspace != null)
            {
                UnloadRunspace(_runspace);
            }

            if (_currentPowerShell != null)
            {
                _currentPowerShell.Stop();
                _currentPowerShell = null;
            }

            ReleaseWaitHandler();

            _debuggingCommand = string.Empty;
            _localVariables.Clear();
            _propVariables.Clear();

            if (_callback != null)
            {
                _callback.DebuggerFinished();
            }
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

        private void InitializeRunspace(PSHost psHost)
        {
            ServiceCommon.Log("Initializing run space with debugger");
            InitialSessionState iss = InitialSessionState.CreateDefault();

            var assemblyBasePath = Path.GetDirectoryName(GetType().GetTypeInfo().Assembly.Location);
            iss.ImportPSModulesFromPath(Path.Combine(assemblyBasePath, "Modules"));

            iss.ApartmentState = ApartmentState.STA;
            iss.ThreadOptions = PSThreadOptions.ReuseThread;

            _runspace = RunspaceFactory.CreateRunspace(psHost, iss);
            _runspace.Open();

            ProvideProfileVariable();
            ServiceCommon.Log("Done initializing runspace");
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
                ps.Runspace = _runspace;

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

        private object RetrieveVariable(string varName)
        {
            var psVar = _localVariables.FirstOrDefault(v => v.Name == varName);
            object psVariable = (psVar == null) ? null : psVar.Value;

            if (psVariable == null && _propVariables.ContainsKey(varName))
            {
                psVariable = _propVariables[varName];
            }

            return psVariable;
        }

        private void ReleaseWaitHandler()
        {
            _resumeAction = DebugEngineConstants.Debugger_Stop;
            _pausedEvent.Set();
        }

        /// <summary>
        /// Provides the $dte Variable if we are in a local runspace and if the $dte variable has not yet been set.
        /// </summary>
        private static void ProvideDteVariable(Runspace runspace)
        {
            // only do this when we are working with a local runspace
            if (runspace.ConnectionInfo == null)
            {
                // Preset dte as PS variable if not yet
                if (runspace.SessionStateProxy.PSVariable.Get(DteVariableName) == null)
                {
                    ServiceCommon.Log("Providing $dte variable to the local runspace.");

                    DTE2 dte = DTEManager.GetDTE(App.VsProcessId);

                    if (dte != null)
                    {
                        // We want to make $dte constant so that it can't be overridden; similar to the $psISE analog

                        PSVariable dteVar = new PSVariable(DteVariableName, dte, ScopedItemOptions.Constant);
                        runspace.SessionStateProxy.PSVariable.Set(dteVar);
                    }
                    else
                    {
                        ServiceCommon.Log("Dte object not found.");
                    }
                }
            }
        }

        private static void ProvideProfileVariable()
        {
            if (_runspace.ConnectionInfo == null)
            {
                // Provide profile as PS variable if not yet defined
                if (_runspace.SessionStateProxy.PSVariable.Get(ProfileVariableName) == null)
                {
                    ServiceCommon.Log("Providing $profile variable to the local runspace.");

                    string psHome = (string)_runspace.SessionStateProxy.PSVariable.Get("PsHome").Value;
                    string windowsPowerShell = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "WindowsPowerShell");

                    PSObject profile = new PSObject(Path.Combine(windowsPowerShell, DebugEngineConstants.PowerShellToolsProfileFileName));

                    // 1. All Users, All Hosts, 2. All Users, Current Host, 3. Current User All Hosts, 4. Current User Current Host
                    profile.Members.Add(new PSNoteProperty(DebugEngineConstants.PowerShellProfiles[0][0], Path.Combine(psHome, DebugEngineConstants.PowerShellProfiles[0][1])));
                    profile.Members.Add(new PSNoteProperty(DebugEngineConstants.PowerShellProfiles[1][0], Path.Combine(psHome, DebugEngineConstants.PowerShellProfiles[1][1])));
                    profile.Members.Add(new PSNoteProperty(DebugEngineConstants.PowerShellProfiles[2][0], Path.Combine(windowsPowerShell, DebugEngineConstants.PowerShellProfiles[2][1])));
                    profile.Members.Add(new PSNoteProperty(DebugEngineConstants.PowerShellProfiles[3][0], Path.Combine(windowsPowerShell, DebugEngineConstants.PowerShellProfiles[3][1])));

                    PSVariable profileVar = new PSVariable(ProfileVariableName, profile, ScopedItemOptions.Constant);
                    _runspace.SessionStateProxy.PSVariable.Set(profileVar);
                }
            }
        }

        private string ExecuteDebuggingCommand(string debuggingCommand, bool output)
        {
            // Need to be thread-safe here, to ensure every debugging command get processed.
            // e.g: Set/Enable/Disable/Remove breakpoint during debugging 
            lock (_executeDebugCommandLock)
            {
                ServiceCommon.Log("Client asks for executing debugging command");
                _debugOutput = output;
                _debugCommandOutput = string.Empty;
                _debuggingCommand = debuggingCommand;
                _debugCommandEvent.Reset();
                _pausedEvent.Set();
                _debugCommandEvent.WaitOne();
                _debugOutput = true;
                _debuggingCommand = string.Empty;
                return _debugCommandOutput;
            }
        }

        private void objects_DataAdded(object sender, DataAddedEventArgs e)
        {
            var list = sender as PSDataCollection<PSObject>;
            StringBuilder outputString = new StringBuilder();
            foreach (PSObject obj in list)
            {
                outputString.AppendLine(obj.ToString());
            }

            if (_debugOutput)
            {
                NotifyOutputString(outputString.ToString());
            }
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
            _runspace.Debugger.ProcessCommand(psCommand, result);

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
            _runspace.Debugger.DebuggerStop += Debugger_DebuggerStop;
            _runspace.Debugger.BreakpointUpdated += Debugger_BreakpointUpdated;
            _runspace.StateChanged += _runspace_StateChanged;
            _runspace.AvailabilityChanged += _runspace_AvailabilityChanged;
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
                    powerShell.Runspace = _runspace;
                    powerShell.AddScript(script);
                    return powerShell.Invoke();
                }
            }

            powerShell.Commands.Clear();
            powerShell.Runspace = _runspace;
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

            powerShell.Runspace = _runspace;
            powerShell.Commands.Clear();
            powerShell.Commands = enterSession;
            powerShell.Invoke();
        }
    }
}
