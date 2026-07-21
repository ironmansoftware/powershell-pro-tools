---
document type: cmdlet
external help file: PowerShellToolsPro.Cmdlets.dll-Help.xml
HelpUri: ''
Locale: en-US
Module Name: PowerShellProTools
ms.date: 07-21-2026
PlatyPS schema version: 2024-05-01
title: Expand-Object
---

# Expand-Object

## SYNOPSIS

Expands an object into name and value entries.

## SYNTAX

### __AllParameterSets

```
Expand-Object [-InputObject] <Object> [<CommonParameters>]
```

## ALIASES

This cmdlet has no aliases.

## DESCRIPTION

Expands an input object into name and value entries that are easier to inspect in PowerShell Pro Tools views.

## EXAMPLES

### Example 1

```powershell
Get-Process -Id $PID | Expand-Object
```
Expands the current PowerShell process object for inspection.

## PARAMETERS

### -InputObject

The object to display, expand, or store.

```yaml
Type: System.Object
DefaultValue: ''
SupportsWildcards: false
Aliases: []
ParameterSets:
- Name: (All)
  Position: 0
  IsRequired: true
  ValueFromPipeline: true
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

### System.Object

This command returns objects described by the section heading.

## OUTPUTS

### System.Object

This command returns objects described by the section heading.

## NOTES

This command is intended for use with PowerShell Pro Tools automation.

## RELATED LINKS

https://docs.poshtools.com/sitemap.md
