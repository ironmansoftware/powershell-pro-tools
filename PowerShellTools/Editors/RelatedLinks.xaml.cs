using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using PowerShellTools.Editors;

namespace HelpEditorOS
{
	/// <summary>
	/// Interaction logic for RelatedLinks.xaml
	/// </summary>
	public partial class RelatedLinks : UserControl
	{
		public RelatedLinks()
		{
			this.InitializeComponent();
		}

             /// <summary>
        /// This routine handles the Save button on the Related Link page
        /// It adds the new record to the tree. 
        /// If this is already in the tree it updates it with the latest 
        /// info in the page.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        public void AddRelatedLink_Click(object sender, RoutedEventArgs args)
        {


            TreeViewItem SelectedNode = (TreeViewItem)MainWindow.NavControl.CmdletTreeView.SelectedItem;
            if (SelectedNode != null)
            {
                int count = SelectedNode.Items.Count;
                if (this.RelatedLinks1.LinkIDTextBox.Text == "")
                //This is a new link
                {
                    //Add new related link info.
                    TreeViewItem ParentNode = (TreeViewItem)SelectedNode.Parent;
                    cmdletDescription Cmdletdesc = (cmdletDescription)ParentNode.DataContext;
                    int LinkCount = 0;
                    if (Cmdletdesc.RelatedLinks != null)
                    {
                        LinkCount = Cmdletdesc.RelatedLinks.Count;
                    }

                    relatedlink linkDetails = new relatedlink();
                    linkDetails.LinkText = this.RelatedLinks1.RelatedLinkTextBox.Text;
                    linkDetails.LinkID = LinkCount;
                    TreeViewItem NewLinkNode = new TreeViewItem();
                    NewLinkNode.DataContext = linkDetails;
                    NewLinkNode.Header = linkDetails.LinkText;
                    if (Cmdletdesc.RelatedLinks == null)
                    {
                        Collection<relatedlink> Links = new Collection<relatedlink>();
                        Links.Add(linkDetails);
                        Cmdletdesc.RelatedLinks = Links;
                        SelectedNode.DataContext = Links;
                    }
                    else
                    {
                        Cmdletdesc.RelatedLinks.Add(linkDetails);
                    }
                    MainWindow.ResetLinksPage();

                    if ((String)SelectedNode.Header == "Related Links")
                    {
                        if (sender != null)
                        {
                            SelectedNode.Items.Add(NewLinkNode);
                            SelectedNode.IsExpanded = true;
                            TreeViewItem ChildNode = (TreeViewItem)SelectedNode.Items[count];
                            ChildNode.IsSelected = true;
                        }
                        else
                        {
                            SelectedNode.Items.Add(NewLinkNode);
                            SelectedNode.IsExpanded = true;
                        }
                    }

                }

                else //this is an existing link
                {
                    if (SelectedNode.Header.ToString() == "Related Links")
                    {
                        //Update link info.
                        Collection<relatedlink> linkDetails = (Collection<relatedlink>)SelectedNode.DataContext;
                        relatedlink linkDetail = new relatedlink();
                        linkDetail.LinkText = this.RelatedLinks1.RelatedLinkTextBox.Text;
                        linkDetails.Add(linkDetail);

                    }
                    else
                    {
                        relatedlink linkDetails = (relatedlink)SelectedNode.DataContext;
                        linkDetails.LinkText = this.RelatedLinks1.RelatedLinkTextBox.Text;
                        SelectedNode.Header = this.RelatedLinks1.RelatedLinkTextBox.Text;
                    }

                }
            }

        }

        public void NewRelatedLink_Click(object sender, RoutedEventArgs e)
        {
            TreeViewItem ChildNode = (TreeViewItem)MainWindow.NavControl.CmdletTreeView.SelectedItem;
            String Header = (String)ChildNode.Header;
            if (Header.ToLower() != "related links")
            {
                ChildNode = (TreeViewItem)ChildNode.Parent;
                ChildNode.IsSelected = true;
            }
            MainWindow.ResetLinksPage();

        }

        /// <summary>
        /// This routine removes a related link from the tree.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        public void DeleteLink_Click(object sender, RoutedEventArgs args)
        {
            if (this.RelatedLinks1.LinkIDTextBox.Text == "")
            {
                //Safe to simply reset the Examples page.
                MainWindow.ResetLinksPage();
            }
            else //We must remove the current record from the Examples list
            {
                TreeViewItem selectedNode = (TreeViewItem)MainWindow.NavControl.CmdletTreeView.SelectedItem;
                TreeViewItem ParentNode = (TreeViewItem)selectedNode.Parent;
                TreeViewItem CmdletNode = (TreeViewItem)ParentNode.Parent;
                ParentNode.IsSelected = true;
                ParentNode.Items.Remove(selectedNode);

                cmdletDescription selectedCmdlet = (cmdletDescription)CmdletNode.DataContext;
                relatedlink selectedLink = (relatedlink)selectedNode.DataContext;

                foreach (cmdletDescription mycmdlet in MainWindow.CmdletsHelps)
                {

                    if (mycmdlet.CmdletName == selectedCmdlet.CmdletName)
                    {
                        Collection<relatedlink> myLinks = mycmdlet.RelatedLinks;
                        relatedlink linktoRemove = new relatedlink();
                        foreach (relatedlink mylink in myLinks)
                        {
                            if (mylink.LinkID == selectedLink.LinkID)
                            {
                                linktoRemove = mylink;

                            }
                        }
                        if (linktoRemove.LinkID.ToString() != null)
                        {
                            mycmdlet.RelatedLinks.Remove(linktoRemove);
                            ParentNode.Items.Remove(selectedNode);
                        }
                    }
                }



            }

        }
	}
}