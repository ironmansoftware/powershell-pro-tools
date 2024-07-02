using System.Text;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using NSubstitute;

namespace PowerShellToolsPro.Test.Mocking
{
	public class TextEditor
	{
		private readonly StringBuilder _contents;

		public TextEditor(string contents, int caretPosition)
		{
			_contents = new StringBuilder(contents);

			var wpfTextView = Substitute.For<IWpfTextView>();
			var textBuffer = Substitute.For<ITextBuffer>();
			var currentSnapshot = Substitute.For<ITextSnapshot>();
			var selection = Substitute.For<ITextSelection>();
			var textEdit = Substitute.For<ITextEdit>();

			wpfTextView.TextBuffer.Returns(textBuffer);
			textBuffer.CurrentSnapshot.Returns(currentSnapshot);
			textBuffer.CreateEdit().Returns(textEdit);

			textEdit.Insert(Arg.Any<int>(), Arg.Any<string>())
				.Returns(true)
				.AndDoes(x =>
				{
					_contents.Insert(x.Arg<int>(), x.Arg<string>());
				});

			wpfTextView.TextSnapshot.Returns(currentSnapshot);
			currentSnapshot.GetText().Returns(x => _contents.ToString());
			currentSnapshot.Length.Returns(x => _contents.Length);

			wpfTextView.Selection.Returns(selection);
			selection.ActivePoint.Returns(x => new VirtualSnapshotPoint(new SnapshotPoint(currentSnapshot, caretPosition)));

			TextView = wpfTextView;
		}
		
		public IWpfTextView TextView { get; }
	}
}
