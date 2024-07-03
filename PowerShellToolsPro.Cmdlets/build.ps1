param($Configuration = "Release", [string]$OutputDirectory)

Push-Location $PSScriptRoot

$platyps = Import-Module PlatyPS -PassThru -ErrorAction SilentlyContinue
if ($platyps -eq $null) {
	Install-Module PlatyPS -Force -Scope CurrentUser
	Import-Module PlatyPS
}

dotnet publish -f netstandard2.0 -c $Configuration

if (-not $OutputDirectory) {
	$OutputDirectory = Join-Path $PSScriptRoot "out"
}

$FormDesigner = Join-Path $OutputDirectory "FormDesigner"
Remove-Item $OutputDirectory -Force -ErrorAction SilentlyContinue -Recurse
New-Item -ItemType directory $OutputDirectory
New-Item -ItemType directory $FormDesigner

Copy-Item -Path (Join-Path $PSScriptRoot 'PowerShellProTools.psm1') -Destination $OutputDirectory

$WinFormDesigner = (Join-Path $PSScriptRoot '..\WinFormDesigner\bin\x64\Release\WinFormDesigner.exe')
if (-not (Test-Path $WinFormDesigner)) {
	$WinFormDesigner = (Join-Path $PSScriptRoot '..\WinFormDesigner\bin\Release\WinFormDesigner.exe')
}
Copy-Item -Path $WinFormDesigner -Destination $FormDesigner

if ($ENV:APPVEYOR) {
	Copy-Item -Path (Join-Path $PSScriptRoot "bin\Any CPU\$Configuration\netstandard2.0\publish\*") -Destination $OutputDirectory -Recurse
	Copy-Item -Path (Join-Path $PSScriptRoot "bin\Any CPU\$Configuration\netstandard2.0\PowerShellProTools.psd1") -Destination $OutputDirectory -Recurse
}
else {
	Copy-Item -Path (Join-Path $PSScriptRoot "bin\$Configuration\netstandard2.0\publish\*") -Destination $OutputDirectory -Recurse
	Copy-Item -Path (Join-Path $PSScriptRoot "bin\$Configuration\netstandard2.0\PowerShellProTools.psd1") -Destination $OutputDirectory -Recurse
}

Pop-Location