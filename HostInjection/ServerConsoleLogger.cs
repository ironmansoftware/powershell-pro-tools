using PowerShellProTools.Common;
using System;

namespace PowerShellProTools.Host
{
    public class ServerConsoleLogger : IServerLogger
    {
        public void WriteLog(string output)
        {
            Console.WriteLine(output);
        }
    }
}
