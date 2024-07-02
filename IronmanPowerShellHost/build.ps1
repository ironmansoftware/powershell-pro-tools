Set-Location $PSScriptRoot
$HostPath = "$Env:LocalAppData\IronmanPowerShellHost\9.9.9"
Remove-Item $HostPath -ErrorAction SilentlyContinue -Recurse
New-Item $HostPath -ItemType Directory -Force

dotnet publish -f net472 -c Release -o "$HostPath" -p:AssemblyName=IronmanPowerShellHost .\IronmanPowerShellHost.csproj 

function Start-PS7Build {
    param(
        [string]$PowerShellVersion, 
        [ValidateSet("net6.0", "net7.0")]
        [string]$dotnetVersion,
        [Switch]$Lightweight,
        [Switch]$WinForms
    )

    $framework = $dotnetVersion
    if ($WinForms) {
        $framework = "$framework-windows"
    }

    $properties = @("-o", $HostPath, "-p:AssemblyName=Temp", "--runtime", "win-x64", "-p:PowerShellVersion=$PowerShellVersion", "/p:nowarn=SYSLIB0012", "/p:nowarn=IL3000", "/p:nowarn=IL3002")

    if ($WinForms) {
        $properties += "-p:UseWindowsForms=true"
    }

    $configuration = "Release"
    if ($Lightweight) {
        $configuration = "NoPowerShell"
        $properties += "-p:PublishSingleFile=true"
        $properties += "--no-self-contained"
        $properties += "-p:IncludeAllContentForSelfExtract=false"
    }
    else {
        $properties += "-p:PublishSingleFile=true"
        $properties += "--self-contained"
        $properties += "-p:IncludeAllContentForSelfExtract=false"

    }

    $TargetFileName = "IronmanPowerShellHost.$PowerShellVersion"
    if ($WinForms) {
        $TargetFileName += ".winforms"
    }

    if ($Lightweight) {
        $TargetFileName += ".lightweight"
    }

    $TargetFileName += ".exe"

    dotnet publish -f $framework -c $configuration $properties .\IronmanPowerShellHost.csproj
    Rename-Item "$HostPath\Temp.exe" "$HostPath\$TargetFileName"

    Get-ChildItem  $HostPath -Exclude "*.exe" | Remove-Item -Recurse
}

Start-PS7Build -PowerShellVersion "7.2.7" -dotnetVersion "net6.0"
Start-PS7Build -PowerShellVersion "7.2.7" -dotnetVersion "net6.0" -Lightweight
Start-PS7Build -PowerShellVersion "7.2.7" -dotnetVersion "net6.0" -WinForms
Start-PS7Build -PowerShellVersion "7.2.7" -dotnetVersion "net6.0" -Lightweight -WinForms

Copy-Item ".\host.manifest.json" -Destination $HostPath -Force