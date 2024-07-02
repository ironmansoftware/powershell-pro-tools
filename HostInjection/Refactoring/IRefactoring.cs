using System;
using System.Collections.Generic;
using System.Management.Automation.Language;

namespace PowerShellProTools.Host.Refactoring
{
    public interface IRefactoring
    {
        RefactorInfo RefactorInfo { get; }
        bool CanRefactor(TextEditorState state, Ast ast, IEnumerable<RefactoringProperty> properties);
        IEnumerable<TextEdit> Refactor(TextEditorState state, Ast ast, IEnumerable<RefactoringProperty> properties);
    }
}
