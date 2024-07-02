namespace PowerShellTools.Commands
{
    /// <summary>
    /// A class represents the result from the Parameter editor.
    /// </summary>
    internal sealed class ScriptParameterResult
    {
        public ScriptParameterResult(string args, bool ShouldExecute)
        {
            this.ScriptArgs = args;
            this.ShouldExecute = ShouldExecute;
        }

        /// <summary>
        /// The argument that will append to the script execution command.
        /// </summary>
        public string ScriptArgs { get; private set; }

        /// <summary>
        /// Determine whether to execute this script. 
        /// </summary>
        /// <remarks>
        /// If user clicks Cancel at the end of editing parameters, then return false.
        /// If user click Ok, even the script argument is empty, the script should be executed.
        /// </remarks>
        public bool ShouldExecute { get; private set; }
    }
}
