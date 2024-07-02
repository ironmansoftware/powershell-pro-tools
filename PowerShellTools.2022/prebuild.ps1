$Version = (Get-Content "$PSScriptRoot\..\poshtools.version.txt" -Raw).Trim()
[xml]$xml = Get-Content "$PSScriptRoot\PowerShellTools.15.0.imagemanifest"
$xml.imagemanifest.symbols.String.Value = "/PowerShellTools.2022.$Version;component/Resources"
$Xml.Save("$PSScriptRoot\PowerShellTools.15.0.imagemanifest")