using Microsoft.VisualStudio.Text.Classification;

namespace PowerShellTools
{
    /// <summary>
    /// Imported editor services through ComponentModel.
    /// </summary>
    /// <remarks>
    /// For a MEF type, MEF components can be imported directly using Import. 
    /// For non-MEF types where MEF components are needed, they can use ComponentModel to import the services. 
    /// </remarks>
    internal static class EditorImports
    {
        private static IClassificationTypeRegistryService _classificationTypeRegistryService;

        /// <summary>
        /// Imported IClassificationTypeRegistryService.
        /// </summary>
        public static IClassificationTypeRegistryService ClassificationTypeRegistryService
        {
            get
            {
                if (_classificationTypeRegistryService == null)
                {
                    _classificationTypeRegistryService = PowerShellToolsPackage.GetService<IClassificationTypeRegistryService>();
                }
                return _classificationTypeRegistryService;
            }
            set
            {
                // For unit tests use
                _classificationTypeRegistryService = value;
            }
        }

    }
}
