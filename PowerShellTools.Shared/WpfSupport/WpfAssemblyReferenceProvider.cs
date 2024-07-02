using Microsoft.Windows.Design.Host;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Windows;

namespace PowerShellTools.Shared.WpfSupport
{
    internal class WpfAssemblyReferenceProvider : AssemblyReferenceProvider
    {
        private readonly List<AssemblyName> _assemblies;

        public WpfAssemblyReferenceProvider()
        {
            _assemblies = new List<AssemblyName>
            {
                typeof(Application).Assembly.GetName(),
                typeof(PresentationSource).Assembly.GetName()
            };
        }

        public override string Identifier => "PoshTools";

        public override AssemblyName LocalAssemblyName => typeof(WpfAssemblyReferenceProvider).Assembly.GetName();

        public override IEnumerable<AssemblyName> ReferencedAssemblies => _assemblies;

        public override event EventHandler ReferencedAssembliesChanged;
        public override event EventHandler ReferencedAssemblyChanged;
        public override event EventHandler ReferencesInvalid;
        public override event EventHandler<GeneratedAssemblyFilesChangedEventArgs> GeneratedAssemblyFilesChanged;

        public override void AddReference(AssemblyName newReference)
        {
            _assemblies.Add(newReference);
        }
    }
}
