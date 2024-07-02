using System;
using System.IO;
using System.Linq;
using System.Reflection.PortableExecutable;
using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.CSharp;
using ICSharpCode.Decompiler.Metadata;
using ICSharpCode.Decompiler.TypeSystem;
using PowerShellToolsPro.Cmdlets.VSCode;

namespace PowerShellProTools.Host
{
    public static class Decompiler 
    {
        public static string Decompile(string assemblyName, string typeName)
        {
            var assembly = PSAssembly.GetAssemblies().FirstOrDefault(m => m.Name.Equals(assemblyName, StringComparison.OrdinalIgnoreCase));
            if (string.IsNullOrEmpty(assembly?.Path)) return null;
            var module = new PEFile(assembly.Path, new FileStream(assembly.Path, FileMode.Open, FileAccess.Read), PEStreamOptions.Default);
            var decompiler = new CSharpDecompiler(assembly.Path, new DecompilerSettings(LanguageVersion.Latest) {
                ThrowOnAssemblyResolveErrors = false
            });

            var source = decompiler.DecompileTypeAsString(new FullTypeName(typeName));
            var tempFile = Path.GetTempFileName();
            File.WriteAllText(tempFile, source);
            return tempFile;
        }

    }
}