---
external help file: PowerShellProTools.VSCode.dll-Help.xml
Module Name: PowerShellProTools.VSCode
online version:
schema: 2.0.0
---

# Show-VSCodeQuickPick

## SYNOPSIS
Shows a quick pick list for a user to select items from.

## SYNTAX

```
Show-VSCodeQuickPick [-PlaceHolder <String>] -Items <String[]> [-CanPickMany] [-IgnoreFocusOut] [-Wait]
 [-ResponseTimeout <Int32>] [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

## DESCRIPTION
Shows a quick pick list for a user to select items from. This cmdlet will return the user's selection to PowerShell.

## EXAMPLES

### Example 1
```powershell
PS C:\> Show-VSCodeQuickPick -PlaceHolder 'What should we do?' -Items @('Party', 'Sleep')
```

Shows a dropdown prompt at the command bar with a list of selectable items.

## PARAMETERS

### -CanPickMany
Specify that multiple items can be checked.

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

### -IgnoreFocusOut
Don't close if focus is lost.

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

### -Items
An array of selectable items to display.

```yaml
Type: String[]
Parameter Sets: (All)
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -PlaceHolder
The default item to select in the prompt.

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