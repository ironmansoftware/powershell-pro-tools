---
external help file: PowerShellToolsPro.Cmdlets.dll-Help.xml
online version: 
schema: 2.0.0
---

# Merge-Script

## SYNOPSIS
Packages, bundles and\or obfuscates scripts.

## SYNTAX

### Parameters
```
Merge-Script -Script <String> [-OutputPath <String>] [-Bundle] [-Package] [-Obfuscate] [<CommonParameters>]
```

### ConfigFile
```
Merge-Script -ConfigFile <String> [<CommonParameters>]
```

### Config
```
Merge-Script -Config <Hashtable> [<CommonParameters>]
```

## DESCRIPTION
Packages, bundles and\or obfuscates scripts. Packaging and bundling are not mutually exclusive. Obfuscation
requires packaging.

## EXAMPLES

### Example 1
```
PS C:\> Merge-Script -Script .\MyScript.ps1 -Output .\ -Package
```

Packages MyScript.ps1 into MyScript.exe and then outputs it to .\

### Example 2
```
PS C:\> Merge-Script -Script .\MyScript.ps1 -Output .\Bundle -Bundle
```

Bundles MyScript.ps1 and any scripts it dot sources into a single file and outputs it to .\Bundle.

### Example 3
```
PS C:\> Merge-Script -Script .\MyScript.ps1 -Output .\Bundle -Bundle -Package
```

Bundles MyScript.ps1 and any scripts it dot sources into a single file and then packages it into MyScript.exe and outputs it to .\Bundle.

### Example 4
```
PS C:\> Merge-Script -Script .\MyScript.ps1 -Output .\Bundle -Bundle -Package -Obfuscate
```

Bundles MyScript.ps1 and any scripts it dot sources into a single file and then packages it into MyScript.exe and outputs it to .\Bundle. The resulting executable will be obfuscated. 

### Example 5
```
PS C:\> Merge-Script -Config @{ Root = ".\MyScript.ps1"; Output = ".\"; Bundle = @{ Enabled = $true; NestedModules = $true; Modules = $true; } }
```

Uses a config hashtable to bundle MyScript.ps1 and any modules and nested modules with the script. 

## PARAMETERS

### -Bundle
Bundles the script with dot sourced scripts found in the script.

```yaml
Type: SwitchParameter
Parameter Sets: Parameters
Aliases: 

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Config
A hashtable to specify the config for the cmdlet. See about_MergeScriptConfig.

```yaml
Type: Hashtable
Parameter Sets: Config
Aliases: 

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ConfigFile
A config file to specify for the cmdlet.  See about_MergeScriptConfig.

```yaml
Type: String
Parameter Sets: ConfigFile
Aliases: 

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Obfuscate
Obfuscate the .NET executable and PowerShell script.

```yaml
Type: SwitchParameter
Parameter Sets: Parameters
Aliases: 

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -OutputPath
The output path for the resulting script or executable.
This should be a directory.

```yaml
Type: String
Parameter Sets: Parameters
Aliases: 

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Package
Package the script as a .NET executable.

```yaml
Type: SwitchParameter
Parameter Sets: Parameters
Aliases: 

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Script
The script to package in an executable and optionally bundle with other scripts.

```yaml
Type: String
Parameter Sets: Parameters
Aliases: 

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see about_CommonParameters (http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### None

## OUTPUTS

### System.Object

## NOTES

## RELATED LINKS

