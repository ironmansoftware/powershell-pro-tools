using System;
using System.Management.Automation;

namespace PowerShellToolsPro.Cmdlets.VSCode
{
    [Cmdlet(VerbsCommon.Show, "VSCodeInputBox")]
    public class ShowInputBoxCommand : VSCodeCmdlet
    {
        [Parameter()]
        public SwitchParameter IgnoreFocusOut { get; set; }

        [Parameter()]
        public SwitchParameter Password { get; set; }

        [Parameter()]
        public string PlaceHolder { get; set; }

        [Parameter()]
        public string Prompt { get; set; }

        [Parameter()]
        public string Value { get; set; }

        [Parameter()]
        public int StartValueSelection { get; set; }

        [Parameter()]
        public int EndValueSelection { get; set; } 

        protected override void BeginProcessing()
        {
            Wait = SwitchParameter.Present;
            var options = new InputBoxOptions();

            if (MyInvocation.BoundParameters.ContainsKey(nameof(Password)))
            {
                options.password = Password.IsPresent;
            }

            if (MyInvocation.BoundParameters.ContainsKey(nameof(IgnoreFocusOut)))
            {
                options.ignoreFocusOut = IgnoreFocusOut.IsPresent;
            }

            options.value = Value;
            options.prompt = Prompt;
            options.placeHolder = PlaceHolder;

            if (MyInvocation.BoundParameters.ContainsKey(nameof(StartValueSelection)) && MyInvocation.BoundParameters.ContainsKey(nameof(EndValueSelection)))
            {
                options.valueSelection = new Tuple<int, int>(StartValueSelection, EndValueSelection);
            }


            var result = SendCommand($"vscode.window.showInputBox", options);

            if (result != null)
                WriteObject(result);
        }
    }

    internal class InputBoxOptions 
    {
        public bool? ignoreFocusOut;
        public bool? password; 
        public string prompt;
        public string placeHolder;
        public string value; 
        public Tuple<int, int> valueSelection;
    }
}
