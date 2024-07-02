using System;
using System.Management.Automation.Language;
using EnvDTE80;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using PowerShellTools.Commands.UserInterface;

namespace PowerShellTools.Commands
{
    /// <summary>
    /// Command for executing a script with parameters prompt from the editor context menu.
    /// </summary>
    internal sealed class ExecuteWithParametersAsScriptCommand : ExecuteFromEditorContextMenuCommand
    {
        private IVsTextManager _textManager;
        private IVsEditorAdaptersFactoryService _adaptersFactory;

        internal ExecuteWithParametersAsScriptCommand(IVsEditorAdaptersFactoryService adaptersFactory, IVsTextManager textManager)
        {
            _adaptersFactory = adaptersFactory;
            _textManager = textManager;
        }

        protected override bool ShouldExecuteWithScriptArgs
        {
            get
            {
                return true;
            }
        }

        protected override int Id
        {
            get 
            { 
                return (int)GuidList.CmdidExecuteWithParametersAsScript; 
            }
        }

        public override void Execute(object sender, EventArgs args)
        {
            ParamBlockAst scriptParameters = ParameterEditorHelper.GetScriptParameters(_adaptersFactory, _textManager);
            ScriptArgs = ParameterEditorHelper.PromptForScriptParameterValues(scriptParameters);

            if (ScriptArgs.ShouldExecute)
            {
                base.Execute(sender, args);
            }
        }
    }
}
