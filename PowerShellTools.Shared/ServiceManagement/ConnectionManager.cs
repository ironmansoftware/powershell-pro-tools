using System;
using System.Diagnostics;
using Common.ServiceManagement;
using Microsoft.VisualStudio.Shell;
using PowerShellTools.Common.Logging;
using PowerShellTools.Common.ServiceManagement.DebuggingContract;
using PowerShellTools.Common.ServiceManagement.ExplorerContract;
using PowerShellTools.Common.ServiceManagement.IntelliSenseContract;
using PowerShellTools.DebugEngine;
using PowerShellTools.Intellisense;
using PowerShellTools.Options;
using PowerShellTools.HostService.ServiceManagement.Debugging;
using System.Management.Automation.Runspaces;

namespace PowerShellTools.ServiceManagement
{


    /// <summary>
    /// Manage the process and channel creation.
    /// </summary>
    public sealed class ConnectionManager 
    {
        private IPowerShellIntelliSenseService _powerShellIntelliSenseService;
        private IPowerShellDebuggingService _powerShellDebuggingService;
        private IPowerShellExplorerService _powerShellExplorerService;
        private IAnalysisService _analysisService;
        
        private object _syncObject = new object();
        private Process _process;
        private static readonly ILog Log = LogManager.GetLogger(typeof(PowerShellToolsPackage));

        /// <summary>
        /// Event is fired when the connection exception happened.
        /// </summary>
        public event EventHandler ConnectionException;

        public ConnectionManager()
        {
            GeneralOptions.Instance.PowerShellVersionChanged += Instance_PowerShellVersionChanged;

            OpenClientConnection();
        }

        /// <summary>
        /// The IntelliSense service channel.
        /// </summary>
        public IPowerShellIntelliSenseService PowerShellIntelliSenseSerivce
        {
            get
            {
                if (_powerShellIntelliSenseService == null)
                {
                    OpenClientConnection();
                }

                return _powerShellIntelliSenseService;
            }
        }

        /// <summary>
        /// The debugging service channel.
        /// </summary>
        public IPowerShellDebuggingService PowerShellDebuggingService
        {
            get
            {
                if (_powerShellDebuggingService == null)
                {
                    OpenClientConnection();
                }
                return _powerShellDebuggingService;
            }
        }

        public IPowerShellExplorerService PowerShellExplorerService
        {
            get
            {
                if (_powerShellExplorerService == null)
                {
                    OpenClientConnection();
                }
                return _powerShellExplorerService;
            }
        }

        public IAnalysisService AnalysisService
        {
            get
            {
                if (_analysisService == null)
                {
                    OpenClientConnection();
                }
                return _analysisService;
            }
        }

        /// <summary>
        /// PowerShell host process
        /// </summary>
        public PowerShellHostProcess HostProcess { get; private set; }

        public void OpenClientConnection()
        {
            lock (_syncObject)
            {
                if (_powerShellIntelliSenseService == null || _powerShellDebuggingService == null || _powerShellExplorerService == null)
                {
                    EnsureCloseProcess();

                    var version = GeneralOptions.Instance.PowerShellVersion;

                    try
                    {
                        HostProcess = PowershellHostProcessHelper.CreatePowerShellHostProcess(version);
                        if (HostProcess != null)
                        {
                            _process = HostProcess.Process;
                            _process.Exited += ConnectionExceptionHandler;
                            _process.OutputDataReceived += _process_OutputDataReceived;
                            _process.ErrorDataReceived += _process_ErrorDataReceived;
                        }

                        _powerShellDebuggingService = new PowerShellDebuggingService(new DebugServiceEventsHandlerProxy(), _process == null ? -1 : _process.Id);
                        _powerShellIntelliSenseService = new HostService.ServiceManagement.PowerShellIntelliSenseService();
                        _powerShellExplorerService = new HostService.ServiceManagement.PowerShellExplorerService();
                        _analysisService = new HostService.Services.AnalysisService();
                    }
                    catch (Exception ex)
                    {
                        // Connection has to be established...
                        Log.Error("Connection establish failed...", ex);
                        EnsureCloseProcess();

                        _powerShellIntelliSenseService = null;
                        _powerShellDebuggingService = null;
                        _powerShellExplorerService = null;
                        throw;
                    }
                }
            }
        }

        private void _process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data != null)
            {
                ThreadHelper.JoinableTaskFactory.Run(async () =>
                {
                    await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                    PowerShellToolsPackage.Debugger.HostUi.VsOutputString(e.Data + Environment.NewLine);
                });
            }
        }

        private void _process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data != null)
            {
                ThreadHelper.JoinableTaskFactory.Run(async () =>
                {
                    await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                    try
                    {
                        PowerShellToolsPackage.Debugger.HostUi.VsOutputString(e.Data + Environment.NewLine);
                    } catch { }
                });
            }
        }

        private void Instance_PowerShellVersionChanged(object sender, EventArgs e)
        {
            _powerShellIntelliSenseService = null;
            _powerShellDebuggingService = null;
            _powerShellExplorerService = null;

            EnsureCloseProcess();
            OpenClientConnection();
        }

        public void ProcessEventHandler(BitnessOptions bitness)
        {
            Log.DebugFormat("Bitness had been changed to {1}", bitness);
            EnsureCloseProcess();
        }

        public void EnsureCloseProcess()
        {
            if (_process != null)
            {
                try
                {
                    _process.Kill();
                    _process = null;
                }
                catch (Exception ex)
                {
                    Log.ErrorFormat("Error when closing process.  Message: {0}", ex.Message);
                }
            }
        }

        private void ConnectionExceptionHandler(object sender, EventArgs e)
        {
            PowerShellToolsPackage.DebuggerReadyEvent.Reset();

            if (ConnectionException != null)
            {
                ConnectionException(this, EventArgs.Empty);
            }
        }
    }
}
