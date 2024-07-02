[void][System.Reflection.Assembly]::Load('System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a')
[void][System.Reflection.Assembly]::Load('System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089')
[SYstem.Windows.Forms.Application]::EnableVisualStyles()
$$itemname$ = New-Object -TypeName System.Windows.Forms.Form
function InitializeComponent
{
$$itemname$.SuspendLayout()
#
#$itemname$
#
$$itemname$.Name = '$itemname$'
$$itemname$.ResumeLayout($false)
}

InitializeComponent