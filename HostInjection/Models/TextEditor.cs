using Newtonsoft.Json;

namespace PowerShellToolsPro.Cmdlets.VSCode
{
    public class VsCodeTextEditor
    {   
        [JsonProperty("_document")]     
        public VsCodeTextDocument Document;
        [JsonProperty("_languageId")]
        public string LanguageId;
    }
}