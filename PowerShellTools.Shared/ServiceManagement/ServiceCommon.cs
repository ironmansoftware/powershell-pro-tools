using PowerShellTools.Common.Logging;
using System.Diagnostics;

namespace PowerShellTools.HostService
{
    public sealed class ServiceCommon
    {

        public static object RunspaceLock = new object();

        private static readonly ILog Logger = LogManager.GetLogger(typeof(ServiceCommon));

        /// <summary>
        /// Log a message.
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="args"></param>
        public static void Log(string msg, params object[] args)
        {
#if DEBUG
            Debug.WriteLine(string.Format(msg, args));
#endif
            Logger.InfoFormat(msg, args);
        }

        /// <summary>
        /// Log a callback event.
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="args"></param>
        public static void LogCallbackEvent(string msg, params object[] args)
        {
#if DEBUG
            Debug.WriteLine(string.Format(msg, args));
#endif
            Logger.WarnFormat(msg, args);
        }
    }
}
