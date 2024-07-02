Import-Module Pester
Import-Module "$PSScriptRoot\TestUtilities.psm1" -Force

Describe "Merge-Script" {
    Context "PowerShellCore" {
        It "should bundle script with PS Core" {
            $Script = Join-Path ([IO.Path]::GetTempPath()) "Script.ps1"
            $OutputPath = Join-Path ([IO.Path]::GetTempPath()) "output"
            $OutputExe = Join-Path $OutputPath "Script.exe"

            '$PSVersionTable' | Out-File $Script

            Merge-Script -Config @{
                Root = $Script
                OutputPath = $OutputPath
                Package = @{
                    Enabled = $true
                    PowerShellCore = $true
                }
                Bundle = @{
                    Enabled = $true
                    Modules = $true
                }
            }

            $Val = . $OutputExe
            $Val[4].Contains("Core") | Should be $true

            Remove-Item -Force -Path $Script 
            Remove-Item -Force -Path $OutputPath -Recurse
        }
    }
}