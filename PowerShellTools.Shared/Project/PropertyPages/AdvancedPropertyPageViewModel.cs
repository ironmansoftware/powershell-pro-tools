using PowerShellToolsPro;
using PowerShellToolsPro.Options;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;

namespace PowerShellTools.Shared.Project.PropertyPages
{
    public class AdvancedPropertyPageViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private string _entryPoint;
        private bool _bundle;
        private bool _packageAsExecutable;
        private string _netVersion;
        private string _powerShellVersion;
        private string _outputType;
        private string _platform;
        private bool _hideConsoleWindow;
        private bool _packageModules;
        private bool _requireElevation;
        private bool _obfuscateExecutable;
        private bool _highDpiSupport;
        private string _icon;
        private string _powerShellArguments;
        private string _fileDescription;
        private string _fileVersion;
        private string _productName;
        private string _productVersion;
        private string _copyright;
        private string _serviceName;
        private string _serviceDisplayName;
        private bool _servicePropertiesEnabled;
        private bool _filePropertiesEnabled;
        private bool _dotNetVersionEnabled;
        private string _outputName;
        private string _companyName;
        private string _operatingSystem;
        private AdvancedOptionsPane _pane;
        private IVisualStudio _visualStudio;

        public AdvancedPropertyPageViewModel(IVisualStudio visualStudio)
        {
            _visualStudio = visualStudio;

            PowerShellVersions = new[]
            {
                "Windows PowerShell",
                "7.0.0",
                "7.0.1",
                "7.0.2",
                "7.0.3",
                "7.0.6",
                "7.1.0",
                "7.1.1",
                "7.1.2",
                "7.1.3",
                "7.1.4",
                "7.1.5",
                "7.2.0",
                "7.2.1",
                "7.2.2",
                "7.2.3",
                "7.2.4",
                "7.2.5",
                "7.2.6",
                "7.2.7",
                "7.3.0",
                "7.3.1",
                "7.3.2",
                "7.3.3",
                "7.3.4",
                "7.3.5",
                "7.3.6",
                "7.3.7",
                "7.3.8",
                "7.3.9",
                "7.3.10",
                "7.4.0"

            };

            NetVersions = new[]
            {
                "net462",
                "net470",
                "net471",
                "net472",
                "net480",
                "netcoreapp31",
                "net5.0",
                "net6.0",
                "net7.0",
                "net8.0",
                "net9.0"
            };

            OutputTypes = new[]
            {
                "Console",
                "Service"
            };

            Platforms = new[]
            {
                "x86",
                "x64"
            };

            OperatingSystems = new[]
            {
                "Windows",
                "Linux"
            };
        }

        public void SetProperties(AdvancedOptionsPane pane)
        {
            _pane = pane;
            OutputName = pane.OutputName;
            EntryPoint = pane.PackageEntryPoint;
            Bundle = pane.Bundle;
            PackageAsExecutable = pane.PackageAsExecutable;
            NetVersion = pane.DotNetVersion;
            PowerShellVersion = pane.PowerShellVersion;
            OutputType = pane.PackageType;
            Platform = pane.PackagePlatform;
            HideConsoleWindow = pane.HideConsoleWindow;
            PackageModules = pane.PackageModules;
            RequireElevation = pane.RequireElevation;
            ObfuscateExecutable = pane.Obfuscate;
            HighDpiSupport = pane.HighDPISupport;
            Icon = pane.ApplicationIconPath;
            PowerShellArguments = pane.PowerShellArgs;
            FileDescription = pane.FileDescription;
            FileVersion = pane.FileVersion;
            ProductName = pane.ProductName;
            ProductVersion = pane.ProductVersion;
            Copyright = pane.Copyright;
            ServiceName = pane.ServiceName;
            ServiceDisplayName = pane.ServiceDisplayName;
            CompanyName = pane.CompanyName;
            OperatingSystem = pane.OperatingSystem;

            var items = new List<string>();
            foreach (var file in _visualStudio.ActiveWindowProject.Files)
            {
                string fileName = file.FileName;
                if (file.FileName.EndsWith(".xaml", StringComparison.OrdinalIgnoreCase) && File.Exists(file.FileName + ".ps1"))
                {
                    fileName = file.FileName + ".ps1";
                }

                if (!fileName.EndsWith(".ps1", StringComparison.OrdinalIgnoreCase)) continue;
                var fileInfo = new FileInfo(_pane.VisualStudio.ActiveWindowProject.FullName);

                fileName = fileName.Replace(fileInfo.DirectoryName, string.Empty);
                fileName = fileName.TrimStart('\\');

                items.Add(fileName);
            }

            Files = items.ToArray();
        }

        public string[] OutputTypes { get; set; }
        public string[] Platforms { get; set; }
        public string[] NetVersions { get; set; }
        public string[] OperatingSystems { get; set; }

        public string[] PowerShellVersions { get; set; }

        public string[] Files { get; set; }

