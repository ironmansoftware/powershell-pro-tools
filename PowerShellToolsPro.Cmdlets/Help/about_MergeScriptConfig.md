# MergeScriptConfig
## about_MergeScriptConfig
         
# SHORT DESCRIPTION
About config hashtables for Merge-Script

# LONG DESCRIPTION
This about file contains information about using hashtables and PSD1 files to 
configure Merge-Script.

## Config File Schema
```
@{
	Root = "" # The root script. This is required.
	OutputPath = "" # The output path for Merge-Script. This is required.
	Bundle = @{
		Enabled = $true # Whether bundling is enabled
		Modules = $true # Whether to bundle imported modules
		NestedModules = $true #Whether to bundle modules required by other modules
		RequiredAssemblies = $true # Whether to bundle required assemblies 
	}
	Package = @{
		Enabled = $true # Whether to package as an executable
		Obfuscate = $true # Whether to obfuscate the executable
		HideConsoleWindow = $true # Whether to hide the console window after starting the script
	}
}
```

## Using a config file

A config file can be used either from within a PowerShell script as a hashtable or imported from a PSD1
file containing the hashtable. 

# EXAMPLES

It is not required to include all aspects of the config when using Merge-Script. The only required
components are Root and OutputPath. Aside from that, anything that is not include will be considered 
false. This means that in the below example, packaging is disabled but bundling is not. The below operation
will not bundle nested modules or required assemblies of any modules it is bundling.
```
Merge-Script -Config @{ 
	Root = ".\MyScript.ps1"
	OutputPath = ".\"
	Bundle = @{
		Enabled = $true
		Modules = $true
	}
}
```
