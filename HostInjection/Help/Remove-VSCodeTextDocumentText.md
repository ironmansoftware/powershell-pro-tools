---
external help file: PowerShellProTools.VSCode.dll-Help.xml
Module Name: PowerShellProTools.VSCode
online version:
schema: 2.0.0
---

# Remove-VSCodeTextDocumentText

## SYNOPSIS
Removes a range of text from a document.

## SYNTAX

```
Remove-VSCodeTextDocumentText -TextDocument <VsCodeTextDocument> -Range <VsCodeRange> [-Wait]
 [-ResponseTimeout <Int32>] [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

## DESCRIPTION
Given a document object, removes a range of text. This creates an edit but does not save the file.

## EXAMPLES

### Example 1
```powershell
PS C:\> $Range = New-VSCodeRange -StartLine 0 -EndLine 0 -StartCharacter 0 -EndCharacter 10
PS C:\> Get-VSCodeTextDocument | Remove-VSCodeTextDocumentText -Range $Range
```

Removes 10 characters from the first line of a document.

## PARAMETERS

### -Range
A file range retrieved with New-VSCodeRange.

```yaml
Type: VsCodeRange
Parameter Sets: (All)
Aliases:

Required: True
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
The text document to act on, retrieved with Get-VSCodeTextDocument.

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

## RELATED LINKS
https://docs.poshtools.com/powershell-pro-tools-documentation/visual-studio-code/automating-visual-studio-code