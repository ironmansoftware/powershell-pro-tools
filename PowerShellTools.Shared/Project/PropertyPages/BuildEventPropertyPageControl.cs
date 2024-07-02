using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PowerShellTools.Project.PropertyPages
{
    public partial class BuildEventPropertyPageControl : PropertyPageUserControl
    {
        public BuildEventPropertyPageControl(BuildEventPropertyPage propertyPage) : base(propertyPage)
        {
            InitializeComponent();

            txtPrebuild.TextChanged += Changed;
            txtPostbuild.TextChanged += Changed;
        }

        public string Prebuild
        {
            get { return txtPrebuild.Text; }
            set { txtPrebuild.Text = value; }
        }

        public string Postbuild
        {
            get { return txtPostbuild.Text; }
            set { txtPostbuild.Text = value; }
        }
    }
}
