---
external help file: PowerShellProTools.VSCode.dll-Help.xml
Module Name: PowerShellProTools.VSCode
online version:
schema: 2.0.0
---

# New-VSCodeRange

## SYNOPSIS
Returns a range between start and end positions.

## SYNTAX

```
New-VSCodeRange -StartLine <Int32> -EndLine <Int32> -StartCharacter <Int32> -EndCharacter <Int32> [-Wait]
 [-ResponseTimeout <Int32>] [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

## DESCRIPTION
This cmdlet returns an object representing the range between two positions in a given document.

## EXAMPLES

### Example 1
```powershell
PS C:\> New-VSCodeRange -StartLine 0 -EndLine 10 -StartCharacter 0 -EndCharacter 80
```

Gets the range between the start of the document to line 10, character 80.

### Example 2
```powershell
PS C:\> $Range = New-VSCodeRange -StartLine 0 -EndLine 0 -StartCharacter 0 -EndCharacter 55
PS C:\> Get-VSCodeTextEditor | Set-VSCodeTextEditorDecoration -BackgroundColor 'descriptionForeground' -Range $Range -Key 12321 -FontWeight bold
```

Sets the text decoration in a given range.

## PARAMETERS

### -EndCharacter
The 0-index end character.

```yaml
Type: Int32
Parameter Sets: (All)
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -EndLine
The 0-index end line.

```yaml
Type: Int32
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

### -StartCharacter
The 0-index beginning character.

```yaml
Type: Int32
Parameter Sets: (All)
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -StartLine
The 0-index beginning line.

```yaml
Type: Int32
Parameter Sets: (All)
Aliases:

Required: True
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
The four required parameters are 0-index, meaning they're 1 less than the visible editor lines and columns.

## RELATED LINKS
https://docs.poshtools.com/powershell-pro-tools-documentation/visual-studio-code/automating-visual-studio-code