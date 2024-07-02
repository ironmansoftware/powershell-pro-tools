using System.Management.Automation;
using System.Management.Automation.Runspaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PowerShellTools.DebugEngine;
using PowerShellTools.HostService.ServiceManagement.Debugging;

namespace PowerShellTools.Test
{
    [TestClass]
    [Ignore]
    public class VsxHostTest
    {
        private PowerShellDebuggingService _debuggingService;
        private ScriptDebugger _host;

        [TestInitialize]
        public void Init()
        {
            _debuggingService = new PowerShellDebuggingService(null, -1);
            _host = new ScriptDebugger(true, _debuggingService, null);
        }

        [TestMethod]
        public void TestWriteHost()
        {
            var command = new Command("Write-Host");
            command.Parameters.Add("Object", "Test");

            string output = "";
            _debuggingService.HostUi.OutputString = x =>
            {
                output += x;
            };

            using (var pipe = PowerShell.Create())
            {
                pipe.Runspace = PowerShellDebuggingService.Runspace; 
                pipe.AddCommand("Write-Host").AddParameter("Object", "Test");
                pipe.Invoke();
            }

            Assert.AreEqual("Test\n", output);
        }
    }
}
