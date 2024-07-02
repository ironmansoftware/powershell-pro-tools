using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.Text;

namespace PowerShellProTools.Host
{
    public class Module
    {
        public Module(PSModuleInfo module)
        {
            Name = module.Name;
            Version = module.Version.ToString();
            Path = module.ModuleBase;
            FromRepository = module.RepositorySourceLocation != null;
        }

        public Module() { }

        public string Name { get; set; }
        public string Version { get; set; }
        public string Path { get; set; }
        public bool FromRepository { get; set; }
        public List<ModuleVersion> Versions { get; set; }
    }

    public class ModuleVersion
    {
        public string Version { get; set; }
        public string ModuleBase { get; set; }
    }
}
