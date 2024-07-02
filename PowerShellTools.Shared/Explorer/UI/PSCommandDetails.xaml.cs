using System.Windows;
using PowerShellTools.Common;

namespace PowerShellTools.Explorer
{
    /// <summary>
    /// Interaction logic for PSCommandDetails.xaml
    /// </summary>
    public partial class PSCommandDetails : Window, IDialog
    {
        public PSCommandDetails(IDataProvider dataProvider, IPowerShellCommand commandInfo)
        {
            InitializeComponent();

            this.DataContext = new PSCommandDetailsViewModel(this, dataProvider, commandInfo);
        }
    }
}
