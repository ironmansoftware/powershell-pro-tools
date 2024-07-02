param(
    [ValidateSet('Debug', 'Release')]
    $Configuration = 'Release'
)

Push-Location $PSScriptRoot\powershellprotools

$RootDir = "$PSScriptRoot\powershellprotools"

task Clean {
    $directories = @(
        (Join-Path $RootDir "node_modules"),
        (Join-Path $RootDir "package-lock.json"),
        (Join-Path $RootDir "out")
        (Join-Path $RootDir "kit")
    )

    $directories | ForEach-Object {
        if (Test-Path $_) {
            Remove-Item $_ -Force -Recurse
        }
    }

    New-Item (Join-Path $RootDir "kit") -ItemType Directory -ErrorAction SilentlyContinue
    New-Item (Join-Path $RootDir "out") -ItemType Directory -ErrorAction SilentlyContinue
}

task Init {
    if ($null -eq (Get-Module VSSetup -ListAvailable)) {
        Install-Module VSSetup -Scope CurrentUser -Force -ErrorAction SilentlyContinue
    }
}

task BuildPSScriptPad {
    $instance = Get-VSSetupInstance -All -Prerelease | Select-VSSetupInstance -Require 'Microsoft.Component.MSBuild' -Latest
    $installDir = $instance.installationPath
    Write-Host "Visual Studio is found at $installDir"
    $msBuild = $installDir + '\MSBuild\Current\Bin\MSBuild.exe' 
    
    Push-Location (Join-Path $RootDir ".\..\..\FormDesigner.Host")
    & $msBuild -property:Configuration=Release .\PSScriptPad.csproj
    Pop-Location
}

task BuildHost {
    Push-Location (Join-Path $RootDir ".\..\..\HostInjection")
    dotnet build -c $Configuration -o $RootDir\Modules\PowerShellProTools.VSCode
    Pop-Location
}

task BuildCmdlets {
    Push-Location (Join-Path $RootDir ".\..\..\PowerShellToolsPro.Cmdlets")
    & .\build.ps1 -OutputDirectory $RootDir\Modules\PowerShellProTools
    Pop-Location
}

task BuildExtension {
    Push-Location $RootDir
    & {
        $ErrorActionPreference = 'SilentlyContinue'
        npm install -g npm
        npm install -g typescript@latest
        npm install -g vsce
        npm install

        vsce package

        Move-Item (Join-Path $RootDir "*.vsix") (Join-Path $RootDir "kit")
    }
    Pop-Location
}

task Build BuildHost, BuildPSScriptPad, BuildCmdlets, BuildExtension

task . Init, Clean, Build

Pop-Location