---
external help file: PowerShellProTools.VSCode.dll-Help.xml
Module Name: PowerShellProTools.VSCode
online version:
schema: 2.0.0
---

# Get-VSCodeTerminal

## SYNOPSIS
Retrieves a list of open terminals.

## SYNTAX

```
Get-VSCodeTerminal [-Wait] [-ResponseTimeout <Int32>] [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

## DESCRIPTION
This cmdlet gets the currently opened terminals, returning their names, IDs, columns, rows, and creation options.

## EXAMPLES

### Example 1
```powershell
(Get-VSCodeTerminal -Wait)[-1]
```

This returns the last terminal in the list.

## PARAMETERS

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