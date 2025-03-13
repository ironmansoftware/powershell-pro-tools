---
external help file: PowerShellProTools.VSCode.dll-Help.xml
Module Name: PowerShellProTools.VSCode
online version:
schema: 2.0.0
---

# Out-VSCodeGridView

## SYNOPSIS
Displays data in a grid view similar to Out-GridView, except in a VS Code web view.

## SYNTAX

### PassThru (Default)
```
Out-VSCodeGridView [-InputObject <PSObject>] [-Title <String>] [-PassThru] [-Wait] [-ResponseTimeout <Int32>]
 [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

### OutputMode
```
Out-VSCodeGridView [-InputObject <PSObject>] [-Title <String>] [-OutputMode <OutputModeOption>] [-Wait]
 [-ResponseTimeout <Int32>] [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

## DESCRIPTION
Works the same as `Out-GridView`, except in a VS Code document. Uses the same parameters, allowing for pipeline-enabled passthru with optional item selection.

## EXAMPLES

### Example 1
```powershell
PS C:\> Get-Process | Out-VSCodeGridView
```

Shows all running processes.

### Example 2
```powershell
PS C:\> Get-History | Out-VSCodeGridView -OutputMode Multiple
```

Shows all commands run in the current session, returning selected items.

## PARAMETERS

### -InputObject
The object(s) to display.

```yaml
Type: PSObject
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: True (ByValue)
Accept wildcard characters: False
```

### -OutputMode
Works similarly to `Out-GridView -OutputMode <Value>`.

Specifies the items that the interactive window sends down the pipeline as input to other commands. By default, this cmdlet does not generate any output. To send items from the interactive window down the pipeline, click to select the items and then click OK.

The values of this parameter determine how many items you can send down the pipeline.

```yaml
Type: OutputModeOption
Parameter Sets: OutputMode
Aliases:
Accepted values: None, Single, Multiple

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -PassThru
Works similar to `Out-GridView -PassThru`.

Indicates that the cmdlet sends items from the interactive window down the pipeline as input to other commands. By default, this cmdlet does not generate any output. This parameter is equivalent to using the Multiple value of the `-OutputMode` parameter.

```yaml
Type: SwitchParameter
Parameter Sets: PassThru
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ResponseTimeout
How long to wait before returning.

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

### -Title
Similar to `Out-GridView -Title "Window Title"`, the text to show in the document title.

Specifies the text that appears in the title bar of the `Out-VSCodeGridView` document. By default, the title bar displays nothing.

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

### -Wait
Similar to `Out-GridView -Wait`, specifies that the cmdlet should halt processing until the window is closed.

Indicates that the cmdlet suppresses the command prompt and prevents Windows PowerShell from closing until the `Out-VSCodeGridView` window is closed. By default, the command prompt returns when the `Out-VSCodeGridView` window opens.

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

### System.Management.Automation.PSObject

## OUTPUTS

### None
By default, the cmdlet generates no output.

### System.Object
If `-PassThru` or `-OutputMode <Single, Multiple>` is used, sends the selected items through the pipeline.

## NOTES

## RELATED LINKS
https://learn.microsoft.com/en-us/powershell/module/microsoft.powershell.utility/out-gridview?view=powershell-7.4&viewFallbackFrom=powershell-7.3&WT.mc_id=ps-gethelp