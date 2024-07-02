using System.Collections.Generic;
using System.Linq;

namespace PowerShellToolsPro.Packager
{
    public class ModuleInfo
    {
        public ModuleInfo()
        { 
        }

        public ModuleInfo(System.Management.Automation.PSModuleInfo mi)
        {
            Name = mi.Name;
            Path = mi.Path;
            ModuleBase = mi.ModuleBase;
            RequiredModules = mi.RequiredModules?.Select(m => new ModuleInfo(m)).ToList();
            NestedModules = mi.NestedModules?.Select(m => new ModuleInfo(m)).ToList();
        }
        public string Name { get; set; }
        public string Path { get; set; }
        public string ModuleBase { get; set; }
        public List<ModuleInfo> RequiredModules { get; set; }
        public List<ModuleInfo> NestedModules { get; set;  }
    }
}
