using System;
using System.Collections.Generic;
using System.Management.Automation.Language;
using System.Text;

namespace PowerShellProTools.Host.Refactoring
{
    public class SplitPipe : IRefactoring
    {
        public RefactorInfo RefactorInfo => new RefactorInfo("Split pipeline", RefactorType.SplitPipe);

        public bool CanRefactor(TextEditorState state, Ast ast, IEnumerable<RefactoringProperty> properties)
        {
            var command = ast.GetAstUnderCursor<PipelineAst>(state);
            return command != null && command.PipelineElements.Count > 1;
        }

        public IEnumerable<TextEdit> Refactor(TextEditorState state, Ast ast, IEnumerable<RefactoringProperty> properties)
        {
            var pipeline = ast.GetAstUnderCursor<PipelineAst>(state);

            if (pipeline == null)
            {
                yield break;
            }

            var stringBuilder = new StringBuilder();
            var variable = 0;
            foreach(var element in pipeline.PipelineElements)
            {
                if (variable == 0)
                {
                    stringBuilder.AppendFormat("$Result = {0}{1}", element.ToString(), Environment.NewLine);
                }
                else if (variable == pipeline.PipelineElements.Count - 1)
                {
                    stringBuilder.AppendFormat("$Result | {0}{1}", element.ToString(), Environment.NewLine);
                }
                else
                {
                    stringBuilder.AppendFormat("$Result = $Result | {0}{1}", element.ToString(), Environment.NewLine);
                }
                variable++;
            }

            stringBuilder.AppendLine("Remove-Variable -Name 'Result'");

            yield return new TextEdit
            {
                Uri = state.Uri,
                Content = stringBuilder.ToString(),
                Type = TextEditType.Replace,
                Start = new TextPosition
                {
                    Line = pipeline.Extent.StartLineNumber - 1,
                    Character = pipeline.Extent.StartColumnNumber - 1,
                    Index = pipeline.Extent.StartOffset
                },
                End = new TextPosition
                {
                    Line = pipeline.Extent.EndLineNumber - 1,
                    Character = pipeline.Extent.EndColumnNumber - 1,
                    Index = pipeline.Extent.EndOffset
                },
                FileName = state.FileName
            };
        }
    }
}
