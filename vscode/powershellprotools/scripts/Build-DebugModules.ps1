param(
    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Debug'
)

$ErrorActionPreference = 'Stop'

$ExtensionRoot = Resolve-Path (Join-Path $PSScriptRoot '..')
$RepoRoot = Resolve-Path (Join-Path $ExtensionRoot '..\..')
$ModulesRoot = Join-Path $ExtensionRoot 'Modules'
$VSCodeModuleOutput = Join-Path $ModulesRoot 'PowerShellProTools.VSCode'
$PowerShellProToolsOutput = Join-Path $ModulesRoot 'PowerShellProTools'

Remove-Item $VSCodeModuleOutput -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item $PowerShellProToolsOutput -Recurse -Force -ErrorAction SilentlyContinue
New-Item -ItemType Directory -Path $VSCodeModuleOutput -Force | Out-Null
New-Item -ItemType Directory -Path $PowerShellProToolsOutput -Force | Out-Null

$HostInjectionProject = Join-Path $RepoRoot 'HostInjection\PowerShellProTools.Common.csproj'
$CmdletsProject = Join-Path $RepoRoot 'PowerShellToolsPro.Cmdlets\PowerShellToolsPro.Cmdlets.csproj'

Push-Location $RepoRoot
try {
    dotnet build $HostInjectionProject -c $Configuration -o $VSCodeModuleOutput
    if ($LASTEXITCODE -ne 0) {
        throw "Failed to build PowerShellProTools.VSCode module."
    }

    dotnet publish $CmdletsProject -f netstandard2.0 -c $Configuration -o $PowerShellProToolsOutput -p:SkipPowerShellProToolsPostBuild=true
    if ($LASTEXITCODE -ne 0) {
        throw "Failed to build PowerShellProTools module."
    }
}
finally {
    Pop-Location
}
