# 2021.9.0 

- Fixed an issue where updating modules would not work properly
- Fixed an issue where packaging scripts would not locate modules properly 
- Added support for switching .NET SDK versions in the package.psd1
- Fixed an MSBuild warning that was shown while packaging on Windows
- Added support for embedding additional XAML files into a package
- Fixed an issue where .NET Core 5.0 would not produce correct PowerShell executables