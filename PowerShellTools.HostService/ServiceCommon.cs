using PowerShellTools.Common.Logging;

namespace PowerShellTools.HostService
{
    internal sealed class ServiceCommon
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
            Logger.InfoFormat(msg, args);
        }

        /// <summary>
        /// Log a callback event.
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="args"></param>
        public static void LogCallbackEvent(string msg, params object[] args)
        {
            Logger.WarnFormat(msg, args);
        }
    }
}
