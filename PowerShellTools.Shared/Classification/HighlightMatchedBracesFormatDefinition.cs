using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;
using System.Windows.Media;

namespace PowerShellTools.Classification
{
    /// <summary>
    /// The format definition for the Highlight matced braces tag.  Provides color/ordering and other formatting definitions.
    /// </summary>
    [Export(typeof(EditorFormatDefinition))]
    [Name(PowerShellConstants.HighlightMatchedBracesFormatDefinition)]
    [UserVisible(true)]
    internal sealed class HighlightMatchedBracesFormatDefinition : MarkerFormatDefinition
    {
	/// <summary>
	/// The Constructor.
	/// </summary>
	[ImportingConstructor]
	public HighlightMatchedBracesFormatDefinition()
	{
	    // Fill Color
	    // This is the same default color as C#/VB's HighlightedReferenceMarkerDefinition. We cannot use their definition without Error.
	    // This is for the color if not overridden. For Dark & High Contrast, see PowerShellToolsColors.pkgdef
	    BackgroundColor = Color.FromRgb(219, 224, 204); 

	    DisplayName = Resources.HighlightMatchedBracesTagDisplayName;
	    ZOrder = 5;
	}
    }

}
