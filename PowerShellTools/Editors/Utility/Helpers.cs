using System;
using HelpEditorOS;

namespace PowerShellTools.Editors.Utility
{
    class Helpers
    {
        /// <summary>
        /// Write the Cmdlet Parameter Sets syntax.
        /// </summary>
        /// <param name="CmdletHelp"></param>
        /// <returns></returns>
        public static String WriteSyntax(cmdletDescription CmdletHelp)
        {
            String Syntax = "";
            if (CmdletHelp.ParameterSets != null)
            {
                foreach (parameterSet parSet in CmdletHelp.ParameterSets)
                {
                    Syntax += "    " + CmdletHelp.CmdletName + " ";
                    foreach (parametersetParameter ParameterName in parSet.Parameters)
                    {
                        foreach (parameterDecription param in CmdletHelp.ParameterDecription)
                        {
                            if (param.Name == ParameterName.Name)
                            {
                                if (param.isMandatory && param.Position.ToLower() != "named")
                                {
                                    Syntax += "[-" + param.Name + "] <" + param.ParameterType + "> ";
                                }
                                else if (param.isMandatory && param.Position.ToLower() == "named")
                                {
                                    Syntax += "-" + param.Name + " <" + param.ParameterType + "> ";
                                }
                                else if (!param.isMandatory && param.Position.ToLower() != "named")
                                {
                                    Syntax += "[[-" + param.Name + "] <" + param.ParameterType + ">] ";
                                }
                                else if (!param.isMandatory && param.Position == "named")
                                {
                                    Syntax += "[-" + param.Name + " <" + param.ParameterType + ">] ";
                                }
                            }
                        }
                    }
                    Syntax += "\r\n";
                }
            }
            else
            {
                foreach (parameterDecription param in CmdletHelp.ParameterDecription)
                {
                    if (param.isMandatory && param.Position.ToLower() != "named")
                    {
                        Syntax += "[-" + param.Name + "] <" + param.ParameterType + "> ";
                    }
                    else if (param.isMandatory && param.Position.ToLower() == "named")
                    {
                        Syntax += "-" + param.Name + " <" + param.ParameterType + "> ";
                    }
                    else if (!param.isMandatory && param.Position.ToLower() != "named")
                    {
                        Syntax += "[[-" + param.Name + "] <" + param.ParameterType + ">] ";
                    }
                    else if (!param.isMandatory && param.Position == "named")
                    {
                        Syntax += "[-" + param.Name + " <" + param.ParameterType + ">] ";
                    }
                }

            }

            return Syntax;
        }

        /// <summary>
        /// Write the Example section in the text viewer.
        /// </summary>
        /// <param name="examp"></param>
        /// <returns></returns>
        public static String WriteExample(example examp)
        {
            var exampleStream = "    -------------------------- " + examp.ExampleName + " --------------------------\r\n\r\n";
            exampleStream += "    C:\\PS>" + examp.ExampleCmd + "\r\n\r\n";
            exampleStream += "    " + examp.ExampleDescription + "\r\n\r\n";
            exampleStream += "    " + examp.ExampleOutput + "\r\n\r\n";

            return exampleStream;
        }

        /// <summary>
        /// Write the parameter in the text viewer.
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public static String WriteParameter(parameterDecription param)
        {
            var textStream = "    -" + param.Name + " <" + param.ParameterType + ">\r\n";
            textStream += "        " + param.NewDescription + "\r\n\r\n";
            textStream += "        Required?                    ";
            if (param.isMandatory)
            {
                textStream += "true\r\n";
            }
            else
            {
                textStream += "false\r\n";
            }
            textStream += "        Position?                    " + param.Position + "\r\n";
            textStream += "        Default value                " + param.DefaultValue + "\r\n";
            String pipelineInput;
            if (param.VFP || param.VFPBPN)
            {
                pipelineInput = "true (";
                if (param.VFP) { pipelineInput += "ByValue"; }
                if (param.VFPBPN)
                {
                    if (pipelineInput.Length > 6)
                    {
                        pipelineInput += ", ByPropertyName)";
                    }
                    else
                    {
                        pipelineInput += "ByPropertyName)";
                    }
                }
                else
                {
                    pipelineInput += ")";
                }
            }
            else
            {
                pipelineInput = "false";
            }
            textStream += "        Accept pipeline input?       " + pipelineInput + "\r\n";
            if (param.Globbing)
            {
                textStream += "        Accept wildcard characters?  true\r\n\r\n";
            }
            else
            {
                textStream += "        Accept wildcard characters?  false\r\n\r\n";
            }

            return textStream;


        }


    }
}
