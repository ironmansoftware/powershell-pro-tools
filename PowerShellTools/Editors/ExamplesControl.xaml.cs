using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using HelpEditorOS;

namespace PowerShellTools.Editors
{
    /// <summary>
    /// Interaction logic for ExamplesControl.xaml
    /// </summary>
    public partial class ExamplesControl : UserControl
    {
        public ExamplesControl()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// This routine handles the Save button on the Examples page
        /// It adds the new record to the tree. 
        /// If this is already in the tree it updates it with the latest 
        /// info in the page.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        public void AddExample_Click(object sender, RoutedEventArgs args)
        {

            TreeViewItem SelectedNode = (TreeViewItem)MainWindow.NavControl.CmdletTreeView.SelectedItem;
            if (SelectedNode != null)
            {
                int count = SelectedNode.Items.Count;
                if (this.ExamplesControl1.ExampleNameTextBox.Text == "")
                {
                    this.ExamplesControl1.ExampleNameTextBox.Text = "Example " + (count + 1).ToString();
                }

                if (this.ExamplesControl1.ExampleID.Text == "")
                //This is a new example
                {
                    //Add new exampe info.
                    TreeViewItem ParentNode = new TreeViewItem();
                    if ((String)SelectedNode.Header == "Examples")
                    {
                        ParentNode = (TreeViewItem)SelectedNode.Parent;
                    }
                    else
                    {
                        ParentNode = (TreeViewItem)SelectedNode.Parent;
                        ParentNode = (TreeViewItem)ParentNode.Parent;
                    }

                    cmdletDescription Cmdletdesc = (cmdletDescription)ParentNode.DataContext;
                    example examDetails = new example();
                    examDetails.ExampleCmd = this.ExamplesControl1.ExampleCommandTextBox.Text;
                    examDetails.ExampleDescription = this.ExamplesControl1.ExampleDescriptionTextBox.Text;
                    examDetails.ExampleName = this.ExamplesControl1.ExampleNameTextBox.Text;
                    examDetails.ExampleOutput = this.ExamplesControl1.ExampleOutputTextBox.Text;
                    examDetails.ExampleID = count;
                    TreeViewItem NewExampleNode = new TreeViewItem();
                    NewExampleNode.DataContext = examDetails;
                    NewExampleNode.Header = examDetails.ExampleName;
                    if (Cmdletdesc.Examples == null)
                    {
                        Collection<example> Examples = new Collection<example>();
                        Examples.Add(examDetails);
                        Cmdletdesc.Examples = Examples;
                        SelectedNode.DataContext = Examples;
                    }
                    else
                    {
                        Cmdletdesc.Examples.Add(examDetails);
                    }
                    MainWindow.ResetExamplesPage();
                    if ((String)SelectedNode.Header == "Examples")
                    {


                        if (sender != null)
                        {
                            SelectedNode.Items.Add(NewExampleNode);
                            SelectedNode.IsExpanded = true;
                            //int count = SelectedNode.Items.Count;
                            TreeViewItem ChildNode = (TreeViewItem)SelectedNode.Items[count];
                            ChildNode.IsSelected = true;
                        }
                        else
                        {
                            SelectedNode.Items.Add(NewExampleNode);
                            SelectedNode.IsExpanded = true;
                        }
                    }

                }


                else //this is an existing example
                {
                    //Update Example info.
                    if (SelectedNode.Header.ToString() == "Examples")
                    {
                        Collection<example> examDetails = (Collection<example>)SelectedNode.DataContext;
                        example examDetail = new example();
                        examDetail.ExampleCmd = this.ExamplesControl1.ExampleCommandTextBox.Text;
                        examDetail.ExampleDescription = this.ExamplesControl1.ExampleDescriptionTextBox.Text;
                        examDetail.ExampleName = this.ExamplesControl1.ExampleNameTextBox.Text;
                        examDetail.ExampleOutput = this.ExamplesControl1.ExampleOutputTextBox.Text;
                        examDetails.Add(examDetail);
                        //SelectedNode.Header = this.ExampleNameTextBox.Text;

                    }
                    else
                    {
                        example examDetails = (example)SelectedNode.DataContext;
                        examDetails.ExampleCmd = this.ExamplesControl1.ExampleCommandTextBox.Text;
                        examDetails.ExampleDescription = this.ExamplesControl1.ExampleDescriptionTextBox.Text;
                        examDetails.ExampleName = this.ExamplesControl1.ExampleNameTextBox.Text;
                        examDetails.ExampleOutput = this.ExamplesControl1.ExampleOutputTextBox.Text;
                        SelectedNode.Header = this.ExamplesControl1.ExampleNameTextBox.Text;
                    }

                }
            }


        }

        /// <summary>
        /// Create a new Example record in the tree
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void NewExample_Click(object sender, RoutedEventArgs e)
        {
            TreeViewItem ChildNode = (TreeViewItem)MainWindow.NavControl.CmdletTreeView.SelectedItem;
            String Header = (String)ChildNode.Header;
            if (Header.ToLower() != "examples")
            {
                ChildNode = (TreeViewItem)ChildNode.Parent;
                ChildNode.IsSelected = true;
            }
            MainWindow.ResetExamplesPage();
        }

        /// <summary>
        /// This routine removes examples from the tree.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        public void DeleteExample_Click(object sender, RoutedEventArgs args)
        {
            if (this.ExamplesControl1.ExampleID.Text == "")
            {
                //Safe to simply reset the Examples page.
                MainWindow.ResetExamplesPage();
            }
            else //We must remove the current record from the Examples list
            {
                TreeViewItem selectedNode = (TreeViewItem)MainWindow.NavControl.CmdletTreeView.SelectedItem;
                TreeViewItem ParentNode = (TreeViewItem)selectedNode.Parent;
                TreeViewItem CmdletNode = (TreeViewItem)ParentNode.Parent;
                ParentNode.IsSelected = true;
                //int ItemToRemove = selectedNode.Items.IndexOf();
                //
                cmdletDescription selectedCmdlet = (cmdletDescription)CmdletNode.DataContext;
                example selectedExample = (example)selectedNode.DataContext;

                foreach (cmdletDescription mycmdlet in MainWindow.CmdletsHelps)
                {

                    if (mycmdlet.CmdletName == selectedCmdlet.CmdletName)
                    {
                        Collection<example> myExamples = mycmdlet.Examples;
                        example exampletoRemove = new example();
                        foreach (example myexample in myExamples)
                        {
                            if (myexample.ExampleID == selectedExample.ExampleID)
                            {
                                exampletoRemove = myexample;

                            }
                        }
                        if (exampletoRemove.ExampleID.ToString() != null)
                        {
                            mycmdlet.Examples.Remove(exampletoRemove);
                            ParentNode.Items.Remove(selectedNode);
                        }
                    }
                }

            }

        }
    }
}