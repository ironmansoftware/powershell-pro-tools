using Serilog;
using System;

namespace PowerShellTools.Common.Logging
{
    public class Log : ILog
    {
        private readonly ILogger _log;

        public bool IsDebugEnabled => true;

        public Log(ILogger log)
        {
            _log = log;
        }

        public void Error(string message)
        {
            _log.Error(message);
        }

        public void Error(string message, Exception exception)
        {
            _log.Error(exception, message);
        }

        public void ErrorFormat(string format, params object[] param)
        {
            _log.Error(format, param);
        }

        public void Info(string message)
        {
            _log.Information(message);
        }

        public void InfoFormat(string format, params object[] param)
        {
            _log.Information(format, param);
        }

        public void Debug(string message)
        {
            _log.Debug(message);
        }

        public void DebugFormat(string format, params object[] param)
        {
            _log.Debug(format, param);
        }

        public void Debug(string message, Exception exception)
        {
            _log.Debug(exception, message);
        }

        public void Warn(string message, Exception exception)
        {
            _log.Warning(exception, message);
        }

        public void Warn(string message)
        {
            _log.Warning(message);
        }

        public void WarnFormat(string format, params object[] param)
        {
            _log.Warning(format, param);
        }
    }
}
