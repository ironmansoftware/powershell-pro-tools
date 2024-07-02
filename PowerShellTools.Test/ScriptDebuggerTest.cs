using System.Collections.Generic;
using System.IO;
using System.Management.Automation.Runspaces;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PowerShellTools.DebugEngine;
using PowerShellTools.HostService.ServiceManagement.Debugging;
using PowerShellTools.Common.ServiceManagement.DebuggingContract;
using System.Threading.Tasks;
using System.Management.Automation;

namespace PowerShellTools.Test
{
    [TestClass]
    [DeploymentItem("TestFile.ps1")]
    public class ScriptDebuggerTest
    {
        private ScriptDebugger _debugger;
        private Runspace _runspace;
        private PowerShellDebuggingService _debuggingService;

        [TestInitialize]
        public void Init()
        {
            _runspace = RunspaceFactory.CreateRunspace();
            _runspace.Open();
            
            _debuggingService = new PowerShellDebuggingService(null, -1);
            _debugger = new ScriptDebugger(true, _debuggingService, null);
            _debugger.BreakpointManager = new BreakpointManager(_debugger);
            _debuggingService.CallbackService = new DebugServiceEventsHandlerProxy(_debugger, false); 
        }

        [TestCleanup]
        public void Clean()
        {
            _runspace.Dispose();
            _runspace = null;
        }

        [TestMethod]
        public void ShouldClearBreakpoints()
        {
            _debuggingService.SetBreakpoint(new PowerShellBreakpoint(".\\TestFile.ps1", 1, 0));
            _debugger.BreakpointManager.SetBreakpoints(new List<ScriptBreakpoint>());

            using (var pipe = PowerShell.Create())
            {
                pipe.Runspace = PowerShellDebuggingService.Runspace;
                pipe.AddCommand("Get-PSBreakpoint");
                var breakpoints = pipe.Invoke();
                
                Assert.AreEqual(0, breakpoints.Count);
            }
        }

        [TestMethod]
        public void ShouldNotDieIfNoBreakpoints()
        {
            using (var pipe = PowerShell.Create())
            {
                pipe.Runspace = PowerShellDebuggingService.Runspace;
                pipe.AddCommand("Get-PSBreakpoint");
                var breakpoints = pipe.Invoke();

                Assert.AreEqual(0, breakpoints.Count);
            }
        }


        [TestMethod]
        public void ShouldSetLineBreakpoint()
        {
            var engineEvents = new Mock<IEngineEvents>();

            var sbps = new List<ScriptBreakpoint>
                           {
                               new ScriptBreakpoint(null, ".\\TestFile.ps1", 1, 0, engineEvents.Object)
                           };

            _debugger.BreakpointManager.SetBreakpoints(sbps);
            foreach (var bp in sbps)
            {
                bp.Bind();
            }

            using (var pipe = PowerShell.Create())
            {
                pipe.Runspace = PowerShellDebuggingService.Runspace;
                pipe.AddCommand("Get-PSBreakpoint");
                var breakpoints = pipe.Invoke();

                //Verify the breakpoint was added to the runspace.
                Assert.AreEqual(1, breakpoints.Count);
            }

            //Verify the callback event was triggered.
            engineEvents.Verify(m => m.Breakpoint(null, sbps[0]), Times.Once());
        }

        [TestMethod]
        [Ignore]
        public void ShouldBreakOnBreakPoint()
        {
            var engineEvents = new Mock<IEngineEvents>();

            var fi = new FileInfo(".\\TestFile.ps1");

            var sbps = new List<ScriptBreakpoint>
                           {
                               new ScriptBreakpoint(null, fi.FullName, 3, 0, engineEvents.Object)
                           };

            _debugger.BreakpointManager.SetBreakpoints(sbps);
            foreach (var bp in sbps)
            {
                bp.Bind();
            }

            var node = new ScriptProgramNode(null);
            node.IsFile = true;
            node.FileName = fi.FullName;

            var mre = new ManualResetEvent(false);

            bool breakpointHit = false;
            _debugger.BreakpointManager.BreakpointHit += (sender, args) => { breakpointHit = true; System.Threading.Tasks.Task.Factory.StartNew(()=>_debugger.Continue()); };
            _debugger.DebuggingFinished += (sender, args) => mre.Set();
            _debugger.IsDebugging = true;
            _debugger.Execute(node);
            _debugger.IsDebugging = false;

            Assert.IsTrue(mre.WaitOne(5000));
            Assert.IsTrue(breakpointHit);
        }

        [TestMethod]
        [Ignore]
        public void ShouldAcceptArguments()
        {
            var fi = new FileInfo(".\\TestFile.ps1");

            var sbps = new List<ScriptBreakpoint>();
            _debugger.BreakpointManager.SetBreakpoints(sbps);

            var node = new ScriptProgramNode(null);
            node.FileName = fi.FullName;
            node.IsFile = true;
            node.Arguments = "'Arg1' 'Arg2'";

            var mre = new ManualResetEvent(false);
            _debugger.DebuggingFinished += (sender, args) => mre.Set();

            _debugger.Execute(node);

            Assert.IsTrue(mre.WaitOne(5000));

            var arg1 = PowerShellDebuggingService.Runspace.SessionStateProxy.GetVariable("Argument1");
            var arg2 = PowerShellDebuggingService.Runspace.SessionStateProxy.GetVariable("Argument2");

            Assert.AreEqual("Arg1", arg1);
            Assert.AreEqual("Arg2", arg2);
        }

        [TestMethod]
        public void ShouldExecuteSnippet()
        {
            var sbps = new List<ScriptBreakpoint>();
            _debugger.BreakpointManager.SetBreakpoints(sbps);

            var node = new ScriptProgramNode(null);
            node.FileName = "$Global:MyVariable = 'Test'";

            var mre = new ManualResetEvent(false);
            _debugger.DebuggingFinished += (sender, args) => mre.Set();

            _debugger.Execute(node);

            Assert.IsTrue(mre.WaitOne(5000));

            var myVariable = PowerShellDebuggingService.Runspace.SessionStateProxy.GetVariable("MyVariable");

            Assert.AreEqual("Test", myVariable);
        }

        [TestMethod]
        [Ignore]
        public void ShouldSupportRemoteSession()
        {
            var sbps = new List<ScriptBreakpoint>();

            _runspace.Dispose();
            _runspace = RunspaceFactory.CreateRunspace();
            _runspace.Open();
            _debugger.BreakpointManager.SetBreakpoints(sbps);

            var node = new ScriptProgramNode(null);
            node.FileName = "Enter-PSSession localhost";

            var mre = new ManualResetEvent(false);
            string outputString = null;
            _debugger.DebuggingFinished += (sender, args) => mre.Set();
            _debugger.Execute(node);

            Assert.IsTrue(mre.WaitOne(5000));

            mre.Reset();
            node = new ScriptProgramNode(null);
            node.FileName = "$host.Name";
            _debugger.Execute(node);
            Assert.IsTrue(mre.WaitOne(5000));

            Assert.AreEqual("ServerRemoteHost", outputString);
        }
    }
}
