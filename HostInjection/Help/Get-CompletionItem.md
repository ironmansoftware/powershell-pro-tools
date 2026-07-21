---
document type: cmdlet
external help file: PowerShellProTools.VSCode.dll-Help.xml
HelpUri: ''
Locale: en-US
Module Name: PowerShellProTools.VSCode
ms.date: 07-21-2026
PlatyPS schema version: 2024-05-01
title: Get-CompletionItem
---

# Get-CompletionItem

## SYNOPSIS

Returns completion items for PowerShell code.

## SYNTAX

### Types

```
Get-CompletionItem -Types [-IgnoredTypes <string>] [-IgnoredAssemblies <string>]
 [<CommonParameters>]
```

### Commands

```
Get-CompletionItem -Command <CommandInfo> [-IgnoredModules <string>] [-IgnoredCommands <string>]
 [<CommonParameters>]
```

### Variable

```
Get-CompletionItem -Variable <Variable> [-IgnoredVariables <string>] [<CommonParameters>]
```

### File

```
Get-CompletionItem -File <FileInfo> [<CommonParameters>]
```

### Directory

```
Get-CompletionItem -Directory <DirectoryInfo> [<CommonParameters>]
```

## ALIASES

This cmdlet has no aliases.

## DESCRIPTION

Returns completion items for a command, variable, type, or file path based on the supplied PowerShell context. The command is used by RapidSense and editor completion features.

## EXAMPLES

### Example 1

```powershell
Get-CompletionItem -Command 'Get-Pro'
```
Returns completion items for a partially typed command.

## PARAMETERS

### -Command

The command text to complete.

```yaml
Type: System.Management.Automation.CommandInfo
DefaultValue: ''
SupportsWildcards: false
Aliases: []
ParameterSets:
- Name: Commands
  Position: Named
  IsRequired: true
  ValueFromPipeline: true
  ValueFromPipelineByPropertyName: false
  ValueFromRemainingArguments: false
DontShow: false
AcceptedValues: []
HelpMessage: ''
```

### -Directory

The working directory used when resolving completions.

```yaml
Type: System.IO.DirectoryInfo
DefaultValue: ''
SupportsWildcards: false
Aliases: []
ParameterSets:
- Name: Directory
  Position: Named
  IsRequired: true
  ValueFromPipeline: true
  ValueFromPipelineByPropertyName: false
  ValueFromRemainingArguments: false
DontShow: false
AcceptedValues: []
HelpMessage: ''
```

### -File

The file path used as completion context.

```yaml
Type: System.IO.FileInfo
DefaultValue: ''
SupportsWildcards: false
Aliases: []
ParameterSets:
- Name: File
  Position: Named
  IsRequired: true
  ValueFromPipeline: true
  ValueFromPipelineByPropertyName: false
  ValueFromRemainingArguments: false
DontShow: false
AcceptedValues: []
HelpMessage: ''
```

### -IgnoredAssemblies

Assembly names to exclude from completion results.

```yaml
Type: System.String
DefaultValue: ''
SupportsWildcards: false
Aliases: []
ParameterSets:
- Name: Types
  Position: Named
  IsRequired: false
  ValueFromPipeline: false
  ValueFromPipelineByPropertyName: false
  ValueFromRemainingArguments: false
DontShow: false
AcceptedValues: []
HelpMessage: ''
```

### -IgnoredCommands

Command names to exclude from completion results.

```yaml
Type: System.String
DefaultValue: ''
SupportsWildcards: false
Aliases: []
ParameterSets:
- Name: Commands
  Position: Named
  IsRequired: false
  ValueFromPipeline: false
  ValueFromPipelineByPropertyName: false
  ValueFromRemainingArguments: false
DontShow: false
AcceptedValues: []
HelpMessage: ''
```

### -IgnoredModules

Module names to exclude from completion results.

```yaml
Type: System.String
DefaultValue: ''
SupportsWildcards: false
Aliases: []
ParameterSets:
- Name: Commands
  Position: Named
  IsRequired: false
  ValueFromPipeline: false
  ValueFromPipelineByPropertyName: false
  ValueFromRemainingArguments: false
DontShow: false
AcceptedValues: []
HelpMessage: ''
```

### -IgnoredTypes

Type names to exclude from completion results.

```yaml
Type: System.String
DefaultValue: ''
SupportsWildcards: false
Aliases: []
ParameterSets:
- Name: Types
  Position: Named
  IsRequired: false
  ValueFromPipeline: false
  ValueFromPipelineByPropertyName: false
  ValueFromRemainingArguments: false
DontShow: false
AcceptedValues: []
HelpMessage: ''
```

### -IgnoredVariables

Variable names to exclude from completion results.

```yaml
Type: System.String
DefaultValue: ''
SupportsWildcards: false
Aliases: []
ParameterSets:
- Name: Variable
  Position: Named
  IsRequired: false
  ValueFromPipeline: false
  ValueFromPipelineByPropertyName: false
  ValueFromRemainingArguments: false
DontShow: false
AcceptedValues: []
HelpMessage: ''
```

### -Types

Type names to include in completion context.

```yaml
Type: System.Management.Automation.SwitchParameter
DefaultValue: ''
SupportsWildcards: false
Aliases: []
ParameterSets:
- Name: Types
  Position: Named
  IsRequired: true
  ValueFromPipeline: false
  ValueFromPipelineByPropertyName: false
  ValueFromRemainingArguments: false
DontShow: false
AcceptedValues: []
HelpMessage: ''
```

### -Variable

The variable name to complete.

```yaml
Type: PowerShellTools.Common.ServiceManagement.DebuggingContract.Variable
DefaultValue: ''
SupportsWildcards: false
Aliases: []
ParameterSets:
- Name: Variable
  Position: Named
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

### System.Management.Automation.CommandInfo

This command returns objects described by the section heading.

### PowerShellTools.Common.ServiceManagement.DebuggingContract.Variable

This command returns objects described by the section heading.

### System.IO.FileInfo

This command returns objects described by the section heading.

### System.IO.DirectoryInfo

This command returns objects described by the section heading.

## OUTPUTS

### System.Object

This command returns objects described by the section heading.

## NOTES

This command is intended for use with PowerShell Pro Tools automation.

## RELATED LINKS

https://docs.poshtools.com/sitemap.md
