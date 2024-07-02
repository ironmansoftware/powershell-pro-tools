function Pretty {
    Get-Item "..\PowerShellTools\PrettyPrint.ps1"
}

function New-Pretty ([string]$command, [string]$fileName) {
    $command | Out-File $fileName -Encoding utf8 -Force
    return Get-Item $fileName
}

function Remove-Pretty ([string]$fileName) {
    Get-ChildItem | where fullname -eq $fileName | Remove-Item
}