using PowerShellToolsPro.Cmdlets.Profiling;
using System;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Reflection;
using System.Text;

namespace PowerShellToolsPro.Cmdlets
{
    [Cmdlet(VerbsDiagnostic.Measure, "Script")]
    public class MeasureScriptCommand : PSCmdlet
    {
        [Parameter(Mandatory = true, ParameterSetName = "ScriptBlock", Position = 0)]
        public ScriptBlock ScriptBlock { get; set; }

        [Parameter(Mandatory = true, ParameterSetName = "FilePath", Position = 0)]
        public string FilePath { get; set; }

        protected override void EndProcessing()
        {
            var instrumentation = new Instrumentation();

            string fullPath = null;
            if (ParameterSetName == "FilePath")
            {
                fullPath = GetUnresolvedProviderPathFromPSPath(FilePath);
                if (!File.Exists(fullPath))
                {
                    throw new Exception("File not found");
                }

                var scriptContents = File.ReadAllText(fullPath);

                ScriptBlock = ScriptBlock.Create(scriptContents);
            }

            var sb = instrumentation.Instrument(ScriptBlock, this, fullPath);

            ProfilingSession.Start();

            using (var ps = PowerShell.Create())
            {
                ps.AddScript(sb.ToString());
                using (ProfilingSession.Current.Step(new SequencePoint(fullPath, 0, ScriptBlock.Ast.Extent.EndOffset)))
                {
                    ps.Invoke();
                }

                if (ps.HadErrors)
                {
                    foreach(var error in ps.Streams.Error)
                    {
                        WriteError(error);
                    }
                }
            }

            var result = ProfilingSession.Stop();

            WriteObject(result);
        }

    }
}
