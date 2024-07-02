using System.Management.Automation;

namespace PowerShellToolsPro.Cmdlets.VSCode
{
    [Cmdlet(VerbsCommon.New, "VSCodeRange")]
    public class NewRangeCommand : VSCodeCmdlet
    {
        [Parameter(Mandatory = true)]
        public int StartLine { get; set; }

        [Parameter(Mandatory = true)]
        public int EndLine { get; set; }
        [Parameter(Mandatory = true)]
        public int StartCharacter { get; set; }

        [Parameter(Mandatory = true)]
        public int EndCharacter { get; set; }

        protected override void BeginProcessing()
        {
            WriteObject(new VsCodeRange(StartLine, StartCharacter, EndLine, EndCharacter));
        }
    }
}
