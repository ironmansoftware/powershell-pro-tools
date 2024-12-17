using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace PowerShellProTools.Common
{
    public static class ParentProcessWatcher
    {
        private static Process devEnv;
        public static void WatchProcess(int processId)
        {
            try
            {
                devEnv = Process.GetProcessById(processId);
                devEnv.EnableRaisingEvents = true;
                devEnv.Exited += (sender, e) => Environment.Exit(0);
            }
            catch (Exception)
            {
                
            }
        }
    }
}
