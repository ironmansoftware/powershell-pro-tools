using System;
using System.Collections.Generic;
using System.Management.Automation.Language;
using System.Text;

namespace PowerShellProTools.Host.Refactoring
{
    public class IntroduceUsing : IRefactoring
    {
        public RefactorInfo RefactorInfo => new RefactorInfo("Introduce using namespace", RefactorType.IntroduceUsingNamespace);

        public bool CanRefactor(TextEditorState state, Ast ast, IEnumerable<RefactoringProperty> properties)
        {
            return ast.GetAstUnderCursor<TypeExpressionAst>(state) != null;
        }

        public IEnumerable<TextEdit> Refactor(TextEditorState state, Ast ast, IEnumerable<RefactoringProperty> properties)
        {
            var typeExpression = ast.GetAstUnderCursor<TypeExpressionAst>(state);

            if (typeExpression == null)
            {
                yield break;
            }

            var fullTypeName = typeExpression.TypeName.FullName;

            var index = fullTypeName.LastIndexOf('.');

            if (index < 0) yield break;

            var namespaceName = fullTypeName.Substring(0, index);
            var typeName = fullTypeName.Substring(index + 1, fullTypeName.Length - index - 1);

            yield return new TextEdit
            {
                Content = $"[{typeName}]",
                Type = TextEditType.Replace,
                Start = new TextPosition
                {
                    Line = typeExpression.Extent.StartLineNumber - 1,
                    Character = typeExpression.Extent.StartColumnNumber - 1
                },
                End = new TextPosition
                {
                    Line = typeExpression.Extent.EndLineNumber - 1,
                    Character = typeExpression.Extent.EndColumnNumber - 1
                },
                FileName = state.FileName
            };

            yield return new TextEdit
            {
                Uri = state.Uri,
                Content = $"using namespace {namespaceName}{Environment.NewLine}",
                FileName = state.FileName,
                Type = TextEditType.Insert,
                Start = new TextPosition
                {
                    Line = 0,
                    Character = 0,
                    Index = 0
                }
            };
        }
    }
}
