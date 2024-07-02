using System;
using Microsoft.VisualStudio.Text;

namespace PowerShellTools.LanguageService
{
    internal static class IndentUtilities
    {
	/// <summary>
	/// Get current indentation of targeting line.
	/// </summary>
	/// <param name="lineText">Text of the targeting line.</param>
	/// <param name="tabSize">Tab size.</param>
	/// <returns>The indentation of the targeting line.</returns>
	public static int GetCurrentLineIndentation(string lineText, int tabSize)
	{
	    int indentSize = 0;
	    for (int i = 0; i < lineText.Length; i++)
	    {
		if (lineText[i] == ' ')
		{
		    indentSize++;
		}
		else if (lineText[i] == '\t')
		{
		    indentSize += tabSize;
		}
		else
		{
		    break;
		}
	    }

	    return indentSize;
	}

	/// <summary>
	/// Get the first preceding non-blank line and its text.
	/// </summary>
	/// <param name="line">Current line.</param>
	/// <param name="baselineText">The first preceding non-blank line text.</param>
	/// <param name="baseline">The first preceding non-blank line.</param>
	public static void SkipPrecedingBlankLines(ITextSnapshotLine line, out string baselineText, out ITextSnapshotLine baseline)
	{
	    string text;
	    while (line.LineNumber > 0)
	    {
		line = line.Snapshot.GetLineFromLineNumber(line.LineNumber - 1);
		text = line.GetText();
		if (!String.IsNullOrWhiteSpace(text))
		{
		    baseline = line;
		    baselineText = text;
		    return;
		}
	    }
	    baselineText = line.GetText();
	    baseline = line;
	}
    }
}
