---
document type: cmdlet
external help file: PowerShellToolsPro.Cmdlets.dll-Help.xml
HelpUri: ''
Locale: en-US
Module Name: PowerShellProTools
ms.date: 07-21-2026
PlatyPS schema version: 2024-05-01
title: Show-WinFormDesigner
---

# Show-WinFormDesigner

## SYNOPSIS

Opens the Windows Forms designer for PowerShell scripts.

## SYNTAX

### __AllParameterSets

```
Show-WinFormDesigner -DesignerFilePath <string> -CodeFilePath <string> [<CommonParameters>]
```

## ALIASES

This cmdlet has no aliases.

## DESCRIPTION

Starts the PowerShell Pro Tools Windows Forms designer for the specified designer and code-behind script files. This command is only supported on Windows.

## EXAMPLES

### Example 1

```powershell
Show-WinFormDesigner -DesignerFilePath .\MainForm.designer.ps1 -CodeFilePath .\MainForm.ps1
```
Opens the Windows Forms designer for the specified form files.

## PARAMETERS

### -CodeFilePath

The path to the form code-behind script.

```yaml
Type: System.String
DefaultValue: ''
SupportsWildcards: false
Aliases: []
ParameterSets:
- Name: (All)
  Position: Named
  IsRequired: true
  ValueFromPipeline: false
  ValueFromPipelineByPropertyName: false
  ValueFromRemainingArguments: false
DontShow: false
AcceptedValues: []
HelpMessage: ''
```

### -DesignerFilePath

The path to the designer script file.

```yaml
Type: System.String
DefaultValue: ''
SupportsWildcards: false
Aliases: []
ParameterSets:
- Name: (All)
  Position: Named
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
