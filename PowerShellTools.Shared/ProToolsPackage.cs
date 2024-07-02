using System;
using System.Collections;
using System.ComponentModel.Design;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using PowerShellTools.Commands;
using PowerShellToolsPro.Commands;

namespace PowerShellToolsPro
{
	/// <summary>
	/// This is the class that implements the package exposed by this assembly.
	/// </summary>
	/// <remarks>
	/// <para>
	/// The minimum requirement for a class to be considered a valid package for Visual Studio
	/// is to implement the IVsPackage interface and register itself with the shell.
	/// This package uses the helper classes defined inside the Managed Package Framework (MPF)
	/// to do it: it derives from the Package class that provides the implementation of the
	/// IVsPackage interface and uses the registration attributes defined in the framework to
	/// register itself and its components with the shell. These attributes tell the pkgdef creation
	/// utility what data to put into .pkgdef file.
	/// </para>
	/// <para>
	/// To get loaded into VS, the package must be referred by &lt;Asset Type="Microsoft.VisualStudio.VsPackage" ...&gt; in .vsixmanifest file.
	/// </para>
	/// </remarks>
	[PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
	[ProvideAutoLoad(PowerShellProjectUiContextString, PackageAutoLoadFlags.BackgroundLoad)]
	[InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)] // Info on this package for Help/About
	[Guid(PackageGuidString)]
	[SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "pkgdef, VS and vsixmanifest are valid VS terms")]
	public sealed class ProToolsPackage : AsyncPackage
	{
		/// <summary>
		/// VSPackage GUID string.
		/// </summary>
		public const string PackageGuidString = "e6d9f061-7054-42d0-b665-1e4c966129f3";

		/// <summary>
		/// This is the GUID in string form of the Visual Studio UI Context when in PowerShell project is opened/created.
		/// </summary>
		public const string PowerShellProjectUiContextString = "8b1141ab-519d-4c1e-a86c-510e5a56bf64";

		public ICommand[] Commands;

		/// <summary>
		/// Initializes a new instance of the <see cref="ProToolsPackage"/> class.
		/// </summary>
		public ProToolsPackage()
		{
			Commands = new ICommand[]
			{
				new PacakgeScript()
			};
		}

		#region Package Members

		protected override async System.Threading.Tasks.Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            await base.InitializeAsync(cancellationToken, progress);

            var log = GetGlobalService(typeof(SVsActivityLog)) as IVsActivityLog;

            log?.LogEntry(3, "PowerShellProTools", Environment.OSVersion.VersionString);

            try
            {
                using (var ps = PowerShell.Create())
                {
                    ps.AddScript("$PSVersionTable");
                    var versionTable = ps.Invoke<Hashtable>().First();
                    log?.LogEntry(3, "PowerShellProTools", versionTable["PSVersion"].ToString());
                }
            }
            catch (Exception ex)
            {
                log?.LogEntry(1, "PowerShellProTools", $"Exception checking PowerShell version: {ex.Message}");
            }

            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            if (await GetServiceAsync(typeof(IMenuCommandService)) is OleMenuCommandService mcs)
            {
                foreach (var command in Commands)
                {
                    var menuCommand = new OleMenuCommand(command.Execute, command.CommandId);
                    menuCommand.BeforeQueryStatus += command.QueryStatus;
                    mcs.AddCommand(menuCommand);
                }
            }
        }


#endregion
	}
}
