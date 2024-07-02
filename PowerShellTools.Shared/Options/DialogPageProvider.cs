namespace PowerShellTools.Options
{
    internal class DialogPageProvider
    {
        public class General : BaseOptionPage<GeneralOptions> {
            public General() : base(x => new GeneralOptionsControl(x.Self)) { }
        }
        public class Diagnostics : BaseOptionPage<DiagnosticOptions> { }
    }
}
