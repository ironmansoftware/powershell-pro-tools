using System;
using PowerShellProTools.Host;
using PowerShellToolsPro.FormDesigner;
using PowerShellToolsPro.FormsDesigner;
using Xunit;

namespace PowerShellToolsPro.Test
{
	public class PowerShellCodeDomProviderTest
	{
        [Fact]
		public void ShouldReturnPowerShellCodeGenerator()
		{
            var visualStudio = NSubstitute.Substitute.For<IVisualStudio>();
			var codeDomProvider = new PSCodeDomProvider(visualStudio);
			Assert.IsType<PowerShellCodeGenerator>(codeDomProvider.CreateGenerator());
		}

		[Fact]
		public void ShouldThrowANotImplementedExceptionWhenCallingCreateCompiler()
		{
            var visualStudio = NSubstitute.Substitute.For<IVisualStudio>();
            var codeDomProvider = new PSCodeDomProvider(visualStudio);

			Assert.Throws(typeof(NotImplementedException), codeDomProvider.CreateCompiler);
		}
	}
}
