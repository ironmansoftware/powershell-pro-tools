using System;
using System.Management.Automation.Language;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.TextManager.Interop;
using PowerShellTools.Classification;
using PowerShellTools.Common;

namespace PowerShellTools.Commands.UserInterface
{
    /// <summary>
    /// A helper class for getting script parameters.
    /// </summary>
    internal static class ParameterEditorHelper
    {
        public static ScriptParameterResult PromptForScriptParameterValues(ParamBlockAst paramBlock)
        {
            string scriptArgs;
            if (ShowParameterEditor(paramBlock, out scriptArgs) == true)
            {
                return new ScriptParameterResult(scriptArgs, true);
            }
            return new ScriptParameterResult(null, false);
        }

        private static bool? ShowParameterEditor(ParamBlockAst paramBlockAst, out string scriptArgs)
        {
            scriptArgs = String.Empty;
            var model = PowerShellParseUtilities.ParseParameters(paramBlockAst);
            var viewModel = new ParameterEditorViewModel(model);
            var view = new ParameterEditorView(viewModel);

            bool? wasOkClicked = view.ShowModal();
            if (wasOkClicked != true)
            {
                return wasOkClicked;
            }

            scriptArgs = GenerateScripArgsFromModel(model);
            return wasOkClicked;
        }

        internal static string GenerateScripArgsFromModel(ParameterEditorModel model)
        {
            Arguments.ValidateNotNull<ParameterEditorModel>(model, "model");
            string scriptArgs = String.Empty;
            foreach (var p in model.Parameters)
            {
                if (p.Value != null)
                {
                    switch (p.Type)
                    {
                        case ParameterType.Boolean:
                            scriptArgs += WrapParameterName(p.Name);
                            scriptArgs += String.Format(" ${0}", p.Value.ToString());
                            break;

                        case ParameterType.Switch:
                            scriptArgs += WrapParameterName(p.Name);
                            scriptArgs += String.Format(":${0}", p.Value.ToString());
                            break;

                        case ParameterType.Byte:
                        case ParameterType.Int32:
                        case ParameterType.Int64:

                        case ParameterType.Float:
                        case ParameterType.Double:
                        case ParameterType.Decimal:

                        case ParameterType.Array:
                        case ParameterType.Unknown:
                            scriptArgs += WrapParameterName(p.Name);
                            scriptArgs += WrapValue(p.Value.ToString());
                            break;

                        case ParameterType.Enum:
                        case ParameterType.Char:
                        case ParameterType.String:
                            scriptArgs += WrapParameterName(p.Name);
                            scriptArgs += p.Value is string ? WrapStringValueWithQuotes(p.Value as string) : p.Value;
                            break;
                    }
                }
            }

            foreach (var p in model.CommonParameters)
            {
                if (p.Value != null)
                {
                    switch (p.Type)
                    {
                        case ParameterType.Switch:
                            scriptArgs += WrapParameterName(p.Name);
                            scriptArgs += String.Format(":${0}", p.Value.ToString());
                            break;

                        case ParameterType.Enum:
                            if (!String.IsNullOrEmpty(p.Value as string))
                            {
                                scriptArgs += WrapParameterName(p.Name);
                                scriptArgs += WrapValue(p.Value as string);
                            }
                            break;

                        default:
                            scriptArgs += WrapParameterName(p.Name);
                            scriptArgs += WrapValue(p.Value as string);
                            break;
                    }
                }
            }

            return scriptArgs;
        }

        public static ParamBlockAst GetScriptParameters(IVsEditorAdaptersFactoryService adaptersFactory, IVsTextManager textManager)
        {
            IVsTextView vsTextView;
            ParamBlockAst paramBlock = null;

            //Returns the active or previously active view.
            //
            // Parameters:
            //   fMustHaveFocus:
            //     [in] If true, then the current UI active view is returned. If false, then
            //     the last active view is returned, regardless of whether this view is currently
            //     UI active.
            //
            //   pBuffer:
            //     [in] Pass null for pBuffer to get the previously active code view, regardless
            //     of the text buffer that it was associated with. If you pass in a valid pointer
            //     to a buffer, then you are returned the last active view for that particular
            //     buffer.
            //
            //   ppView:
            //     [out] Pointer to the Microsoft.VisualStudio.TextManager.Interop.IVsTextView
            //     interface.
            textManager.GetActiveView(1, null, out vsTextView);
            if (vsTextView == null)
            {
                return null;
            }

            IVsTextLines textLines;
            vsTextView.GetBuffer(out textLines);
            ITextBuffer textBuffer = adaptersFactory.GetDataBuffer(textLines as IVsTextBuffer);
            Ast scriptAst;
            if (!textBuffer.Properties.TryGetProperty<Ast>(BufferProperties.Ast, out scriptAst))
            {
                return null;
            }

            PowerShellParseUtilities.HasParamBlock(scriptAst, out paramBlock);
            return paramBlock;
        }

        private static string WrapParameterName(string name)
        {
            return String.Format(" -{0}", name);
        }

        private static string WrapValue(string value)
        {
            return String.Format(" {0}", value);
        }

        private static string WrapStringValueWithQuotes(string value)
        {
            if (value.StartsWith("\"", StringComparison.OrdinalIgnoreCase) && value.EndsWith("\"", StringComparison.OrdinalIgnoreCase))
            {
                return value;
            }
            return String.Format(" \"{0}\"", value);
        }
    }
}
