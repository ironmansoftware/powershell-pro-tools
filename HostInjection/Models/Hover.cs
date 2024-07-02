using PowerShellToolsPro.Cmdlets.VSCode;

namespace HostInjection.Models
{
    public class Hover
    {
        public string Markdown { get; set; }
        public VsCodeRange Range { get; set; } 
    }
}
