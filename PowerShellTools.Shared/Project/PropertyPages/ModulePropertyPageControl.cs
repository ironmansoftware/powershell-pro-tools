using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.VisualStudioTools.Project;

namespace PowerShellTools.Project.PropertyPages
{
    public partial class ModulePropertyPageControl : PropertyPageUserControl
    {
        public ModulePropertyPageControl(CommonPropertyPage page) : base(page)
        {
            InitializeComponent();

            txtModuleList.TextChanged += Changed;
            txtModuleToProcess.TextChanged += Changed;
            txtNestedModules.TextChanged += Changed;
            txtFormatsToProcess.TextChanged += Changed;
            txtFunctionsToProcess.TextChanged += Changed;
            //txtScriptsToProcess.TextChanged += Changed;
            txtTypesToProcess.TextChanged += Changed;
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
