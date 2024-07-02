using Serilog;
using System;
using System.IO;

namespace PowerShellTools.Common.Logging
{
    public class LogManager
    {
        private static bool _initialized;

        public static ILog GetLogger(string type)
        {
            Initialize();

            return new Log(Serilog.Log.Logger);
        }

        public static ILog GetLogger(Type type)
        {
            Initialize();

            return new Log(Serilog.Log.Logger);
        }

        private static void Initialize()
        {
            if (_initialized) return;

            var folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "PowerShell Tools for Visual Studio");
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            Serilog.Log.Logger = new LoggerConfiguration()
                .WriteTo.File(Path.Combine(folder, "log-.txt"), rollingInterval: RollingInterval.Day)
                .MinimumLevel.Debug()
                .CreateLogger();

            _initialized = true;
        }

        public static void SetLoggingLevel(string levelString)
        {
            Initialize();
        }

    }
}
