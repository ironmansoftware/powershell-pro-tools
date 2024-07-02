using System;
using System.Linq;
using System.Management.Automation;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace PowerShellTools.Project
{
    public class SignScript : Task
    {
        [Required]
        public ITaskItem Certificate { get; set; }

        [Required]
        public ITaskItem[] Scripts { get; set; }

        public override bool Execute()
        {
            try
            {
                using (var ps = System.Management.Automation.PowerShell.Create())
                {
                    ps.AddCommand("Get-ChildItem");
                    ps.AddParameter("Path", Certificate.ItemSpec);
                    var cert = ps.Invoke();

                    if (cert.FirstOrDefault() == null)
                    {
                        this.Log.LogError(String.Format("Certificate with path [{0}] could not be found.", Certificate));
                        return false;
                    }

                    ps.Commands.Clear();
                    
                    ps.AddCommand("Set-AuthenticodeSignature");
                    ps.AddParameter("Cert", cert.FirstOrDefault());
                    ps.AddParameter("Force");
                    ps.Invoke(Scripts.Select(m => m.ItemSpec));
                }
            }
            catch (Exception ex)
            {
                this.Log.LogErrorFromException(ex);
                return false;
            }


            return true;
        }
    }

    public class NewModuleManifest : Task
    {
        public ITaskItem[] AliasesToExport { get; set; }

        [Required]
        public ITaskItem Author { get; set; }

        public ITaskItem ClrVersion { get; set; }

        public ITaskItem[] CmdletsToExport { get; set; }

        [Required]
        public ITaskItem CompanyName { get; set; }

        [Required]
        public ITaskItem Copyright { get; set; }

        [Required]
        public ITaskItem Description { get; set; }

        public ITaskItem DotNetFrameworkVersion { get; set; }

        [Required]
        public ITaskItem[] FileList { get; set; }

        [Required]
        public ITaskItem[] FormatsToProcess { get; set; }

        public ITaskItem[] FunctionsToProcess { get; set; }

        public ITaskItem Guid { get; set; }

        public ITaskItem[] ModuleList { get; set; }

        [Required]
        public ITaskItem ModuleToProcess { get; set; }

        public ITaskItem ModuleVersion { get; set; }

        [Required]
        public ITaskItem[] NestedModules { get; set; }

        [Required]
        public ITaskItem Path { get; set; }

        public ITaskItem PowerShellHostName { get; set; }

        public ITaskItem PowerShellHostVersion { get; set; }

        public ITaskItem PowerShellVersion { get; set; }

        public ITaskItem ProcessorArchitecture { get; set; }

        [Required]
        public ITaskItem[] RequiredAssemblies { get; set; }

        public ITaskItem[] RequiredModules { get; set; }

        public ITaskItem[] ScriptsToProcess { get; set; }

        [Required]
        public ITaskItem[] TypesToProcess { get; set; }

        public ITaskItem[] VariablesToExport { get; set; }


        public override bool Execute()
        {
            try
            {
                using (var ps = System.Management.Automation.PowerShell.Create())
                {
                    ps.AddCommand("New-ModuleManifest");

                    if (AliasesToExport != null)
                    {
                        ps.AddParameter("AliasesToExport", AliasesToExport.Select(m => m.ItemSpec));    
                    }
                    ps.AddParameter("Author", Author.ItemSpec);

                    if (ClrVersion != null)
                    {
                        ps.AddParameter("ClrVersion", ClrVersion.ItemSpec);
                    }

                    if (CmdletsToExport != null)
                    {
                        ps.AddParameter("CmdletsToExport", CmdletsToExport.Select(m => m.ItemSpec));
                    }

                    ps.AddParameter("CompanyName", CompanyName.ItemSpec);
                    ps.AddParameter("Copyright", Copyright.ItemSpec);
                    ps.AddParameter("Description", Description.ItemSpec);

                    if (DotNetFrameworkVersion != null)
                    {
                        ps.AddParameter("DotNetFrameworkVersion", DotNetFrameworkVersion.ItemSpec);
                    }

                    ps.AddParameter("FileList", FileList.Select(m => m.ItemSpec));
                    ps.AddParameter("FormatsToProcess", FormatsToProcess.Select(m => m.ItemSpec));

                    if (FunctionsToProcess != null)
                    {
                        ps.AddParameter("FunctionsToProcess", FunctionsToProcess.Select(m => m.ItemSpec));
                    }

                    if (Guid != null)
                    {
                        ps.AddParameter("Guid", Guid.ItemSpec);
                    }

                    if (ModuleList != null)
                    {
                        ps.AddParameter("ModuleList", ModuleList.Select(m => m.ItemSpec));
                    }

                    ps.AddParameter("ModuleToProcess", ModuleToProcess.ItemSpec);

                    if (ModuleVersion != null)
                    {
                        ps.AddParameter("ModuleVersion", ModuleVersion.ItemSpec);
                    }

                    ps.AddParameter("NestedModules", NestedModules.Select(m => m.ItemSpec));
                    ps.AddParameter("Path", Path.ItemSpec);

                    if (PowerShellHostName != null)
                    {
                        ps.AddParameter("PowerShellHostName", PowerShellHostName.ItemSpec);
                    }

                    if (PowerShellHostVersion != null)
                    {
                        ps.AddParameter("PowerShellHostVersion", PowerShellHostVersion.ItemSpec);
                    }

                    if (PowerShellVersion != null)
                    {
                        ps.AddParameter("PowerShellVersion", PowerShellVersion.ItemSpec);
                    }

                    if (ProcessorArchitecture != null)
                    {
                        ps.AddParameter("ProcessorArchitecture", ProcessorArchitecture.ItemSpec);
                    }

                    ps.AddParameter("RequiredAssemblies", RequiredAssemblies.Select(m => m.ItemSpec));

                    if (RequiredModules != null)
                    {
                        ps.AddParameter("RequiredModules", RequiredModules.Select(m => m.ItemSpec));
                    }

                    if (ScriptsToProcess != null)
                    {
                        ps.AddParameter("ScriptsToProcess", ScriptsToProcess.Select(m => m.ItemSpec));
                    }

                    ps.AddParameter("TypesToProcess", TypesToProcess.Select(m => m.ItemSpec));

                    if (VariablesToExport != null)
                    {
                        ps.AddParameter("VariablesToExport", VariablesToExport.Select(m => m.ItemSpec));
                    }

                    ps.Invoke();
                }
            }
            catch (Exception ex)
            {
                this.Log.LogErrorFromException(ex);
                return false;
            }


            return true;
        
        }
    }
}