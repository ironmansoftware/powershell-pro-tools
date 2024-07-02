using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using HelpEditorOS;

namespace PowerShellTools.Editors
{
    public partial class EditorControl : UserControl
    {
        public EditorControl(string fileName)
        {
            InitializeComponent();
            var wpfControl = new MainWindow(fileName);
            wpfControl.InitializeComponent();

            elementHost.Child = wpfControl;


        }
    }
}
