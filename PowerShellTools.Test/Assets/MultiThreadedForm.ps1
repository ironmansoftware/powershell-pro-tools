$btnName_Click = {
	Start-RSJob -ScriptBlock {
		$Item = $_
		1..100 | ForEach-Object {
			Start-Sleep -Milliseconds 100
			$Item.Value = $_
			[System.Windows.Forms.Application]::DoEvents();
		}
	} -InputObject $MainForm.Progress
}

. (Join-Path $PSScriptRoot 'MultiThreadedForm.designer.ps1')

$MainForm.ShowDialog()