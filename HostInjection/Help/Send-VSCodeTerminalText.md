---
document type: cmdlet
external help file: PowerShellProTools.VSCode.dll-Help.xml
HelpUri: ''
Locale: en-US
Module Name: PowerShellProTools.VSCode
ms.date: 07-21-2026
PlatyPS schema version: 2024-05-01
title: Send-VSCodeTerminalText
---

# Send-VSCodeTerminalText

## SYNOPSIS

Sends text to a Visual Studio Code terminal.

## SYNTAX

### name

```
Send-VSCodeTerminalText -Name <string> -Text <string> [-AddNewLine] [-Wait] [-ResponseTimeout <int>]
 [<CommonParameters>]
```

### terminal

```
Send-VSCodeTerminalText -Terminal <VsCodeTerminal> -Text <string> [-AddNewLine] [-Wait]
 [-ResponseTimeout <int>] [<CommonParameters>]
```

## ALIASES

This cmdlet has no aliases.

## DESCRIPTION

Sends text to a Visual Studio Code terminal. You can target a terminal by object or name and control whether a newline is appended.

## EXAMPLES

### Example 1

```powershell
Send-VSCodeTerminalText -Name 'PowerShell' -Text 'Get-Process' -AddNewLine
```
Sends a command to the named terminal and appends a newline.

## PARAMETERS

### -AddNewLine

Appends a newline after the text is sent to the terminal.

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

### -Name

The terminal name.

```yaml
Type: System.String
DefaultValue: ''
SupportsWildcards: false
Aliases: []
ParameterSets:
- Name: name
  Position: Named
  IsRequired: true
  ValueFromPipeline: true
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

### -Terminal

The terminal object to receive text.

```yaml
Type: PowerShellToolsPro.Cmdlets.VSCode.VsCodeTerminal
DefaultValue: ''
SupportsWildcards: false
Aliases: []
ParameterSets:
- Name: terminal
  Position: Named
  IsRequired: true
  ValueFromPipeline: true
  ValueFromPipelineByPropertyName: false
  ValueFromRemainingArguments: false
DontShow: false
AcceptedValues: []
HelpMessage: ''
```

### -Text

The text to insert or send.

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

### System.String

This command returns objects described by the section heading.

### PowerShellToolsPro.Cmdlets.VSCode.VsCodeTerminal

This command returns objects described by the section heading.

## OUTPUTS

### System.Object

This command returns objects described by the section heading.

## NOTES

This command is intended for use with PowerShell Pro Tools automation.

## RELATED LINKS

https://docs.poshtools.com/sitemap.md
