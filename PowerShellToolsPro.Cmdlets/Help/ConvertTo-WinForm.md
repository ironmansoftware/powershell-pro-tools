---
document type: cmdlet
external help file: PowerShellToolsPro.Cmdlets.dll-Help.xml
HelpUri: ''
Locale: en-US
Module Name: PowerShellProTools
ms.date: 07-21-2026
PlatyPS schema version: 2024-05-01
title: ConvertTo-WinForm
---

# ConvertTo-WinForm

## SYNOPSIS

Generates Windows Forms scripts from PowerShell commands or functions.

## SYNTAX

### filePath

```
ConvertTo-WinForm [-Path <string>] [-OutputPath <string>] [-PackageAsExecutable]
 [<CommonParameters>]
```

### def

```
ConvertTo-WinForm [-FunctionDefinition <scriptblock>] [-OutputPath <string>] [-PackageAsExecutable]
 [<CommonParameters>]
```

### functionInfo

```
ConvertTo-WinForm -Function <FunctionInfo> [-OutputPath <string>] [-PackageAsExecutable]
 [<CommonParameters>]
```

### cmdletInfo

```
ConvertTo-WinForm -Cmdlet <CmdletInfo> [-OutputPath <string>] [-PackageAsExecutable]
 [<CommonParameters>]
```

## ALIASES

This cmdlet has no aliases.

## DESCRIPTION

Creates a Windows Forms designer script and companion logic script from a function, cmdlet, script block, or script file. The generated files can be edited with the Windows Forms designer and optionally packaged as an executable.

## EXAMPLES

### Example 1

```powershell
ConvertTo-WinForm -Path .\Get-UserInput.ps1 -OutputPath .\Get-UserInput.form.ps1
```
Generates a Windows Forms designer script and logic script from a PowerShell script file.

## PARAMETERS

### -Cmdlet

A cmdlet whose parameters are used to generate a form.

```yaml
Type: System.Management.Automation.CmdletInfo
DefaultValue: ''
SupportsWildcards: false
Aliases: []
ParameterSets:
- Name: cmdletInfo
  Position: Named
  IsRequired: true
  ValueFromPipeline: true
  ValueFromPipelineByPropertyName: false
  ValueFromRemainingArguments: false
DontShow: false
AcceptedValues: []
HelpMessage: ''
```

### -Function

A function whose parameters are used to generate a form.

```yaml
Type: System.Management.Automation.FunctionInfo
DefaultValue: ''
SupportsWildcards: false
Aliases: []
ParameterSets:
- Name: functionInfo
  Position: Named
  IsRequired: true
  ValueFromPipeline: true
  ValueFromPipelineByPropertyName: false
  ValueFromRemainingArguments: false
DontShow: false
AcceptedValues: []
HelpMessage: ''
```

### -FunctionDefinition

A script block whose parameters are used to generate a form.

```yaml
Type: System.Management.Automation.ScriptBlock
DefaultValue: ''
SupportsWildcards: false
Aliases: []
ParameterSets:
- Name: def
  Position: Named
  IsRequired: false
  ValueFromPipeline: false
  ValueFromPipelineByPropertyName: false
  ValueFromRemainingArguments: false
DontShow: false
AcceptedValues: []
HelpMessage: ''
```

### -OutputPath

The output file or directory path.

```yaml
Type: System.String
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

### -PackageAsExecutable

Packages the generated form script as an executable.

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

### -Path

The path to the input file or host variable.

```yaml
Type: System.String
DefaultValue: ''
SupportsWildcards: false
Aliases: []
ParameterSets:
- Name: filePath
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

### System.Management.Automation.FunctionInfo

This command returns objects described by the section heading.

### System.Management.Automation.CmdletInfo

This command returns objects described by the section heading.

## OUTPUTS

### System.Object

This command returns objects described by the section heading.

## NOTES

This command is intended for use with PowerShell Pro Tools automation.

## RELATED LINKS

https://docs.poshtools.com/sitemap.md
