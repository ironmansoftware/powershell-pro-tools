using System;
using System.Management.Automation.Host;

namespace PowerShellTools.Common
{
    public class PowerShellRawHostOptions
    {
        public ConsoleColor ForegroundColor;

        public ConsoleColor BackgroundColor;

        public Coordinates CursorPosition;

        public Coordinates WindowPosition;

        public int CursorSize;

        public string WindowTitle;
    }
}
