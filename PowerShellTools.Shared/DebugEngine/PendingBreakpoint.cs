namespace PowerShellTools.DebugEngine
{
    public enum BreakpointType
    {
        Line,
        Command
    }

    /// <summary>
    /// A breakpoint that has yet to be bound by Visual Studio.
    /// </summary>
    public class PendingBreakpoint
    {
        public string Context { get; set; }
        public int Line { get; set; }
        public int Column { get; set; }
        public BreakpointType BreakpointType { get; set; }
        public string Language { get; set; }
    }
}
