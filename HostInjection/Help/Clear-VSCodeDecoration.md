---
external help file: PowerShellProTools.VSCode.dll-Help.xml
Module Name: PowerShellProTools.VSCode
online version:
schema: 2.0.0
---

# Clear-VSCodeDecoration

## SYNOPSIS
Clear decorations created by Set-VSCodeTextEditorDecoration.

## SYNTAX

```
Clear-VSCodeDecoration [-Key <String>] [-Wait] [-ResponseTimeout <Int32>] [-ProgressAction <ActionPreference>]
 [<CommonParameters>]
```

## DESCRIPTION
This cmdlet clears all decorations, or just one if `-Key` is specified.

## EXAMPLES

### Example 1
```powershell
PS C:\> Clear-VSCodeDecoration
```

Clears all decorations.

### Example 2
```powershell
PS C:\> Clear-VSCodeDecoration -Key 12321
```

Clears the decoration associated with key `12321`.

## PARAMETERS

### -Key
The specific decoration to clear when it was set with `-Key`, instead of all.

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ResponseTimeout
How long to wait for the cmdlet to return in milliseconds. Defaults to 5 seconds.

```yaml
Type: Int32
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Wait
Whether to wait for the cmdlet to finish or return immediately.

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases:

Required: False
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
https://docs.poshtools.com/powershell-pro-tools-documentation/visual-studio-code/automating-visual-studio-code