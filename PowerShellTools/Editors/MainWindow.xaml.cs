using System;
using System.Collections.ObjectModel;
using System.Management.Automation;
using System.Windows.Controls;
using HelpEditorOS;
using PowerShellTools.Editors.ViewModel;

namespace PowerShellTools.Editors
{

    
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public  partial class MainWindow :  UserControl
    {
        // Used to create the XML MAML output file.
        public static WriteXMLHelpFileHelperClass helpWriter = new WriteXMLHelpFileHelperClass();

        // This is PsSnapin node in the tree view. 
        public static TreeViewItem PsSnapinNode = new TreeViewItem();

        // This denotes that the tree view contains at least one obsolete info
        // we use this prperty to warn the user that saving the help file will cause him 
        // to lose the obsolete help content.
        public static Boolean ObsoleteInfo = false;


        // This property holds the PsSnapin name which is loaded 
        public static String PsSnapinName = "";

        // This property holds the name of te assembly of the PsSnapin
        // This is needed to create the default help file name.
        public static String PsSnapinModuleName = "";

        // this is a flag to denote that we opened an existing help file.
        public static Boolean OldHelpFileExist = false;

        // Path where the help file was loaded from
        public static String HelpFilePath = "";

        // This is the sped project name. We use this across the app to
        // determine whether we are working offline or online.
        public static String ProjectName = "";

        // Clipboard object used in the copy and paste operations.
        public static ClipBoardObject ClipBoardRecord = new ClipBoardObject();


        //Define zthe global CmdletDesciption record which will contain all the Cmdlets.
        // this record contains all the data from Spec, help and code.
        public static Collection<cmdletDescription> CmdletsHelps = new Collection<cmdletDescription>();

        // This holds the PsObject results from the get-command.
        public static Collection<PSObject> results = new Collection<PSObject>();

        public static ModuleObject selectedModule = new ModuleObject();

        public  MainWindow(string fileName)
        {
            InitializeComponent();

            var mwvw = new MainWindowViewModel();
            mwvw.LoadCmdletHelp(fileName);
            DataContext = mwvw;
        }
    }
}
