using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation.Language;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.TextManager.Interop;
using PowerShellTools.Classification;

namespace PowerShellTools.LanguageService
{
    internal static class BreakpointValidationHelper
    {
        internal static Ast GetScript(IVsEditorAdaptersFactoryService adaptersFactory, IVsTextBuffer buffer)
        {
            ITextBuffer textBuffer = adaptersFactory.GetDataBuffer(buffer);
            Ast scriptAst;

            if (!textBuffer.Properties.TryGetProperty<Ast>(BufferProperties.Ast, out scriptAst))
            {
                return null;
            }

            return scriptAst;
        }

        internal static BreakpointPositionInfo GetBreakpointPosition(Ast script, int lineIndex)
        {
            var astLineIndex = lineIndex + 1;
            if (script == null || astLineIndex < 0 || script.Extent.EndLineNumber < astLineIndex)
            {
                return BreakpointPositionInfo.InvalidBreakpointPosition;
            }
            
            var node = GetValidBreakpointNodeAtLinePosition(script, astLineIndex);

            if (node == null)
            {
                return BreakpointPositionInfo.InvalidBreakpointPosition;
            }

            return GetBreakpointPositionInfo(node);
        }

        internal static Ast GetValidBreakpointNodeAtLinePosition(Ast script, int lineIndex)
        {
            // There might be multiple nodes on a single line so first find them all
            var lineNodes = (IEnumerable<Ast>)script.FindAll(x => x.Extent != null && x.Extent.StartLineNumber == lineIndex, true);

            // And then find the first valid position to set the breakpoint on
            return lineNodes.FirstOrDefault(x => GetBreakpointPositionInfo(x).IsValid);
        }

        internal static TextSpan MapBreakpointPositionToTextSpan(BreakpointPositionInfo breakpointPosition)
        {
            return BreakpointValidationHelper.MapScriptExtentToTextSpan(breakpointPosition.Extent, breakpointPosition.DisplayStyle);
        }

        internal static TextSpan MapScriptExtentToTextSpan(IScriptExtent extent, BreakpointDisplayStyle displayStyle)
        {
            if (extent == null)
            {
                return new TextSpan();
            }

            switch (displayStyle)
            {
                case BreakpointDisplayStyle.Margin:
                    return new TextSpan()
                    {
                        iStartLine = extent.StartLineNumber - 1,
                        iStartIndex = 0,
                        iEndLine = extent.StartLineNumber - 1,
                        iEndIndex = 0
                    };

                case BreakpointDisplayStyle.Line:
                    return new TextSpan()
                    {
                        iStartLine = extent.StartLineNumber - 1,
                        iStartIndex = extent.StartColumnNumber - 1,
                        iEndLine = extent.StartLineNumber - 1,
                        iEndIndex = extent.EndColumnNumber - 1
                    };

                case BreakpointDisplayStyle.Block:
                    return new TextSpan()
                    {
                        iStartLine = extent.StartLineNumber - 1,
                        iStartIndex = extent.StartColumnNumber - 1,
                        iEndLine = extent.EndLineNumber - 1,
                        iEndIndex = extent.EndColumnNumber - 1
                    };

                case BreakpointDisplayStyle.Unset:
                default:
                    return new TextSpan()
                    {
                        iStartLine = extent.StartLineNumber - 1,
                        iStartIndex = 0,
                        iEndLine = extent.StartLineNumber - 1,
                        iEndIndex = 0
                    };
            }
        }

