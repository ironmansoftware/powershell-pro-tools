using PowerShellTools.Common.Debugging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Host;
using System.Management.Automation.Runspaces;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Diagnostics;
using PowerShellTools.Common;
using System.Runtime.InteropServices;
using PowerShellTools.Common.ServiceManagement.DebuggingContract;

namespace PowerShellTools.HostService.ServiceManagement.Debugging
{
    public partial class PowerShellDebuggingService : PSHost, IHostSupportsInteractiveSession
    {
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
                        Task.Run(() =>
                        {
                            MonitorUserInputRequest();
                        });
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

            if (_callback != null)
            {
                _callback.SetRemoteRunspace(false);
            }

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

            if (_installedPowerShellVersion < RequiredPowerShellVersionForRemoteSessionDebugging
                && _callback != null)
            {
                _callback.OutputStringLine(string.Format(Resources.Warning_HigherVersionRequiredForDebugging, Constants.PowerShellInstallFWLink));
            }
            else
            {
                if (Runspace.Debugger != null)
                {
                    SetRemoteScriptDebugMode40(Runspace);
                }
                else
                {
                    _callback.OutputStringLine(string.Format(Resources.Warning_HigherVersionOnTargetRequiredForDebugging, Constants.PowerShellInstallFWLink));
                }
            }

            if (_callback != null)
            {
                _callback.SetRemoteRunspace(true);
            }

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

                if (ret != 0 && _callback != null)
                {
                    // Tactic Fix (TODO: github issue https://github.com/Microsoft/poshtools/issues/479)
                    // Give a bit of time for case where app crashed on readline/readkey
                    // We dont want to put any dirty content into stdin stream buffer
                    // Which can only be flushed out till the next readline/readkey
                    System.Threading.Thread.Sleep(50);

                    if (_appRunning)
                    {
                        _callback.RequestUserInputOnStdIn();
                    }
                    else
                    {
                        break;
                    }
                }

                System.Threading.Thread.Sleep(50);
            }
        }

        #endregion
    }
}
