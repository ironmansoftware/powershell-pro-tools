---
external help file: PowerShellProTools.VSCode.dll-Help.xml
Module Name: PowerShellProTools.VSCode
online version:
schema: 2.0.0
---

# Get-VSCodeTextDocument

## SYNOPSIS
Returns a list of currently open documents.

## SYNTAX

```
Get-VSCodeTextDocument [-Wait] [-ResponseTimeout <Int32>] [-ProgressAction <ActionPreference>]
 [<CommonParameters>]
```

## DESCRIPTION
This cmdlet returns a list of open document paths. It can be piped into other cmdlets to make changes, i.e. `Add-VSCodeTextDocumentText`.

## EXAMPLES

### Example 1
```powershell
PS C:\> Get-VSCodeTextDocument | Get-VSCodeTextDocumentText
```

Retrieves the full text of the current document.

### Example 2
```powershell
PS C:\> Get-VSCodeTextDocument | Get-VSCodeTextDocumentText -Range (New-VSCodeRange -StartLine 10 -EndLine 20 -StartCharacter 0 -EndCharacter 16)
```

Retrieves lines 10 through 20 (up to column 16) of the current document.

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
Returns all open document paths, containing one FileName property.

## RELATED LINKS
https://docs.poshtools.com/powershell-pro-tools-documentation/visual-studio-code/automating-visual-studio-code