        internal static BreakpointPositionInfo GetBreakpointPositionInfo(Ast node)
        {
            switch(node.GetType().Name)
            {
                case AstDataTypeConstants.ArrayExpression:
                    return new BreakpointPositionInfo(node.Extent, true, BreakpointDisplayStyle.Margin);
                case AstDataTypeConstants.ArrayLiteral:
                    return new BreakpointPositionInfo(node.Extent, true, BreakpointDisplayStyle.Margin);
                case AstDataTypeConstants.AssignmentStatement:
                    return new BreakpointPositionInfo(node.Extent, true, BreakpointDisplayStyle.Line);
                case AstDataTypeConstants.Attribute:
                    return BreakpointPositionInfo.InvalidBreakpointPosition;
                case AstDataTypeConstants.AttributeBase:
                    return BreakpointPositionInfo.InvalidBreakpointPosition;
                case AstDataTypeConstants.AttributeExpression:
                    return new BreakpointPositionInfo(node.Extent, true, BreakpointDisplayStyle.Margin);
                case AstDataTypeConstants.BinaryExpression:
                    return new BreakpointPositionInfo(node.Extent, true, BreakpointDisplayStyle.Margin);
                case AstDataTypeConstants.BlockStatement:
                    return new BreakpointPositionInfo(node.Extent, true, BreakpointDisplayStyle.Margin);
                case AstDataTypeConstants.BreakStatement:
                    return new BreakpointPositionInfo(node.Extent, true, BreakpointDisplayStyle.Line);
                case AstDataTypeConstants.CatchClause:
                    return new BreakpointPositionInfo(node.Extent, true, BreakpointDisplayStyle.Block);
                case AstDataTypeConstants.Command:
                    return new BreakpointPositionInfo(node.Extent, true, BreakpointDisplayStyle.Margin);
                case AstDataTypeConstants.CommandBase:
                    return new BreakpointPositionInfo(node.Extent, true, BreakpointDisplayStyle.Margin);
                case AstDataTypeConstants.CommandElement:
                    return new BreakpointPositionInfo(node.Extent, true, BreakpointDisplayStyle.Margin);
                case AstDataTypeConstants.CommandExpression:
                    return new BreakpointPositionInfo(node.Extent, true, BreakpointDisplayStyle.Margin);
                case AstDataTypeConstants.CommandParameter:
                    return new BreakpointPositionInfo(node.Extent, true, BreakpointDisplayStyle.Margin);
                case AstDataTypeConstants.ConstantExpression:
                    return new BreakpointPositionInfo(node.Extent, true, BreakpointDisplayStyle.Line);
                case AstDataTypeConstants.ContinueStatement:
                    return new BreakpointPositionInfo(node.Extent, true, BreakpointDisplayStyle.Line);
                case AstDataTypeConstants.ConvertExpression:
                    return new BreakpointPositionInfo(node.Extent, true, BreakpointDisplayStyle.Margin);
                case AstDataTypeConstants.DataStatement:
                    return new BreakpointPositionInfo(node.Extent, true, BreakpointDisplayStyle.Margin);
                case AstDataTypeConstants.DoUntilStatement:
                    return GetBreakpointPositionInfo(((DoUntilStatementAst)node).Condition);
                case AstDataTypeConstants.DoWhileStatement:
                    return GetBreakpointPositionInfo(((DoWhileStatementAst)node).Condition);
                case AstDataTypeConstants.ErrorExpression:
                    return BreakpointPositionInfo.InvalidBreakpointPosition;
                case AstDataTypeConstants.ErrorStatement:
                    return BreakpointPositionInfo.InvalidBreakpointPosition;
                case AstDataTypeConstants.ExitStatement:
                    return new BreakpointPositionInfo(node.Extent, true, BreakpointDisplayStyle.Margin);
                case AstDataTypeConstants.ExpandableStringExpression:
                    return new BreakpointPositionInfo(node.Extent, true, BreakpointDisplayStyle.Margin);
                case AstDataTypeConstants.Expression:
                    return new BreakpointPositionInfo(node.Extent, true, BreakpointDisplayStyle.Margin);
                case AstDataTypeConstants.FileRedirection:
                    return new BreakpointPositionInfo(node.Extent, true, BreakpointDisplayStyle.Margin);
                case AstDataTypeConstants.ForEachStatement:
                    return GetBreakpointPositionInfo(((ForEachStatementAst)node).Condition);
                case AstDataTypeConstants.ForStatement:
                    return GetBreakpointPositionInfo(((ForStatementAst)node).Initializer);
                case AstDataTypeConstants.FunctionDefinition:
                    return new BreakpointPositionInfo(node.Extent, true, BreakpointDisplayStyle.Margin);
                case AstDataTypeConstants.FunctionMember:
                    return new BreakpointPositionInfo(node.Extent, true, BreakpointDisplayStyle.Margin);
                case AstDataTypeConstants.HashTable:
                    return new BreakpointPositionInfo(node.Extent, true, BreakpointDisplayStyle.Block);
                case AstDataTypeConstants.IfStatement:
                    return GetBreakpointPositionInfo(((IfStatementAst)node).Clauses[0].Item1);
                case AstDataTypeConstants.IndexExpression:
                    return new BreakpointPositionInfo(node.Extent, true, BreakpointDisplayStyle.Margin);
                case AstDataTypeConstants.InvokeMemberExpression:
                    return new BreakpointPositionInfo(node.Extent, true, BreakpointDisplayStyle.Margin);
                case AstDataTypeConstants.LabelStatement:
                    return new BreakpointPositionInfo(node.Extent, true, BreakpointDisplayStyle.Margin);
                case AstDataTypeConstants.LoopStatement:
                    return GetBreakpointPositionInfo(((LoopStatementAst)node).Condition);
                case AstDataTypeConstants.MemberExpression:
                    return new BreakpointPositionInfo(node.Extent, true, BreakpointDisplayStyle.Margin);
                case AstDataTypeConstants.MergingRedirection:
                    return new BreakpointPositionInfo(node.Extent, true, BreakpointDisplayStyle.Margin);
                case AstDataTypeConstants.NamedAttributeArgument:
                    return BreakpointPositionInfo.InvalidBreakpointPosition;
                case AstDataTypeConstants.NamedBlock:
                    return ((NamedBlockAst)node).Unnamed ? BreakpointPositionInfo.InvalidBreakpointPosition : new BreakpointPositionInfo(node.Extent, true, BreakpointDisplayStyle.Block);
                case AstDataTypeConstants.ParamBlock:
                    return BreakpointPositionInfo.InvalidBreakpointPosition;
                case AstDataTypeConstants.Parameter:
                    return BreakpointPositionInfo.InvalidBreakpointPosition;
                case AstDataTypeConstants.ParenExpression:
                    return new BreakpointPositionInfo(node.Extent, true, BreakpointDisplayStyle.Margin);
                case AstDataTypeConstants.Pipeline:
                    return new BreakpointPositionInfo(node.Extent, true, BreakpointDisplayStyle.Block);
                case AstDataTypeConstants.PipelineBase:
                    return new BreakpointPositionInfo(node.Extent, true, BreakpointDisplayStyle.Line);
                case AstDataTypeConstants.Redirection:
                    return new BreakpointPositionInfo(node.Extent, true, BreakpointDisplayStyle.Margin);
                case AstDataTypeConstants.ReturnStatement:
                    return new BreakpointPositionInfo(node.Extent, true, BreakpointDisplayStyle.Line);
                case AstDataTypeConstants.ScriptBlock:
                    return BreakpointPositionInfo.InvalidBreakpointPosition;
                case AstDataTypeConstants.ScriptBlockExpression:
                    return new BreakpointPositionInfo(node.Extent, true, BreakpointDisplayStyle.Block);
                case AstDataTypeConstants.Statement:
                    return new BreakpointPositionInfo(node.Extent, true, BreakpointDisplayStyle.Block);
                case AstDataTypeConstants.StatementBlock:
                    return new BreakpointPositionInfo(node.Extent, true, BreakpointDisplayStyle.Block);
                case AstDataTypeConstants.StringConstantExpression:
                    return BreakpointPositionInfo.InvalidBreakpointPosition;
                case AstDataTypeConstants.SubExpression:
                    return new BreakpointPositionInfo(node.Extent, true, BreakpointDisplayStyle.Margin);
                case AstDataTypeConstants.SwitchStatement:
                    return GetBreakpointPositionInfo(((SwitchStatementAst)node).Condition);
                case AstDataTypeConstants.ThrowStatement:
                    return new BreakpointPositionInfo(node.Extent, true, BreakpointDisplayStyle.Line);
                case AstDataTypeConstants.TrapStatement:
                    return new BreakpointPositionInfo(node.Extent, true, BreakpointDisplayStyle.Margin);
                case AstDataTypeConstants.TryStatement:
                    return GetBreakpointPositionInfo(((TryStatementAst)node).CatchClauses[0]);
                case AstDataTypeConstants.TypeConstraint:
                    return BreakpointPositionInfo.InvalidBreakpointPosition;
                case AstDataTypeConstants.TypeExpression:
                    return BreakpointPositionInfo.InvalidBreakpointPosition;
                case AstDataTypeConstants.UnaryExpression:
                    return new BreakpointPositionInfo(node.Extent, true, BreakpointDisplayStyle.Block);
                case AstDataTypeConstants.UsingExpression:
                    return BreakpointPositionInfo.InvalidBreakpointPosition;
                case AstDataTypeConstants.VariableExpression:
                    return BreakpointPositionInfo.InvalidBreakpointPosition;
                case AstDataTypeConstants.WhileStatement:
                    return GetBreakpointPositionInfo(((WhileStatementAst)node).Condition);

                // A Ast node will never be a empty line or a comment
                // so we can safely return a default valid breakpoint position
                // and default it to the margin of the code window
                default:
                    return new BreakpointPositionInfo(node.Extent, true, BreakpointDisplayStyle.Margin);
            }
        }
    }
}
