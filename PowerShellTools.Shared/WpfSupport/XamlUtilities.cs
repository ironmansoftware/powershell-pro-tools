using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace PowerShellToolsPro.WpfSupport
{
	public class XamlUtilities
	{
		public string GetNameOfSelectedControl(string xamlContext, int selectionPosition)
		{
			var textUpToSelection = xamlContext.Substring(0, selectionPosition);
			var lineNumber = textUpToSelection.Split(new[] { "\r\n" }, StringSplitOptions.None).Length;
			var linePosition = textUpToSelection.Length - textUpToSelection.LastIndexOf("\r\n");

			var document = XDocument.Parse(xamlContext, LoadOptions.SetLineInfo);
			var selectedNode = document.Descendants().FirstOrDefault(m => ((IXmlLineInfo)m).LineNumber == lineNumber && ((IXmlLineInfo)m).LinePosition <= linePosition);

			var nameAttribute = selectedNode?.Attributes().FirstOrDefault(m => m.Name.LocalName.Equals("Name", StringComparison.OrdinalIgnoreCase) && 
																			   m.Name.NamespaceName.Equals("http://schemas.microsoft.com/winfx/2006/xaml", StringComparison.OrdinalIgnoreCase));
			var controlName = nameAttribute?.Value;

			return controlName;
		}

        public static string SetNameOfSelectedControl(string xamlContext, int selectionPosition, string name)
        {
            var textUpToSelection = xamlContext.Substring(0, selectionPosition);
            var lineNumber = textUpToSelection.Split(new[] { "\r\n" }, StringSplitOptions.None).Length;
            var linePosition = textUpToSelection.Length - textUpToSelection.LastIndexOf("\r\n");

            var document = XDocument.Parse(xamlContext, LoadOptions.SetLineInfo);
            var selectedNode = document.Descendants().FirstOrDefault(m => ((IXmlLineInfo)m).LineNumber == lineNumber && ((IXmlLineInfo)m).LinePosition <= linePosition);

            var nameAttribute = selectedNode?.Attributes().FirstOrDefault(m => m.Name.LocalName.Equals("Name", StringComparison.OrdinalIgnoreCase) &&
                                                                               m.Name.NamespaceName.Equals("http://schemas.microsoft.com/winfx/2006/xaml", StringComparison.OrdinalIgnoreCase));
            if (nameAttribute != null || selectedNode == null) return document.ToString();

            XNamespace ns = "http://schemas.microsoft.com/winfx/2006/xaml";
            selectedNode.Add(new XAttribute(ns + "Name", name));

            return document.ToString();
		}
	}
}
