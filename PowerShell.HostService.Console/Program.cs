using PowerShellTools.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PowerShell.HostService.Console
{
    internal sealed class Program
    {
        private static AutoResetEvent _processExitEvent = new AutoResetEvent(false);

        public static void Main(string[] args)
        {
            try
            {
                // Process command line args
                if (args.Length != 1 || !(args[0].StartsWith(Constants.ConsoleProcessIdArg, StringComparison.OrdinalIgnoreCase)))
                {
                    return;
                }

                int _powerShellHostProcessId;
                if (!int.TryParse(args[0].Remove(0, Constants.ConsoleProcessIdArg.Length),
                                NumberStyles.None,
                                CultureInfo.InvariantCulture,
                                out _powerShellHostProcessId))
                {
                    return;
                }

                // get parent process (the PowerShell host process)
                Process p = Process.GetProcessById(_powerShellHostProcessId);

                if (p != null)
                {
                    p.EnableRaisingEvents = true;

                    // Make sure the console process terminates when host process exits.
                    p.Exited += new EventHandler(
                        (sender, eventArgs) =>
                        {
                            _processExitEvent.Set();
                        });
                }

                _processExitEvent.WaitOne();
            }
            catch { }
        }
    }
}
