using Microsoft.VisualStudioTools.Project;

namespace PowerShellTools.Project.PropertyPages
{
    public partial class ModuleManifestPropertyPageControl : PropertyPageUserControl
    {
        public ModuleManifestPropertyPageControl(CommonPropertyPage page) : base(page)
        {
            InitializeComponent();

            txtPath.TextChanged += Changed;
            txtModuleList.TextChanged += Changed;
            txtModuleToProcess.TextChanged += Changed;
            txtNestedModules.TextChanged += Changed;
            txtFormatsToProcess.TextChanged += Changed;
            txtFunctionsToProcess.TextChanged += Changed;
            //txtScriptsToProcess.TextChanged += Changed;
            txtTypesToProcess.TextChanged += Changed;
            txtAlisesToExport.TextChanged += Changed;
            txtCmdletsToExport.TextChanged += Changed;
            txtVariablesToExport.TextChanged += Changed;
            txtAuthor.TextChanged += Changed;
            txtCompany.TextChanged += Changed;
            txtCopyright.TextChanged += Changed;
            txtDescription.TextChanged += Changed;
            txtGuid.TextChanged += Changed;
            txtVersion.TextChanged += Changed;
            txtPowerShellHostVersion.TextChanged += Changed;
            txtPowerShellVersion.TextChanged += Changed;
            cmoProcessorArchitecture.SelectedIndexChanged += Changed;
            txtRequiredModules.TextChanged += Changed;
            txtClrVersion.TextChanged += Changed;
            chkGenerateModuleManifest.CheckedChanged += Changed;
        }

        public bool GenerateModuleManifest
        {
            get { return chkGenerateModuleManifest.Checked; }
            set { chkGenerateModuleManifest.Checked = value; }
        }

        public string Path
        {
            get { return txtPath.Text; }
            set { txtPath.Text = value; }
        }

        public string ClrVersion
        {
            get { return txtClrVersion.Text; }
            set { txtClrVersion.Text = value; }
        }

        public string PowerShellHostVersion
        {
            get { return txtPowerShellHostVersion.Text; }
            set { txtPowerShellHostVersion.Text = value; }
        }

        public string PowerShellVersion
        {
            get { return txtPowerShellVersion.Text; }
            set { txtPowerShellVersion.Text = value; }
        }

        public string ProcessorArchitecture
        {
            get { return cmoProcessorArchitecture.SelectedText; }
            set { cmoProcessorArchitecture.SelectedText = value; }
        }

        public string RequiredAssemblies
        {
            get { return txtRequiredAssemblies.Text; }
            set { txtRequiredAssemblies.Text = value; }
        }

        public string RequiredModules
        {
            get { return txtRequiredModules.Text; }
            set { txtRequiredModules.Text = value; }
        }

        public string Author
        {
            get { return txtAuthor.Text; }
            set { txtAuthor.Text = value; }
        }

        public string Company
        {
            get { return txtCompany.Text; }
            set { txtCompany.Text = value; }
        }

        public string Copyright
        {
            get { return txtCopyright.Text; }
            set { txtCopyright.Text = value; }
        }

        public string Description
        {
            get { return txtDescription.Text; }
            set { txtDescription.Text = value; }
        }


        public string Guid
        {
            get { return txtGuid.Text; }
            set { txtGuid.Text = value; }
        }

        public string Version
        {
            get { return txtVersion.Text; }
            set { txtVersion.Text = value; }
        }

        public string AliasesToExport
        {
            get { return txtAlisesToExport.Text; }
            set { txtAlisesToExport.Text = value; }
        }

        public string CmdletsToExport
        {
            get { return txtCmdletsToExport.Text; }
            set { txtCmdletsToExport.Text = value; }
        }

        public string VariablesToExport
        {
            get { return txtVariablesToExport.Text; }
            set { txtVariablesToExport.Text = value; }
        }


        public string ModuleList
        {
            get { return txtModuleList.Text; }
            set { txtModuleList.Text = value; }
        }

        public string ModulesToProcess
        {
            get { return txtModuleToProcess.Text; }
            set { txtModuleToProcess.Text = value; }
        }

        public string NestedModules
        {
            get { return txtNestedModules.Text; }
            set { txtNestedModules.Text = value; }
        }

        public string ScriptsToProcess
        {
            //get { return txtScriptsToProcess.Text; }
            //set { txtScriptsToProcess.Text = value; }

            get;
            set;
        }

        public string TypesToProcess
        {
            get { return txtTypesToProcess.Text; }
            set { txtTypesToProcess.Text = value; }
        }
        public string FormatsToProcess
        {
            get { return txtFormatsToProcess.Text; }
            set { txtFormatsToProcess.Text = value; }
        }

        public string FunctionsToProcess
        {
            get { return txtFunctionsToProcess.Text; }
            set { txtFunctionsToProcess.Text = value; }
        }

    }
}
