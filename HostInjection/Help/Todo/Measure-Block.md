---
external help file: PowerShellProTools.VSCode.dll-Help.xml
Module Name: PowerShellProTools.VSCode
online version:
schema: 2.0.0
---

# Measure-Block

## SYNOPSIS
{{ Fill in the Synopsis }}

## SYNTAX

### file
```
Measure-Block [-ScriptBlock <ScriptBlock>] [-FileName <String>] -StartOffset <Int32> -EndOffset <Int32>
 [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

### module
```
Measure-Block [-ScriptBlock <ScriptBlock>] -ModuleName <String> -CommandName <String> -PipelineMethod <String>
 [-ProgressAction <ActionPreference>] [<CommonParameters>]
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

### -CommandName
{{ Fill CommandName Description }}

```yaml
Type: String
Parameter Sets: module
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -EndOffset
{{ Fill EndOffset Description }}

```yaml
Type: Int32
Parameter Sets: file
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -FileName
{{ Fill FileName Description }}

```yaml
Type: String
Parameter Sets: file
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ModuleName
{{ Fill ModuleName Description }}

```yaml
Type: String
Parameter Sets: module
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -PipelineMethod
{{ Fill PipelineMethod Description }}

```yaml
Type: String
Parameter Sets: module
Aliases:

Required: True
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

### -ScriptBlock
{{ Fill ScriptBlock Description }}

```yaml
Type: ScriptBlock
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -StartOffset
{{ Fill StartOffset Description }}

```yaml
Type: Int32
Parameter Sets: file
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### None

## OUTPUTS

### System.Object
## NOTES

## RELATED LINKS
