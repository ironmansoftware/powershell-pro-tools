using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using Microsoft;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Utilities;
using Microsoft.VisualStudioTools;
using PowerShellTools.Classification;
using PowerShellTools.Commands;
using PowerShellTools.Common.ServiceManagement.DebuggingContract;
using PowerShellTools.Common.ServiceManagement.IntelliSenseContract;
using PowerShellTools.Contracts;
using PowerShellTools.DebugEngine;
using PowerShellTools.Intellisense;
using PowerShellTools.LanguageService;
using PowerShellTools.Options;
using PowerShellTools.Project.PropertyPages;
using PowerShellTools.Service;
using PowerShellTools.ServiceManagement;
using Engine = PowerShellTools.DebugEngine.Engine;
using MessageBox = System.Windows.MessageBox;
using Threading = System.Threading.Tasks;
using PowerShellTools.Common.Logging;
using PowerShellTools.DebugEngine.Remote;
using PowerShellTools.Explorer;
using PowerShellTools.Common.ServiceManagement.ExplorerContract;
using Microsoft.VisualStudio.ComponentModelHost;
using PowerShellTools.Diagnostics;
using Common.ServiceManagement;
using System.IO;
using System.Windows.Media;
using PowerShellProTools.Host;
using PowerShellTools.ToolWindows;
using PowerShellTools.Shared.ToolWindows;
using PowerShellToolsPro.Commands;
using PowerShellToolsPro;
using System.ComponentModel.Composition;
using ModernWpf;
using Microsoft.VisualStudio.PlatformUI;

namespace PowerShellTools
{
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    ///
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the
    /// IVsPackage interface and uses the registration attributes defined in the framework to
    /// register itself and its components with the shell.
    /// </summary>
    // This attribute tells the PkgDef creation utility (CreatePkgDef.exe) that this class is
    // a package.
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    // This attribute is used to register the information needed to show this package
    // in the Help/About dialog of Visual Studio.
    //[InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]

