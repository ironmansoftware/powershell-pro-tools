# PowerShell Pro Tools

> PowerShell Pro Tools adds script packaging, Form Designers, variable explorer, VS Code automation and more! 

❔ [About Powershell Pro Tools](https://ironmansoftware.com/powershell-pro-tools/)
📕 [Documentation](https://docs.poshtools.com/powershell-pro-tools-documentation/visual-studio-code)
🧾 [Changelog](https://docs.poshtools.com/poshtools-vscode-changelog) 
🐛 [Issues](https://github.com/ironmansoftware/powershell-pro-tools)

## Installation 

[Learn More](https://docs.poshtools.com/powershell-pro-tools-documentation/installation-and-configuration#visual-studio-code-1)

# Features

- Automate Visual Studio Code
- Debugging Helpers
    - Run in new Terminal 
    - One-Click Attach
- .NET Decompiler
- Enhanced Hover 
    - AST Hover
    - Variable Value and Type Hover
- Generate UI from a Function
- Generate Tool from a Function 
- Compile to Executable
- PowerShell Explorer
    - AST Explorer 
    - Custom Tree View
    - History Explorer
    - Host Process Explorer 
    - Modules Explorer 
    - Providers Explorer 
    - Reflection Explorer
    - Session Explorer
    - Variables Explorer 
- Profiler
- Pin Session
- Sign On Save
- RapidSense
- Refactoring
    - Convert to $_
    - Convert to $PSItem
    - Convert to Multiline Command
    - Convert to Splat
    - Export Module Member 
    - Extract Function
    - Extract Selection to File
    - Generate Function from Usage
    - Generate Proxy Function
    - Introduce Using Namespace
    - Reorder Parameters
    - Split Pipeline
- Rename Symbol
- Out-VSCodeGridView
- Quick Scripts
- Windows Forms Designer

## Automate Visual Studio Code

You can automate Visual Studio Code with PowerShell scripts. Script repetitive actions and make your own tools without having to build extensions. 

Available Commands 

### Open Documents

```powershell
Open-VSCodeTextDocument -FileName .\form.designer.ps1
```

### Close Text Editors

```powershell
Get-VSCodeTextEditor | Remove-VSCodeTextEditor
```

### Get Document Text

```powershell
Get-VSCodeTextDocument | Get-VSCodeTextDocumentText
```

### Insert Document Text

```powershell
$position = New-VSCodePosition -Line 0 -Character 2
Get-VSCodeTextDocument | Add-VSCodeTextDocumentText -Position $position -Text NewText
```

### Remove Document Text 

```powershell
$Range = New-VSCodeRange -StartLine 0 -EndLine 0 -StartCharacter 0 -EndCharacter 10
Get-VSCodeTextDocument | Remove-VSCodeTextDocumentText -Range $Range
```

### Setting Text Decorations

```powershell
$Range = New-VSCodeRange -StartLine 0 -EndLine 0 -StartCharacter 0 -EndCharacter 55
Get-VSCodeTextEditor | Set-VSCodeTextEditorDecoration -BackgroundColor 'descriptionForeground' -Range $Range -Key 12321 -FontWeight bold
```

### Send Text to a Terminal

```powershell
Get-VSCodeTerminal | Where-Object Name -eq 'PowerShell Extension' | Send-VSCodeTerminalText -Text 'Write-Host "Hello World!"'
```

### Showing Messages

```powershell
Show-VSCodeMessage -Message 'Error!!!' -Type Error
```

### Showing Messages with a Response

```powershell
Show-VSCodeMessage -Message 'What should we do?' -Items @('Party', 'Sleep')
```

### Showing a Quick Pick List 

```powershell
Show-VSCodeQuickPick -PlaceHolder 'What should we do?' -Items @('Party', 'Sleep')
```

### Showing an Input Box

```powershell
Show-VSCodeInputBox -PlaceHolder 'Enter some text'
```

### Setting the Status Bar Message

```powershell
Set-VSCodeStatusBarMessage -Message 'Hellllloooo'
```

## Debugging Helpers

### Run in New Terminal

Execute a PowerShell script in a new terminal instead of the integrated terminal.

![](https://gblobscdn.gitbook.com/assets%2F-LNFE66tpE_51uobNA70%2F-MOS5jx0jezagZNkzkuN%2F-MOSB0HyvzgA_o3yAlT9%2Fnew-terminal.gif)

### One-Click Attach

Attached to external PowerShell hosts for debugging.

[See it in action](https://www.youtube.com/watch?v=mZo12kq-92c)

## .NET Decompiler 

Decompile .NET types that are loaded into your PowerShell session. 

![](https://gblobscdn.gitbook.com/assets%2F-LNFE66tpE_51uobNA70%2F-MZTldJ_cyHt1nZXiTSA%2F-MZTmGuHnVmlm04QrG3O%2Fdecompiler.gif)

## Enhanced Hover 

Enhanced hover support provides additional information about symbols within PowerShell scripts. 

### AST Hover 

![](https://gblobscdn.gitbook.com/assets%2F-LNFE66tpE_51uobNA70%2F-MP_jsF5OH0LnvY0pOqc%2F-MP_kWdv_GJN_qeI_6Rh%2Fimage.png)

### Variable Value and Type Hover

![](https://gblobscdn.gitbook.com/assets%2F-LNFE66tpE_51uobNA70%2F-MP_jsF5OH0LnvY0pOqc%2F-MP_kaz1HnLefdzO42C9%2Fimage.png)

## Generate UI from a Function

The `PowerShell: Generate Windows Form` command will generate a Windows form based on the function defined within a PS1 file. 

[Learn More](https://docs.poshtools.com/powershell-pro-tools-documentation/visual-studio-code/generating-a-ui-from-a-function)

## Generate Tool from a Function 

The `PowerShell Pro Tools: Generate Tool` command will generate a form and compile it to an executable based on the function defined within a PS1 file. 

[Learn More](https://docs.poshtools.com/powershell-pro-tools-documentation/visual-studio-code/generate-a-tool-from-a-function)

## Compile to Executable

You can compile PowerShell scripts into executables using the `Package Script as Exe` function. Customize packaging with a `package.psd1` file to create PowerShell 7 executables, services, hide the console window and more.

![](https://gblobscdn.gitbook.com/assets%2F-LNFE66tpE_51uobNA70%2F-MMfXUs2AQpLghNUDbO8%2F-MMfYUFOXuAduy3B7kf-%2Fimage.png)

[Learn More](https://docs.poshtools.com/powershell-pro-tools-documentation/visual-studio-code/packaging-in-visual-studio-code)

## Pin Session

Pin a session to a document to quickly switch between PowerShell instances. 

![](https://gblobscdn.gitbook.com/assets%2F-LNFE66tpE_51uobNA70%2F-M_7JbHehRt2C5jYHSov%2F-M_7Kz_b_om7f1PZXJuN%2Fpinsession.gif)

## PowerShell Explorer

The PowerShell Explorer displays an tree view with numerous sections developed for different aspects of your PowerShell environment. 

### AST Explorer 

![](https://gblobscdn.gitbook.com/assets%2F-LNFE66tpE_51uobNA70%2F-LtjTLCd7gXPBEMm8Hat%2F-LtjToLS5m2FCUMDaT1_%2Fimage.png)

### Custom Tree View

![](https://gblobscdn.gitbook.com/assets%2F-LNFE66tpE_51uobNA70%2F-MZsxTF2sk8gYl6b7uZe%2F-MZsyBv4UsKbm1JI2q0h%2Fcustomtreeview.gif)

### History Explorer

![](https://gblobscdn.gitbook.com/assets%2F-LNFE66tpE_51uobNA70%2F-MZtNJkaoxgasOfuKXtO%2F-MZtNOQX99IZI8VzjDwb%2Fimage.png)

### Host Process Explorer 

![](https://gblobscdn.gitbook.com/assets%2F-LNFE66tpE_51uobNA70%2F-MORWP4x75FISy2OLNfK%2F-MORam-RkmY0SFPMwfM_%2Fimage.png)

### Jobs Explorer 

![](https://gblobscdn.gitbook.com/assets%2F-LNFE66tpE_51uobNA70%2F-M_MLlS_Egh8bemxwgrU%2F-M_MLyioLMiBLm7NXOvh%2Fimage.png)

### Modules Explorer 

![](https://gblobscdn.gitbook.com/assets%2F-LNFE66tpE_51uobNA70%2F-LtjTLCd7gXPBEMm8Hat%2F-LtjUmBvqKcAFJPectEx%2Fmodules.PNG)

### Providers Explorer 

![](https://gblobscdn.gitbook.com/assets%2F-LNFE66tpE_51uobNA70%2F-LtjTLCd7gXPBEMm8Hat%2F-LtjV7p9TaG62yLjunPv%2Fproviders.PNG)

### Reflection Explorer

![](https://gblobscdn.gitbook.com/assets%2F-LNFE66tpE_51uobNA70%2F-MZP5mzR6yxyf5gWNFPF%2F-MZP6WSoXJPHcyL0JWVU%2Fimage.png)

### Session Explorer 

![](https://gblobscdn.gitbook.com/assets%2F-LNFE66tpE_51uobNA70%2F-M_7JbHehRt2C5jYHSov%2F-M_7JyFZvn9cZgx8f12x%2Fimage.png)

### Variables Explorer 

![](https://gblobscdn.gitbook.com/assets%2F-LNFE66tpE_51uobNA70%2F-LtwS5Ki3QHuAisZYmlc%2F-LtwSXh4IQiIDvk4P71t%2Fvariables.png)

## Profiler

Profile the performance of scripts and view the timing within VS Code. 

![](https://gblobscdn.gitbook.com/assets%2F-LNFE66tpE_51uobNA70%2F-LRoyY1pilx2tp36VDau%2F-LRoygTJw92LfHDrEA3x%2Fimage.png)

## Sign On Save

Sign scripts when they are saved with a configurable code-signing certificate. 

![](https://gblobscdn.gitbook.com/assets%2F-LNFE66tpE_51uobNA70%2F-MZJgXHw7KwsCZMvOXjs%2F-MZJhS6b7O8rNfJ0jZgS%2Fimage.png)

## RapidSense

Enable high performance, configurable IntelliSense for PowerShell in VS Code. 

[Learn More](https://www.youtube.com/watch?v=rIp6VPh91h0)

## Refactoring

Quickly adjust scripts with refactoring commands. 

### Convert to $_

![](https://gblobscdn.gitbook.com/assets%2F-LNFE66tpE_51uobNA70%2F-MPXNbhKjVqiTlhr4AWh%2F-MPXO04v5yHzXhiR1aTm%2Fconverttodollarunder.gif)

### Convert to $PSItem

![](https://gblobscdn.gitbook.com/assets%2F-LNFE66tpE_51uobNA70%2F-MPXNbhKjVqiTlhr4AWh%2F-MPXOJpL1Oup_fK-FvSh%2Fconverttopsitem.gif)

### Convert to Multiline Command

![](https://gblobscdn.gitbook.com/assets%2F-LNFE66tpE_51uobNA70%2F-MOZRXr5WM1oozM7xgJV%2F-MOZS9SXQsx48tayR16e%2Fmulti-line.gif)

### Convert to Splat

![](https://gblobscdn.gitbook.com/assets%2F-LNFE66tpE_51uobNA70%2F-MOU6iPzGXgBkpIjnghK%2F-MOUGowA5Q-fn30u2UTF%2Fconvert-to-splat.gif)

### Export Module Member 

![](https://gblobscdn.gitbook.com/assets%2F-LNFE66tpE_51uobNA70%2F-MOTs49qXGdqNpZ6Ywez%2F-MOU6cJ8KcTt9uMTBq7s%2Fexport-module-member.gif)

### Extract Function

![](https://gblobscdn.gitbook.com/assets%2F-LNFE66tpE_51uobNA70%2F-MOUfrWXgiPeun0CyZ-f%2F-MOUgHXdNw7fZnFH_Z1v%2Fexport-function.gif)

### Extract Selection to File

![](https://gblobscdn.gitbook.com/assets%2F-LNFE66tpE_51uobNA70%2F-MOTrNxvTCnmuoFnoQcL%2F-MOTrwRfBGcOtWZdOcM_%2Fextract-file.gif)

### Generate Function from Usage

![](https://gblobscdn.gitbook.com/assets%2F-LNFE66tpE_51uobNA70%2F-MOniVKODpgMVe1cJGa2%2F-MOnjGH6ICKfhLoAMKdW%2Fgenerate-function.gif)

### Generate Proxy Function

![](https://gblobscdn.gitbook.com/assets%2F-LNFE66tpE_51uobNA70%2F-MPsHjJG6X4qsp0HYexm%2F-MPsICFqK3dXdKAczbda%2Fproxy.gif)

### Introduce Using Namespace

![](https://gblobscdn.gitbook.com/assets%2F-LNFE66tpE_51uobNA70%2F-MPSBxhqDCF6kJS3S6xn%2F-MPSCRt_mo2RtJDZ8U63%2Fintroduce-using.gif)

### Reorder Parameters

![](https://gblobscdn.gitbook.com/assets%2F-LNFE66tpE_51uobNA70%2F-MOsQpeCGJN7K6kIG_6K%2F-MOsRAZOJjGKyJVvLHSZ%2Fmove-parameters.gif)

### Split Pipeline

![](https://gblobscdn.gitbook.com/assets%2F-LNFE66tpE_51uobNA70%2F-MPRtE0c_spmE9BnQb84%2F-MPRuUr-DpGW2sRPuBs1%2Fsplit-pipe.gif)

## Rename Symbol

![](https://gblobscdn.gitbook.com/assets%2F-LNFE66tpE_51uobNA70%2F-Ma-Yg8HhLoWVK3Syb9g%2F-Ma-Z0JEx1YO27lBZTE5%2Fglobal-vars.gif)

## Out-VSCodeGridView

Out-GridView support for VS Code. Pipe your data directly to Out-VSCodeGridView and view it in the VS Code web view. 

![](https://ironmansoftware.com/wp-content/uploads/2020/04/outgridview.png)


## Quick Scripts

Bookmark scripts found anywhere on your system and access them in any workspace.

![](https://gblobscdn.gitbook.com/assets%2F-LNFE66tpE_51uobNA70%2F-LtjlS99-N5Ljha_bQq4%2F-LtjlfrR5FWM67YEHvUj%2Fquick-script-list.png)

## Windows Forms Designer

Design Windows Forms using PSScriptPad integration in VS Code. 

![](https://gblobscdn.gitbook.com/assets%2F-LNFE66tpE_51uobNA70%2F-LU7-k9sJoDGitc8k-Mb%2F-LU73h7_sjSmyrDnKmw0%2Fimage.png)

## WPF Designer

Design WPF Windows using PSScriptPad integration in VS Code. 

![](https://gblobscdn.gitbook.com/assets%2F-LNFE66tpE_51uobNA70%2F-MQguAs16QK6mb2mzxeX%2F-MQh95mUAjxFHjN72AEn%2Fvscode.gif)