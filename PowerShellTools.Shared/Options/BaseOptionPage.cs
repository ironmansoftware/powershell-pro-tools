using Microsoft.VisualStudio.Shell;
using System;
using System.Windows.Forms;

namespace PowerShellTools.Options
{
    /// <summary>
    /// A base class for a DialogPage to show in Tools -> Options.
    /// </summary>
    internal class BaseOptionPage<T> : DialogPage where T : BaseOptionModel<T>, new()
    {
        private BaseOptionModel<T> _model;
        private Func<BaseOptionModel<T>, IWin32Window> _customPageFactory;

        public BaseOptionPage()
        {
#pragma warning disable VSTHRD104 // Offer async methods
            _model = ThreadHelper.JoinableTaskFactory.Run(BaseOptionModel<T>.CreateAsync);
#pragma warning restore VSTHRD104 // Offer async methods
        }

        public BaseOptionPage(Func<BaseOptionModel<T>, IWin32Window> customPageFactory)
        {
            _customPageFactory = customPageFactory;
#pragma warning disable VSTHRD104 // Offer async methods
            _model = ThreadHelper.JoinableTaskFactory.Run(BaseOptionModel<T>.CreateAsync);
#pragma warning restore VSTHRD104 // Offer async methods
        }

        protected override IWin32Window Window
        {
            get
            {
                if (_customPageFactory == null) return base.Window;
                return _customPageFactory(_model);
            }
        }

        public override object AutomationObject
        {
            get
            {
                if (_customPageFactory == null) return _model;
                return base.AutomationObject;
            }
        }

        public override void LoadSettingsFromStorage()
        {
            _model.Load();
        }

        public override void SaveSettingsToStorage()
        {
            _model.Save();
        }
    }
}