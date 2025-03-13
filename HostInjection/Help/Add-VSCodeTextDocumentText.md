---
external help file: PowerShellProTools.VSCode.dll-Help.xml
Module Name: PowerShellProTools.VSCode
online version:
schema: 2.0.0
---

# Add-VSCodeTextDocumentText

## SYNOPSIS
Inserts text into a particular position in the selected document. This creates an edit but does not save the file.

## SYNTAX

```
Add-VSCodeTextDocumentText -TextDocument <VsCodeTextDocument> -Position <VsCodePosition> -Text <String> [-Wait]
 [-ResponseTimeout <Int32>] [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

## DESCRIPTION
Given a TextDocument and Position, this cmdlet inserts text into the selected document. This creates an edit but does not save the file.

## EXAMPLES

### Example 1
```powershell
Get-VSCodeTextDocument | Add-VSCodeTextDocumentText -Position (New-VSCodePosition -Line 0 -Character 0) -Text '# I came from Add-VSCodeTextDocumentText!'
```

This inserts a comment at the start of the current file.

## PARAMETERS

### -Position
A position (line and column) in the document, created with New-VSCodePosition.

```yaml
Type: VsCodePosition
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

### -Text
The text to insert.

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -TextDocument
An object representing the document to interact with. Retrieve with Get-VSCodeTextDocument.

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
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -ProgressAction, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### PowerShellToolsPro.Cmdlets.VSCode.VsCodeTextDocument

## OUTPUTS

### System.Object
## NOTES

## RELATED LINKS
https://docs.poshtools.com/powershell-pro-tools-documentation/visual-studio-code/automating-visual-studio-code