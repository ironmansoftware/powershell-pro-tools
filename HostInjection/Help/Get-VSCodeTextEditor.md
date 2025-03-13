---
external help file: PowerShellProTools.VSCode.dll-Help.xml
Module Name: PowerShellProTools.VSCode
online version:
schema: 2.0.0
---

# Get-VSCodeTextEditor

## SYNOPSIS
Retrieves the currently visible text editor.

## SYNTAX

```
Get-VSCodeTextEditor [-Wait] [-ResponseTimeout <Int32>] [-ProgressAction <ActionPreference>]
 [<CommonParameters>]
```

## DESCRIPTION
This cmdlet returns a list of the currently visible editors along with their language ID. So, if you have two groups open, it will return the visible file paths in each group.

## EXAMPLES

### Example 1
```powershell
PS C:\> (Get-VSCodeTextEditor)[0]
```

Returns the first visible text editor.

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
Unlike Get-VSCodeTextDocument, this only returns the open editors that are currently visible.

Each item has a Document and LanguageId property. The Document property is actually VSCodeTextDocument, which can be piped to any cmdlet that accepts Get-VSCodeTextDocument.
## RELATED LINKS
https://docs.poshtools.com/powershell-pro-tools-documentation/visual-studio-code/automating-visual-studio-code