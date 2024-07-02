using System;
using System.Globalization;
using Microsoft.VisualStudio.Shell;

namespace PowerShellTools.Project
{
    /// <summary>
    /// This attribute registers a DeveloperActivity key.  
    /// If present, the position of the language root node
    /// is controlled by the Developer Settings dialog box.
    /// </summary>
    internal sealed class DeveloperActivityAttribute : RegistrationAttribute
    {
        private readonly string _regKeyName;
        private readonly string _developerActivity;

        /// <summary>
        /// Creates a new DeveloperActivityAttribute
        /// </summary>
        /// <param name="developerActivity">Name of language</param>
        /// <param name="projectPackageType">Project package type</param>
        public DeveloperActivityAttribute(string developerActivity, Type projectPackageType)
        {
            _developerActivity = developerActivity;
            _regKeyName = string.Format(CultureInfo.InvariantCulture, "NewProjectTemplates\\TemplateDirs\\{0}\\/1", projectPackageType.GUID.ToString("B"));
        }

        /// <summary>
        /// Called to register this attribute with the given context
        /// </summary>
        /// <param name="context">The registration context.</param>
        public override void Register(RegistrationContext context)
        {
            var key = context.CreateKey(_regKeyName);
            key.SetValue("DeveloperActivity", _developerActivity);
        }

        /// <summary>
        /// Unregisters this package's load key information
        /// </summary>
        /// <param name="context">The registration context.</param>
        public override void Unregister(RegistrationContext context)
        {
            context.RemoveKey(_regKeyName);
        }
    }
}
