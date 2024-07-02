using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Xml;
using HelpEditorOS;

namespace PowerShellTools.Editors.DataModel
{
    class XMLReaderHelper
    {
        public static IEnumerable<cmdletDescription> GetExistingHelpInfo(String path)
        {
            //String path1 = @"C:\Windows\System32\WindowsPowerShell\v1.0\en-US\Microsoft.PowerShell.Commands.Management.dll-Help.xml";
            XmlDocument doc = new XmlDocument();
            doc.Load(path);

            XmlNamespaceManager ns = new XmlNamespaceManager(doc.NameTable);
            ns.AddNamespace("", "http://msh");
            ns.AddNamespace("command", "http://schemas.microsoft.com/maml/dev/command/2004/10");
            ns.AddNamespace("maml", "http://schemas.microsoft.com/maml/2004/10");
            ns.AddNamespace("dev", "http://schemas.microsoft.com/maml/dev/2004/10");
            XmlNodeList commandNodes = doc.SelectNodes("//command:command", ns);
            foreach (XmlNode commandNode in commandNodes)
            {
                var CmdletHelp = new cmdletDescription();

                XmlNode nameNode = commandNode.SelectSingleNode("command:details/command:name", ns);

                    // MessageBox.Show(commandNode.OuterXml);
                    XmlNode tempNode = null;

                    tempNode = commandNode.SelectSingleNode("command:details/maml:description", ns);
                    if (tempNode != null)
                    {
                        CmdletHelp.OldShortDescription = CmdletHelp.ShortDescription = tempNode.InnerText.Trim();
                    }

                    tempNode = commandNode.SelectSingleNode("maml:description", ns);
                    if (tempNode != null)
                    {
                        CmdletHelp.OldLongDescription = CmdletHelp.LongDescription = tempNode.InnerText.Trim();
                    }

                    tempNode = commandNode.SelectSingleNode("command:inputTypes/command:inputType/dev:type/maml:name", ns);
                    if (tempNode != null)
                    {
                        CmdletHelp.OldInputType = CmdletHelp.InputType = tempNode.InnerText.Trim();
                    }

                    tempNode = commandNode.SelectSingleNode("command:inputTypes/command:inputType/dev:type/maml:description", ns);
                    if (tempNode != null)
                    {
                        CmdletHelp.OldInputDesc = CmdletHelp.InputDesc = tempNode.InnerText.Trim();
                    }

                    tempNode = commandNode.SelectSingleNode("command:returnValues/command:returnValue/dev:type/maml:name", ns);
                    if (tempNode != null)
                    {
                        CmdletHelp.OldOutputType = CmdletHelp.OutputType = tempNode.InnerText.Trim();
                    }

                    tempNode = commandNode.SelectSingleNode("command:returnValues/command:returnValue/dev:type/maml:description", ns);
                    if (tempNode != null)
                    {
                        CmdletHelp.OldOutputDesc = CmdletHelp.OutputDesc = tempNode.InnerText.Trim();
                    }

                    tempNode = commandNode.SelectSingleNode("maml:alertSet/maml:alert", ns);
                    if (tempNode != null)
                    {
                        CmdletHelp.OldNote = CmdletHelp.Note = tempNode.InnerText.Trim();
                    }

                    XmlNodeList exampleNodes = commandNode.SelectNodes("command:examples/command:example", ns);

                    int exampleCount = 0;
                    Collection<example> CmdletExamples = new Collection<example>();
                    foreach (XmlNode exampleNode in exampleNodes)
                    {
                        example CmdletExample = new example();

                        tempNode = exampleNode.SelectSingleNode("maml:title", ns);
                        if (tempNode != null)
                        {
                            CmdletExample.OldExampleName = CmdletExample.ExampleName = tempNode.InnerText.Trim().Replace("-", "");
                            if (CmdletExample.ExampleName.Length == 0 || CmdletExample.ExampleName.Replace(" ", "") == "") { CmdletExample.ExampleName = CmdletExample.OldExampleName = "Unkown"; }
                        }

                        CmdletExample.ExampleID = exampleCount;

                        tempNode = exampleNode.SelectSingleNode("dev:code", ns);
                        if (tempNode != null)
                        {
                            CmdletExample.OldExampleCmd = CmdletExample.ExampleCmd = tempNode.InnerText.Trim();
                        }

                        tempNode = exampleNode.SelectSingleNode("dev:remarks", ns);
                        if (tempNode != null)
                        {
                            int NodeCount = 0;
                            foreach (XmlNode DescriptionNode in tempNode)
                            {
                                if (NodeCount == 0)
                                {
                                    CmdletExample.OldExampleDescription = CmdletExample.ExampleDescription = DescriptionNode.InnerText.Trim();
                                }
                                else
                                {

                                    CmdletExample.OldExampleOutput += DescriptionNode.InnerText.Trim();
                                    CmdletExample.ExampleOutput = CmdletExample.OldExampleOutput;
                                }
                                NodeCount++;
                            }
                        }

                        tempNode = exampleNode.SelectSingleNode("command:commandLines", ns);
                        if (tempNode != null)
                        {
                            CmdletExample.OldExampleOutput += tempNode.InnerText.Trim();
                            CmdletExample.ExampleOutput = CmdletExample.OldExampleOutput;
                        }

                        exampleCount++;

                        CmdletExamples.Add(CmdletExample);
                    }

                    CmdletHelp.Examples = CmdletExamples;

                    Collection<relatedlink> RelatedLinks = new Collection<relatedlink>();
                    XmlNodeList NodeLinks = commandNode.SelectNodes("maml:relatedLinks/maml:navigationLink", ns);
                    int LinkCount = 0;
                    foreach (XmlNode linkNode in NodeLinks)
                    {
                        relatedlink RelatedLink = new relatedlink();

                        tempNode = linkNode.SelectSingleNode("maml:linkText", ns);
                        if (tempNode != null)
                        {
                            RelatedLink.OldLinkText = RelatedLink.LinkText = tempNode.InnerText.Trim();
                            if (RelatedLink.LinkText.Length == 0) { RelatedLink.LinkText = RelatedLink.LinkText = "Unkown"; }
                        }

                        RelatedLink.LinkID = LinkCount;

                        LinkCount++;

                        RelatedLinks.Add(RelatedLink);
                    }
                    CmdletHelp.RelatedLinks = RelatedLinks;


                    //iterate through parameters
                    XmlNodeList parameterNodes = commandNode.SelectNodes("command:parameters/command:parameter", ns);
                    if (CmdletHelp.ParameterDecription != null)
                    {
                        foreach (parameterDecription CmdletParameter in CmdletHelp.ParameterDecription)
                        {
                            //maml:description
                            foreach (XmlNode parameterNode in parameterNodes)
                            {
                                tempNode = parameterNode.SelectSingleNode("maml:name", ns);
                                if (tempNode != null)
                                {
                                    if (CmdletParameter.Name.ToLower() == tempNode.InnerText.Trim().ToLower())
                                    {
                                        tempNode = parameterNode.SelectSingleNode("maml:description", ns);
                                        if (tempNode != null)
                                        {
                                            CmdletParameter.OldDescription = CmdletParameter.NewDescription = tempNode.InnerText.Trim();
                                        }

                                        tempNode = parameterNode.SelectSingleNode("dev:defaultValue", ns);
                                        if (tempNode != null)
                                        {
                                            CmdletParameter.DefaultValue = CmdletParameter.OldDefaultValue = tempNode.InnerText.Trim();
                                        }

                                        tempNode = parameterNode.SelectSingleNode("@globbing", ns);

                                        if (tempNode.Value.ToLower().Trim() == "true")
                                        {
                                            CmdletParameter.Globbing = CmdletParameter.OldGlobbing = true;


                                        }
                                        else
                                        {
                                            CmdletParameter.Globbing = CmdletParameter.OldGlobbing = false;

                                        }
                                    }

                                }
                            }
                    }

                    //I do not have code parameters. Get only help ones and mark them in red.

                    foreach (XmlNode parameterNode in parameterNodes)
                    {
                        Boolean ParameterFound = false;
                        tempNode = parameterNode.SelectSingleNode("maml:name", ns);
                        String ParameterName = tempNode.InnerText.Trim();
                        if (CmdletHelp.ParameterDecription != null)
                        {
                            foreach (parameterDecription CmdletParameter in CmdletHelp.ParameterDecription)
                            {

                                if (CmdletParameter.Name.ToLower() == ParameterName.ToLower())
                                {
                                    ParameterFound = true;
                                    break;
                                }
                            }
                        }
                        if (ParameterFound == false)
                        {
                            //Get help parameter.
                            parameterDecription CmdletParameter = new parameterDecription();
                            CmdletParameter.HelpOnlyParameter = true;
                            MainWindow.ObsoleteInfo = true;
                            tempNode = parameterNode.SelectSingleNode("maml:name", ns);
                            if (tempNode != null)
                            {
                                //  if (CmdletParameter.Name.ToLower() == tempNode.InnerText.Trim().ToLower())
                                // {
                                tempNode = parameterNode.SelectSingleNode("maml:description", ns);
                                if (tempNode != null)
                                {
                                    CmdletParameter.OldDescription = CmdletParameter.NewDescription = tempNode.InnerText.Trim();
                                }

                                tempNode = parameterNode.SelectSingleNode("dev:defaultValue", ns);
                                if (tempNode != null)
                                {
                                    CmdletParameter.DefaultValue = CmdletParameter.OldDefaultValue = tempNode.InnerText.Trim();
                                }

                                tempNode = parameterNode.SelectSingleNode("@globbing", ns);

                                if (tempNode.Value.ToLower().Trim() == "true")
                                {
                                    CmdletParameter.Globbing = CmdletParameter.OldGlobbing = true;

                                }
                                else
                                {
                                    CmdletParameter.Globbing = CmdletParameter.OldGlobbing = false;

                                }
                                tempNode = parameterNode.SelectSingleNode("@pipelineInput", ns);

                                if (tempNode != null)
                                {
                                    if (tempNode.Value.ToLower().Trim() == "true (ByPropertyName)")
                                    {
                                        CmdletParameter.VFPBPN = CmdletParameter.VFPBPN = true;

                                    }
                                    else if (tempNode.Value.ToLower().Trim() == "true (ByValue, ByPropertyName)")
                                    {
                                        CmdletParameter.VFPBPN = CmdletParameter.VFPBPN = true;
                                        CmdletParameter.VFP = CmdletParameter.VFP = true;
                                    }
                                    else if (tempNode.Value.ToLower().Trim() == "true (ByValue)")
                                    {
                                        CmdletParameter.VFP = CmdletParameter.VFP = true;
                                    }
                                    else
                                    {
                                        CmdletParameter.VFPBPN = CmdletParameter.VFPBPN = false;
                                        CmdletParameter.VFP = CmdletParameter.VFP = false;
                                    }
                                }
                                tempNode = parameterNode.SelectSingleNode("@position", ns);

                                if (tempNode != null)
                                {
                                    CmdletParameter.Position = tempNode.Value.ToLower().Trim();
                                }

                                tempNode = parameterNode.SelectSingleNode("@required", ns);
                                if (tempNode != null)
                                {
                                    if (tempNode.Value.ToLower().Trim() == "true")
                                    {
                                        CmdletParameter.isMandatory = true;
                                    }
                                }
                                tempNode = parameterNode.SelectSingleNode("dev:type/maml:name", ns);
                                if (tempNode != null)
                                {

                                    if (tempNode.InnerText != null)
                                    {
                                        CmdletParameter.ParameterType = tempNode.InnerText.Trim().ToLower();
                                    }
                                }

                                CmdletParameter.Name = ParameterName;
                                CmdletParameter.CmdletName = "Test";
                                if (CmdletHelp.ParameterDecription == null)
                                {
                                    CmdletHelp.ParameterDecription = new Collection<parameterDecription>();
                                }
                                CmdletHelp.ParameterDecription.Add(CmdletParameter);

                            }

                        }
                    }

                        yield return CmdletHelp;
                    }
            }
        }

