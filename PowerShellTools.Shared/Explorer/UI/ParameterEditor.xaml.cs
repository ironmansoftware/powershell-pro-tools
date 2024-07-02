using System.Windows;
using PowerShellTools.Common;

namespace PowerShellTools.Explorer
{
    /// <summary>
    /// Interaction logic for ParameterEditor.xaml
    /// </summary>
    public partial class ParameterEditor : Window, IDialog
    {
        public ParameterEditor(IDataProvider dataProvider, IPowerShellCommand commandInfo)
        {
            InitializeComponent();

            this.DataContext = new ParameterEditorViewModel(this, dataProvider, commandInfo);
        }
    }
}
