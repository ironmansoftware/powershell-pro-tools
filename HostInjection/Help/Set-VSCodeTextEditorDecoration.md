---
external help file: PowerShellProTools.VSCode.dll-Help.xml
Module Name: PowerShellProTools.VSCode
online version:
schema: 2.0.0
---

# Set-VSCodeTextEditorDecoration

## SYNOPSIS
Decorates a range of text with an optional set of colors, outlines, borders, and text.

## SYNTAX

```
Set-VSCodeTextEditorDecoration -TextEditor <VsCodeTextEditor> -Range <VsCodeRange> -Key <String>
 [-BackgroundColor <String>] [-Border <String>] [-BorderColor <String>] [-BorderRadius <String>]
 [-BorderStyle <String>] [-BorderWidth <String>] [-Color <String>] [-Cursor <String>] [-FontStyle <String>]
 [-FontWeight <String>] [-IsWholeLine] [-LetterSpacing <String>] [-Opacity <String>] [-Outline <String>]
 [-OutlineColor <String>] [-OutlineStyle <String>] [-OutlineWidth <String>] [-RangeBehavior <String>]
 [-TextDecoration <String>] [-After <DecorationAttachment>] [-Before <DecorationAttachment>] [-Wait]
 [-ResponseTimeout <Int32>] [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

## DESCRIPTION
The cmdlet decorates a range of text with an optional set of colors, outlines, borders, and text.

## EXAMPLES

### Example 1
```powershell
PS C:\> $Range = New-VSCodeRange -StartLine 0 -EndLine 0 -StartCharacter 0 -EndCharacter 55
PS C:\> Get-VSCodeTextEditor | Set-VSCodeTextEditorDecoration -BackgroundColor 'descriptionForeground' -Range $Range -Key 12321 -FontWeight bold
```

Sets a document range's background color to the 'descriptionForeground' element, and the text to bold.

## PARAMETERS

### -After
Ignored. Commented out in vscodeService.ts: `//after = After`.

```yaml
Type: DecorationAttachment
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -BackgroundColor
The background color. This is passed directly to `vscode.TextEditor.setDecoration` in the extension.

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

### -Before
Ignored. Commented out in vscodeService.ts: `//before = Before`.

```yaml
Type: DecorationAttachment
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Border
This is passed directly to `vscode.TextEditor.setDecoration` in the extension.

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

### -BorderColor
This is passed directly to `vscode.TextEditor.setDecoration` in the extension.

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

### -BorderRadius
This is passed directly to `vscode.TextEditor.setDecoration` in the extension.

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

### -BorderStyle
This is passed directly to `vscode.TextEditor.setDecoration` in the extension.

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

### -BorderWidth
This is passed directly to `vscode.TextEditor.setDecoration` in the extension.

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

### -Color
This is passed directly to `vscode.TextEditor.setDecoration` in the extension.

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

### -Cursor
This is passed directly to `vscode.TextEditor.setDecoration` in the extension.

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

### -FontStyle
This is passed directly to `vscode.TextEditor.setDecoration` in the extension.

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

### -FontWeight
This is passed directly to `vscode.TextEditor.setDecoration` in the extension.

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

### -IsWholeLine
This is passed directly to `vscode.TextEditor.setDecoration` in the extension.

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

### -Key
This is passed directly to `vscode.TextEditor.setDecoration` in the extension. See: `this.decorations[msg.args.key] = decorationType;`

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

### -LetterSpacing
This is passed directly to `vscode.TextEditor.setDecoration` in the extension.

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

### -Opacity
This is passed directly to `vscode.TextEditor.setDecoration` in the extension.

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

### -Outline
This is passed directly to `vscode.TextEditor.setDecoration` in the extension.

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

### -OutlineColor
This is passed directly to `vscode.TextEditor.setDecoration` in the extension.

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

### -OutlineStyle
This is passed directly to `vscode.TextEditor.setDecoration` in the extension.

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

### -OutlineWidth
This is passed directly to `vscode.TextEditor.setDecoration` in the extension.

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

### -Range
The document range to set decoration on.

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

### -RangeBehavior
Passed to `vscode.DecorationRangeBehavior`.

```yaml
Type: String
Parameter Sets: (All)
Aliases:
Accepted values: ClosedClosed, ClosedOpen, OpenClosed, OpenOpen

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

### -TextDecoration
This is passed directly to `vscode.TextEditor.setDecoration` in the extension.

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

### -TextEditor
The text editor object to set the decoration on.

```yaml
Type: VsCodeTextEditor
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

### PowerShellToolsPro.Cmdlets.VSCode.VsCodeTextEditor

## OUTPUTS

### System.Object

## NOTES

## RELATED LINKS
https://docs.poshtools.com/powershell-pro-tools-documentation/visual-studio-code/automating-visual-studio-code