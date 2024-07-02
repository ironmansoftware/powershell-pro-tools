using HostInjection;
using HostInjection.Models;
using NSubstitute;
using PowerShellProTools.Host;
using PowerShellTools.Common.ServiceManagement.DebuggingContract;
using PowerShellToolsPro.Cmdlets.VSCode;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace PowerShellToolsPro.Test
{
    public class RapidSenseServiceTest : IDisposable
    {
        private readonly RapidSenseService rapidSenseService;
        private readonly IPoshToolsServer _server;
        private readonly Runspace _runspace;
        private readonly RapidSenseOptions options;
        

        public RapidSenseServiceTest()
        {
            options = new RapidSenseOptions();

            _server = Substitute.For<IPoshToolsServer>();
            _server.ExecutePowerShellMainRunspace<PSObject>(Arg.Any<string>()).Returns((info) => ExecutePowerShell<PSObject>(info.ArgAt<string>(0)));

            rapidSenseService = new RapidSenseService(_server);

            var initialSessionState = InitialSessionState.CreateDefault();
            initialSessionState.ImportPSModule(typeof(Variable).Assembly.Location);

            _runspace = RunspaceFactory.CreateRunspace(initialSessionState);
            _runspace.Open();

        }

        private IEnumerable<T> ExecutePowerShell<T>(string value)
        {
            using(var ps = PowerShell.Create())
            {
                ps.Runspace = _runspace;
                ps.AddScript(value);
                var result = ps.Invoke<T>(value);
                if (ps.HadErrors)
                {
                    foreach(var error in ps.Streams.Error)
                    {
                        throw new Exception(error.ToString());
                    }
                }
                return result;
            }
        }

        public void Dispose()
        {
            _runspace.Dispose();
        }

        [Fact]
        public void ShouldCompleteVariable()
        {
            var variables = new[]
            {
                new PSObject(new CompletionItem("$Name", CompletionKind.Variable))
            };

            ExecutePowerShell<PSObject>("$Name = 'Test'");

            rapidSenseService.UpdateCache(options);
            var item = rapidSenseService.Complete("$", "", 0).First(m => m.InsertText == "$Name");

            Assert.Equal("$Name", item.InsertText);
            Assert.Equal(CompletionKind.Variable, item.CompletionKind);
        }

        [Fact]
        public void ShouldCompleteVariableProperties()
        {
            ExecutePowerShell<PSObject>("$Name = [PSCustomObject]@{ Test = 123 }");

            rapidSenseService.UpdateCache(options);
            var item = rapidSenseService.Complete(".", "$Name.", 6).First(m => m.InsertText == "Test");

            Assert.Equal("Test", item.InsertText);
            Assert.Equal(CompletionKind.Property, item.CompletionKind);
        }

        [Fact]
        public void ShouldCompleteVariableNestedProperties()
        {
            ExecutePowerShell<PSObject>("$Name = [PSCustomObject]@{ Test = [PSCustomObject]@{ Nest = 123 } }");

            rapidSenseService.UpdateCache(options);
            var item = rapidSenseService.Complete(".", "$Name.Test.", 11).First(m => m.InsertText == "Nest");

            Assert.Equal("Nest", item.InsertText);
            Assert.Equal(CompletionKind.Property, item.CompletionKind);
        }

        [Fact]
        public void ShouldCompleteVariableMethods()
        {
            ExecutePowerShell<PSObject>("$Name = @{ Test = 123 }");
            
            rapidSenseService.UpdateCache(options);
            var item = rapidSenseService.Complete(".", "$Name.", 6).First(m => m.InsertText == "Add");

            Assert.Equal("Add", item.InsertText);
            Assert.Equal(CompletionKind.Method, item.CompletionKind);
        }

        [Fact]
        public void ShouldCacheVariablePaths()
        {
            ExecutePowerShell<PSObject>("$Name = @{ Test = 123 }");
            var ps = $"Get-PoshToolsVariable -Path '$Name' -ValueOnly | Get-Member | Select-Object Name,MemberType | ForEach-Object {{ [PowerShellToolsPro.Cmdlets.VSCode.CompletionItem]$_  }} ";

            rapidSenseService.UpdateCache(options);
            var item = rapidSenseService.Complete(".", "$Name.", 6).First(m => m.InsertText == "Add");
            item = rapidSenseService.Complete(".", "$Name.", 6).First(m => m.InsertText == "Add");

            _server.Received(1).ExecutePowerShellMainRunspace<PSObject>(ps);
        }

        [Fact]
        public void ShouldCompleteCommand()
        {
            rapidSenseService.UpdateCache(options);
            var item = rapidSenseService.Complete("-", "Get-", 4).First(m => m.InsertText == "Get-Variable");

            Assert.Equal("Get-Variable", item.InsertText);
            Assert.Equal(CompletionKind.Function, item.CompletionKind);
        }

        [Fact]
        public void ShouldCompleteCommandInPipeline()
        {
            rapidSenseService.UpdateCache(options);
            var items = rapidSenseService.Complete("-", "Get-Process | Stop-", 19);
            var item = items.First(m => m.InsertText == "Stop-Process");

            Assert.Equal("Stop-Process", item.InsertText);
            Assert.Equal(CompletionKind.Function, item.CompletionKind);
        }

        [Fact]
        public void ShouldCompleteCommandInScriptBlock()
        {
            rapidSenseService.UpdateCache(options);
            var items = rapidSenseService.Complete("-", "ForEach-Object { Stop-", 22);
            var item = items.First(m => m.InsertText == "Stop-Process");

            Assert.Equal("Stop-Process", item.InsertText);
            Assert.Equal(CompletionKind.Function, item.CompletionKind);
        }

        [Fact]
        public void ShouldCompleteCommandInScriptBlock2()
        {
            rapidSenseService.UpdateCache(options);
            var items = rapidSenseService.Complete("-", "ForEach-Object { Stop- }", 22);
            var item = items.First(m => m.InsertText == "Stop-Process");

            Assert.Equal("Stop-Process", item.InsertText);
            Assert.Equal(CompletionKind.Function, item.CompletionKind);
        }

        [Fact]
        public void ShouldCompleteCommandParameters()
        {
            rapidSenseService.UpdateCache(options);
            var item = rapidSenseService.Complete("-", "Get-Variable -", 14).First(m => m.InsertText == "-Name");

            Assert.Equal("-Name", item.InsertText);
            Assert.Equal(CompletionKind.Property, item.CompletionKind);
        }

        [Fact]
        public void ShouldNotCompleteParameterInString()
        {
            rapidSenseService.UpdateCache(options);
            var item = rapidSenseService.Complete("-", "Get-Variable '-'", 15).Any();

            Assert.False(item);
        }

        [Fact]
        public void ShouldCompleteCommandParametersInPipeline()
        {
            rapidSenseService.UpdateCache(options);
            var item = rapidSenseService.Complete("-", "Get-Variable -Name 'notepad' | Stop-Process -", 45).First(m => m.InsertText == "-Force");

            Assert.Equal("-Force", item.InsertText);
            Assert.Equal(CompletionKind.Property, item.CompletionKind);
        }

        [Fact]
        public void ShouldCompleteCommandParametersInScriptBlock()
        {
            rapidSenseService.UpdateCache(options);
            var item = rapidSenseService.Complete("-", "ForEach-Object { Start-Process - }", 32).First(m => m.InsertText == "-Verb");

            Assert.Equal("-Verb", item.InsertText);
            Assert.Equal(CompletionKind.Property, item.CompletionKind);
        }

        [Fact]
        public void ShouldCompleteTypes()
        {
            rapidSenseService.UpdateCache(options);
            var item = rapidSenseService.Complete("[", "[", 1).First(m => m.InsertText == "System.Type");

            Assert.Equal("System.Type", item.InsertText);
            Assert.Equal(CompletionKind.Class, item.CompletionKind);
        }

        [Fact]
        public void ShouldCompleteSubType()
        {
            rapidSenseService.UpdateCache(options);
            var items = rapidSenseService.Complete(".", "[System.", 8).ToArray();
            var item = items.First(m => m.InsertText == "Type");

            Assert.Equal("Type", item.InsertText);
            Assert.Equal(CompletionKind.Class, item.CompletionKind);
        }

        [Fact(Skip = "Fix later")]
        public void ShouldCompleteFile()
        {
            var tempFile = Path.GetTempFileName();
            File.WriteAllText(tempFile, "test");
            var fileInfo = new FileInfo(tempFile);

            rapidSenseService.UpdateCache(options);
            var str = $"{Path.GetTempPath()}\\";
            var items = rapidSenseService.Complete("\\", str, str.Length).ToArray();
            var item = items.First(m => m.InsertText == fileInfo.Name);

            Assert.Equal(fileInfo.Name, item.InsertText);
            Assert.Equal(CompletionKind.File, item.CompletionKind);
        }
    }
}
