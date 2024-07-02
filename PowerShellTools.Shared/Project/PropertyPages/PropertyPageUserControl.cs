using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudioTools.Project;

namespace PowerShellTools.Project.PropertyPages
{
    public class PropertyPageUserControl : System.Windows.Forms.UserControl
    {
        private readonly CommonPropertyPage _page;

        public bool LoadingSettings { get; set; }

        public PropertyPageUserControl()
        {
        }

        public PropertyPageUserControl(CommonPropertyPage page)
        {
            _page = page;
        }

        protected void Changed(object sender, EventArgs e)
        {
            if (!LoadingSettings)
                _page.IsDirty = true;
        }
    }
}
