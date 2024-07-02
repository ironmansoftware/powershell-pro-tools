using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PowerShellTools.Common.ServiceManagement.DebuggingContract;
using PowerShellTools.HostService.ServiceManagement.Debugging;

namespace PowerShellTools.Test
{
    [TestClass]
    public class RemoteDebuggingTest
    {
        private AutoResetEvent _attachSemaphore;
        private PowerShellDebuggingServiceAttachUtilities _attachUtilities;
        private DebugScenario _preScenario;
        private string _stringResult;
        private bool _boolResult;

        [TestInitialize]
        public void Init()
        {
            _attachUtilities = new PowerShellDebuggingServiceAttachUtilities(null);
            _attachSemaphore = new AutoResetEvent(false);
        }

        [TestMethod]
        public void LocalRunspaceAttachSemaPass()
        {
            _attachSemaphore.Set();
            _preScenario = DebugScenario.Local;
            var debuggingService = new Mock<IPowerShellDebuggingService>();
            debuggingService.Setup(m => m.GetDebugScenario()).Returns(DebugScenario.LocalAttach);
            _attachUtilities.DebuggingService = debuggingService.Object;

            _stringResult = _attachUtilities.VerifyAttachToRunspace(_preScenario, _attachSemaphore);
            Assert.IsTrue(string.IsNullOrEmpty(_stringResult));
        }

        [TestMethod]
        public void LocalRunspaceDetachSemaPass()
        {
            _attachSemaphore.Set();
            _preScenario = DebugScenario.LocalAttach;
            var debuggingService = new Mock<IPowerShellDebuggingService>();
            debuggingService.Setup(m => m.GetDebugScenario()).Returns(DebugScenario.Local);
            _attachUtilities.DebuggingService = debuggingService.Object;

            _boolResult = _attachUtilities.VerifyDetachFromRunspace(_preScenario, _attachSemaphore);
            Assert.IsTrue(_boolResult);
        }

        [TestMethod]
        public void LocalRunspaceAttachSemaFailScenarioValid()
        {
            _attachSemaphore.Reset();
            _preScenario = DebugScenario.Local;
            var debuggingService = new Mock<IPowerShellDebuggingService>();
            debuggingService.Setup(m => m.GetDebugScenario()).Returns(DebugScenario.LocalAttach);
            _attachUtilities.DebuggingService = debuggingService.Object;

            _stringResult = _attachUtilities.VerifyAttachToRunspace(_preScenario, _attachSemaphore);
            Assert.IsTrue(string.IsNullOrEmpty(_stringResult));
        }

        [TestMethod]
        public void LocalRunspaceAttachSemaFailScenarioInvalid()
        {
            _attachSemaphore.Reset();
            _preScenario = DebugScenario.Local;
            var debuggingService = new Mock<IPowerShellDebuggingService>();
            debuggingService.Setup(m => m.GetDebugScenario()).Returns(DebugScenario.Local);
            _attachUtilities.DebuggingService = debuggingService.Object;

            _stringResult = _attachUtilities.VerifyAttachToRunspace(_preScenario, _attachSemaphore);
            Assert.IsFalse(string.IsNullOrEmpty(_stringResult));
        }

        [TestMethod]
        public void LocalRunspaceDetachSemaFailScenarioValid()
        {
            _attachSemaphore.Reset();
            _preScenario = DebugScenario.LocalAttach;
            var debuggingService = new Mock<IPowerShellDebuggingService>();
            debuggingService.Setup(m => m.GetDebugScenario()).Returns(DebugScenario.Local);
            _attachUtilities.DebuggingService = debuggingService.Object;

            _boolResult = _attachUtilities.VerifyDetachFromRunspace(_preScenario, _attachSemaphore);
            Assert.IsTrue(_boolResult);
        }

        [TestMethod]
        public void LocalRunspaceDetachSemaFailScenarioInvalid()
        {
            _attachSemaphore.Reset();
            _preScenario = DebugScenario.LocalAttach;
            var debuggingService = new Mock<IPowerShellDebuggingService>();
            debuggingService.Setup(m => m.GetDebugScenario()).Returns(DebugScenario.LocalAttach);
            _attachUtilities.DebuggingService = debuggingService.Object;

            _boolResult = _attachUtilities.VerifyDetachFromRunspace(_preScenario, _attachSemaphore);
            Assert.IsFalse(_boolResult);
        }

