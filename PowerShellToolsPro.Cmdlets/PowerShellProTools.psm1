New-Alias -Name 'pad' -Value 'Show-PSScriptPad' -Force

$Platform = 'windows'
if ($IsLinux) {
    $Platform = 'linux'
}
elseif ($IsMacOS) {
    $Platform = 'mac'
}

$Runtime = "net472"
if ($IsCoreCLR) {
    if ($PSVersionTable.PSVersion.Major -eq 7 -and $PSVersionTable.PSVersion.Minor -ge 1) {
        $Runtime = 'net5.0'
    } 
    elseif ($PSVersionTable.PSVersion.Major -eq 7 -and $PSVersionTable.PSVersion.Minor -eq 0) {
        $Runtime = 'netcoreapp3.1'
    } 
}

function Expand-Object {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory, ValueFromPipeline)]
        [object]$InputObject
    )

    Process {
        if ($InputObject -eq $null) {
            return
        }
        $InputObject | Get-Member -MemberType Properties | ForEach-Object {
            try {
                $Value = $InputObject.($_.Name)
                $Node = [Terminal.Gui.Trees.TreeNode]::new("$($_.Name) = $Value")

                if ($Value -ne $null) {
                    $Children = Expand-Object -InputObject $Value
                    foreach ($child in $Children) {
                        $Node.Children.Add($child)
                    }
                }

                $Node
            }
            catch {
                Write-Host $_
            }
        }

    }
}
