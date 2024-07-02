using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using HelpEditorOS;

namespace PowerShellTools.Editors.Utility
{
    class FileHelper
    {

        /// <summary>
        /// The actual Save file routine.
        /// It calls the XML writer routine.
        /// </summary>
        /// <param name="FilePath"></param>
        public void SaveHelpFile(String FilePath)
        {
            Boolean save = true;
            if (MainWindow.ObsoleteInfo)
            {
                MessageBoxResult Result = System.Windows.MessageBox.Show("This action will make you loose all Obsolete Help info Marked in Bold red in the Tree View. Are you sure you want to proceed?", "Warning!", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (Result == MessageBoxResult.No)
                {
                    save = false;
                }
            }

            // If we are sure we want to save the file then we do the actual save action.
            if (save)
            {
                try
                {
                    XmlWriter writer = null;
                    XmlWriterSettings settings = new XmlWriterSettings();
                    settings.Indent = true;
                    settings.IndentChars = ("    ");
                    settings.NewLineHandling = NewLineHandling.None;

                    settings.ConformanceLevel = ConformanceLevel.Document;

                    writer = XmlWriter.Create(FilePath, settings);


                    writer.WriteStartDocument();

                    writer.WriteRaw("\r\n\r\n<helpItems xmlns=\"http://msh\" schema=\"maml\">\r\n\r\n");
                    foreach (cmdletDescription CmdletHelp in MainWindow.CmdletsHelps)
                    {
                        // Code that creates the MAML content for one cmdlet at a time.
                        SaveHelpFileBody(writer, CmdletHelp);
                    }
                    writer.WriteRaw("</helpItems>\r\n");

                    writer.Flush();
                    writer.Close();
                    System.Windows.Forms.MessageBox.Show(MainWindow.HelpFilePath + "\n\nFile saved.", "File saved", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    System.Windows.Forms.MessageBox.Show(ex.Message, "Failed to save the help file");
                }
            }
        }


        /// <summary>
        /// This routine is used to create the one Cmdlet help MAML
        /// at a time.
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="CmdletHelp"></param>
        public void SaveHelpFileBody(XmlWriter writer, cmdletDescription CmdletHelp)
        {
            try
            {

                writer.WriteRaw("<command:command xmlns:maml=\"http://schemas.microsoft.com/maml/2004/10\" xmlns:command=\"http://schemas.microsoft.com/maml/dev/command/2004/10\" xmlns:dev=\"http://schemas.microsoft.com/maml/dev/2004/10\">\r\n");
                writer = MainWindow.helpWriter.writeCmdletDetails(writer, CmdletHelp);

                //Add Syntax section <command:syntax>
                writer.WriteRaw("	<command:syntax>");

                Collection<parameterDecription> ParamDescription = CmdletHelp.ParameterDecription;
                Collection<parameterSet> HelpParameterSets = CmdletHelp.ParameterSets;
                //Iterate through the Syntax section and add Syntax items.
                //foreach (parameterset in paramDescription.ParameterSets[0].Parameters[0].Name
                if (HelpParameterSets != null)
                {
                    foreach (parameterSet HelpParameterSet in HelpParameterSets)
                    {
                        writer.WriteRaw("		<command:syntaxItem>\r\n");
                        //Cdmlet Name.
                        writer.WriteRaw("			<maml:name>");
                        writer.WriteRaw(CmdletHelp.CmdletName);
                        writer.WriteRaw("</maml:name>\r\n");
                        // writer.WriteRaw("		<!--New Syntax Item-->\r\n");
                        if (HelpParameterSet.Parameters != null)
                        {
                            foreach (parametersetParameter HelpParameter in HelpParameterSet.Parameters)
                            {
                                foreach (parameterDecription param in ParamDescription)
                                {
                                    if (param.Name.ToLower() == HelpParameter.Name.ToLower())
                                    {
                                        writer = MainWindow.helpWriter.createSyntaxItem(writer, param, CmdletHelp);
                                        break;
                                    }
                                }
                            }
                        }
                        //End <command:syntaxItem>
                        writer.WriteRaw("		</command:syntaxItem>\r\n");
                    }
                }
                else
                {
                    writer.WriteRaw("		<command:syntaxItem>\r\n");
                    writer.WriteRaw("		</command:syntaxItem>\r\n");

                }


                writer.WriteRaw("	</command:syntax>\r\n");
                //writer.WriteRaw("	</command:syntax>\r\n");

                //Add Parameters section <command:parameters>
                writer.WriteRaw("	<command:parameters>\r\n");
                //writer.WriteComment("Parameters section");

                //Iterate through the parameters section and add parameters Items.


                foreach (parameterDecription parameter in ParamDescription)
                {
                    writer = MainWindow.helpWriter.createParameters(writer, parameter);
                }


                writer.WriteRaw("	</command:parameters>\r\n");


                //Input Object Section
                writer = MainWindow.helpWriter.createInputSection(writer, CmdletHelp);

                //Output Object Section
                writer = MainWindow.helpWriter.createOutputSection(writer, CmdletHelp);


                // Error Section (Static section not used)
                //<command:terminatingErrors />
                //<command:nonTerminatingErrors />
                writer.WriteRaw("	<command:terminatingErrors>\r\n");
                //writer.WriteComment("Terminating errors section");
                writer.WriteRaw("	</command:terminatingErrors>\r\n");
                writer.WriteRaw("	<command:nonTerminatingErrors>\r\n");
                //writer.WriteComment("Non terminating errors section");
                writer.WriteRaw("	</command:nonTerminatingErrors>\r\n");


                //AlertSet  <!-- Notes section  -->
                writer = MainWindow.helpWriter.createAlertSetSection(writer, CmdletHelp);

                //Examples section.
                //Examples header goes here <command:examples>
                writer.WriteRaw("	<command:examples>\r\n");

                //Iterate through all the examples here
                if (CmdletHelp.Examples != null)
                {
                    foreach (example ExampleRecord in CmdletHelp.Examples)
                    {
                        writer = MainWindow.helpWriter.createExampleItemSection(writer, ExampleRecord);
                        //End examples section

                    }
                }
                writer.WriteRaw("	</command:examples>\r\n");


                //Links section
                // <maml:relatedLinks>
                writer.WriteRaw("	<maml:relatedLinks>\r\n");
                //Iterate through the links

                if (CmdletHelp.RelatedLinks != null)
                {
                    foreach (relatedlink RelatedLinkRecord in CmdletHelp.RelatedLinks)
                    {
                        writer = MainWindow.helpWriter.createLinksSection(writer, RelatedLinkRecord);

                    }
                }
                writer.WriteRaw("	</maml:relatedLinks>\r\n");

                //Write the end node for the starting <command:command> node.
                //writer.WriteRaw();
                writer.WriteRaw("</command:command>\r\n");
                //  }

                //Write the help file.

                //  }

            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message, "Error writing the XML file", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                //FailedToWrite = true;
                if (writer != null)
                {
                    writer.Close();
                }

            }
        }
    }
}