        [TestMethod]
        public void RemoteRunspaceAttachScenarioValid()
        {
            _preScenario = DebugScenario.RemoteSession;
            var debuggingService = new Mock<IPowerShellDebuggingService>();
            debuggingService.Setup(m => m.GetDebugScenario()).Returns(DebugScenario.RemoteSession);
            _attachUtilities.DebuggingService = debuggingService.Object;

            _stringResult = _attachUtilities.VerifyAttachToRunspace(_preScenario, _attachSemaphore);
            Assert.IsTrue(string.IsNullOrEmpty(_stringResult));
        }

        [TestMethod]
        public void RemoteRunspaceAttachScenarioInvalid()
        {
            _preScenario = DebugScenario.RemoteSession;
            var debuggingService = new Mock<IPowerShellDebuggingService>();
            debuggingService.Setup(m => m.GetDebugScenario()).Returns(DebugScenario.Local);
            _attachUtilities.DebuggingService = debuggingService.Object;

            _stringResult = _attachUtilities.VerifyAttachToRunspace(_preScenario, _attachSemaphore);
            Assert.IsFalse(string.IsNullOrEmpty(_stringResult));
        }

        [TestMethod]
        public void RemoteRunspaceDetachScenarioValid()
        {
            _preScenario = DebugScenario.RemoteAttach;
            var debuggingService = new Mock<IPowerShellDebuggingService>();
            debuggingService.Setup(m => m.GetDebugScenario()).Returns(DebugScenario.RemoteSession);
            _attachUtilities.DebuggingService = debuggingService.Object;

            _boolResult = _attachUtilities.VerifyDetachFromRunspace(_preScenario, _attachSemaphore);
            Assert.IsTrue(_boolResult);
        }

        [TestMethod]
        public void RemoteRunspaceDetachScenarioInvalid()
        {
            _preScenario = DebugScenario.RemoteAttach;
            var debuggingService = new Mock<IPowerShellDebuggingService>();
            debuggingService.Setup(m => m.GetDebugScenario()).Returns(DebugScenario.RemoteAttach);
            _attachUtilities.DebuggingService = debuggingService.Object;

            _boolResult = _attachUtilities.VerifyDetachFromRunspace(_preScenario, _attachSemaphore);
            Assert.IsFalse(_boolResult);
        }

        [TestMethod]
        public void RemoteAttachScenarioValid()
        {
            var debuggingService = new Mock<IPowerShellDebuggingService>();
            debuggingService.Setup(m => m.GetDebugScenario()).Returns(DebugScenario.RemoteSession);
            _attachUtilities.DebuggingService = debuggingService.Object;

            _boolResult = _attachUtilities.VerifyAttachToRemoteRunspace();
            Assert.IsTrue(_boolResult);
        }

        [TestMethod]
        public void RemoteAttachScenarioInvalid()
        {
            var debuggingService = new Mock<IPowerShellDebuggingService>();
            debuggingService.Setup(m => m.GetDebugScenario()).Returns(DebugScenario.Local);
            _attachUtilities.DebuggingService = debuggingService.Object;

            _boolResult = _attachUtilities.VerifyAttachToRemoteRunspace();
            Assert.IsFalse(_boolResult);
        }

        [TestMethod]
        public void RemoteDetachScenarioValid()
        {
            var debuggingService = new Mock<IPowerShellDebuggingService>();
            debuggingService.Setup(m => m.GetDebugScenario()).Returns(DebugScenario.Local);
            _attachUtilities.DebuggingService = debuggingService.Object;

            _boolResult = _attachUtilities.VerifyDetachFromRemoteRunspace();
            Assert.IsTrue(_boolResult);
        }

        [TestMethod]
        public void RemoteDetachScenarioInvalid()
        {
            var debuggingService = new Mock<IPowerShellDebuggingService>();
            debuggingService.Setup(m => m.GetDebugScenario()).Returns(DebugScenario.RemoteSession);
            _attachUtilities.DebuggingService = debuggingService.Object;

            _boolResult = _attachUtilities.VerifyDetachFromRemoteRunspace();
            Assert.IsFalse(_boolResult);
        }

