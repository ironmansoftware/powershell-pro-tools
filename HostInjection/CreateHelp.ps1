param(
    [string]$HelpPath = (Join-Path $PSScriptRoot "Help"),
    [string]$OutputPath = (Join-Path $PSScriptRoot "en-US")
)

$platyps = Import-Module Microsoft.PowerShell.PlatyPS -RequiredVersion 1.0.1 -PassThru -ErrorAction SilentlyContinue
if ($null -eq $platyps) {
    Install-Module Microsoft.PowerShell.PlatyPS -RequiredVersion 1.0.1 -Scope CurrentUser -Force -AllowClobber
    Import-Module Microsoft.PowerShell.PlatyPS -RequiredVersion 1.0.1
}

$commandHelp = Import-MarkdownCommandHelp -Path (Join-Path $HelpPath "*.md")
$externalHelpFiles = $commandHelp | Select-Object -ExpandProperty ExternalHelpFile -Unique
Export-MamlCommandHelp -CommandHelp $commandHelp -OutputFolder $OutputPath -Force | Out-Null

Get-ChildItem $OutputPath -Filter "*-Help.xml" -Recurse |
    Where-Object { $_.DirectoryName -ne (Resolve-Path $OutputPath).Path } |
    Where-Object { $externalHelpFiles -contains $_.Name } |
    ForEach-Object {
        Copy-Item $_.FullName -Destination (Join-Path $OutputPath $_.Name) -Force
    }

Get-ChildItem $OutputPath -Directory | Remove-Item -Recurse -Force

$utf8NoBom = New-Object System.Text.UTF8Encoding $false
Get-ChildItem $OutputPath -Filter "*-Help.xml" | ForEach-Object {
    if ($externalHelpFiles -notcontains $_.Name) {
        return
    }

    $content = [System.IO.File]::ReadAllText($_.FullName)
    $lines = $content -split "\r?\n" | ForEach-Object { $_ -replace "\s+$", "" }
    $content = ($lines | Where-Object { $null -ne $_ }) -join "`n"
    if (-not $content.EndsWith("`n")) {
        $content += "`n"
    }
    [System.IO.File]::WriteAllText($_.FullName, $content, $utf8NoBom)
}
