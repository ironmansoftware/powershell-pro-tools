using System.ComponentModel.Composition;

namespace PowerShellTools.Repl
{
#if POWERSHELL
    using IReplEvaluator = IPowerShellReplEvaluator;
    using IReplEvaluatorProvider = IPowerShellReplEvaluatorProvider;
#endif

    [Export(typeof(IReplEvaluatorProvider))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    internal class PowerShellReplEvaluatorProvider : IReplEvaluatorProvider
    {
        public PowerShellReplEvaluator psEval;

        public IReplEvaluator GetEvaluator(string replId)
        {
            if (replId != "PowerShell") return null;

            if (psEval == null)
            {
                psEval = new PowerShellReplEvaluator();
            }

            return psEval;
        }
    }
}
