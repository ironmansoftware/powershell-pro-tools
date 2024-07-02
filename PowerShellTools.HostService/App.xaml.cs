using PowerShellTools.Common;
using PowerShellTools.Common.Logging;
using PowerShellTools.Common.ServiceManagement.DebuggingContract;
using PowerShellTools.Common.ServiceManagement.ExplorerContract;
using PowerShellTools.Common.ServiceManagement.IntelliSenseContract;
using PowerShellTools.HostService.ServiceManagement;
using PowerShellTools.HostService.ServiceManagement.Debugging;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Net.Security;
using System.ServiceModel;
using System.Threading;
using System.Windows;

namespace PowerShellTools.HostService
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static ServiceHost _powerShellServiceHost;
        private static ServiceHost _powerShellDebuggingServiceHost;
        private static ServiceHost _powerShellExplorerServiceHost;
        private static readonly ILog Log = LogManager.GetLogger(typeof(App));

        public static int VsProcessId { get; private set; }

        public static string EndpointGuid { get; private set; }

        void App_Startup(object sender, StartupEventArgs e)
        {
            ArgumentException argException = new ArgumentException();

            Log.DebugFormat("Host process started. Arguments: {0}", string.Join(" ", e.Args));
            // Application is running
            // Process command line e.Args
            if (e.Args.Length != 3 ||
                !(e.Args[0].StartsWith(Constants.UniqueEndpointArg, StringComparison.OrdinalIgnoreCase)
                && e.Args[1].StartsWith(Constants.VsProcessIdArg, StringComparison.OrdinalIgnoreCase)
                && e.Args[2].StartsWith(Constants.ReadyEventUniqueNameArg, StringComparison.OrdinalIgnoreCase)))
            {
                Log.Error("Invalid args");
                // Exit the current process because of the invalid arg(s). The HResult value should be 0x80070057 (E_INVALIDARG) which is well-known.
                Environment.Exit(argException.HResult);
            }

            EndpointGuid = e.Args[0].Remove(0, Constants.UniqueEndpointArg.Length);
            if (EndpointGuid.Length != Guid.Empty.ToString().Length)
            {
                Log.Error("Invalid EndpointGUID");
                Environment.Exit(argException.HResult);
            }

            int vsProcessId;
            if (!int.TryParse(e.Args[1].Remove(0, Constants.VsProcessIdArg.Length),
                            NumberStyles.None,
                            CultureInfo.InvariantCulture,
                            out vsProcessId))
            {
                Log.Error("Invalid vsProcessId");
                Environment.Exit(argException.HResult);
            }

            VsProcessId = vsProcessId;

            string readyEventName = e.Args[2].Remove(0, Constants.ReadyEventUniqueNameArg.Length);
            // the readyEventName should be VsPowershellToolProcess:TheGeneratedGuid
            if (readyEventName.Length != (Constants.ReadyEventPrefix.Length + Guid.Empty.ToString().Length))
            {
                Log.Error("Invalid readyEventName");
                Environment.Exit(argException.HResult);
            }

            // Step 1: Create the NetNamedPipeBinding. 
            // Note: the setup of the binding should be same as the client side, otherwise, the connection won't get established
            Uri baseAddress = new Uri(Constants.ProcessManagerHostUri + EndpointGuid);
            NetNamedPipeBinding binding = new NetNamedPipeBinding(NetNamedPipeSecurityMode.None);
            binding.ReceiveTimeout = TimeSpan.MaxValue;
            binding.SendTimeout = TimeSpan.MaxValue;
            binding.Security.Transport.ProtectionLevel = ProtectionLevel.None;
            binding.MaxReceivedMessageSize = Constants.BindingMaxReceivedMessageSize;

            // Step 2: Create the service host.
            try
            {
                CreatePowerShellIntelliSenseServiceHost(baseAddress, binding);
                CreatePowerShellDebuggingServiceHost(baseAddress, binding);
                CreatePowerShellExplorerServiceHost(baseAddress, binding);
            }
            catch (Exception ex)
            {
                Log.Error("Failed to create service host.", ex);
                Environment.Exit(ex.HResult);
            }

            // Step 3: Signal parent process that host is ready so that it can proceed.
            EventWaitHandle readyEvent = new EventWaitHandle(false, EventResetMode.ManualReset, readyEventName);
            readyEvent.Set();
            readyEvent.Close();

            try
            {
                // get parent process (The VS process)
                Process p = Process.GetProcessById(vsProcessId);

                if (p != null)
                {
                    p.EnableRaisingEvents = true;
                    // Make sure the host process terminates when VS exits.
                    p.Exited += new EventHandler(
                        (s, eventArgs) =>
                        {
                            if (_powerShellServiceHost != null)
                            {
                                _powerShellServiceHost.Close();
                                _powerShellServiceHost = null;
                            }
                            if (_powerShellDebuggingServiceHost != null)
                            {
                                _powerShellDebuggingServiceHost.Close();
                                _powerShellDebuggingServiceHost = null;
                            }
                            if (_powerShellExplorerServiceHost != null)
                            {
                                _powerShellExplorerServiceHost.Close();
                                _powerShellExplorerServiceHost = null;
                            }

                            Log.Info("Visual Studio exited. Exiting...");
                            Environment.Exit(0);
                        });
                }
            }
            catch (Exception ex)
            {
                Log.Error("Failed to get parent process.", ex);
            }
        }

        private static void CreatePowerShellIntelliSenseServiceHost(Uri baseAddress, NetNamedPipeBinding binding)
        {
            try
            {
                _powerShellServiceHost = new ServiceHost(typeof(PowerShellIntelliSenseService), baseAddress);

                _powerShellServiceHost.AddServiceEndpoint(typeof(IPowerShellIntelliSenseService),
                                                          binding,
                                                          Constants.IntelliSenseHostRelativeUri);

                _powerShellServiceHost.Open();
            }
            catch (Exception exception)
            {
                Log.Error("Failed to open IntelliSense service host.", exception);
                throw;
            }
        }

        private static void CreatePowerShellDebuggingServiceHost(Uri baseAddress, NetNamedPipeBinding binding)
        {
            try
            {
                _powerShellDebuggingServiceHost = new ServiceHost(typeof(PowerShellDebuggingService), baseAddress);

                _powerShellDebuggingServiceHost.AddServiceEndpoint(typeof(IPowerShellDebuggingService),
                    binding,
                    Constants.DebuggingHostRelativeUri);

                _powerShellDebuggingServiceHost.Open();
            }
            catch (Exception exception)
            {
                Log.Error("Failed to open Debugging service host.", exception);
                throw;
            }
        }

        private static void CreatePowerShellExplorerServiceHost(Uri baseAddress, NetNamedPipeBinding binding)
        {
            try
            {
                _powerShellExplorerServiceHost = new ServiceHost(typeof(PowerShellExplorerService), baseAddress);

                _powerShellExplorerServiceHost.AddServiceEndpoint(typeof(IPowerShellExplorerService),
                    binding,
                    Constants.ExplorerHostRelativeUri);

                _powerShellExplorerServiceHost.Open();
            }
            catch (Exception exception)
            {
                Log.Error("Failed to open Explorer service host.", exception);
                throw;
            }
        }
    }
}
