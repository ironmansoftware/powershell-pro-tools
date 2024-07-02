using System.Windows.Controls;

namespace PowerShellTools.Explorer
{
    /// <summary>
    /// Interaction logic for MyControl.xaml
    /// </summary>
    public partial class PSCommandExplorer : UserControl
    {
        public PSCommandExplorer(IHostWindow hostWindow, IDataProvider dataProvider)
        {
            InitializeComponent();

            DataContext = new PSCommandExplorerViewModel(hostWindow, dataProvider); ;
        }
    }
}