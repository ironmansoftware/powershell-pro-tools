using Microsoft.VisualStudio.Settings;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Settings;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;
using System;
using System.ComponentModel.Composition;
using System.Reflection;
using System.Windows;

namespace PowerShellTools
{
    [ContentType("PowerShell")]
    [Name("PowerShell Redirect Margin")]
    [MarginContainer("Top")]
    [Export(typeof(IWpfTextViewMarginProvider))]
    [TextViewRole("DOCUMENT")]
    internal sealed class UpdateMarginProvider : IWpfTextViewMarginProvider, IWpfTextViewMargin
    {
        public FrameworkElement VisualElement => new UpdateMarginControl();

        public double MarginSize => 10;

        public bool Enabled => true;

        public IWpfTextViewMargin CreateMargin(IWpfTextViewHost wpfTextViewHost, IWpfTextViewMargin marginContainer)
        {
            try
            {
                SettingsManager settingsManager = new ShellSettingsManager(ServiceProvider.GlobalProvider);
                var writeableStore = settingsManager.GetWritableSettingsStore(SettingsScope.UserSettings);
                if (!writeableStore.CollectionExists("PoshTools"))
                {
                    writeableStore.CreateCollection("PoshTools");
                }

                var currentVersion = Assembly.GetExecutingAssembly().GetName().Version;
                if (!writeableStore.PropertyExists("PoshTools", "LastVersion"))
                {
                    writeableStore.SetString("PoshTools", "LastVersion", currentVersion.ToString());
                    return this;
                }

                var lastVersion = writeableStore.GetString("PoshTools", "LastVersion");
                if (Version.TryParse(lastVersion, out Version lastVersionVersion) && lastVersionVersion < currentVersion)
                {
                    writeableStore.SetString("PoshTools", "LastVersion", currentVersion.ToString());

                    return this;
                }

                return null;
            }
            catch
            {
                return null;
            }
        }

        public void Dispose()
        {
            
        }

        public ITextViewMargin GetTextViewMargin(string marginName)
        {
            return this;
        }
    }
}
