using PowerShellTools.Common.ServiceManagement.DebuggingContract;
using PowerShellToolsPro.Cmdlets.VSCode;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace PowerShellProTools
{
    public static class IntelliSenseManager
    {
        private static MethodInfo _invokePSCommandMethod;
        private static MethodInfo _invokePSCommandMethodNonGeneric;
        private static object _internalHost;
        private static object _executionOptions;

        public static void InvokeCommand(string command)
        {
            var source = new CancellationTokenSource();
            var psCommand = new PSCommand();
            psCommand.AddScript(command);

            var task = (Task)_invokePSCommandMethodNonGeneric.Invoke(_internalHost, new object[] { psCommand, source.Token, _executionOptions });
            task.Wait();
        }

        public static object InvokeCommandWithResult(string command)
        {
            var source = new CancellationTokenSource();
            var psCommand = new PSCommand();
            psCommand.AddScript(command);

            var genericMethod = _invokePSCommandMethod.MakeGenericMethod(typeof(object));

            var task = (Task<IReadOnlyList<object>>)genericMethod.Invoke(_internalHost, new object[] { psCommand, source.Token, _executionOptions });
            return task.Result;
        }

        public static bool Isv3()
        {
            return AppDomain.CurrentDomain
                    .GetAssemblies()
                    .Any(m => m.GetName().Name.Equals("Microsoft.PowerShell.EditorServices.Hosting", StringComparison.OrdinalIgnoreCase) &&
                    m.GetName().Version >= new Version(3, 0));
        }

        public static void InitV3(int id)
        {
            using (var powerShell = PowerShell.Create())
            {
                powerShell.AddScript($"Get-Runspace -Id {id}");
                var runspace = powerShell.Invoke<Runspace>().FirstOrDefault();

                var host = runspace.GetType().GetProperty("Host", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(runspace);
                _internalHost = host.GetType().GetField("_internalHost", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(host);
                var type = _internalHost.GetType().Assembly.GetType("Microsoft.PowerShell.EditorServices.Services.PowerShell.Execution.PowerShellExecutionOptions");
                _executionOptions = Activator.CreateInstance(type);
                _invokePSCommandMethod = _internalHost.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public).Single(m => m.Name == "ExecutePSCommandAsync" && m.IsGenericMethodDefinition);
                _invokePSCommandMethodNonGeneric = _internalHost.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public).Single(m => m.Name == "ExecutePSCommandAsync" && !m.IsGenericMethodDefinition);
            }
        }

        private static SemaphoreSlim semaphoreSlim;

        public static void DisablePsesIntellisense()
        {
            var assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(m => m.GetName().Name == "Microsoft.PowerShell.EditorServices");
            var type = assembly.GetType("Microsoft.PowerShell.EditorServices.Services.Symbols.AstOperations");
            var field = type.GetField("s_completionHandle", BindingFlags.Static | BindingFlags.NonPublic);
            semaphoreSlim = (SemaphoreSlim)field.GetValue(null);
            var task = semaphoreSlim.WaitAsync();
        }

        public static void EnablePsesIntellisense()
        {
            semaphoreSlim.Release();
        }
    }

    public static class CommandQueue
    {
        private static Queue<Command> _commands = new Queue<Command>();

        public static Command GetCommand()
        {
            if (_commands.Any())
            {
                return _commands.Dequeue();
            }

            return null;
        }

        public static object InvokeCommand(string value)
        {
            var command = new Command
            {
                Value = value
            };

            _commands.Enqueue(command);

            return GetResults(command);
        }

        public static void SetResults(object results, Command command)
        {
            command.Results = results;
            command.Completed.Set();
        }

        public static object GetResults(Command command)
        {
            if (!command.Completed.WaitOne(20000))
            {
                throw new Exception("Timed out waiting on runspace scheduler.");
            }
            return command.Results;
        }
    }

    public class Command
    {
        public string Value { get; set; }
        public object Results { get; set; }
        public AutoResetEvent Completed { get; } = new AutoResetEvent(false);
    }
}
