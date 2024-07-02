using System;

namespace PowerShellTools.Common.Logging
{
    public interface ILog
    {
        void Error(string message);
        void Error(string message, Exception exception);
        void ErrorFormat(string format, params object[] param);
        void Info(string message);
        void InfoFormat(string format, params object[] param);
        bool IsDebugEnabled { get; }
        void Debug(string message);
        void DebugFormat(string format, params object[] param);
        void Debug(string message, Exception exception);
        void Warn(string message, Exception exception);
        void Warn(string message);
        void WarnFormat(string format, params object[] param);
    }
}
