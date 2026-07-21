---
document type: cmdlet
external help file: PowerShellProTools.VSCode.dll-Help.xml
HelpUri: ''
Locale: en-US
Module Name: PowerShellProTools.VSCode
ms.date: 07-21-2026
PlatyPS schema version: 2024-05-01
title: Measure-Script
---

# Measure-Script

## SYNOPSIS

Measures execution timing for a script file or script block.

## SYNTAX

### ScriptBlock

```
Measure-Script [-ScriptBlock] <scriptblock> [<CommonParameters>]
```

### FilePath

```
Measure-Script [-FilePath] <string> [<CommonParameters>]
```

## ALIASES

This cmdlet has no aliases.

## DESCRIPTION

Runs a PowerShell script file or script block under the PowerShell Pro Tools profiler and returns timing information for the profiled script.

## EXAMPLES

### Example 1

```powershell
Measure-Script -FilePath .\Build.ps1
```
Profiles a PowerShell script file.

## PARAMETERS

### -FilePath

The path to the PowerShell script file.

```yaml
Type: System.String
DefaultValue: ''
SupportsWildcards: false
Aliases: []
ParameterSets:
- Name: FilePath
  Position: 0
  IsRequired: true
  ValueFromPipeline: false
  ValueFromPipelineByPropertyName: false
  ValueFromRemainingArguments: false
DontShow: false
AcceptedValues: []
HelpMessage: ''
```

### -ScriptBlock

The script block to profile or measure.

```yaml
Type: System.Management.Automation.ScriptBlock
DefaultValue: ''
SupportsWildcards: false
Aliases: []
ParameterSets:
- Name: ScriptBlock
  Position: 0
  IsRequired: true
  ValueFromPipeline: false
  ValueFromPipelineByPropertyName: false
  ValueFromRemainingArguments: false
DontShow: false
AcceptedValues: []
HelpMessage: ''
```

### CommonParameters

This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable,
-InformationAction, -InformationVariable, -OutBuffer, -OutVariable, -PipelineVariable,
-ProgressAction, -Verbose, -WarningAction, and -WarningVariable. For more information, see
[about_CommonParameters](https://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

## OUTPUTS

### System.Object

This command returns objects described by the section heading.

## NOTES

This command is intended for use with PowerShell Pro Tools automation.

## RELATED LINKS

https://docs.poshtools.com/sitemap.md
