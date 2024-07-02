using Newtonsoft.Json;

namespace PowerShellToolsPro.Cmdlets.VSCode
{
    public class VsCodeRange 
    {
        public VsCodeRange(int startLine, int startCharacter, int endLine, int endCharacter)
        {
            Start = new VsCodePosition(startLine, startCharacter);
            End = new VsCodePosition(endLine, endCharacter);
        }

        public VsCodeRange() {}

        [JsonProperty("_start")]
        public VsCodePosition Start;
        [JsonProperty("_end")]
        public VsCodePosition End;
    }
}