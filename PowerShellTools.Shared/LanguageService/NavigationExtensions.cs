using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management.Automation.Language;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using System;
using Common.Analysis;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell;
using EnvDTE;
using EnvDTE80;

namespace PowerShellTools.LanguageService
{
    /// <summary>
    /// Helper functions for finding and navigating to objects
    /// </summary>
    internal static class NavigationExtensions
    {
        /// <summary>
        /// Finds all possible function definitions for the token at the caret position
        /// </summary>
        public static IEnumerable<FunctionDefinitionAst> FindFunctionDefinitions(Ast script, ITextSnapshot currentSnapshot, int caretPosition)
        {
            if (script != null)
            {
                var reference = script.Find(node =>
                    node is CommandAst &&
                    caretPosition >= node.Extent.StartOffset &&
                    caretPosition <= node.Extent.EndOffset, true) as CommandAst;

                FunctionDefinitionAst definition = null;
                if (reference != null)
                {
                    return FindDefinition(reference);
                }
                else
                {
                    definition = script.Find(node =>
                    {
                        if (node is FunctionDefinitionAst)
                        {
                            var functionNameSpan = GetFunctionNameSpan(node as FunctionDefinitionAst);
                            return functionNameSpan.HasValue &&
                                   caretPosition >= functionNameSpan.Value.Start &&
                                   caretPosition <= functionNameSpan.Value.End;
                        }

                        return false;
                    }, true) as FunctionDefinitionAst;

                    if (definition != null)
                    {
                        return new List<FunctionDefinitionAst>() { definition };
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// 1. Moves the caret to the end of the name of the function.
        /// 2. Highlights the name of the function.
        /// 3. Updates the viewport so that the function name will be centered.
        /// 4. Moves focus to the text view to ensure the user can continue typing.
        /// </summary>
        public static void NavigateToFunctionDefinition(ITextView textView, FunctionDefinitionAst definition)
        {
            var functionNameSpan = GetFunctionNameSpan(definition);

            if (functionNameSpan.HasValue && functionNameSpan.Value.Start >= 0 && functionNameSpan.Value.End <= textView.TextBuffer.CurrentSnapshot.Length)
            {
                var snapshotSpan = new SnapshotSpan(textView.TextBuffer.CurrentSnapshot, functionNameSpan.Value);
                textView.Caret.MoveTo(new SnapshotPoint(textView.TextBuffer.CurrentSnapshot, snapshotSpan.End));
                textView.Selection.Select(snapshotSpan, false);
                textView.ViewScroller.EnsureSpanVisible(snapshotSpan, EnsureSpanVisibleOptions.AlwaysCenter);
                ((Control)textView).Focus();
            }
        }

        /// <summary>
        /// 1. Moves the caret to the end of the name of the function.
        /// 2. Highlights the name of the function.
        /// 3. Updates the viewport so that the function name will be centered.
        /// 4. Moves focus to the text view to ensure the user can continue typing.
        /// </summary>
        public static void NavigateToDefinition(ITextView textView, IDefinition definition)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var dte2 = (DTE2)Package.GetGlobalService(typeof(SDTE));
            if (dte2 != null)
            {
                var document = dte2.Solution.FindProjectItem(definition.FileName);
                var window = document.Open();
                window.Visible = true;

                var selection = (TextSelection)dte2.ActiveDocument.Selection;
                selection.GotoLine(definition.Line, true);
            }
        }


        /// <summary>
        /// 1. Moves the caret to the end of the name of the function.
        /// 2. Highlights the name of the function.
        /// 3. Updates the viewport so that the function name will be centered.
        /// 4. Moves focus to the text view to ensure the user can continue typing.
        /// </summary>
        public static void NavigateToFunctionDefinition(ITextView textView, FunctionDefinition definition)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var dte2 = (DTE2)Package.GetGlobalService(typeof(SDTE));
            if (dte2 != null)
            {
                var document = dte2.Solution.FindProjectItem(definition.FileName);
                var window = document.Open();
                window.Visible = true;

                var selection = (TextSelection)dte2.ActiveDocument.Selection;
                selection.GotoLine(definition.Line, true);
            }
        }

        /// <summary>
        /// 1. Moves the caret to the specified index in the current snapshot.
        /// 2. Updates the viewport so that the caret will be centered.
        /// 3. Moves focus to the text view to ensure the user can continue typing.
        /// </summary>
        public static void NavigateToLocation(ITextView textView, int location)
        {
            textView.Caret.MoveTo(new SnapshotPoint(textView.TextBuffer.CurrentSnapshot, location));
            textView.ViewScroller.EnsureSpanVisible(new SnapshotSpan(textView.TextBuffer.CurrentSnapshot, location, 1), EnsureSpanVisibleOptions.AlwaysCenter);
            ((Control)textView).Focus();
        }

        /// <summary>
        /// Returns the span containing the function definition name
        /// </summary>
        public static Span? GetFunctionNameSpan(FunctionDefinitionAst definition)
        {
            if (definition != null)
            {
                // To find the function name, we look for the last instance of the name before the body or parameters are defined
                var paramOrBodyStart = definition.Extent.Text.IndexOf(definition.Extent.Text.FirstOrDefault(c => c.Equals('(') || c.Equals('{')));
                var nameStart = definition.Extent.Text.Substring(0, paramOrBodyStart).LastIndexOf(definition.Name);
                if (nameStart >= 0)
                {
                    return new Span(definition.Extent.StartOffset + nameStart, definition.Name.Length);
                }
            }

            return null;
        }

        private static IEnumerable<FunctionDefinitionAst> FindDefinition(CommandAst reference)
        {
            var scope = GetParentScope(reference);
            if (scope != null)
            {
                // If in the same scope as the reference call, the function must be defined before the call
                var definitions = scope.Statements.OfType<FunctionDefinitionAst>().
                    Where(def => def.Name.Equals(reference.GetCommandName(), StringComparison.OrdinalIgnoreCase) && def.Extent.EndOffset <= reference.Extent.StartOffset);

                if (definitions.Any())
                {
                    // Since we are in the same scope as the reference, we always go to the last function defined before the call
                    return new List<FunctionDefinitionAst>()
                    {
                        definitions.Last()
                    };
                }

                while ((scope = GetParentScope(scope)) != null)
                {
                    definitions = scope.Statements.OfType<FunctionDefinitionAst>().Where(def => def.Name.Equals(reference.GetCommandName(), StringComparison.OrdinalIgnoreCase));

                    if (definitions.Any())
                    {
                        return new List<FunctionDefinitionAst>(definitions);
                    }
                }
            }

            return null;
        }

        private static NamedBlockAst GetParentScope(Ast node)
        {
            node = node.Parent;

            while (node != null && !(node is NamedBlockAst))
            {
                node = node.Parent;
            }

            return node as NamedBlockAst;
        }
    }
}