using System.Management.Automation;

namespace PowerShellToolsPro.Cmdlets.VSCode
{
    [Cmdlet(VerbsCommon.New, "VSCodePosition")]
    public class NewPositionCommand : VSCodeCmdlet
    {
        [Parameter(Mandatory = true)]
        public int Line { get; set; }

        [Parameter(Mandatory = true)]
        public int Character { get; set; }

        protected override void BeginProcessing()
        {
            WriteObject(new VsCodePosition(Line, Character));
        }
    }
}
