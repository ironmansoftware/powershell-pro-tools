using System.Collections.Generic;
using System.Management.Automation;
using Newtonsoft.Json;

namespace PowerShellToolsPro.Cmdlets.VSCode
{
    public class TreeView
    {   
        public string Label { get; set; }
        public string Description { get; set; }
        public string Tooltip { get; set; }
        public string Icon { get; set; }
        internal Dictionary<string, TreeItem> _treeItemCache = new Dictionary<string, TreeItem>();

        [JsonIgnore]
        public ScriptBlock LoadChildren { get; set; }
        [JsonIgnore]
        public ScriptBlock InvokeChild { get; set; }
    }
}