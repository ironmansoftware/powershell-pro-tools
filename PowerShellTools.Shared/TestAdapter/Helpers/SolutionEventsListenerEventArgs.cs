namespace PowerShellTools.TestAdapter.Helpers
{
    public enum SolutionChangedReason
    {
        None,
        Load,
        Unload,
    }


    public class SolutionEventsListenerEventArgs : System.EventArgs
    {
        public IProject Project { get; private set; }
        public SolutionChangedReason ChangedReason { get; private set; }

        public SolutionEventsListenerEventArgs(IProject project, SolutionChangedReason reason)
        {
            Project = project;
            ChangedReason = reason;
        }
    }
}