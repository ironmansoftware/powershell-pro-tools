---
document type: cmdlet
external help file: PowerShellProTools.VSCode.dll-Help.xml
HelpUri: ''
Locale: en-US
Module Name: PowerShellProTools.VSCode
ms.date: 07-21-2026
PlatyPS schema version: 2024-05-01
title: Get-VSCodeTextDocumentText
---

# Get-VSCodeTextDocumentText

## SYNOPSIS

Gets text from a Visual Studio Code text document.

## SYNTAX

### __AllParameterSets

```
Get-VSCodeTextDocumentText -TextDocument <VsCodeTextDocument> [-Range <VsCodeRange>] [-Wait]
 [-ResponseTimeout <int>] [<CommonParameters>]
```

## ALIASES

This cmdlet has no aliases.

## DESCRIPTION

Gets text from a Visual Studio Code text document. Provide a range to retrieve only part of the document.

## EXAMPLES

### Example 1

```powershell
$document = Get-VSCodeTextDocument
Get-VSCodeTextDocumentText -TextDocument $document
```
Returns all text from the active document.

## PARAMETERS

### -Range

The range of text to read, remove, or decorate.

```yaml
Type: PowerShellToolsPro.Cmdlets.VSCode.VsCodeRange
DefaultValue: ''
SupportsWildcards: false
Aliases: []
ParameterSets:
- Name: (All)
  Position: Named
  IsRequired: false
  ValueFromPipeline: false
  ValueFromPipelineByPropertyName: false
  ValueFromRemainingArguments: false
DontShow: false
AcceptedValues: []
HelpMessage: ''
```

### -ResponseTimeout

The number of milliseconds to wait for a response from Visual Studio Code.

```yaml
Type: System.Int32
DefaultValue: ''
SupportsWildcards: false
Aliases: []
ParameterSets:
- Name: (All)
  Position: Named
  IsRequired: false
  ValueFromPipeline: false
  ValueFromPipelineByPropertyName: false
  ValueFromRemainingArguments: false
DontShow: false
AcceptedValues: []
HelpMessage: ''
```

### -TextDocument

The text document to read or update.

```yaml
Type: PowerShellToolsPro.Cmdlets.VSCode.VsCodeTextDocument
DefaultValue: ''
SupportsWildcards: false
Aliases: []
ParameterSets:
- Name: (All)
  Position: Named
  IsRequired: true
  ValueFromPipeline: true
  ValueFromPipelineByPropertyName: false
  ValueFromRemainingArguments: false
DontShow: false
AcceptedValues: []
HelpMessage: ''
```

### -Wait

Waits for the VS Code command response before returning.

```yaml
Type: System.Management.Automation.SwitchParameter
DefaultValue: ''
SupportsWildcards: false
Aliases: []
ParameterSets:
- Name: (All)
  Position: Named
  IsRequired: false
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

### PowerShellToolsPro.Cmdlets.VSCode.VsCodeTextDocument

This command returns objects described by the section heading.

## OUTPUTS

### System.Object

This command returns objects described by the section heading.

## NOTES

This command is intended for use with PowerShell Pro Tools automation.

## RELATED LINKS

https://docs.poshtools.com/sitemap.md
