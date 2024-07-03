param($OutDir = $PSScriptRoot, $Configuration)

$ManifestPath = Join-Path $OutDir PowerShellProTools.psd1
Remove-Item $ManifestPath -ErrorAction SilentlyContinue

$Version = "2024.7.0"
#$prerelease = "-beta1"

$powerShellGet = Import-Module PowerShellGet  -PassThru -ErrorAction Ignore -RequiredVersion 2.2.5

if ($powerShellGet.Version -lt ([Version]'1.6.0')) {
	Install-Module PowerShellGet -Scope CurrentUser -Force -AllowClobber -RequiredVersion 2.2.5
}

$Arguments = @{
	Author          = "Ironman Software, LLC"
	Copyright       = "Ironman Software, LLC"
	RootModule      = "PowerShellProTools.psm1"
	NestedModules   = @("PowerShellToolsPro.Cmdlets.dll")
	Description     = "PowerShell script packaging, profiling, WPF and Windows Forms design, and desktop automation."
	ReleaseNotes    = "https://docs.poshtools.com/powershell-pro-tools-module"
	AliasesToExport = @("pad")
	CmdletsToExport = @("Merge-Script", "Show-WinFormDesigner","ConvertTo-WinForm","Show-PSScriptPad")
	ModuleVersion   = $Version
}


New-ModuleManifest $ManifestPath @Arguments

Import-Module $ManifestPath -Force

New-ExternalHelp -Path (Join-Path $PSScriptRoot "help") -OutputPath (Join-Path $PSScriptRoot "en-US") -Force


