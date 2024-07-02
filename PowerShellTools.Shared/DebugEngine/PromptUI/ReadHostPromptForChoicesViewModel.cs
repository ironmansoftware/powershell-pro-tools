using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Windows.Input;
using PowerShellTools.Common.Controls;
using PowerShellTools.Common.ServiceManagement.DebuggingContract;

namespace PowerShellTools.DebugEngine.PromptUI
{
    internal sealed class ReadHostPromptForChoicesViewModel
    {
        private readonly ObservableCollection<ChoiceButtonItem> _choices;
        private ICommand _chooseCommand;

        public ReadHostPromptForChoicesViewModel(string caption, string message, IList<ChoiceItem> choices, int defaultChoice)
        {
            this.Caption = caption;
            this.Message = message;
            _choices = new ObservableCollection<ChoiceButtonItem>(choices.Select(c => new ChoiceButtonItem(c, choices[defaultChoice] == c)));
        }

        public string Caption
        {
            get;
            private set;
        }

        public string Message
        {
            get;
            private set;
        }

        public ObservableCollection<ChoiceButtonItem> Choices
        {
            get
            {
                return _choices;
            }
        }

        public ICommand Command
        {
            get
            {
                return LazyInitializer.EnsureInitialized<ICommand>(ref _chooseCommand, () => new ViewModelCommand(o => Choose(o)));
            }
        }

        private void Choose(object o)
        {
            string label = o as string;
            this.UserChoice = this.Choices.IndexOf(this.Choices.FirstOrDefault(c => c.Choice.Label.Equals(label, StringComparison.OrdinalIgnoreCase)));
        }

        public int UserChoice
        {
            get;
            private set;
        }

        #region Design-time

        /// <summary>
        /// Gets an instance of this view model suitable for use at design time.
        /// </summary>
        public static object DesignerViewModel
        {
            get
            {
                return new
                {
#if DEBUG
                    Caption = "Confirm",
                    Message = "Are sure you want to perform this action?" + Environment.NewLine + "Performing the operation \"Remove Directory\" on target \"C:\\Users\\USERNAME\\foo\"",
                    Choices = new ChoiceButtonItem[] 
                    {
                        new ChoiceButtonItem(new ChoiceItem("_label1", "message1"), true),
                        new ChoiceButtonItem(new ChoiceItem("l_abel2", "message2"), false),
                        new ChoiceButtonItem(new ChoiceItem("longlable is  here la_bel3", "message3"), false),
                        new ChoiceButtonItem(new ChoiceItem("lab_el4", "message4"), false),
                        new ChoiceButtonItem(new ChoiceItem("labe_l5", "message5"), false)
                    }, 
                    UserChoice = 0
#endif
                };
            }
        }

        #endregion Design-time
    }
}
