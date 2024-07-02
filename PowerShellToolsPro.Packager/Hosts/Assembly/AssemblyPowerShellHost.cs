using System;
using System.Globalization;
using System.Management.Automation.Host;
using System.Threading;

namespace PowerShellToolsPro.Packager.Hosts.Assembly
{
	internal class AssemblyPowerShellHost : PSHost
	{
		private CultureInfo originalCultureInfo = Thread.CurrentThread.CurrentCulture;

		private CultureInfo originalUICultureInfo = Thread.CurrentThread.CurrentUICulture;

		private static Guid Id = Guid.NewGuid();

		private AssemblyPowerShellHostUserInterface hostUserInterface = new AssemblyPowerShellHostUserInterface();

		public override CultureInfo CurrentCulture
		{
			get
			{
				return this.originalCultureInfo;
			}
		}

		public override CultureInfo CurrentUICulture
		{
			get
			{
				return this.originalUICultureInfo;
			}
		}

		public override Guid InstanceId
		{
			get
			{
				return Id;
			}
		}

		public override string Name
		{
			get
			{
				return "PoshProToolsAssemblyPowerShellHost";
			}
		}

		public override PSHostUserInterface UI
		{
			get
			{
				return this.hostUserInterface;
			}
		}

		public override Version Version
		{
			get
			{
				return new Version(5, 0, 0, 0);
			}
		}

		public override void EnterNestedPrompt()
		{
		}

		public override void ExitNestedPrompt()
		{
		}

		public override void NotifyBeginApplication()
		{
		}

		public override void NotifyEndApplication()
		{
		}

		public override void SetShouldExit(int exitCode)
		{
		}
	}
}
