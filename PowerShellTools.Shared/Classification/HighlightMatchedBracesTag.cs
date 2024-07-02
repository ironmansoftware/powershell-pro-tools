using Microsoft.VisualStudio.Text.Tagging;

namespace PowerShellTools.Classification
{
    /// <summary>
    /// The highlight matched braces Tag.
    /// </summary>
    internal sealed class HighlightMatchedBracesTag : TextMarkerTag
    {
        /// <summary>
        /// The constructor.
        /// </summary>
        public HighlightMatchedBracesTag()
            : base(PowerShellConstants.HighlightMatchedBracesFormatDefinition)
        {

        }
    }

}