        /// <summary>
        /// This is the static method which Asim helped me with.
        /// I get all the Cmdlet Mamal info from here.
        /// I need to pass the cmdletDescription record before adding it to the CmdletHelps collection.
        /// 
        /// </summary>
        public static cmdletDescription GetExistingHelpInfo(cmdletDescription CmdletHelp, String CmdletName, String path)
        {
            //String path1 = @"C:\Windows\System32\WindowsPowerShell\v1.0\en-US\Microsoft.PowerShell.Commands.Management.dll-Help.xml";
            XmlDocument doc = new XmlDocument();
            doc.Load(path);

            XmlNamespaceManager ns = new XmlNamespaceManager(doc.NameTable);
            ns.AddNamespace("", "http://msh");
            ns.AddNamespace("command", "http://schemas.microsoft.com/maml/dev/command/2004/10");
            ns.AddNamespace("maml", "http://schemas.microsoft.com/maml/2004/10");
            ns.AddNamespace("dev", "http://schemas.microsoft.com/maml/dev/2004/10");
            XmlNodeList commandNodes = doc.SelectNodes("//command:command", ns);
            foreach (XmlNode commandNode in commandNodes)
            {
                XmlNode nameNode = commandNode.SelectSingleNode("command:details/command:name", ns);
                if (nameNode.InnerText.Trim().ToLower() == CmdletName.ToLower())
                {
                    // MessageBox.Show(commandNode.OuterXml);
                    XmlNode tempNode = null;

                    tempNode = commandNode.SelectSingleNode("command:details/maml:description", ns);
                    if (tempNode != null)
                    {
                        CmdletHelp.OldShortDescription = CmdletHelp.ShortDescription = tempNode.InnerText.Trim();
                    }

                    tempNode = commandNode.SelectSingleNode("maml:description", ns);
                    if (tempNode != null)
                    {
                        CmdletHelp.OldLongDescription = CmdletHelp.LongDescription = tempNode.InnerText.Trim();
                    }

                    tempNode = commandNode.SelectSingleNode("command:inputTypes/command:inputType/dev:type/maml:name", ns);
                    if (tempNode != null)
                    {
                        CmdletHelp.OldInputType = CmdletHelp.InputType = tempNode.InnerText.Trim();
                    }

                    tempNode = commandNode.SelectSingleNode("command:inputTypes/command:inputType/dev:type/maml:description", ns);
                    if (tempNode != null)
                    {
                        CmdletHelp.OldInputDesc = CmdletHelp.InputDesc = tempNode.InnerText.Trim();
                    }

                    tempNode = commandNode.SelectSingleNode("command:returnValues/command:returnValue/dev:type/maml:name", ns);
                    if (tempNode != null)
                    {
                        CmdletHelp.OldOutputType = CmdletHelp.OutputType = tempNode.InnerText.Trim();
                    }

                    tempNode = commandNode.SelectSingleNode("command:returnValues/command:returnValue/dev:type/maml:description", ns);
                    if (tempNode != null)
                    {
                        CmdletHelp.OldOutputDesc = CmdletHelp.OutputDesc = tempNode.InnerText.Trim();
                    }

                    tempNode = commandNode.SelectSingleNode("maml:alertSet/maml:alert", ns);
                    if (tempNode != null)
                    {
                        CmdletHelp.OldNote = CmdletHelp.Note = tempNode.InnerText.Trim();
                    }

                    XmlNodeList exampleNodes = commandNode.SelectNodes("command:examples/command:example", ns);

                    int exampleCount = 0;
                    Collection<example> CmdletExamples = new Collection<example>();
                    foreach (XmlNode exampleNode in exampleNodes)
                    {
                        example CmdletExample = new example();

                        tempNode = exampleNode.SelectSingleNode("maml:title", ns);
                        if (tempNode != null)
                        {
                            CmdletExample.OldExampleName = CmdletExample.ExampleName = tempNode.InnerText.Trim().Replace("-", "");
                            if (CmdletExample.ExampleName.Length == 0 || CmdletExample.ExampleName.Replace(" ", "") == "") { CmdletExample.ExampleName = CmdletExample.OldExampleName = "Unkown"; }
                        }

                        CmdletExample.ExampleID = exampleCount;

                        tempNode = exampleNode.SelectSingleNode("dev:code", ns);
                        if (tempNode != null)
                        {
                            CmdletExample.OldExampleCmd = CmdletExample.ExampleCmd = tempNode.InnerText.Trim();
                        }

                        tempNode = exampleNode.SelectSingleNode("dev:remarks", ns);
                        if (tempNode != null)
                        {
                            int NodeCount = 0;
                            foreach (XmlNode DescriptionNode in tempNode)
                            {
                                if (NodeCount == 0)
                                {
                                    CmdletExample.OldExampleDescription = CmdletExample.ExampleDescription = DescriptionNode.InnerText.Trim();
                                }
                                else
                                {

                                    CmdletExample.OldExampleOutput += DescriptionNode.InnerText.Trim();
                                    CmdletExample.ExampleOutput = CmdletExample.OldExampleOutput;
                                }
                                NodeCount++;
                            }
                        }

                        tempNode = exampleNode.SelectSingleNode("command:commandLines", ns);
                        if (tempNode != null)
                        {
                            CmdletExample.OldExampleOutput += tempNode.InnerText.Trim();
                            CmdletExample.ExampleOutput = CmdletExample.OldExampleOutput;
                        }

                        exampleCount++;

                        CmdletExamples.Add(CmdletExample);
                    }

                    CmdletHelp.Examples = CmdletExamples;

                    Collection<relatedlink> RelatedLinks = new Collection<relatedlink>();
                    XmlNodeList NodeLinks = commandNode.SelectNodes("maml:relatedLinks/maml:navigationLink", ns);
                    int LinkCount = 0;
                    foreach (XmlNode linkNode in NodeLinks)
                    {
                        relatedlink RelatedLink = new relatedlink();

                        tempNode = linkNode.SelectSingleNode("maml:linkText", ns);
                        if (tempNode != null)
                        {
                            RelatedLink.OldLinkText = RelatedLink.LinkText = tempNode.InnerText.Trim();
                            if (RelatedLink.LinkText.Length == 0) { RelatedLink.LinkText = RelatedLink.LinkText = "Unkown"; }
                        }

                        RelatedLink.LinkID = LinkCount;

                        LinkCount++;

                        RelatedLinks.Add(RelatedLink);
                    }
                    CmdletHelp.RelatedLinks = RelatedLinks;


                    //iterate through parameters
                    XmlNodeList parameterNodes = commandNode.SelectNodes("command:parameters/command:parameter", ns);
                    if (CmdletHelp.ParameterDecription != null)
                    {
                        foreach (parameterDecription CmdletParameter in CmdletHelp.ParameterDecription)
                        {
                            //maml:description
                            foreach (XmlNode parameterNode in parameterNodes)
                            {
                                tempNode = parameterNode.SelectSingleNode("maml:name", ns);
                                if (tempNode != null)
                                {
                                    if (CmdletParameter.Name.ToLower() == tempNode.InnerText.Trim().ToLower())
                                    {
                                        tempNode = parameterNode.SelectSingleNode("maml:description", ns);
                                        if (tempNode != null)
                                        {
                                            CmdletParameter.OldDescription = CmdletParameter.NewDescription = tempNode.InnerText.Trim();
                                        }

                                        tempNode = parameterNode.SelectSingleNode("dev:defaultValue", ns);
                                        if (tempNode != null)
                                        {
                                            CmdletParameter.DefaultValue = CmdletParameter.OldDefaultValue = tempNode.InnerText.Trim();
                                        }

                                        tempNode = parameterNode.SelectSingleNode("@globbing", ns);

                                        if (tempNode.Value.ToLower().Trim() == "true")
                                        {
                                            CmdletParameter.Globbing = CmdletParameter.OldGlobbing = true;


                                        }
                                        else
                                        {
                                            CmdletParameter.Globbing = CmdletParameter.OldGlobbing = false;

                                        }
                                    }

                                }
                            }


                        }
                    }

                    //I do not have code parameters. Get only help ones and mark them in red.

                    foreach (XmlNode parameterNode in parameterNodes)
                    {
                        Boolean ParameterFound = false;
                        tempNode = parameterNode.SelectSingleNode("maml:name", ns);
                        String ParameterName = tempNode.InnerText.Trim();
                        if (CmdletHelp.ParameterDecription != null)
                        {
                            foreach (parameterDecription CmdletParameter in CmdletHelp.ParameterDecription)
                            {

                                if (CmdletParameter.Name.ToLower() == ParameterName.ToLower())
                                {
                                    ParameterFound = true;
                                    break;
                                }
                            }
                        }
                        if (ParameterFound == false)
                        {
                            //Get help parameter.
                            parameterDecription CmdletParameter = new parameterDecription();
                            CmdletParameter.HelpOnlyParameter = true;
                            MainWindow.ObsoleteInfo = true;
                            tempNode = parameterNode.SelectSingleNode("maml:name", ns);
                            if (tempNode != null)
                            {
                                //  if (CmdletParameter.Name.ToLower() == tempNode.InnerText.Trim().ToLower())
                                // {
                                tempNode = parameterNode.SelectSingleNode("maml:description", ns);
                                if (tempNode != null)
                                {
                                    CmdletParameter.OldDescription = CmdletParameter.NewDescription = tempNode.InnerText.Trim();
                                }

                                tempNode = parameterNode.SelectSingleNode("dev:defaultValue", ns);
                                if (tempNode != null)
                                {
                                    CmdletParameter.DefaultValue = CmdletParameter.OldDefaultValue = tempNode.InnerText.Trim();
                                }

                                tempNode = parameterNode.SelectSingleNode("@globbing", ns);

                                if (tempNode.Value.ToLower().Trim() == "true")
                                {
                                    CmdletParameter.Globbing = CmdletParameter.OldGlobbing = true;

                                }
                                else
                                {
                                    CmdletParameter.Globbing = CmdletParameter.OldGlobbing = false;

                                }
                                tempNode = parameterNode.SelectSingleNode("@pipelineInput", ns);

                                if (tempNode != null)
                                {
                                    if (tempNode.Value.ToLower().Trim() == "true (ByPropertyName)")
                                    {
                                        CmdletParameter.VFPBPN = CmdletParameter.VFPBPN = true;

                                    }
                                    else if (tempNode.Value.ToLower().Trim() == "true (ByValue, ByPropertyName)")
                                    {
                                        CmdletParameter.VFPBPN = CmdletParameter.VFPBPN = true;
                                        CmdletParameter.VFP = CmdletParameter.VFP = true;
                                    }
                                    else if (tempNode.Value.ToLower().Trim() == "true (ByValue)")
                                    {
                                        CmdletParameter.VFP = CmdletParameter.VFP = true;
                                    }
                                    else
                                    {
                                        CmdletParameter.VFPBPN = CmdletParameter.VFPBPN = false;
                                        CmdletParameter.VFP = CmdletParameter.VFP = false;
                                    }
                                }
                                tempNode = parameterNode.SelectSingleNode("@position", ns);

                                if (tempNode != null)
                                {
                                    CmdletParameter.Position = tempNode.Value.ToLower().Trim();
                                }

                                tempNode = parameterNode.SelectSingleNode("@required", ns);
                                if (tempNode != null)
                                {
                                    if (tempNode.Value.ToLower().Trim() == "true")
                                    {
                                        CmdletParameter.isMandatory = true;
                                    }
                                }
                                tempNode = parameterNode.SelectSingleNode("dev:type/maml:name", ns);
                                if (tempNode != null)
                                {

                                    if (tempNode.InnerText != null)
                                    {
                                        CmdletParameter.ParameterType = tempNode.InnerText.Trim().ToLower();
                                    }
                                }

                                CmdletParameter.Name = ParameterName;
                                CmdletParameter.CmdletName = CmdletName;
                                if (CmdletHelp.ParameterDecription == null)
                                {
                                    CmdletHelp.ParameterDecription = new Collection<parameterDecription>();
                                }
                                CmdletHelp.ParameterDecription.Add(CmdletParameter);

                            }

                        }
                    }

                    // break;
                }


            }
            return CmdletHelp;

        }