    // There are a few user scenarios which will trigger package to load
    // 1. Open/Create any type of PowerShell project
    // 2. Open/Create PowerShell file(.ps1, .psm1, .psd1) from file->open/create or solution explorer
    // 3. Execute PowerShell script file from solution explorer
    [ProvideAutoLoad(PowerShellTools.Common.Constants.PowerShellProjectUiContextString, PackageAutoLoadFlags.BackgroundLoad)]
    // 4. PowerShell interactive window open
    [ProvideAutoLoad(PowerShellTools.Common.Constants.PowerShellReplCreationUiContextString, PackageAutoLoadFlags.BackgroundLoad)]
    [ProvideAutoLoad(PowerShellTools.Common.Constants.PowerShellDebuggingUiContextString, PackageAutoLoadFlags.BackgroundLoad)]
    // 5. PowerShell service execution
    [ProvideService(typeof(IPowerShellService))]
    [ProvideService(typeof(IPowerShellHostClientService))]
    [ProvideLanguageService(typeof(PowerShellLanguageInfo),
                            PowerShellConstants.LanguageName,
                            101,
                            ShowSmartIndent = true,
                            ShowDropDownOptions = true,
                            EnableCommenting = true,
                            EnableLineNumbers = true,
                            RequestStockColors = true)]
    [ProvideEditorFactory(typeof(PowerShellEditorFactory), 114, TrustLevel = __VSEDITORTRUSTLEVEL.ETL_AlwaysTrusted)]
    [ProvideBraceCompletion(PowerShellConstants.LanguageName)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideKeyBindingTable(GuidList.guidCustomEditorEditorFactoryString, 102)]
    [Guid(GuidList.PowerShellToolsPackageGuid)]
    [ProvideObject(typeof(AdvancedPropertyPage))]
    [ProvideObject(typeof(ModuleManifestPropertyPage))]
    [ProvideObject(typeof(GeneralPropertyPage))]
    [ProvideObject(typeof(DebugPropertyPage))]
    [ProvideObject(typeof(BuildEventPropertyPage))]
    [ProvideToolWindow(typeof(Variables))]
    [ProvideToolWindow(typeof(Modules))]
    [ProvideToolWindow(
        typeof(PSCommandExplorerWindow),
        Style = Microsoft.VisualStudio.Shell.VsDockStyle.Tabbed,
        Window = "dd9b7693-1385-46a9-a054-06566904f861")]
    [Microsoft.VisualStudio.Shell.ProvideDebugEngine("{43ACAB74-8226-4920-B489-BFCF05372437}", "PowerShell",
        PortSupplier = "{708C1ECA-FF48-11D2-904F-00C04FA302A1}",
        ProgramProvider = "{08F3B557-C153-4F6C-8745-227439E55E79}", Attach = true,
        CLSID = "{C7F9F131-53AB-4FD0-8517-E54D124EA392}")]
    [Clsid(Clsid = "{C7F9F131-53AB-4FD0-8517-E54D124EA392}",
           Assembly = "PowerGuiVsx.Core.DebugEngine",
        Class = "PowerGuiVsx.Core.DebugEngine.Engine")]
    [Clsid(Clsid = "{08F3B557-C153-4F6C-8745-227439E55E79}",
           Assembly = "PowerGuiVsx.Core.DebugEngine",
        Class = "PowerGuiVsx.Core.DebugEngine.ScriptProgramProvider")]
    [Microsoft.VisualStudioTools.ProvideDebugEngine("PowerShell",
                                                    typeof(ScriptProgramProvider),
                                                    typeof(Engine),
        "{43ACAB74-8226-4920-B489-BFCF05372437}")]
    [ProvideIncompatibleEngineInfo("{92EF0900-2251-11D2-B72E-0000F87572EF}")]
    [ProvideIncompatibleEngineInfo("{F200A7E7-DEA5-11D0-B854-00A0244A1DE2}")]
    [ProvideOptionPage(typeof(DialogPageProvider.General), PowerShellConstants.LanguageDisplayName, "General", 101, 106, true)]
    [ProvideOptionPage(typeof(DialogPageProvider.Diagnostics), PowerShellConstants.LanguageDisplayName, "Diagnostics", 101, 106, true)]
    [ProvideDiffSupportedContentType(".ps1;.psm1;.psd1", ";")]
    [ProvideLanguageExtension(typeof(PowerShellLanguageInfo), ".ps1")]
    [ProvideLanguageExtension(typeof(PowerShellLanguageInfo), ".psm1")]
    [ProvideLanguageExtension(typeof(PowerShellLanguageInfo), ".psd1")]
    [ProvideCodeExpansions(GuidList.PowerShellLanguage, false, 106, "PowerShell", @"Snippets\SnippetsIndex.xml", @"Snippets\PowerShell\")]
    [ProvideDebugPortSupplier("Powershell Remote Debugging (SSL Required)", typeof(RemoteDebugPortSupplier), PowerShellTools.Common.Constants.PortSupplierId, typeof(RemotePortPicker))]
    [ProvideDebugPortSupplier("Powershell Remote Debugging", typeof(RemoteUnsecuredDebugPortSupplier), PowerShellTools.Common.Constants.UnsecuredPortSupplierId, typeof(RemoteUnsecuredPortPicker))]
    [ProvideDebugPortPicker(typeof(RemotePortPicker))]
    [ProvideDebugLanguage(PowerShellConstants.LanguageName, "{45CF952F-A269-4DE3-8403-E0638D94292D}", "5CA5D431-02A9-4E75-87A3-F8752D9FC7A9", "{43ACAB74-8226-4920-B489-BFCF05372437}")]
    [ProvideToolWindow(
        typeof(PSCommandExplorerWindow),
        Style = Microsoft.VisualStudio.Shell.VsDockStyle.Tabbed,
        Window = "dd9b7693-1385-46a9-a054-06566904f861")]
    public sealed class PowerShellToolsPackage : AsyncPackage
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(PowerShellToolsPackage));
        private Lazy<PowerShellHostClientService> _powerShellHostClientService;
        private static Dictionary<ICommand, MenuCommand> _commands;
        private IContentType _contentType;
        public static EventWaitHandle DebuggerReadyEvent = new EventWaitHandle(false, EventResetMode.ManualReset);
        public static bool PowerShellHostInitialized = false;
        private SolutionEventsListener _solutionEventsListener;
        public static ConnectionManager ConnectionManager;
        private readonly IVisualStudio _visualStudio;

        /// <summary>
        /// Default constructor of the package.
        /// Inside this method you can place any initialization code that does not require
        /// any Visual Studio service because at this point the package object is created but
        /// not sited yet inside Visual Studio environment. The place to do all the other
        /// initialization is the Initialize method.
        /// </summary>
        public PowerShellToolsPackage()
        {
            _visualStudio = new VisualStudio();
            DiagnosticConfiguration.EnableDiagnostics();
            Log.Info("PowerShellToolsPackage()");
            _commands = new Dictionary<ICommand, MenuCommand>();
        }

        /// <summary>
        /// Returns the current package instance.
        /// </summary>
        internal static PowerShellToolsPackage Instance
        {
            get; private set;
        }

        public static IPowerShellDebuggingService DebuggingService
        {
            get
            {
                return ConnectionManager?.PowerShellDebuggingService;
            }
        }

        public IAnalysisService AnalysisService
        {
            get
            {
                return ConnectionManager.AnalysisService;
            }
        }

        public new object GetService(Type type)
        {
            return base.GetService(type);
        }

        /// <summary>
        /// Returns the PowerShell host for the package.
        /// </summary>
        internal static ScriptDebugger Debugger
        {
            get { return _debugger; }
            set
            {
                _debugger = value;
                if (value != null)
                {
                    DebuggerAvailable?.Invoke(null, new EventArgs());
                }
            }
        }

        internal static event EventHandler DebuggerAvailable;
        internal static ScriptDebugger _debugger;

        public static IPowerShellIntelliSenseService IntelliSenseService
        {
            get
            {
                return ConnectionManager?.PowerShellIntelliSenseSerivce;
            }
        }

        public static IPowerShellExplorerService ExplorerService
        {
            get
            {
                return ConnectionManager.PowerShellExplorerService;
            }
        }

        internal IContentType ContentType
        {
            get
            {
                if (_contentType == null)
                {
                    _contentType = GetService<IContentTypeRegistryService>().GetContentType(PowerShellConstants.LanguageName);
                }
                return _contentType;
            }
        }

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override async Threading.Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            var currentVersion = Assembly.GetExecutingAssembly().GetName().Version;

            Log.InfoFormat("InitializeAsync()");
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            Log.InfoFormat("Switch to main thread");

            try
            {
                EnvDTE.DTE dte = (EnvDTE.DTE)GetGlobalService(typeof(EnvDTE.DTE));
                Log.InfoFormat("PowerShell Tools Version: {0}", Assembly.GetExecutingAssembly().GetName().Version);
                Log.InfoFormat("Visual Studio Version: {0}", dte.Version);
                Log.InfoFormat("Windows Version: {0}", Environment.OSVersion);
                Log.InfoFormat("Current Culture: {0}", CultureInfo.CurrentCulture);

                base.Initialize();

                await InitializeInternalAsync();

                _powerShellHostClientService = new Lazy<PowerShellHostClientService>(() => { return new PowerShellHostClientService(); });

                RegisterServices();
            }
            catch (Exception ex)
            {
                Log.Error("Failed to initialize package.", ex);
                MessageBox.Show(
                    Resources.PowerShellToolsInitializeFailed + ex,
                    Resources.MessageBoxErrorTitle,
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            finally
            {
                Log.InfoFormat("Set PacakgeInitialized");
                Instance = this;
            }

            Log.InfoFormat("Done InitializeAsync()");
        }

        public static T GetService<T>() where T : class
        {
            var componentModel = GetGlobalService(typeof(SComponentModel)) as IComponentModel;
            return componentModel.GetService<T>();
        }

        public static string Version
        {
            get
            {
                Assembly asm = Assembly.GetExecutingAssembly();
                FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(asm.Location);
                return fvi.FileVersion;
            }
        }

        public static SettingsControl Settings
        {
            get; private set;
        }

        private async System.Threading.Tasks.Task InitializeInternalAsync()
        {
            Log.Info(string.Format(CultureInfo.CurrentCulture, "Entering Initialize() of: {0}", this));

            Log.Info("Adding language service");
            var langService = new PowerShellLanguageInfo(this);
            ((IServiceContainer)this).AddService(langService.GetType(), langService, true);

            Log.Info("Getting adapters factory");
            var textManager = (IVsTextManager)await GetServiceAsync(typeof(SVsTextManager));
            var adaptersFactory = (IVsEditorAdaptersFactoryService)await GetServiceAsync(typeof(IVsEditorAdaptersFactoryService));

            var backgroundColor = VSColorTheme.GetThemedColor(EnvironmentColors.ToolWindowBackgroundColorKey);

            if (backgroundColor.Name == "fff5f5f5")
            {
                ThemeManager.Current.ApplicationTheme = ApplicationTheme.Light;
            }
            else
            {
                ThemeManager.Current.ApplicationTheme = ApplicationTheme.Dark;
            }

            Settings = new SettingsControl(new SettingsViewModel());
            Settings.ViewModel.FullSettingsVisibility = Visibility.Hidden;

            await VariablesCommand.InitializeAsync(this);
            await ModulesCommand.InitializeAsync(this);
            await SettingsCommand.InitializeAsync(this);

            RefreshCommands(new ExecuteSelectionCommand(),
                            new ExecuteSelectionCommandToolbar(),
                            new ExecuteFromEditorContextMenuCommand(),
                            new ExecuteWithParametersAsScriptCommand(adaptersFactory, textManager),
                            new ExecuteFromSolutionExplorerContextMenuCommand(),
                            new ExecuteWithParametersAsScriptFromSolutionExplorerCommand(adaptersFactory, textManager),
                            new OpenDebugReplCommand(),
                            new OpenExplorerCommand(),
                            new SwitchPowerShellVersionCommand(),
                            new ListPowerShellVersionsCommand(),
                            new PacakgeScript()
                            );

            try
            {
                await InitializePowerShellHostAsync();
            }
            catch (AggregateException ae)
            {
                Log.Error("Failed to initalize PowerShell host.", ae.Flatten());
                MessageBox.Show(
                    Resources.PowerShellHostInitializeFailed,
                    Resources.MessageBoxErrorTitle,
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                throw ae.Flatten();
            }

        }

        internal void ShowExplorerWindow()
        {
            // Get the instance number 0 of this tool window. This window is single instance so this instance
            // is actually the only one.
            // The last flag is set to true so that if the tool window does not exists it will be created.
            ToolWindowPane window = this.FindToolWindow(typeof(PSCommandExplorerWindow), 0, true);
            if ((null == window) || (null == window.Frame))
            {
                throw new NotSupportedException("");
            }
            IVsWindowFrame windowFrame = (IVsWindowFrame)window.Frame;
            ErrorHandler.ThrowOnFailure(windowFrame.Show());
        }

        internal void ShowConsoleWindow()
        {
            return;
        }

        private void RefreshCommands(params ICommand[] commands)
        {
            Log.Info("RefreshCommands");
            // Add our command handlers for menu (commands must exist in the .vsct file)
            var mcs = GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (null != mcs)
            {
                foreach (var command in commands)
                {
                    var menuCommand = new OleMenuCommand(command.Execute, command.CommandId);
                    menuCommand.BeforeQueryStatus += command.QueryStatus;
                    mcs.AddCommand(menuCommand);
                    _commands[command] = menuCommand;
                }
            }
            Log.Info("Done RefreshCommands");
        }

        /// <summary>
        /// Register Services
        /// </summary>
        private void RegisterServices()
        {
            Log.Info("RegisterServices");

            Debug.Assert(this is IServiceContainer, "The package is expected to be an IServiceContainer.");

            var serviceContainer = (IServiceContainer)this;
            serviceContainer.AddService(typeof(IPowerShellHostClientService), (c, t) => _powerShellHostClientService.Value, true);
            serviceContainer.AddService(typeof(IClipboardService), new ClipboardService(), promote: true);

            Log.Info("Done RegisterServices");
        }

        /// <summary>
        /// Initialize the PowerShell host.
        /// </summary>
        private async System.Threading.Tasks.Task InitializePowerShellHostAsync()
        {
            Log.Info("InitializePowerShellHost");

            var instance = await GeneralOptions.GetLiveInstanceAsync();

            Log.Info("Opening PowerShell host connection");
            ConnectionManager = new ConnectionManager();
            Log.Info("Finished opening PowerShell host connection");
            Debugger = new ScriptDebugger(instance.OverrideExecutionPolicyConfiguration, ConnectionManager);

            // Warm up the intellisense service due to the reason that the
            // first intellisense request is often times slower than usual
            // TODO: Should we move this into the HostService's initializiation?
            Log.Info("Initializing IntelliSense");
            await IntelliSenseService.GetDummyCompletionListAsync();

            Log.Info("Debugger is ready");
            DebuggerReadyEvent.Set();
            PowerShellHostInitialized = true;

            if (instance.ShouldLoadProfiles)
            {
                Log.Info("Loading profiles");
                DebuggingService.LoadProfiles();
                Log.Info("Done loading profiles");
            }

            GeneralOptions.Instance.PowerShellVersionChanged += (sender, obj) =>
            {
                SetReplLocationToSolutionDir();
            };

            Log.Info("Set repl location to solution dir");
            SetReplLocationToSolutionDir();
            _solutionEventsListener = new SolutionEventsListener(this);
            _solutionEventsListener.StartListeningForChanges();
            _solutionEventsListener.SolutionOpened += _solutionEventsListener_SolutionOpened;

            Log.Info("Done InitializeInternal()");
        }

        private void _solutionEventsListener_SolutionOpened(object sender, EventArgs e)
        {
            SetReplLocationToSolutionDir();
        }

        private void SetReplLocationToSolutionDir()
        {
            var retries = 0;

            while (retries < 5)
            {
                try
                {
                    var solution = GetService(typeof(IVsSolution)) as IVsSolution;
                    if (solution != null)
                    {
                        string solutionDir, solutionFile, other;
                        if (solution.GetSolutionInfo(out solutionDir, out solutionFile, out other) == VSConstants.S_OK)
                        {
                            Debugger.ExecuteInternal(string.Format("Set-Location '{0}'", solutionDir));
                            break;
                        }
                        else
                        {
                            throw new Exception("Solution not ready.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Error("Failed to set REPL location to solution location.", ex);
                    Thread.Sleep(1000);
                    retries++;
                }
            }
        }

        internal void BitnessSettingChanged(object sender, BitnessEventArgs e)
        {
            ConnectionManager.ProcessEventHandler(e.NewBitness);
        }

        private static DependencyObject FindChild(DependencyObject parent, string childName)
        {
            if (parent == null)
            {
                return null;
            }
            int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childrenCount; i++)
            {
                DependencyObject dependencyObject = VisualTreeHelper.GetChild(parent, i);
                FrameworkElement frameworkElement = dependencyObject as FrameworkElement;
                if (frameworkElement != null && frameworkElement.Name == childName)
                {
                    return frameworkElement;
                }
                dependencyObject = FindChild(dependencyObject, childName);
                if (dependencyObject != null)
                {
                    return dependencyObject;
                }
            }
            return null;
        }
    }
}
