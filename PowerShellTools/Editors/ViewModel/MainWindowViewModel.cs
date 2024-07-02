
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using HelpEditorOS;
using PowerShellTools.Editors.DataModel;

namespace PowerShellTools.Editors.ViewModel
{
    internal class MainWindowViewModel : ViewModelBase
    {
        private Visibility _navigationVisibility;
        private Visibility _descriptionVisibility;
        private Visibility _mainGridVisibility;

        public MainWindowViewModel()
        {
            HeaderViewModel = new HeaderViewModel(this);
            NavigationViewModel = new NavigationViewModel(this);
        }

        public HeaderViewModel HeaderViewModel { get; private set; }
        public NavigationViewModel NavigationViewModel { get; private set; }

        public Visibility NavigationVisibility
        {
            get { return _navigationVisibility; }
            set { _navigationVisibility = value; OnPropertyChanged("NavigationVisibility"); }
        }

        public Visibility DescriptionVisibility
        {
            get { return _descriptionVisibility; }
            set { _descriptionVisibility = value; OnPropertyChanged("DescriptionVisibility"); }
        }

        public Visibility MainGridVisibility
        {
            get { return _mainGridVisibility; }
            set { _mainGridVisibility = value; OnPropertyChanged("MainGridVisibility"); }
        }

