using Newtonsoft.Json;

namespace PowerShellToolsPro.Cmdlets.VSCode
{
    public class VsCodePosition 
    {
        public VsCodePosition(int line, int character)
        {
            Line = line;
            Character = character;
        }

        public VsCodePosition() {}

        [JsonProperty("_line")]
        public int Line;
        [JsonProperty("_character")]
        public int Character;
    }
}