using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using PowerShellTools.Interfaces;
using PowerShellTools.ToolWindows;

namespace PowerShellToolsPro.Options
{
    [Export("AdvancedOptionsPane", typeof(IOptionsPane))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class AdvancedOptionsPane : IOptionsPane
    {
        private const string PackageAsExecutableString = "PackageAsExecutable";
        private const string PackageEntryPointString = "PackageEntryPoint";
        private const string BundleString = "Bundle";
        private const string ObfuscateString = "Obfuscate";
	    private const string HideConsoleWindowString = "HideConsoleWindow";
        private const string DotNetVersionString = "DotNetVersion";
        private const string FileVersionString = "FileVersion";
        private const string FileDescriptionString = "FileDescription";
        private const string ProductNameString = "ProductName";
        private const string ProductVersionString = "ProductVersion";
        private const string CopyrightString = "Copyright";
        private const string RequireElevationString = "RequireElevation";
        private const string PackageModulesString = "PackageModules";
        private const string ApplicationIconPathString = "ApplicationIconPath";
        private const string PowerShellVersionString = "PowerShellVersion";

        private readonly ElementHost _control;
        private readonly AdvancedPropertyPageControl _advancedPropertyPageControl;
        private Dictionary<string, string> _properties;
        private bool _loading;
        
        [ImportingConstructor]
        public AdvancedOptionsPane(IVisualStudio visualStudio)
        {
            VisualStudio = visualStudio;
            _control = new ElementHost();
            _control.Dock = DockStyle.Fill;
            _control.Child = _advancedPropertyPageControl = new AdvancedPropertyPageControl(visualStudio);
        }

        public Control Control
        {
            get { return _control; }
        }

        public IVisualStudio VisualStudio { get; private set; }

        public IEnumerable<string> PropertyNames {
            get
            {
                return new[] {
                    PackageAsExecutableString,
                    PackageEntryPointString,
                    BundleString,
                    ObfuscateString,
	                HideConsoleWindowString,
                    DotNetVersionString,
                    FileVersionString,
                    FileDescriptionString,
                    ProductNameString,
                    ProductVersionString,
                    CopyrightString,
                    RequireElevationString,
                    PackageModulesString,
                    ApplicationIconPathString,
                    nameof(PackageType),
                    nameof(ServiceName),
                    nameof(ServiceDisplayName),
                    nameof(HighDPISupport),
                    nameof(PowerShellArgs),
                    nameof(PackagePlatform),
                    PowerShellVersionString,
                    nameof(OutputName),
                    nameof(CompanyName),
                    nameof(OperatingSystem)
                };

            }
        }

        public Dictionary<string, string> Properties
        {
            get
            {
                return _properties; 
            }
            set
            {
                _loading = true;
                _properties = value;
                _advancedPropertyPageControl.SetProperties(this);
                _loading = false;
            }
        }

        public string PowerShellVersion
        {
            get
            {
                return GetProperty(PowerShellVersionString);
            }
            set
            {
                SetProperty(PowerShellVersionString, value);
            }
        }

        public bool Bundle
        {
            get
            {
                return GetBoolProperty(BundleString);
            }
            set
            {
                SetProperty(BundleString, value);
            }
        }

        public bool PackageAsExecutable
        {
            get
            {
                return GetBoolProperty(PackageAsExecutableString);
            }
            set
            {
                SetProperty(PackageAsExecutableString, value);
            }
        }

        public string ApplicationIconPath
        {
            get
            {
                return GetProperty(ApplicationIconPathString);
            }
            set
            {
                SetProperty(ApplicationIconPathString, value);
            }
        }

        public string PackageEntryPoint
        {
            get
            {
                return GetProperty(PackageEntryPointString);
            }
            set
            {
                SetProperty(PackageEntryPointString, value);
            }
        }

        public string OutputName
        {
            get
            {
                return GetProperty(nameof(OutputName));
            }
            set
            {
                SetProperty(nameof(OutputName), value);
            }
        }

        public bool Obfuscate
        {
            get
            {
                return GetBoolProperty(ObfuscateString);
            }
            set
            {
                SetProperty(ObfuscateString, value);
            }
        }

	    public bool HideConsoleWindow
	    {
		    get
		    {
                return GetBoolProperty(HideConsoleWindowString);
		    }
		    set
		    {
                SetProperty(HideConsoleWindowString, value);
		    }
	    }

        public string DotNetVersion
        {
            get
            {
                return GetProperty(DotNetVersionString);
            }
            set
            {
                SetProperty(DotNetVersionString, value);
            }
        }

        public string OperatingSystem
        {
            get
            {
                return GetProperty(nameof(OperatingSystem));
            }
            set
            {
                SetProperty(nameof(OperatingSystem), value);
            }
        }


        public string FileVersion
        {
            get
            {
                return GetProperty(FileVersionString);
            }
            set
            {
                SetProperty(FileVersionString, value);
            }
        }

        public string FileDescription
        {
            get
            {
                return GetProperty(FileDescriptionString);
            }
            set
            {
                SetProperty(FileDescriptionString, value);
            }
        }

        public string ProductVersion
        {
            get
            {
                return GetProperty(ProductVersionString);
            }
            set
            {
                SetProperty(ProductVersionString, value);
            }
        }

        public string ProductName
        {
            get
            {
                return GetProperty(ProductNameString);
            }
            set
            {
                SetProperty(ProductNameString, value);
            }
        }

        public string Copyright
        {
            get
            {
                return GetProperty(CopyrightString);
            }
            set
            {
                SetProperty(CopyrightString, value);
            }
        }

        public string CompanyName
        {
            get
            {
                return GetProperty(nameof(CompanyName));
            }
            set
            {
                SetProperty(nameof(CompanyName), value);
            }
        }

        public bool RequireElevation
        {
            get
            {
                return GetBoolProperty(RequireElevationString);
            }
            set
            {
                SetProperty(RequireElevationString, value);
            }
        }

        public bool PackageModules
        {
            get
            {
                return GetBoolProperty(PackageModulesString);
            }
            set
            {
                SetProperty(PackageModulesString, value);
            }
        }

        public string PackageType
        {
            get
            {
                return GetProperty(nameof(PackageType));
            }
            set
            {
                SetProperty(nameof(PackageType), value);
            }
        }

        public string ServiceName
        {
            get
            {
                return GetProperty(nameof(ServiceName));
            }
            set
            {
                SetProperty(nameof(ServiceName), value);
            }
        }

        public string ServiceDisplayName
        {
            get
            {
                return GetProperty(nameof(ServiceDisplayName));
            }
            set
            {
                SetProperty(nameof(ServiceDisplayName), value);
            }
        }

        public bool HighDPISupport
        {
            get
            {
                return GetBoolProperty(nameof(HighDPISupport));
            }
            set
            {
                SetProperty(nameof(HighDPISupport), value);
            }
        }

        public string PowerShellArgs
        {
            get
            {
                return GetProperty(nameof(PowerShellArgs));
            }
            set
            {
                SetProperty(nameof(PowerShellArgs), value);
            }
        }

        public string PackagePlatform
        {
            get
            {
                return GetProperty(nameof(PackagePlatform));
            }
            set
            {
                SetProperty(nameof(PackagePlatform), value);
            }
        }

        private string GetProperty(string name)
        {
            if(Properties == null) return null;

            if (Properties.ContainsKey(name))
            {
                return Properties[name];
            }

            return null;
        }

        private bool GetBoolProperty(string name)
        {
            if (Properties == null) return false;

            if (Properties.ContainsKey(name))
            {
                bool value;
                if (bool.TryParse(Properties[name], out value))
                {
                    return value;
                }
            }

            return false;
        }

        private void SetProperty(string name, string value)
        {
            if (Properties == null) return;

            if (Properties.ContainsKey(name))
            {
                Properties[name] = value;
            }
            else
            {
                Properties.Add(name, value);
            }

            if (!_loading)
                RaiseOnDirty();
        }

        private void SetProperty(string name, bool value)
        {
            if (Properties == null) return;

            if (Properties.ContainsKey(name))
            {
                Properties[name] = value.ToString();
            }
            else
            {
                Properties.Add(name, value.ToString());
            }

            if (!_loading)
                RaiseOnDirty();
        }

        public event EventHandler<EventArgs> OnDirty;

        internal void RaiseOnDirty()
        {
            if (_loading) return;

            if (OnDirty != null)
                OnDirty(this, new EventArgs());
        }
    }
}
