using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Text;

namespace PowerShellProTools.Common
{
    public class PipeLogger : IServerLogger
    {
        private NamedPipeServerStream _pipe;

        public PipeLogger(string pipeName)
        {
            _pipe = new NamedPipeServerStream(pipeName + "_log", PipeDirection.Out);
            _pipe.WaitForConnection();
        }

        public void WriteLog(string message)
        {
            var bytes = Encoding.UTF8.GetBytes(message);
            var base64 = Convert.ToBase64String(bytes);
            var data = Encoding.UTF8.GetBytes(base64);
            _pipe.Write(data, 0, data.Length);
        }
    }
}
