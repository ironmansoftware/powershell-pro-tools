using System.Reflection;
using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Design.Serialization;
using VSLangProj;

namespace PowerShellToolsPro.FormsDesigner
{
    public static class DocDataExtensions
    {
        public static string GetDesignerFileName(this DocDataTextWriter writer)
        {
            var fileName = writer.GetFileName();

            var index = fileName.LastIndexOf('.');
            return fileName.Insert(index + 1, "Designer.");
        }

        public static string GetDesignerFileName(this DocDataTextReader reader)
        {
            var fileName = reader.GetFileName();

            var index = fileName.LastIndexOf('.');
            return fileName.Insert(index + 1, "Designer.");
        }

        public static string GetFileName(this DocDataTextWriter writer)
        {
	        return writer.GetPrivatePropertyValue<DocData>("DocData").Name;
        }

        public static string GetFileName(this DocDataTextReader reader)
        {
			return reader.GetPrivatePropertyValue<DocData>("DocData").Name;
		}
    }
}
