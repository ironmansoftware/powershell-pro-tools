---
external help file: PowerShellProTools.VSCode.dll-Help.xml
Module Name: PowerShellProTools.VSCode
online version:
schema: 2.0.0
---

# Get-VSCodeTextDocumentText

## SYNOPSIS
Gets the text of a document.

## SYNTAX

```
Get-VSCodeTextDocumentText -TextDocument <VsCodeTextDocument> [-Range <VsCodeRange>] [-Wait]
 [-ResponseTimeout <Int32>] [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

## DESCRIPTION
This cmdlet gets the text of a document. You can also pass in a range to select only a partial section of the text.

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

### -Range
A range between a given start and end line and character/column. See: New-VSCodeRange.

```yaml
Type: VsCodeRange
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

### -TextDocument
The text document to interact with. See: Get-VSCodeTextDocument.

```yaml
Type: VsCodeTextDocument
Parameter Sets: (All)
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: True (ByValue)
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

### PowerShellToolsPro.Cmdlets.VSCode.VsCodeTextDocument

## OUTPUTS

### System.Object
## NOTES
The lines and characters used in -Range are 0-index (like an array), so for instance, to get line 10 in the editor, you'd actually specify line 9.
## RELATED LINKS
https://docs.poshtools.com/powershell-pro-tools-documentation/visual-studio-code/automating-visual-studio-code