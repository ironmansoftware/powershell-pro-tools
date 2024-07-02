namespace IronmanPowerShellHost.Executors
{
    internal interface IExecutor
    {
        int Run(string script, string[] args);
    }
}
