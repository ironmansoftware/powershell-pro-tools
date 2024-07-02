using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.TextTemplating.VSHost;

namespace PowerShellToolsPro.Templating
{
	[ComVisible(true)]
	[Guid("52B316AA-1997-4c81-9969-83604C09EEB4")]
	[CodeGeneratorRegistration(typeof(PowerShellGenerator), "C# PowerShell Generator", "{FAE04EC1-301F-11D3-BF4B-00C04F79EFBC}", GeneratesDesignTimeSource = true)]
	[CodeGeneratorRegistration(typeof(PowerShellGenerator), "VB.NET PowerShell Generator", "{164B10B9-B200-11D0-8C61-00A0C91E29D5}", GeneratesDesignTimeSource = true)]
	[ProvideObject(typeof(PowerShellGenerator))]
	public class PowerShellGenerator : BaseCodeGeneratorWithSite
	{
#pragma warning disable 0414
		//The name of this generator (use for 'Custom Tool' property of project item)
		internal static string name = "PowerShellGenerator";
#pragma warning restore 0414

		public override string GetDefaultExtension()
		{
			return ".txt";
		}

		protected override byte[] GenerateCode(string inputFileName, string inputFileContent)
		{
			
			return new byte[0];
		}
	}
}
