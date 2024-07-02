using PowerShellTools.Common.Logging;

namespace PowerShellTools.Diagnostics
{
    public class DiagnosticConfiguration
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(DiagnosticConfiguration));

        public static void EnableDiagnostics()
        {
            LogManager.SetLoggingLevel("All");
            Log.Info("Diagnostics enabled.");
        }
    }
}
