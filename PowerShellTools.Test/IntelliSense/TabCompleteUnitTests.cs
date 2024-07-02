using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.Text;
using Moq;
using PowerShellTools.Intellisense;

namespace PowerShellTools.Test.IntelliSense
{
    [TestClass]
    [Ignore]
    public class TabCompleteUnitTests
    {
        private const string TestScript = "Begin tab complete test - {0} - end test";
        private const int TestStartPoint = 26;
        private static readonly List<Completion> TestCompletions = new List<Completion>()
        {
            new Completion("abc"),
            new Completion("testtesttest"),
            new Completion("1")
        };

        private string _mockedScript;
        private int _mockedCaret;

        [TestMethod]
        public void TestCyclingReplaceNext()
        {
            var selectedCompletion = TestCompletions[1];

            _mockedScript = String.Format(TestScript, selectedCompletion.DisplayText);
            _mockedCaret = TestStartPoint + selectedCompletion.DisplayText.Length;

            var textBuffer = TextBufferMock();

            var tabCompleteSession = new TabCompleteSession(TestCompletions, new CompletionSelectionStatus(selectedCompletion, true, true), TestStartPoint);

            tabCompleteSession.ReplaceWithNextCompletion(textBuffer, _mockedCaret);
            Assert.AreEqual(GetExpectedScript(2), _mockedScript);

            tabCompleteSession.ReplaceWithNextCompletion(textBuffer, _mockedCaret);
            Assert.AreEqual(GetExpectedScript(0), _mockedScript);

            tabCompleteSession.ReplaceWithNextCompletion(textBuffer, _mockedCaret);
            Assert.AreEqual(GetExpectedScript(1), _mockedScript);

            tabCompleteSession.ReplaceWithNextCompletion(textBuffer, _mockedCaret);
            Assert.AreEqual(GetExpectedScript(2), _mockedScript);
        }

        [TestMethod]
        public void TestCyclingReplacePrevious()
        {
            var selectedCompletion = TestCompletions[1];

            _mockedScript = String.Format(TestScript, selectedCompletion.DisplayText);
            _mockedCaret = TestStartPoint + selectedCompletion.DisplayText.Length;

            var textBuffer = TextBufferMock();

            var tabCompleteSession = new TabCompleteSession(TestCompletions, new CompletionSelectionStatus(selectedCompletion, true, true), TestStartPoint);

            tabCompleteSession.ReplaceWithPreviousCompletion(textBuffer, _mockedCaret);
            Assert.AreEqual(GetExpectedScript(0), _mockedScript);

            tabCompleteSession.ReplaceWithPreviousCompletion(textBuffer, _mockedCaret);
            Assert.AreEqual(GetExpectedScript(2), _mockedScript);

            tabCompleteSession.ReplaceWithPreviousCompletion(textBuffer, _mockedCaret);
            Assert.AreEqual(GetExpectedScript(1), _mockedScript);

            tabCompleteSession.ReplaceWithPreviousCompletion(textBuffer, _mockedCaret);
            Assert.AreEqual(GetExpectedScript(0), _mockedScript);
        }

        [TestMethod]
        public void TestReplaceNextWithNoSelectedCompletion()
        {
            var textBuffer = TextBufferMock();

            var tabCompleteSession = CreateTabSessionWithNoSelectedCompletion(new CompletionSelectionStatus(null, false, false));
            tabCompleteSession.ReplaceWithNextCompletion(textBuffer, _mockedCaret);
            Assert.AreEqual(GetExpectedScript(0), _mockedScript);

            tabCompleteSession = CreateTabSessionWithNoSelectedCompletion(new CompletionSelectionStatus(TestCompletions[0], false, false));
            tabCompleteSession.ReplaceWithNextCompletion(textBuffer, _mockedCaret);
            Assert.AreEqual(GetExpectedScript(0), _mockedScript);

            tabCompleteSession = CreateTabSessionWithNoSelectedCompletion(new CompletionSelectionStatus(new Completion("bogusCompletion"), true, false));
            tabCompleteSession.ReplaceWithNextCompletion(textBuffer, _mockedCaret);
            Assert.AreEqual(GetExpectedScript(0), _mockedScript);
        }

        [TestMethod]
        public void TestReplacePreviousWithNoSelectedCompletion()
        {
            var textBuffer = TextBufferMock();

            var tabCompleteSession = CreateTabSessionWithNoSelectedCompletion(new CompletionSelectionStatus(null, false, false));
            tabCompleteSession.ReplaceWithPreviousCompletion(textBuffer, _mockedCaret);
            Assert.AreEqual(GetExpectedScript(2), _mockedScript);

            tabCompleteSession = CreateTabSessionWithNoSelectedCompletion(new CompletionSelectionStatus(TestCompletions[0], false, false));
            tabCompleteSession.ReplaceWithPreviousCompletion(textBuffer, _mockedCaret);
            Assert.AreEqual(GetExpectedScript(2), _mockedScript);

            tabCompleteSession = CreateTabSessionWithNoSelectedCompletion(new CompletionSelectionStatus(new Completion("bogusCompletion"), true, false));
            tabCompleteSession.ReplaceWithPreviousCompletion(textBuffer, _mockedCaret);
            Assert.AreEqual(GetExpectedScript(2), _mockedScript);
        }

        private TabCompleteSession CreateTabSessionWithNoSelectedCompletion(CompletionSelectionStatus selectionStatus)
        {
            _mockedScript = String.Format(TestScript, String.Empty);
            _mockedCaret = TestStartPoint;

            return new TabCompleteSession(TestCompletions, selectionStatus, TestStartPoint);
        }

        private string GetExpectedScript(int index)
        {
            return String.Format(TestScript, TestCompletions[index].DisplayText);
        }

        private ITextBuffer TextBufferMock()
        {
            var textBufferMock = new Mock<ITextBuffer>();
            textBufferMock.Setup(t => t.Replace(It.IsAny<Span>(), It.IsAny<string>()))
                .Callback<Span, string>((replaceSpan, replaceWith) =>
                {
                    _mockedScript = _mockedScript.Remove(replaceSpan.Start, replaceSpan.Length);
                    _mockedScript = _mockedScript.Insert(replaceSpan.Start, replaceWith);
                    _mockedCaret = replaceSpan.Start + replaceWith.Length;
                });
            return textBufferMock.Object;
        }
    }
}
