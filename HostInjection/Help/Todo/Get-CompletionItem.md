---
external help file: PowerShellProTools.VSCode.dll-Help.xml
Module Name: PowerShellProTools.VSCode
online version:
schema: 2.0.0
---

# Get-CompletionItem

## SYNOPSIS
{{ Fill in the Synopsis }}

## SYNTAX

### Types
```
Get-CompletionItem [-Types] [-IgnoredTypes <String>] [-IgnoredAssemblies <String>]
 [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

### Commands
```
Get-CompletionItem -Command <CommandInfo> [-IgnoredModules <String>] [-IgnoredCommands <String>]
 [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

### Variable
```
Get-CompletionItem -Variable <Variable> [-IgnoredVariables <String>] [-ProgressAction <ActionPreference>]
 [<CommonParameters>]
```

### File
```
Get-CompletionItem -File <FileInfo> [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

### Directory
```
Get-CompletionItem -Directory <DirectoryInfo> [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

## DESCRIPTION
{{ Fill in the Description }}

## EXAMPLES

### Example 1
```powershell
PS C:\> {{ Add example code here }}
```

{{ Add example description here }}

## PARAMETERS

### -Command
{{ Fill Command Description }}

```yaml
Type: CommandInfo
Parameter Sets: Commands
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: True (ByValue)
Accept wildcard characters: False
```

### -Directory
{{ Fill Directory Description }}

```yaml
Type: DirectoryInfo
Parameter Sets: Directory
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: True (ByValue)
Accept wildcard characters: False
```

### -File
{{ Fill File Description }}

```yaml
Type: FileInfo
Parameter Sets: File
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: True (ByValue)
Accept wildcard characters: False
```

### -IgnoredAssemblies
{{ Fill IgnoredAssemblies Description }}

```yaml
Type: String
Parameter Sets: Types
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -IgnoredCommands
{{ Fill IgnoredCommands Description }}

```yaml
Type: String
Parameter Sets: Commands
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -IgnoredModules
{{ Fill IgnoredModules Description }}

```yaml
Type: String
Parameter Sets: Commands
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -IgnoredTypes
{{ Fill IgnoredTypes Description }}

```yaml
Type: String
Parameter Sets: Types
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -IgnoredVariables
{{ Fill IgnoredVariables Description }}

```yaml
Type: String
Parameter Sets: Variable
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ProgressAction
{{ Fill ProgressAction Description }}

```yaml
Type: ActionPreference
Parameter Sets: (All)
Aliases: proga

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Types
{{ Fill Types Description }}

```yaml
Type: SwitchParameter
Parameter Sets: Types
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Variable
{{ Fill Variable Description }}

```yaml
Type: Variable
Parameter Sets: Variable
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: True (ByValue)
Accept wildcard characters: False
```

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### System.Management.Automation.CommandInfo

### PowerShellTools.Common.ServiceManagement.DebuggingContract.Variable

### System.IO.FileInfo

### System.IO.DirectoryInfo

## OUTPUTS

### System.Object
## NOTES

## RELATED LINKS