        [TestMethod]
        public void ComputerNameNoPort()
        {
            string address = "poshtools";
            Tuple<string, int> parts = _attachUtilities.GetNameAndPort(address);
            Assert.IsTrue(parts.Item1.Equals("poshtools"));
            Assert.IsTrue(parts.Item2.Equals(-1));
        }

        [TestMethod]
        public void ComputerNameWithPort()
        {
            string address = "poshtools:1234";
            Tuple<string, int> parts = _attachUtilities.GetNameAndPort(address);
            Assert.IsTrue(parts.Item1.Equals("poshtools"));
            Assert.IsTrue(parts.Item2.Equals(1234));
        }

        [TestMethod]
        public void IPv4NoPort()
        {
            string address = "123.456.789.876";
            Tuple<string, int> parts = _attachUtilities.GetNameAndPort(address);
            Assert.IsTrue(parts.Item1.Equals("123.456.789.876"));
            Assert.IsTrue(parts.Item2.Equals(-1));
        }

        [TestMethod]
        public void IPv4WithPort()
        {
            string address = "123.456.789.876:1234";
            Tuple<string, int> parts = _attachUtilities.GetNameAndPort(address);
            Assert.IsTrue(parts.Item1.Equals("123.456.789.876"));
            Assert.IsTrue(parts.Item2.Equals(1234));
        }

        [TestMethod]
        public void IPv6NoPort()
        {
            string address = "2001:db8:85a3:8d3:1319:8a2e:370:7348";
            Tuple<string, int> parts = _attachUtilities.GetNameAndPort(address);
            Assert.IsTrue(parts.Item1.Equals("2001:db8:85a3:8d3:1319:8a2e:370:7348"));
            Assert.IsTrue(parts.Item2.Equals(-1));
        }

        [TestMethod]
        public void IPv6WithPort()
        {
            string address = "[2001:db8:85a3:8d3:1319:8a2e:370:7348]:1234";
            Tuple<string, int> parts = _attachUtilities.GetNameAndPort(address);
            Assert.IsTrue(parts.Item1.Equals("2001:db8:85a3:8d3:1319:8a2e:370:7348"));
            Assert.IsTrue(parts.Item2.Equals(1234));
        }

        [TestMethod]
        public void ComputerAddressNoPort()
        {
            string address = "machine.address.net";
            Tuple<string, int> parts = _attachUtilities.GetNameAndPort(address);
            Assert.IsTrue(parts.Item1.Equals("machine.address.net"));
            Assert.IsTrue(parts.Item2.Equals(-1));
        }

        [TestMethod]
        public void ComputerAddressWithPort()
        {
            string address = "machine.address.net:1234";
            Tuple<string, int> parts = _attachUtilities.GetNameAndPort(address);
            Assert.IsTrue(parts.Item1.Equals("machine.address.net"));
            Assert.IsTrue(parts.Item2.Equals(1234));
        }

        [TestMethod]
        public void RemoteIsLoopbackValidIP()
        {
            string address = "123.456.789.876";
            Assert.IsFalse(_attachUtilities.RemoteIsLoopback(address));
        }

        [TestMethod]
        public void RemoteIsLoopbackInvalidIPv4One()
        {
            string address = "127.0.0.1";
            Assert.IsTrue(_attachUtilities.RemoteIsLoopback(address));
        }

        [TestMethod]
        public void RemoteIsLoopbackInvalidIPv4Two()
        {
            string address = "127.255.255.254";
            Assert.IsTrue(_attachUtilities.RemoteIsLoopback(address));
        }

        [TestMethod]
        public void RemoteIsLoopbackInvalidIPv6()
        {
            string address = "0:0:0:0:0:0:0:1";
            Assert.IsTrue(_attachUtilities.RemoteIsLoopback(address));
        }

        [TestMethod]
        public void RemoteIsLoopbackValidName()
        {
            string address = "ComputerName";
            Assert.IsFalse(_attachUtilities.RemoteIsLoopback(address));
        }

        [TestMethod]
        public void RemoteIsLoopbackInvalidNameOne()
        {
            string address = "localhost";
            Assert.IsTrue(_attachUtilities.RemoteIsLoopback(address));
        }

        [TestMethod]
        public void RemoteIsLoopbackInvalidNameTwo()
        {
            string address = "LoCaLhOsT";
            Assert.IsTrue(_attachUtilities.RemoteIsLoopback(address));
        }
    }
}
