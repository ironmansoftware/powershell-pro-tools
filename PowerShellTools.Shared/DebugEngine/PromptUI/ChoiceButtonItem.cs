using PowerShellTools.Common.ServiceManagement.DebuggingContract;

namespace PowerShellTools.DebugEngine.PromptUI
{
    internal sealed class ChoiceButtonItem
    {
        public ChoiceButtonItem(ChoiceItem choice, bool isDefault)
        {
            this.Choice = choice;
            this.IsDefault = isDefault;
        }

        public ChoiceItem Choice
        {
            get;
            private set;
        }

        public bool IsDefault
        {
            get;
            private set;
        }
    }
}
