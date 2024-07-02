using Newtonsoft.Json;

namespace PowerShellToolsPro.Cmdlets.VSCode
{
    public class TreeItem
    {   
        public string TreeViewId { get; set; }
        public string Label { get; set; }
        public string Description { get; set; }
        public string Tooltip { get; set; }
        public string Icon { get; set; }
        public bool HasChildren { get; set; }
        public string Path { get; set; }
        public bool CanInvoke { get; set; }
        public bool DisableInvoke { get; set; }
        
    }
}