        public void LoadCmdletHelp(string helpFilePath)
        {
            var cmdlets = XMLReaderHelper.GetExistingHelpInfo(helpFilePath);
            foreach (var CmdletHelp in cmdlets)
            {
                //Start building the Cmdlet navigation tree
                TreeViewItem Node = new TreeViewItem();
                // The header is the display text for this node.
                Node.Header = CmdletHelp.CmdletName;

                // Check if detailed description and Short description have text, then consider
                // this record is complete. This is based on the speced behavior.
                // Make the text green and not bold.
                if ((CmdletHelp.LongDescription != null || CmdletHelp.LongDescription != "") &&
                    (CmdletHelp.ShortDescription != null || CmdletHelp.ShortDescription != ""))
                {
                    Node.Foreground = Brushes.Green;
                }

                // This boolean flag is used to annotate that there is a mismatch in the parameter metadata
                // between spec and code in the online mode.
                Boolean paramMistmatch = false;

                TreeViewItem ParameterNode = new TreeViewItem();
                TreeViewItem ExamplesNode = new TreeViewItem();
                TreeViewItem LinksNode = new TreeViewItem();

                // Add the Parameters record to the tree.
                // Start with the assumption that the parameter is complete.
                ParameterNode.Foreground = Brushes.Green;
                foreach (parameterDecription nodeparameterDesc in CmdletHelp.ParameterDecription)
                {
                    TreeViewItem paramItem = new TreeViewItem();
                    paramItem.Header = (nodeparameterDesc.Name);
                    paramItem.DataContext = nodeparameterDesc;

                    // If the parameter item under the Parameters node has description then 
                    // mark it in green otherwise mark it in black to indicate the contents are not complete.
                    if ((nodeparameterDesc.NewDescription == null || nodeparameterDesc.NewDescription == ""))
                    {
                        paramItem.Foreground = Brushes.Black;
                        ParameterNode.Foreground = Brushes.Black;
                    }
                    else
                    {
                        paramItem.Foreground = Brushes.Green;
                    }

                    // if this is a parameter that only exist in the help MAML file and not
                    // in code, then mark it in bold red
                    if (nodeparameterDesc.HelpOnlyParameter)
                    {
                        paramItem.Foreground = Brushes.Red;
                        paramItem.FontWeight = FontWeights.Bold;
                        // Set this ObsoleteInfo flag to true to indicate that 
                        // we do have onsolete info in the tree.
                        // We use this flag later to warn the user that saving the MAML file
                        // will cause them to loose this info.
                        MainWindow.ObsoleteInfo = true;
                        Node.Foreground = Brushes.Red;
                    }

                    // If we are in the online mode we check if there were mismatch info reported and we mark them red.
                    if (MainWindow.ProjectName != null && MainWindow.ProjectName != "")
                    {
                        if (nodeparameterDesc.MismatchInfo)
                        {
                            if ((String)(paramItem.Header).ToString().ToLower() != "whatif" && (String)(paramItem.Header).ToString().ToLower() != "confirm")
                            {
                                paramItem.Foreground = Brushes.Red;
                                paramItem.FontWeight = FontWeights.Normal;
                                // This falg is set to mark the parent Parameters node red as well.
                                paramMistmatch = true;
                            }
                        }
                    }
                    ParameterNode.Items.Add(paramItem);


                }

                ParameterNode.Header = "Parameters";
                ParameterNode.DataContext = CmdletHelp.ParameterDecription;


                // Mark the Parameters node red in cas of Mismatch
                if (paramMistmatch)
                {
                    ParameterNode.Foreground = Brushes.Red;
                    ParameterNode.FontWeight = FontWeights.Normal;
                }

                ExamplesNode.Header = "Examples";
                ExamplesNode.DataContext = new Collection<example>();
                // We use this to keep track of the spec example especially when the user is 
                // trying to create a new Help example, he should have access to the spec examples.
                //ExamplesNode.Resources.Add("SpecExample", SpecCmdlet.SpecExamples);

                // Do this if we do have example contents for this cmdlet
                if (CmdletHelp.Examples != null)
                {
                    if (CmdletHelp.Examples.Count > 0)
                    {
                        // if we have contents then the mother examples node need to be green
                        ExamplesNode.Foreground = Brushes.Green;
                        ExamplesNode.DataContext = CmdletHelp.Examples;


                        foreach (example examp in CmdletHelp.Examples)
                        {
                            // add the spec example to every example record in the online mode.
                            // this is redundant. we could use the parent Spec example record and add it.
                            // this code was here before the Examples.Resources section was intrduced above.
                            // I will leave it for now.
                            //if (ProjectName != null && ProjectName != "")
                            //{
                            //    examp.OldExampleDescription = SpecCmdlet.SpecExamples;
                            //}

                            TreeViewItem exmpNode = new TreeViewItem();
                            exmpNode.DataContext = examp;
                            // if we do have text in the cmd and description section, then
                            // mark that example record green otherwise black
                            if (examp.ExampleCmd != null && examp.ExampleCmd != "" &&
                                examp.ExampleDescription != null && examp.ExampleDescription != "")
                            {
                                exmpNode.Foreground = Brushes.Green;
                            }
                            else
                            {
                                exmpNode.Foreground = Brushes.Black;
                                ExamplesNode.Foreground = Brushes.Black;
                            }
                            exmpNode.Header = examp.ExampleName;
                            ExamplesNode.Items.Add(exmpNode);
                        }
                    }
                    else // if we have no examples mark the Examples node Black.
                    {
                        ExamplesNode.Foreground = Brushes.Black;
                    }

                }
                else
                {
                    ExamplesNode.Foreground = Brushes.Black;
                }

                // We do the same thing in Related Links as we did with examples.
                // very similar logic
                LinksNode.Header = "Related Links";
                LinksNode.DataContext = new Collection<relatedlink>();
                // LinksNode.Resources.Add("SpecLinks", SpecCmdlet.RelatedTo);
                if (CmdletHelp.RelatedLinks != null)
                {
                    LinksNode.Foreground = Brushes.Black;
                    if (CmdletHelp.RelatedLinks.Count > 0)
                    {
                        LinksNode.Foreground = Brushes.Green;
                        LinksNode.DataContext = CmdletHelp.RelatedLinks;
                        foreach (relatedlink Link in CmdletHelp.RelatedLinks)
                        {
                            //if (ProjectName != null && ProjectName != "")
                            //{
                            //    Link.OldLinkText = SpecCmdlet.RelatedTo;
                            //}
                            TreeViewItem linkNode = new TreeViewItem();
                            linkNode.DataContext = Link;
                            linkNode.Header = Link.LinkText;
                            linkNode.Foreground = Brushes.Green;
                            LinksNode.Items.Add(linkNode);
                        }
                    }
                    else
                    {
                        LinksNode.Foreground = Brushes.Black;
                    }
                }
                else
                {
                    LinksNode.Foreground = Brushes.Black;
                }

                // Add the sub nodes to the cmdlet record node.
                Node.Items.Add(ParameterNode);
                Node.Items.Add(ExamplesNode);
                Node.Items.Add(LinksNode);

                // If there is no mismatch anywher mark the cmdlet green otherwise
                // Red if errors or black if incomplete.
                if (ExamplesNode.Foreground != Brushes.Green ||
                    LinksNode.Foreground != Brushes.Green ||
                    ParameterNode.Foreground != Brushes.Green ||
                    Node.Foreground != Brushes.Green)
                {
                    if (paramMistmatch)
                    {
                        Node.Foreground = Brushes.Red;
                    }
                    else
                    {
                        Node.Foreground = Brushes.Black;
                    }
                }

                // Fill the cmdlet node with the data structure 
                // we built above.
                Node.Header = CmdletHelp.CmdletName;
                Node.DataContext = CmdletHelp;
                MainWindow.PsSnapinNode.Items.Add(Node);
                MainWindow.CmdletsHelps.Add(CmdletHelp);
            }

            NavigationVisibility = Visibility.Visible;
            DescriptionVisibility = Visibility.Visible;
            NavigationViewModel.AddTreeNode(MainWindow.PsSnapinNode);

            var firstTreeViewItem = MainWindow.PsSnapinNode;
            firstTreeViewItem.IsExpanded = true;
            firstTreeViewItem = (TreeViewItem)firstTreeViewItem.Items[0];
            firstTreeViewItem.IsSelected = true;
            firstTreeViewItem.IsExpanded = true;
        }
    }
}
