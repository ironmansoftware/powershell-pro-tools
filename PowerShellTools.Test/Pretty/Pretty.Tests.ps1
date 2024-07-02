$here = Split-Path -Parent $MyInvocation.MyCommand.Path
$sut = (Split-Path -Leaf $MyInvocation.MyCommand.Path).Replace(".Tests.", ".")
. "$here\$sut"

# Load PrettyPrint.ps1
. (pretty)

# fileName for Test
$fileName = "PrettyTemp.ps1"

# Run Pester Test with Invoke-Pester
Describe "Pretty Alias Should pass Test" {
    # ls to Get-ChildItem
    $file = New-Pretty -Command ls -filename $fileName
	It "Should resolve ls to Get-ChildItem" {
		Format-Script　$file.fullname | Should Be "Get-ChildItem`r`n"
	}
    Remove-Pretty -filename $file.fullname

    # ps to Get-Process
    $file = New-Pretty -Command ps -filename $fileName
	It "Should resolve ps to Get-Process" {
		Format-Script　$file.fullname | Should Be "Get-Process`r`n"
	}
    Remove-Pretty -filename $file.fullname
}

Describe "Pretty Alias Should NOT pass Test" {
    # hoge is not define any alias
    $file = New-Pretty -Command hoge -filename $fileName
	It "Should NOT resolve hoge to $null" {
		Format-Script　$file.fullname | Should not Be $null
	}
    Remove-Pretty -filename $file.fullname

    # hoge must not resolve to any
    $file = New-Pretty -Command hoge -filename $fileName
	It "Should keep hoge as hoge" {
		Format-Script　$file.fullname | Should Be "hoge`r`n"
	}
    Remove-Pretty -filename $file.fullname
}

Describe "Pretty Type Should pass Test" {
    # string to System.String
    $file = New-Pretty -Command [string] -filename $fileName
	It "Should resolve string to Sytem.String" {
		Format-Script　$file.fullname | Should Be "[System.String]`r`n"
	}
    Remove-Pretty -filename $file.fullname

    # PSCustomObject must keep as PSCustomObject
    $file = New-Pretty -Command [PSCustomObject] -filename $fileName
	It "Should keep PSCustomObject as PSCustomObject" {
		Format-Script　$file.fullname | Should Be "[PSCustomObject]`r`n"
	}
    Remove-Pretty -filename $file.fullname
}

Describe "Pretty Type Should not pass Test" {
    $file = New-Pretty -Command [string] -filename $fileName
	It "Should resolve string to Sytem.String" {
		Format-Script　$file.fullname | Should not Be "[System.Int32]`r`n"
	}
    Remove-Pretty -filename $file.fullname

    # PSCustomObject must keep as is
    $file = New-Pretty -Command [PSCustomObject] -filename $fileName
	It "Should not resolve PSCustomObject to System.Management.Automation.PSObject" {
		Format-Script　$file.fullname | Should not Be "[System.Management.Automation.PSObject]`r`n"
	}
    Remove-Pretty -filename $file.fullname
}