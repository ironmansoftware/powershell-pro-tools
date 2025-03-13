---
external help file: PowerShellProTools.VSCode.dll-Help.xml
Module Name: PowerShellProTools.VSCode
online version:
schema: 2.0.0
---

# Send-VSCodeTerminalText

## SYNOPSIS
Sends text to the specified terminal.

## SYNTAX

### name
```
Send-VSCodeTerminalText -Name <String> -Text <String> [-AddNewLine] [-Wait] [-ResponseTimeout <Int32>]
 [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

### terminal
```
Send-VSCodeTerminalText -Terminal <VsCodeTerminal> -Text <String> [-AddNewLine] [-Wait]
 [-ResponseTimeout <Int32>] [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

## DESCRIPTION
The cmdlet sends text to the specified terminal, whether from a terminal name or object. You can commit this text by including the -AddNewLine parameter.

## EXAMPLES

### Example 1
```powershell
PS C:\> Get-VSCodeTerminal | Where-Object Name -eq 'PowerShell Integrated Console' | Send-VSCodeTerminalText -Text 'Write-Host "Hello World!"'
```

Sends the line `Write-Host "Hello World!"` to the PowerShell Integrated Console.

## PARAMETERS

### -AddNewLine
Whether to send a newline at the end of the text, effectively running it immediately.

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

### -Name
The name of the terminal to interact with.

```yaml
Type: String
Parameter Sets: name
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: True (ByValue)
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

### -Terminal
The terminal object to interact with. See: Get-VSCodeTerminal

```yaml
Type: VsCodeTerminal
Parameter Sets: terminal
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: True (ByValue)
Accept wildcard characters: False
```

### -Text
The text string to send to the terminal.

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

### System.String

### PowerShellToolsPro.Cmdlets.VSCode.VsCodeTerminal

## OUTPUTS

### System.Object

## NOTES

## RELATED LINKS
https://docs.poshtools.com/powershell-pro-tools-documentation/visual-studio-code/automating-visual-studio-code