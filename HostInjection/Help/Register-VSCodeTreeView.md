---
external help file: PowerShellProTools.VSCode.dll-Help.xml
Module Name: PowerShellProTools.VSCode
online version:
schema: 2.0.0
---

# Register-VSCodeTreeView

## SYNOPSIS
Creates a TreeView entry in the Custom view.

## SYNTAX

```
Register-VSCodeTreeView -Label <String> [-Description <String>] [-Tooltip <String>] [-Icon <String>]
 [-LoadChildren <ScriptBlock>] [-InvokeChild <ScriptBlock>] [-ProgressAction <ActionPreference>]
 [<CommonParameters>]
```

## DESCRIPTION
This cmdlet creates a TreeView entry, which can contain multiple recursive levels of TreeItem entries, whether invokable or not.

## EXAMPLES

### Example 1
```powershell
Register-VSCodeTreeView -Label 'Test' -LoadChildren {
    1..10 | % { New-VSCodeTreeItem -Label "Test$_" -Icon 'archive' -HasChildren } 
} -Icon 'account' -InvokeChild {
    Show-VSCodeMessage -Message $args[0].Path
}
```

Creates a tree view named Test that creates nested tree items. When each item is clicked, it will display a VS Code message.

### Example 2
```powershell
Register-VSCodeTreeView -Label 'GitHub' -LoadChildren {
    New-VSCodeTreeItem -Label 'PowerShell Universal' -Description 'https://github.com/ironmansoftware/powershell-universal' -Icon 'github-inverted'
    New-VSCodeTreeItem -Label 'Issues' -Description 'https://github.com/ironmansoftware/issues' -Icon 'github-inverted'
    New-VSCodeTreeItem -Label 'PowerShell' -Description 'https://github.com/powershell/powershell' -Icon 'github-inverted'
} -Icon 'github' -InvokeChild {
    Start-Process $args[0].Description
}
```

This example creates a tree view of GitHub repositories and opens each when clicked.

## PARAMETERS

### -Description
The item description, displayed to the right of the label / name.

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

### -Icon
A codicon icon name to display.

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

### -InvokeChild
The action to perform on child TreeItems.

```yaml
Type: ScriptBlock
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Label
The main name displayed on the item which doubles as its ID.

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

### -LoadChildren
A script block containing one or more TreeItem creation actions using New-VSCodeTreeItem.

```yaml
Type: ScriptBlock
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Tooltip
The text to display on hover.

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

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### None

## OUTPUTS

### System.Object

## NOTES

## RELATED LINKS
https://docs.poshtools.com/powershell-pro-tools-documentation/visual-studio-code/powershell-explorer#custom-tree-view