        public string EntryPoint
        {
            get { return _entryPoint; }
            set { _entryPoint = value; _pane.PackageEntryPoint = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(EntryPoint))); }
        }

        public string OutputName
        {
            get { return _outputName; }
            set { _outputName = value; _pane.OutputName = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(OutputName))); }
        }

        public bool Bundle
        {
            get { return _bundle; }
            set { _bundle = value; _pane.Bundle = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Bundle))); }
        }

        public bool PackageAsExecutable
        {
            get { return _packageAsExecutable; }
            set {
                _packageAsExecutable = value;
                _pane.PackageAsExecutable = value;
                FilePropertiesEnabled = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PackageAsExecutable)));
            }
        }

        public bool DotNetVersionEnabled
        {
            get { return _dotNetVersionEnabled; }
            set { _dotNetVersionEnabled = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DotNetVersionEnabled))); }
        }

        public string NetVersion
        {
            get { return _netVersion; }
            set { _netVersion = value; _pane.DotNetVersion = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(NetVersion))); }
        }

        public string PowerShellVersion
        {
            get { return _powerShellVersion; }
            set {
                _powerShellVersion = value;
                _pane.PowerShellVersion = value;

                if (value == "Windows PowerShell")
                {
                    DotNetVersionEnabled = true;
                }

                if (Version.TryParse(value, out Version version) && version.Major == 7 && version.Minor == 0)
                {
                    NetVersion = "netcoreapp31";
                    DotNetVersionEnabled = false;
                }

                if (Version.TryParse(value, out Version version1) && version1.Major == 7 && version1.Minor == 1)
                {
                    NetVersion = "net5.0";
                    DotNetVersionEnabled = false;
                }

                if (Version.TryParse(value, out Version version2) && version2.Major == 7 && version2.Minor == 2)
                {
                    NetVersion = "net6.0";
                    DotNetVersionEnabled = false;
                }

                if (Version.TryParse(value, out Version version3) && version2.Major == 7 && version2.Minor == 3)
                {
                    NetVersion = "net7.0";
                    DotNetVersionEnabled = false;
                }

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PowerShellVersion)));
            }
        }

        public string OutputType
        {
            get { return _outputType; }
            set {
                _outputType = value;
                _pane.PackageType = value;

                if (value == "Console")
                {
                    ServicePropertiesEnabled = false;
                }
                else
                {
                    ServicePropertiesEnabled = true;
                }

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(OutputType)));
            }
        }

        public string Platform
        {
            get { return _platform; }
            set { _platform = value; _pane.PackagePlatform = value;  PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Platform))); }
        }

        public string OperatingSystem
        {
            get { return _operatingSystem; }
            set { _operatingSystem = value; _pane.OperatingSystem = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(OperatingSystem))); }
        }

        public bool HideConsoleWindow
        {
            get { return _hideConsoleWindow; }
            set { _hideConsoleWindow = value; _pane.HideConsoleWindow = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HideConsoleWindow))); }
        }

        public bool PackageModules
        {
            get { return _packageModules; }
            set { _packageModules = value; _pane.PackageModules = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PackageModules))); }
        }

        public bool RequireElevation
        {
            get { return _requireElevation; }
            set { _requireElevation = value; _pane.RequireElevation = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(RequireElevation))); }
        }

        public bool ObfuscateExecutable
        {
            get { return _obfuscateExecutable; }
            set { _obfuscateExecutable = value; _pane.Obfuscate = value;  PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ObfuscateExecutable))); }
        }

        public bool HighDpiSupport
        {
            get { return _highDpiSupport; }
            set { _highDpiSupport = value; _pane.HighDPISupport = value;  PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HighDpiSupport))); }
        }

        public string Icon
        {
            get { return _icon; }
            set { _icon = value; _pane.ApplicationIconPath = value;  PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Icon))); }
        }

        public string PowerShellArguments
        {
            get { return _powerShellArguments; }
            set { _powerShellArguments = value; _pane.PowerShellArgs = value;  PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PowerShellArguments))); }
        }

        public bool FilePropertiesEnabled
        {
            get { return _filePropertiesEnabled; }
            set { _filePropertiesEnabled = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FilePropertiesEnabled))); }
        }

        public string FileDescription
        {
            get { return _fileDescription; }
            set { _fileDescription = value; _pane.FileDescription = value;  PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FileDescription))); }
        }

        public string FileVersion
        {
            get { return _fileVersion; }
            set { _fileVersion = value; _pane.FileVersion = value;  PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FileVersion))); }
        }
        public string ProductName
        {
            get { return _productName; }
            set { _productName = value; _pane.ProductName = value;  PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ProductName))); }
        }
        public string ProductVersion
        {
            get { return _productVersion; }
            set { _productVersion = value; _pane.ProductVersion = value;  PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ProductVersion))); }
        }

        public string Copyright
        {
            get { return _copyright; }
            set { _copyright = value; _pane.Copyright = value;  PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Copyright))); }
        }

        public string CompanyName
        {
            get { return _companyName; }
            set { _companyName = value; _pane.CompanyName = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CompanyName))); }
        }
        public bool ServicePropertiesEnabled
        {
            get { return _servicePropertiesEnabled; }
            set { _servicePropertiesEnabled = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ServicePropertiesEnabled))); }
        }

        public string ServiceName
        {
            get { return _serviceName; }
            set { _serviceName = value; _pane.ServiceName = value;  PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ServiceName))); }
        }

        public string ServiceDisplayName
        {
            get { return _serviceDisplayName; }
            set { _serviceDisplayName = value; _pane.ServiceDisplayName = value;  PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ServiceDisplayName))); }
        }
    }
}
