[void][System.Reflection.Assembly]::Load('System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a')
[void][System.Reflection.Assembly]::Load('System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089')
$MainForm = New-Object -TypeName System.Windows.Forms.Form
[System.Windows.Forms.Button]$btnName = $null
[System.Windows.Forms.ProgressBar]$progress = $null
[System.Windows.Forms.MaskedTextBox]$maskedTextBox1 = $null
[System.Windows.Forms.PictureBox]$pictureBox1 = $null
[System.Windows.Forms.Button]$button1 = $null
function InitializeComponent
{
$resources = Invoke-Expression (Get-Content "$PSScriptRoot\MultiThreadedForm.Designer.psd1" -Raw)
$progress = (New-Object -TypeName System.Windows.Forms.ProgressBar)
$btnName = (New-Object -TypeName System.Windows.Forms.Button)
$maskedTextBox1 = (New-Object -TypeName System.Windows.Forms.MaskedTextBox)
$pictureBox1 = (New-Object -TypeName System.Windows.Forms.PictureBox)
([System.ComponentModel.ISupportInitialize]$pictureBox1).BeginInit()
$MainForm.SuspendLayout()
#
#progress
#
$progress.Location = (New-Object -TypeName System.Drawing.Point -ArgumentList @([System.Int32]12,[System.Int32]12))
$progress.Name = [string]'progress'
$progress.Size = (New-Object -TypeName System.Drawing.Size -ArgumentList @([System.Int32]260,[System.Int32]23))
$progress.TabIndex = [System.Int32]0
#
#btnName
#
$btnName.Location = (New-Object -TypeName System.Drawing.Point -ArgumentList @([System.Int32]71,[System.Int32]64))
$btnName.Name = [string]'btnName'
$btnName.Size = (New-Object -TypeName System.Drawing.Size -ArgumentList @([System.Int32]142,[System.Int32]23))
$btnName.TabIndex = [System.Int32]1
$btnName.Text = [string]'Fill Progress Bar'
$btnName.UseVisualStyleBackColor = $true
$btnName.add_Click($btnName_Click)
#
#maskedTextBox1
#
$maskedTextBox1.Location = (New-Object -TypeName System.Drawing.Point -ArgumentList @([System.Int32]12,[System.Int32]41))
$maskedTextBox1.Name = [string]'maskedTextBox1'
$maskedTextBox1.PromptChar = [System.Char]'8'
$maskedTextBox1.Size = (New-Object -TypeName System.Drawing.Size -ArgumentList @([System.Int32]260,[System.Int32]20))
$maskedTextBox1.TabIndex = [System.Int32]2
#
#pictureBox1
#
$pictureBox1.Image = ([System.Drawing.Image]$resources.'pictureBox1.Image')
$pictureBox1.Location = (New-Object -TypeName System.Drawing.Point -ArgumentList @([System.Int32]12,[System.Int32]93))
$pictureBox1.Name = [string]'pictureBox1'
$pictureBox1.Size = (New-Object -TypeName System.Drawing.Size -ArgumentList @([System.Int32]100,[System.Int32]50))
$pictureBox1.TabIndex = [System.Int32]3
$pictureBox1.TabStop = $false
#
#MainForm
#
$MainForm.ClientSize = (New-Object -TypeName System.Drawing.Size -ArgumentList @([System.Int32]284,[System.Int32]153))
$MainForm.Controls.Add($pictureBox1)
$MainForm.Controls.Add($maskedTextBox1)
$MainForm.Controls.Add($btnName)
$MainForm.Controls.Add($progress)
$MainForm.Icon = ([System.Drawing.Icon]$resources.'$this.Icon')
$MainForm.Name = [string]'MainForm'
$MainForm.Text = [string]'Test'
([System.ComponentModel.ISupportInitialize]$pictureBox1).EndInit()
$MainForm.ResumeLayout($false)
$MainForm.PerformLayout()
Add-Member -InputObject $MainForm -Name base -Value $base -MemberType NoteProperty
Add-Member -InputObject $MainForm -Name btnName -Value $btnName -MemberType NoteProperty
Add-Member -InputObject $MainForm -Name progress -Value $progress -MemberType NoteProperty
Add-Member -InputObject $MainForm -Name maskedTextBox1 -Value $maskedTextBox1 -MemberType NoteProperty
Add-Member -InputObject $MainForm -Name pictureBox1 -Value $pictureBox1 -MemberType NoteProperty
Add-Member -InputObject $MainForm -Name button1 -Value $button1 -MemberType NoteProperty
}
. InitializeComponent
