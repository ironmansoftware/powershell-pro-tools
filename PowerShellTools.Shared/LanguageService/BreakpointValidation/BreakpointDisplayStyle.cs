
namespace PowerShellTools.LanguageService
{
    internal enum BreakpointDisplayStyle
    {
        /// <summary>
        /// Default value for BreakpointDisplayStyle
        /// </summary>
        Unset,

        /// <summary>
        /// Displays the breakpoint marker in the margin of the code editor
        /// </summary>
        Margin,

        
        /// <summary>
        /// Diplays the breakpoint with a highlight around the first line of a code node
        /// </summary>
        Line,

        /// <summary>
        /// Diplays the breakpoint with a highlight around the entire code node
        /// </summary>
        Block
    }
}
