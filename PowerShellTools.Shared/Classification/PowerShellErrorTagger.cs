using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;

namespace PowerShellTools.Classification
{
    /// <summary>
    /// Provides error tagging for PowerShell scripts. 
    /// </summary>
    /// <remarks>
    /// This code takes advantage of the already parsed AST to gather any errors while parsing
    /// and to tag the spans that are in an error state.
    /// </remarks>
    internal class PowerShellErrorTagger : ITagger<ErrorTag>, INotifyTagsChanged
    {
	    public event EventHandler<SnapshotSpanEventArgs> TagsChanged;
	    private ITextBuffer _textBuffer { get; set; }

	    internal PowerShellErrorTagger(ITextBuffer sourceBuffer)
	    {
	        _textBuffer = sourceBuffer;
	        _textBuffer.Properties.AddProperty(typeof(PowerShellErrorTagger).Name, this);
	        _textBuffer.ContentTypeChanged += Buffer_ContentTypeChanged;
	    }

	    public IEnumerable<ITagSpan<ErrorTag>> GetTags(NormalizedSnapshotSpanCollection spans)
	    {
	        var currentSnapshot = _textBuffer.CurrentSnapshot;
	        if (currentSnapshot.Length == 0) yield break;

	        List<TagInformation<ErrorTag>> list;
	        _textBuffer.Properties.TryGetProperty<List<TagInformation<ErrorTag>>>(BufferProperties.TokenErrorTags, out list);
	        if (list == null)
	        {
		    list = new List<TagInformation<ErrorTag>>();
	        }

	        foreach (var tagSpan in list.Select(current => current.GetTagSpan(currentSnapshot)).Where(tagSpan => tagSpan != null))
	        {
		    yield return tagSpan;
	        }
	    }

	    public void OnTagsChanged(SnapshotSpan span)
	    {
	        var tagsChanged = TagsChanged;
	        if (tagsChanged != null)
	        {
		        tagsChanged(this, new SnapshotSpanEventArgs(span));
	        }
	    }

	    private void Buffer_ContentTypeChanged(object sender, ContentTypeChangedEventArgs e)
	    {
	        _textBuffer.ContentTypeChanged -= Buffer_ContentTypeChanged;
	        _textBuffer.Properties.RemoveProperty(typeof(PowerShellErrorTagger).Name);
	    }
    }
}
