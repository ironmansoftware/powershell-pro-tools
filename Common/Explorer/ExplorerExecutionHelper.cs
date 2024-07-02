using System.Management.Automation;

namespace PowerShellTools.Common
{
    public static class ExplorerExecutionHelper
    {
        public static PSDataCollection<T> ExecuteCommand<T>(string command)
        {
            PSDataCollection<T> outputCollection = new PSDataCollection<T>();

            using (PowerShell powerShell = PowerShell.Create())
            {
                powerShell.AddScript(command);
                powerShell.Invoke<T>(null, outputCollection);
            }

            return outputCollection;
        }
    }
}
