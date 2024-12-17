namespace PowerShellTools.Options
{
    internal class DialogPageProvider
    {
        public class General : BaseOptionPage<GeneralOptions> {
            public General() : base(x => new GeneralOptionsControl(x.Self)) { }
        }

        public class Analysis : BaseOptionPage<AnalysisOptions> { }

        public class Diagnostics : BaseOptionPage<DiagnosticOptions> { }


    }
}
