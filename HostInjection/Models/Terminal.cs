using Newtonsoft.Json;

namespace PowerShellToolsPro.Cmdlets.VSCode
{
    public class VsCodeTerminal
    {
        [JsonProperty("_name")]
        public string Name;
        [JsonProperty("_id")]
        public int Id;
        [JsonProperty("_cols")]
        public int Columns;
        [JsonProperty("_rows")]
        public int Rows;
        [JsonProperty("_creationOptions")]
        public VsCodeTerminalCreationOptions CreationOptions;
    }

    public class VsCodeTerminalCreationOptions
    {
        public string Name;
        public string ShellPath;
        public string[] ShellArgs;
        public bool HideFromUser;

        public override string ToString()
        {
            return $"{Name} - {ShellPath}";
        }
    }
}