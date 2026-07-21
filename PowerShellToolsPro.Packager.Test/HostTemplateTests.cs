using System.IO;
using System.Reflection;
using PowerShellToolsPro.Packager;
using Xunit;

namespace PowerShellToolsPro.Test.Packager
{
    public class HostTemplateTests
    {
        [Theory]
        [InlineData("PowerShellToolsPro.Packager.Hosts.Console.ConsolePowerShellHost_Core.cs")]
        [InlineData("PowerShellToolsPro.Packager.Hosts.Console.ConsolePowerShellHost_Linux.cs")]
        [InlineData("PowerShellToolsPro.Packager.Hosts.Console.ConsolePowerShellHost_Osx.cs")]
        [InlineData("PowerShellToolsPro.Packager.Hosts.Service.ConsolePowerShellHost_Core.cs")]
        public void PowerShellCoreHostTemplatesResolveScriptRootFromExecutable(string resourceName)
        {
            var hostTemplate = GetEmbeddedResource(resourceName);

            Assert.Contains("Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName)", hostTemplate);
            Assert.DoesNotContain("Assembly.GetExecutingAssembly().CodeBase", hostTemplate);
        }

        private static string GetEmbeddedResource(string resourceName)
        {
            using (var stream = typeof(Compiler).GetTypeInfo().Assembly.GetManifestResourceStream(resourceName))
            {
                Assert.NotNull(stream);

                using (var reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
        }
    }
}