        public static void GetHelpInfoNotInCode(Collection<cmdletDescription> CodeCmdlets, String path)
        {
            //String path1 = @"C:\Windows\System32\WindowsPowerShell\v1.0\en-US\Microsoft.PowerShell.Commands.Management.dll-Help.xml";
            XmlDocument doc = new XmlDocument();
            doc.Load(path);

            XmlNamespaceManager ns = new XmlNamespaceManager(doc.NameTable);
            ns.AddNamespace("", "http://msh");
            ns.AddNamespace("command", "http://schemas.microsoft.com/maml/dev/command/2004/10");
            ns.AddNamespace("maml", "http://schemas.microsoft.com/maml/2004/10");
            ns.AddNamespace("dev", "http://schemas.microsoft.com/maml/dev/2004/10");
            XmlNodeList commandNodes = doc.SelectNodes("//command:command", ns);

            //TreeViewItem psSnapinNode =  ; 
            foreach (XmlNode commandNode in commandNodes)
            //foreach (cmdletDescription cmdletHelp in CodeCmdlets)
            {
                cmdletDescription CmdletHelp = new cmdletDescription();
                XmlNode nameNode = commandNode.SelectSingleNode("command:details/command:name", ns);
                //CmdletHelp = cmdletHelp;
                Boolean CmdletFound = false;
                foreach (cmdletDescription cmdletHelp in CodeCmdlets)
                {

                    if (nameNode.InnerText.Trim().ToLower() == cmdletHelp.CmdletName.ToLower())
                    {
                        CmdletFound = true;
                        break;
                    }
                }
                if (CmdletFound == false)
                {
                    //Add cmdlet from help to code one and color it.
                    CmdletHelp = GetExistingHelpInfo(CmdletHelp, nameNode.InnerText.Trim(), path);


                    //Start building the Cmdlet navigation tree
                    TreeViewItem Node = new TreeViewItem();

                    Node.Header = nameNode.InnerText.Trim();
                    // Brushes MyBrush = "#FF363B44";
                    //MyBrush = 5;

                    Node.Foreground = Brushes.Red;
                    Node.FontWeight = FontWeights.Bold;
                    MainWindow.ObsoleteInfo = true;


                    TreeViewItem ParameterNode = new TreeViewItem();
                    TreeViewItem ExamplesNode = new TreeViewItem();
                    TreeViewItem LinksNode = new TreeViewItem();
                    //add the Parameters record to the tree
                    if (CmdletHelp.ParameterDecription != null)
                    {
                        foreach (parameterDecription nodeparameterDesc in CmdletHelp.ParameterDecription)
                        {
                            TreeViewItem paramItem = new TreeViewItem();
                            paramItem.Header = (nodeparameterDesc.Name);
                            paramItem.DataContext = nodeparameterDesc;
                            paramItem.Foreground = Brushes.Red;
                            paramItem.FontWeight = FontWeights.Bold;
                            MainWindow.ObsoleteInfo = true;
                            ParameterNode.Items.Add(paramItem);

                        }

                    }
                    ParameterNode.Header = "Parameters";
                    ParameterNode.Foreground = Brushes.Red;
                    ParameterNode.FontWeight = FontWeights.Bold;
                    ParameterNode.DataContext = CmdletHelp.ParameterDecription;
                    ExamplesNode.Header = "Examples";
                    ExamplesNode.Foreground = Brushes.Red;
                    ExamplesNode.FontWeight = FontWeights.Bold;
                    ExamplesNode.DataContext = new Collection<example>();
                    if (CmdletHelp.Examples != null)
                    {
                        ExamplesNode.DataContext = CmdletHelp.Examples;
                        foreach (example examp in CmdletHelp.Examples)
                        {
                            TreeViewItem exmpNode = new TreeViewItem();
                            exmpNode.DataContext = examp;
                            exmpNode.Foreground = Brushes.Red;
                            exmpNode.FontWeight = FontWeights.Bold;
                            exmpNode.Header = examp.ExampleName;
                            ExamplesNode.Items.Add(exmpNode);
                        }

                    }

                    LinksNode.Header = "Related Links";
                    LinksNode.Foreground = Brushes.Red;
                    LinksNode.FontWeight = FontWeights.Bold;
                    LinksNode.DataContext = new Collection<relatedlink>();
                    if (CmdletHelp.RelatedLinks != null)
                    {
                        LinksNode.DataContext = CmdletHelp.RelatedLinks;
                        foreach (relatedlink examp in CmdletHelp.RelatedLinks)
                        {
                            TreeViewItem exmpNode = new TreeViewItem();
                            exmpNode.DataContext = examp;
                            exmpNode.Foreground = Brushes.Red;
                            exmpNode.FontWeight = FontWeights.Bold;
                            exmpNode.Header = examp.LinkText;
                            LinksNode.Items.Add(exmpNode);
                        }

                    }
                    Node.Items.Add(ParameterNode);
                    Node.Items.Add(ExamplesNode);
                    Node.Items.Add(LinksNode);
                    //  Node.Header = CmdletHelp.CmdletName;
                    Node.DataContext = CmdletHelp;

                    //    TreeViewItem PsSnapinNode = new TreeViewItem();

                    MainWindow.PsSnapinNode.Items.Add(Node);
                    //Cmdlets.Add(CmdletHelp);

                }


            }
            // return CodeCmdlets;

        }


    }
}
