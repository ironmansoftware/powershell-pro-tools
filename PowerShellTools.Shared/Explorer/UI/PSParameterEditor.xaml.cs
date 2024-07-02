using System.Windows.Controls;

namespace PowerShellTools.Explorer
{
    /// <summary>
    /// Interaction logic for PSParameterEditor.xaml
    /// </summary>
    public partial class PSParameterEditor : UserControl
    {
        public PSParameterEditor(IHostWindow hostWindow, IDataProvider dataProvider)
        {
            InitializeComponent();

            this.DataContext = new PSParameterEditorViewModel(hostWindow, dataProvider);
        }
    }
}
