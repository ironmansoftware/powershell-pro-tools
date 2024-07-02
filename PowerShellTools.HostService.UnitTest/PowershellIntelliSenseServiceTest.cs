using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PowerShellTools.HostService.ServiceManagement;
using PowerShellTools.Intellisense;
using PowerShellTools.Common.ServiceManagement.IntelliSenseContract;
using System.Threading;

namespace PowerShellTools.HostService.UnitTest
{
    [TestClass]
    public class PowerShellIntelliSenseServiceTest
    {
        private PowerShellIntelliSenseService _service;
        private IIntelliSenseServiceCallback _context;

        [TestInitialize]
        public void Init()
        {
            _context = new IntelliSenseEventsHandlerProxy();
            _service = new PowerShellIntelliSenseService(_context);
        }

        [TestCleanup]
        public void Clean()
        {
        }

        [TestMethod]
        public void GetCompletionResultsDashTriggerTest()
        {
            var mre = new ManualResetEvent(false);

            CompletionResultList result = null;
            ((IntelliSenseEventsHandlerProxy)_context).CompletionListUpdated += (sender, args) => { result = args.Value1; mre.Set(); };
            
            _service.RequestCompletionResults("Write-", 6, 0, DateTime.UtcNow.Ticks);

            mre.WaitOne();

            Assert.AreEqual<int>(0, result.ReplacementIndex);
            Assert.AreEqual<int>(6, result.ReplacementLength);
        }

        [TestMethod]
        public void GetCompletionResultsDollarTriggerTest()
        {
            var mre = new ManualResetEvent(false);

            CompletionResultList result = null;
            ((IntelliSenseEventsHandlerProxy)_context).CompletionListUpdated += (sender, args) => { result = args.Value1; mre.Set(); };

            string script = @"$myVar = 2; $myStrVar = 'String variable'; Write-Host $";
            _service.RequestCompletionResults(script, 55, 0, DateTime.UtcNow.Ticks);

            mre.WaitOne();

            Assert.AreEqual<string>("$myVar", result.CompletionMatches[0].CompletionText);
            Assert.AreEqual<string>("$myStrVar", result.CompletionMatches[1].CompletionText);
            Assert.AreEqual<int>(54, result.ReplacementIndex);
            Assert.AreEqual<int>(1, result.ReplacementLength);
        }
    }
}
