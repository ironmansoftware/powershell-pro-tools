using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Commanding;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Editor.Commanding.Commands;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.Threading;
using Microsoft.VisualStudio.Utilities;
using PowerShellTools.Common.Logging;

namespace PowerShellTools
{
    [Export(typeof(ICommandHandler))]
    [ContentType("PowerShell")]
    [Name(nameof(PowerShellFormatCommandHandler))]
    internal class PowerShellFormatCommandHandler :
        ICommandHandler<FormatDocumentCommandArgs>,
        ICommandHandler<FormatSelectionCommandArgs>
    {

        private readonly ILog _logger = LogManager.GetLogger(typeof(PowerShellFormatCommandHandler));
        private readonly IEditorOperationsFactoryService _editOperationsFactory;
        private readonly ITextBufferUndoManagerProvider _undoManagerFactory;
        private readonly ITextDocumentFactoryService _textDocumentFactoryService;
        private readonly JoinableTaskFactory _joinableTaskFactory;

        [ImportingConstructor]
        public PowerShellFormatCommandHandler(
            [Import] IEditorOperationsFactoryService editOperationsFactory,
            [Import] ITextBufferUndoManagerProvider undoManagerFactory,
            [Import] ITextDocumentFactoryService textDocumentFactoryService,
            [Import] JoinableTaskContext joinableTaskContext
        )
        {
            _editOperationsFactory = editOperationsFactory;
            _undoManagerFactory = undoManagerFactory;
            _textDocumentFactoryService = textDocumentFactoryService;
            _joinableTaskFactory = joinableTaskContext.Factory;
        }

        public string DisplayName => nameof(PowerShellFormatCommandHandler);

        public CommandState GetCommandState(FormatDocumentCommandArgs args)
            => CommandState.Available;

        public CommandState GetCommandState(FormatSelectionCommandArgs args)
        {
            return CommandState.Available;
        }

        public bool ExecuteCommand(FormatDocumentCommandArgs args, CommandExecutionContext context)
        {
            return Execute(args.TextView, args.SubjectBuffer, isFormatSelection: false);
        }

        public bool ExecuteCommand(FormatSelectionCommandArgs args, CommandExecutionContext context)
        {
            return Execute(args.TextView, args.SubjectBuffer, isFormatSelection: true);
        }

        private bool Execute(ITextView textView, ITextBuffer textBuffer, bool isFormatSelection)
        {
            if (isFormatSelection)
            {
                return false;
            }

            var snapshot = textBuffer.CurrentSnapshot;
            var script = snapshot.GetText();

            try
            {
                using (var ps = System.Management.Automation.PowerShell.Create())
                {
                    ps.AddCommand("Invoke-Formatter").AddParameter("ScriptDefinition", script);
                    var result = ps.Invoke<string>().FirstOrDefault();

                    if (ps.HadErrors || result == null)
                    {
                        return false;
                    }

                    if (result.Equals(script))
                    {
                        return false;
                    }

                    var breakpoints = new List<Tuple<string, int>>();
                    var dte2 = Package.GetGlobalService(typeof(DTE)) as DTE2;
                    foreach (Breakpoint bp in dte2.Debugger.Breakpoints)
                    {
                        breakpoints.Add(new Tuple<string, int>(bp.File, bp.FileLine));
                    }

                    var edit = textBuffer.CreateEdit();
                    edit.Replace(new Span(0, snapshot.Length), result);
                    edit.Apply();

                    foreach(var bp in breakpoints)
                    {
                        try
                        {
                            dte2.Debugger.Breakpoints.Add(string.Empty, bp.Item1, bp.Item2);
                        }
                        catch { }
                    }

                    return true;
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Error formatting", ex);
                return false;
            }
        }
    }
}