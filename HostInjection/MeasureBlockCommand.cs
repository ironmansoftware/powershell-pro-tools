using PowerShellToolsPro.Cmdlets.Profiling;
using System.Management.Automation;

namespace PowerShellToolsPro.Cmdlets
{
    [Cmdlet(VerbsDiagnostic.Measure, "Block")]
    public class MeasureBlockCommand : PSCmdlet
    {
        [Parameter]
        public ScriptBlock ScriptBlock { get; set; }

        [Parameter(ParameterSetName = "file")]
        public string FileName { get; set; }

        [Parameter(ParameterSetName = "file", Mandatory = true)]
        public int StartOffset { get; set; }

        [Parameter(ParameterSetName = "file", Mandatory = true)]
        public int EndOffset { get; set; }

        [Parameter(ParameterSetName = "module", Mandatory = true)]
        public string ModuleName { get; set; }

        [Parameter(ParameterSetName = "module", Mandatory = true)]
        public string CommandName { get; set; }

        [Parameter(ParameterSetName = "module", Mandatory = true)]
        public string PipelineMethod { get; set; }

        protected override void EndProcessing()
        {
            if (ParameterSetName == "file")
            {
                using (var step = ProfilingSession.Current.Step(FileName, StartOffset, EndOffset))
                {
                    WriteObject(ScriptBlock.Invoke(), true);
                }
            }
            else
            {
                using (var step = ProfilingSession.Current.Step(ModuleName, CommandName, PipelineMethod))
                {
                    WriteObject(ScriptBlock.Invoke(), true);
                }
            }
        }
    }
}


