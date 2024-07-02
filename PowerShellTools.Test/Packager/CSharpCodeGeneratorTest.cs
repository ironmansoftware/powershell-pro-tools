using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PowerShellToolsPro.Packager;
using PowerShellToolsPro.Packager.Hosts;
using Xunit;

namespace PowerShellToolsPro.Test.Packager
{
	public class CSharpCodeGeneratorTest
	{
		[Fact(Skip = "not doing this")]
		public void ShouldGenerateCode()
		{
			var assCreator = new AssemblyCreator();
			var assembly = assCreator.GenerateNamespace("Hyper-V");
			var generator = new CSharpTextCodeGenerator();
			var source = generator.GenerateNamespace(assembly);
		}
	}
}
