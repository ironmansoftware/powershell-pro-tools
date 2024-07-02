using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace PowerShellToolsPro.Packager.Test.TestModule
{
	[Cmdlet(VerbsCommon.Enter, "Test")]
    public class Command : Cmdlet
    {
	    protected override void ProcessRecord()
	    {
		    WriteObject("Test");
	    }
    }

	[Cmdlet(VerbsCommon.Enter, "DontExport")]
	public class Command2 : Cmdlet
	{
		protected override void ProcessRecord()
		{
			WriteObject("Test");
		}
	}

	public class SomeClass
	{
		public static string GetTestString()
		{
			return "Test";
		}
	}
}
