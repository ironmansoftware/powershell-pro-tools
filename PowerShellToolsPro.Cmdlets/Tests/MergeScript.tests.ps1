<# Describe "Merge-Script" {
	 Context "ConfigFile" {
		 $tempScript = [IO.Path]::GetTempFileName() + ".ps1"
		 "Write-Host Hey!" | Out-File $tempScript
		 $outputPath = [IO.Path]::GetTempPath()
		 "@{ Root = '$tempScript'; OutputPath = '$outputPath'}"

		 It "Uses config file" {
			
		 }

		 Remove-Item $tempScript -Force
	 }
} #>