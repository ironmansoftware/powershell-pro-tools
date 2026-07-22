Import-Module "$PSScriptRoot\PowerShellProTools.VSCode.dll"
Import-Module "$PSScriptRoot\PowerShellProTools.SharedCommands.dll"

if (Get-Variable -Name vscode -Scope Global -ErrorAction SilentlyContinue) {
    Remove-Variable -Name vscode -Scope Global -Force
}

Set-Variable -Name vscode -Scope Global -Option ReadOnly -Value ([PowerShellToolsPro.Cmdlets.VSCode.VSCodeApiBootstrap]::Create())
