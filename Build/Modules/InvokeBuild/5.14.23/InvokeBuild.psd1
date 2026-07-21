@{
	ModuleVersion = '5.14.23'
	RootModule = 'InvokeBuild.psm1'
	GUID = 'a0319025-5f1f-47f0-ae8d-9c7e151a5aae'
	Author = 'Roman Kuzmin'
	CompanyName = 'Roman Kuzmin'
	Copyright = '(c) Roman Kuzmin'
	Description = 'Build and test automation in PowerShell'
	PowerShellVersion = '3.0'
	AliasesToExport = 'Invoke-Build', 'Build-Checkpoint', 'Build-Parallel'
	VariablesToExport = @()
	FunctionsToExport = @()
	CmdletsToExport = @()
	PrivateData = @{
		PSData = @{
			Tags = 'Build', 'Test', 'Automation'
			ProjectUri = 'https://github.com/nightroman/Invoke-Build'
			LicenseUri = 'http://www.apache.org/licenses/LICENSE-2.0'
			IconUri = 'https://raw.githubusercontent.com/nightroman/Invoke-Build/main/ib.png'
			ReleaseNotes = 'https://github.com/nightroman/Invoke-Build/blob/main/Release-Notes.md'
		}
	}
}