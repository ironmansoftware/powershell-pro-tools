[void][System.Reflection.Assembly]::Load('System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a')
[void][System.Reflection.Assembly]::Load('System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089')
$IntelHostingVMwareUtilityForm = New-Object -TypeName System.Windows.Forms.Form
[System.Windows.Forms.ProgressBar]$progressBar1 = $null
[System.Windows.Forms.GroupBox]$GBThreadcounter = $null
[System.Windows.Forms.Label]$lblthreads = $null
[System.Windows.Forms.TextBox]$tbtrackbarval = $null
[System.Windows.Forms.GroupBox]$groupBox3 = $null
[System.Windows.Forms.Label]$lblstatus = $null
[System.Windows.Forms.Label]$label11 = $null
[System.Windows.Forms.MaskedTextBox]$TBPassword = $null
[System.Windows.Forms.TextBox]$TBUserID = $null
[System.Windows.Forms.Label]$label13 = $null
[System.Windows.Forms.Label]$label12 = $null
[System.Windows.Forms.Button]$button2 = $null
[System.Windows.Forms.Label]$label10 = $null
[System.Windows.Forms.GroupBox]$groupBox1 = $null
[System.Windows.Forms.Button]$btnclear = $null
[System.Windows.Forms.TrackBar]$trackBar1 = $null
[System.Windows.Forms.Button]$btnProcess = $null
[System.Windows.Forms.Button]$button3 = $null
[System.Windows.Forms.GroupBox]$groupBox2 = $null
[System.Windows.Forms.RadioButton]$RBUpgrade = $null
[System.Windows.Forms.RadioButton]$RBQueries = $null
[System.Windows.Forms.RadioButton]$RBChange = $null
[System.Windows.Forms.RadioButton]$RBUtilities = $null
[System.Windows.Forms.GroupBox]$GBUpgrade = $null
[System.Windows.Forms.Button]$BtnAbort = $null
[System.Windows.Forms.Button]$button4 = $null
[System.Windows.Forms.GroupBox]$GBUtilities = $null
[System.Windows.Forms.RadioButton]$RBrdp = $null
[System.Windows.Forms.RadioButton]$rbping = $null
[System.Windows.Forms.GroupBox]$GBQueries = $null
[System.Windows.Forms.RadioButton]$radioButton10 = $null
[System.Windows.Forms.RadioButton]$RBNicinfo = $null
[System.Windows.Forms.RadioButton]$BTNgetvmHWversion = $null
[System.Windows.Forms.RadioButton]$rbpowerstate = $null
[System.Windows.Forms.RadioButton]$RBToolsver = $null
[System.Windows.Forms.RadioButton]$RBCurrentHost = $null
[System.Windows.Forms.GroupBox]$GBChange = $null
[System.Windows.Forms.RadioButton]$RBRebootHard = $null
[System.Windows.Forms.RadioButton]$RBSTopVMHard = $null
[System.Windows.Forms.RadioButton]$RBRebootOS = $null
[System.Windows.Forms.RadioButton]$RBStartVM = $null
[System.Windows.Forms.RadioButton]$RBStopVMOS = $null
[System.Windows.Forms.TextBox]$textBox1 = $null
[System.Windows.Forms.Label]$label4 = $null
[System.Windows.Forms.TextBox]$textBox2 = $null
[System.Windows.Forms.Label]$lblprogress = $null
[System.Windows.Forms.Label]$label17 = $null
[System.Windows.Forms.Label]$label9 = $null
[System.Windows.Forms.GroupBox]$groupBox4 = $null
[System.Windows.Forms.Button]$ARCHIVECOMPLETED = $null
[System.Windows.Forms.Button]$BTNCounterUpdate = $null
[System.Windows.Forms.TextBox]$tbcompleted = $null
[System.Windows.Forms.Label]$label14 = $null
[System.Windows.Forms.TextBox]$tbrunning = $null
[System.Windows.Forms.Label]$label16 = $null
[System.Windows.Forms.TextBox]$tbtotals = $null
[System.Windows.Forms.Label]$label15 = $null
[System.Windows.Forms.Label]$label5 = $null
[System.Windows.Forms.Label]$label1 = $null
[System.Windows.Forms.Label]$label2 = $null
[System.Windows.Forms.WebBrowser]$webBrowser4 = $null
[System.Windows.Forms.Label]$label7 = $null
[System.Windows.Forms.WebBrowser]$webBrowser2 = $null
[System.Windows.Forms.WebBrowser]$webBrowser1 = $null
[System.Windows.Forms.WebBrowser]$webBrowser3 = $null
[System.Windows.Forms.Label]$label8 = $null
[System.Windows.Forms.WebBrowser]$webBrowser5 = $null
[System.Windows.Forms.TextBox]$tbstart = $null
[System.Windows.Forms.WebBrowser]$webBrowser6 = $null
[System.Windows.Forms.Label]$label6 = $null
[System.Windows.Forms.WebBrowser]$webBrowser7 = $null
[System.Windows.Forms.Label]$label3 = $null
[System.Windows.Forms.TextBox]$tbhwupgradestart = $null
[System.Windows.Forms.TextBox]$tbupdatetools = $null
[System.Windows.Forms.TextBox]$tbstartrdp = $null
[System.Windows.Forms.TextBox]$tbstop = $null
[System.Windows.Forms.TextBox]$tbcomplete = $null
[System.Windows.Forms.GroupBox]$GBThreads = $null
[System.Windows.Forms.TextBox]$TBThreads = $null
[System.ComponentModel.IContainer]$components = $null
[System.Windows.Forms.TextBox]$TBVcenter = $null
[System.Windows.Forms.TextBox]$tberror = $null
[System.Windows.Forms.Button]$button1 = $null
function InitializeComponent
{
$progressBar1 = (New-Object -TypeName System.Windows.Forms.ProgressBar)
$GBThreadcounter = (New-Object -TypeName System.Windows.Forms.GroupBox)
$lblthreads = (New-Object -TypeName System.Windows.Forms.Label)
$tbtrackbarval = (New-Object -TypeName System.Windows.Forms.TextBox)
$groupBox3 = (New-Object -TypeName System.Windows.Forms.GroupBox)
$lblstatus = (New-Object -TypeName System.Windows.Forms.Label)
$label11 = (New-Object -TypeName System.Windows.Forms.Label)
$TBPassword = (New-Object -TypeName System.Windows.Forms.MaskedTextBox)
$TBUserID = (New-Object -TypeName System.Windows.Forms.TextBox)
$label13 = (New-Object -TypeName System.Windows.Forms.Label)
$label12 = (New-Object -TypeName System.Windows.Forms.Label)
$button2 = (New-Object -TypeName System.Windows.Forms.Button)
$TBVcenter = (New-Object -TypeName System.Windows.Forms.TextBox)
$label10 = (New-Object -TypeName System.Windows.Forms.Label)
$groupBox1 = (New-Object -TypeName System.Windows.Forms.GroupBox)
$btnclear = (New-Object -TypeName System.Windows.Forms.Button)
$trackBar1 = (New-Object -TypeName System.Windows.Forms.TrackBar)
$btnProcess = (New-Object -TypeName System.Windows.Forms.Button)
$button3 = (New-Object -TypeName System.Windows.Forms.Button)
$groupBox2 = (New-Object -TypeName System.Windows.Forms.GroupBox)
$RBUpgrade = (New-Object -TypeName System.Windows.Forms.RadioButton)
$RBQueries = (New-Object -TypeName System.Windows.Forms.RadioButton)
$RBChange = (New-Object -TypeName System.Windows.Forms.RadioButton)
$RBUtilities = (New-Object -TypeName System.Windows.Forms.RadioButton)
$GBUpgrade = (New-Object -TypeName System.Windows.Forms.GroupBox)
$BtnAbort = (New-Object -TypeName System.Windows.Forms.Button)
$button4 = (New-Object -TypeName System.Windows.Forms.Button)
$GBUtilities = (New-Object -TypeName System.Windows.Forms.GroupBox)
$RBrdp = (New-Object -TypeName System.Windows.Forms.RadioButton)
$rbping = (New-Object -TypeName System.Windows.Forms.RadioButton)
$GBQueries = (New-Object -TypeName System.Windows.Forms.GroupBox)
$radioButton10 = (New-Object -TypeName System.Windows.Forms.RadioButton)
$RBNicinfo = (New-Object -TypeName System.Windows.Forms.RadioButton)
$BTNgetvmHWversion = (New-Object -TypeName System.Windows.Forms.RadioButton)
$rbpowerstate = (New-Object -TypeName System.Windows.Forms.RadioButton)
$RBToolsver = (New-Object -TypeName System.Windows.Forms.RadioButton)
$RBCurrentHost = (New-Object -TypeName System.Windows.Forms.RadioButton)
$GBChange = (New-Object -TypeName System.Windows.Forms.GroupBox)
$RBRebootHard = (New-Object -TypeName System.Windows.Forms.RadioButton)
$RBSTopVMHard = (New-Object -TypeName System.Windows.Forms.RadioButton)
$RBRebootOS = (New-Object -TypeName System.Windows.Forms.RadioButton)
$RBStartVM = (New-Object -TypeName System.Windows.Forms.RadioButton)
$RBStopVMOS = (New-Object -TypeName System.Windows.Forms.RadioButton)
$textBox1 = (New-Object -TypeName System.Windows.Forms.TextBox)
$label4 = (New-Object -TypeName System.Windows.Forms.Label)
$textBox2 = (New-Object -TypeName System.Windows.Forms.TextBox)
$lblprogress = (New-Object -TypeName System.Windows.Forms.Label)
$label17 = (New-Object -TypeName System.Windows.Forms.Label)
$label9 = (New-Object -TypeName System.Windows.Forms.Label)
$groupBox4 = (New-Object -TypeName System.Windows.Forms.GroupBox)
$ARCHIVECOMPLETED = (New-Object -TypeName System.Windows.Forms.Button)
$BTNCounterUpdate = (New-Object -TypeName System.Windows.Forms.Button)
$tbcompleted = (New-Object -TypeName System.Windows.Forms.TextBox)
$label14 = (New-Object -TypeName System.Windows.Forms.Label)
$tbrunning = (New-Object -TypeName System.Windows.Forms.TextBox)
$label16 = (New-Object -TypeName System.Windows.Forms.Label)
$tbtotals = (New-Object -TypeName System.Windows.Forms.TextBox)
$label15 = (New-Object -TypeName System.Windows.Forms.Label)
$label5 = (New-Object -TypeName System.Windows.Forms.Label)
$label1 = (New-Object -TypeName System.Windows.Forms.Label)
$label2 = (New-Object -TypeName System.Windows.Forms.Label)
$webBrowser4 = (New-Object -TypeName System.Windows.Forms.WebBrowser)
$label7 = (New-Object -TypeName System.Windows.Forms.Label)
$webBrowser2 = (New-Object -TypeName System.Windows.Forms.WebBrowser)
$webBrowser1 = (New-Object -TypeName System.Windows.Forms.WebBrowser)
$webBrowser3 = (New-Object -TypeName System.Windows.Forms.WebBrowser)
$label8 = (New-Object -TypeName System.Windows.Forms.Label)
$webBrowser5 = (New-Object -TypeName System.Windows.Forms.WebBrowser)
$tbstart = (New-Object -TypeName System.Windows.Forms.TextBox)
$webBrowser6 = (New-Object -TypeName System.Windows.Forms.WebBrowser)
$label6 = (New-Object -TypeName System.Windows.Forms.Label)
$webBrowser7 = (New-Object -TypeName System.Windows.Forms.WebBrowser)
$label3 = (New-Object -TypeName System.Windows.Forms.Label)
$tbhwupgradestart = (New-Object -TypeName System.Windows.Forms.TextBox)
$tbupdatetools = (New-Object -TypeName System.Windows.Forms.TextBox)
$tbstartrdp = (New-Object -TypeName System.Windows.Forms.TextBox)
$tbstop = (New-Object -TypeName System.Windows.Forms.TextBox)
$tbcomplete = (New-Object -TypeName System.Windows.Forms.TextBox)
$tberror = (New-Object -TypeName System.Windows.Forms.TextBox)
$GBThreads = (New-Object -TypeName System.Windows.Forms.GroupBox)
$TBThreads = (New-Object -TypeName System.Windows.Forms.TextBox)
$GBThreadcounter.SuspendLayout()
$groupBox3.SuspendLayout()
$groupBox1.SuspendLayout()
([System.ComponentModel.ISupportInitialize]$trackBar1).BeginInit()
$groupBox2.SuspendLayout()
$GBUpgrade.SuspendLayout()
$GBUtilities.SuspendLayout()
$GBQueries.SuspendLayout()
$GBChange.SuspendLayout()
$groupBox4.SuspendLayout()
$GBThreads.SuspendLayout()
$IntelHostingVMwareUtilityForm.SuspendLayout()
#
#progressBar1
#
$progressBar1.ForeColor = [System.Drawing.Color]::Lime
$progressBar1.Location = (New-Object -TypeName System.Drawing.Point -ArgumentList @([System.Int32]744,[System.Int32]14))
$progressBar1.Name = [System.String]'progressBar1'
$progressBar1.Size = (New-Object -TypeName System.Drawing.Size -ArgumentList @([System.Int32]506,[System.Int32]23))
$progressBar1.TabIndex = [System.Int32]21
#
#GBThreadcounter
#
$GBThreadcounter.Controls.Add($lblthreads)
$GBThreadcounter.Controls.Add($tbtrackbarval)
$GBThreadcounter.Location = (New-Object -TypeName System.Drawing.Point -ArgumentList @([System.Int32]219,[System.Int32]308))
$GBThreadcounter.Name = [System.String]'GBThreadcounter'
$GBThreadcounter.Size = (New-Object -TypeName System.Drawing.Size -ArgumentList @([System.Int32]195,[System.Int32]101))
$GBThreadcounter.TabIndex = [System.Int32]19
$GBThreadcounter.TabStop = $false
#
#lblthreads
#
$lblthreads.AutoSize = $true
$lblthreads.Location = (New-Object -TypeName System.Drawing.Point -ArgumentList @([System.Int32]6,[System.Int32]20))
$lblthreads.Name = [System.String]'lblthreads'
$lblthreads.Size = (New-Object -TypeName System.Drawing.Size -ArgumentList @([System.Int32]130,[System.Int32]18))
$lblthreads.TabIndex = [System.Int32]17
$lblthreads.Text = [System.String]'Thread Counter:'
#
#tbtrackbarval
#
$tbtrackbarval.Enabled = $false
$tbtrackbarval.Location = (New-Object -TypeName System.Drawing.Point -ArgumentList @([System.Int32]142,[System.Int32]15))
$tbtrackbarval.Name = [System.String]'tbtrackbarval'
$tbtrackbarval.Size = (New-Object -TypeName System.Drawing.Size -ArgumentList @([System.Int32]30,[System.Int32]24))
$tbtrackbarval.TabIndex = [System.Int32]18
#
#groupBox3
#
$groupBox3.BackColor = [System.Drawing.SystemColors]::Control
$groupBox3.Controls.Add($lblstatus)
$groupBox3.Controls.Add($label11)
$groupBox3.Controls.Add($TBPassword)
$groupBox3.Controls.Add($TBUserID)
$groupBox3.Controls.Add($label13)
$groupBox3.Controls.Add($label12)
$groupBox3.Controls.Add($button2)
$groupBox3.Controls.Add($TBVcenter)
$groupBox3.Controls.Add($label10)
$groupBox3.ForeColor = [System.Drawing.SystemColors]::MenuText
$groupBox3.Location = (New-Object -TypeName System.Drawing.Point -ArgumentList @([System.Int32]217,[System.Int32]23))
$groupBox3.Name = [System.String]'groupBox3'
$groupBox3.Size = (New-Object -TypeName System.Drawing.Size -ArgumentList @([System.Int32]201,[System.Int32]247))
$groupBox3.TabIndex = [System.Int32]10
$groupBox3.TabStop = $false
#
#lblstatus
#
$lblstatus.AutoSize = $true
$lblstatus.ForeColor = [System.Drawing.Color]::Red
$lblstatus.Location = (New-Object -TypeName System.Drawing.Point -ArgumentList @([System.Int32]70,[System.Int32]24))
$lblstatus.Name = [System.String]'lblstatus'
$lblstatus.Size = (New-Object -TypeName System.Drawing.Size -ArgumentList @([System.Int32]118,[System.Int32]18))
$lblstatus.TabIndex = [System.Int32]14
$lblstatus.Text = [System.String]'Not connected'
#
#label11
#
$label11.AutoSize = $true
$label11.Location = (New-Object -TypeName System.Drawing.Point -ArgumentList @([System.Int32]4,[System.Int32]24))
$label11.Name = [System.String]'label11'
$label11.Size = (New-Object -TypeName System.Drawing.Size -ArgumentList @([System.Int32]61,[System.Int32]18))
$label11.TabIndex = [System.Int32]13
$label11.Text = [System.String]'Status:'
#
#TBPassword
#
$TBPassword.Font = (New-Object -TypeName System.Drawing.Font -ArgumentList @([System.String]'Microsoft Sans Serif',[System.Single]9,[System.Drawing.FontStyle]::Bold,[System.Drawing.GraphicsUnit]::Point,([System.Byte][System.Byte]0)))
$TBPassword.Location = (New-Object -TypeName System.Drawing.Point -ArgumentList @([System.Int32]7,[System.Int32]178))
$TBPassword.Name = [System.String]'TBPassword'
$TBPassword.PasswordChar = [System.Char]'*'
$TBPassword.Size = (New-Object -TypeName System.Drawing.Size -ArgumentList @([System.Int32]190,[System.Int32]21))
$TBPassword.TabIndex = [System.Int32]12
$TBPassword.Text = [System.String]'Pp92Ry'
$TBPassword.add_MaskInputRejected($maskedTextBox1_MaskInputRejected)
#
#TBUserID
#
$TBUserID.Font = (New-Object -TypeName System.Drawing.Font -ArgumentList @([System.String]'Microsoft Sans Serif',[System.Single]9,[System.Drawing.FontStyle]::Bold,[System.Drawing.GraphicsUnit]::Point,([System.Byte][System.Byte]0)))
$TBUserID.Location = (New-Object -TypeName System.Drawing.Point -ArgumentList @([System.Int32]7,[System.Int32]121))
$TBUserID.Name = [System.String]'TBUserID'
$TBUserID.Size = (New-Object -TypeName System.Drawing.Size -ArgumentList @([System.Int32]190,[System.Int32]21))
$TBUserID.TabIndex = [System.Int32]11
$TBUserID.Text = [System.String]'up\ppj25ey7'
$TBUserID.add_TextChanged($textBox3_TextChanged)
#
#label13
#
$label13.AutoSize = $true
$label13.Font = (New-Object -TypeName System.Drawing.Font -ArgumentList @([System.String]'Microsoft Sans Serif',[System.Single]9,[System.Drawing.FontStyle]::Bold,[System.Drawing.GraphicsUnit]::Point,([System.Byte][System.Byte]0)))
$label13.Location = (New-Object -TypeName System.Drawing.Point -ArgumentList @([System.Int32]4,[System.Int32]157))
$label13.Name = [System.String]'label13'
$label13.Size = (New-Object -TypeName System.Drawing.Size -ArgumentList @([System.Int32]107,[System.Int32]15))
$label13.TabIndex = [System.Int32]10
$label13.Text = [System.String]'Enter Password'
$label13.add_Click($label13_Click)
#
#label12
#
$label12.AutoSize = $true
$label12.Font = (New-Object -TypeName System.Drawing.Font -ArgumentList @([System.String]'Microsoft Sans Serif',[System.Single]9,[System.Drawing.FontStyle]::Bold,[System.Drawing.GraphicsUnit]::Point,([System.Byte][System.Byte]0)))
$label12.Location = (New-Object -TypeName System.Drawing.Point -ArgumentList @([System.Int32]5,[System.Int32]100))
$label12.Name = [System.String]'label12'
$label12.Size = (New-Object -TypeName System.Drawing.Size -ArgumentList @([System.Int32]143,[System.Int32]15))
$label12.TabIndex = [System.Int32]9
$label12.Text = [System.String]'Enter Domain\UserID'
$label12.add_Click($label12_Click)
#
#button2
#
$button2.BackColor = [System.Drawing.Color]::LimeGreen
$button2.FlatStyle = [System.Windows.Forms.FlatStyle]::Popup
$button2.Font = (New-Object -TypeName System.Drawing.Font -ArgumentList @([System.String]'Microsoft Sans Serif',[System.Single]10,[System.Drawing.FontStyle]::Bold,[System.Drawing.GraphicsUnit]::Point,([System.Byte][System.Byte]0)))
$button2.ForeColor = [System.Drawing.SystemColors]::ControlText
$button2.Location = (New-Object -TypeName System.Drawing.Point -ArgumentList @([System.Int32]7,[System.Int32]212))
$button2.Name = [System.String]'button2'
$button2.Size = (New-Object -TypeName System.Drawing.Size -ArgumentList @([System.Int32]190,[System.Int32]25))
$button2.TabIndex = [System.Int32]7
$button2.Text = [System.String]'Connect To VCenter'
$button2.UseVisualStyleBackColor = $false
$button2.add_Click($button2_Click)
#
#TBVcenter
#
$TBVcenter.Font = (New-Object -TypeName System.Drawing.Font -ArgumentList @([System.String]'Microsoft Sans Serif',[System.Single]9,[System.Drawing.FontStyle]::Bold,[System.Drawing.GraphicsUnit]::Point,([System.Byte][System.Byte]0)))
$TBVcenter.Location = (New-Object -TypeName System.Drawing.Point -ArgumentList @([System.Int32]9,[System.Int32]65))
$TBVcenter.Name = [System.String]'TBVcenter'
$TBVcenter.Size = (New-Object -TypeName System.Drawing.Size -ArgumentList @([System.Int32]188,[System.Int32]21))
$TBVcenter.TabIndex = [System.Int32]5
$TBVcenter.Text = [System.String]'vcntr0acmgt01.up.acpt.upc'
#
#label10
#
$label10.AutoSize = $true
$label10.Font = (New-Object -TypeName System.Drawing.Font -ArgumentList @([System.String]'Microsoft Sans Serif',[System.Single]9,[System.Drawing.FontStyle]::Bold,[System.Drawing.GraphicsUnit]::Point,([System.Byte][System.Byte]0)))
$label10.Location = (New-Object -TypeName System.Drawing.Point -ArgumentList @([System.Int32]5,[System.Int32]44))
$label10.Name = [System.String]'label10'
$label10.Size = (New-Object -TypeName System.Drawing.Size -ArgumentList @([System.Int32]139,[System.Int32]15))
$label10.TabIndex = [System.Int32]8
$label10.Text = [System.String]'Enter Vcenter Name:'
#
#groupBox1
#
$groupBox1.Controls.Add($btnclear)
$groupBox1.Controls.Add($trackBar1)
$groupBox1.Controls.Add($btnProcess)
$groupBox1.Controls.Add($button3)
$groupBox1.Controls.Add($groupBox2)
$groupBox1.Controls.Add($textBox1)
$groupBox1.Controls.Add($groupBox3)
$groupBox1.Controls.Add($GBThreadcounter)
$groupBox1.FlatStyle = [System.Windows.Forms.FlatStyle]::Popup
$groupBox1.Font = (New-Object -TypeName System.Drawing.Font -ArgumentList @([System.String]'Microsoft Sans Serif',[System.Single]11,[System.Drawing.FontStyle]::Bold,[System.Drawing.GraphicsUnit]::Point,([System.Byte][System.Byte]0)))
$groupBox1.Location = (New-Object -TypeName System.Drawing.Point -ArgumentList @([System.Int32]12,[System.Int32]12))
$groupBox1.Name = [System.String]'groupBox1'
$groupBox1.Size = (New-Object -TypeName System.Drawing.Size -ArgumentList @([System.Int32]424,[System.Int32]915))
$groupBox1.TabIndex = [System.Int32]1
$groupBox1.TabStop = $false
$groupBox1.Text = [System.String]'Enter Servers:'
$groupBox1.add_Enter($groupBox1_Enter)
#
#btnclear
#
$btnclear.BackColor = [System.Drawing.Color]::LimeGreen
$btnclear.FlatAppearance.BorderColor = [System.Drawing.Color]::Lime
$btnclear.FlatStyle = [System.Windows.Forms.FlatStyle]::Popup
$btnclear.Font = (New-Object -TypeName System.Drawing.Font -ArgumentList @([System.String]'Microsoft Sans Serif',[System.Single]10,[System.Drawing.FontStyle]::Bold,[System.Drawing.GraphicsUnit]::Point,([System.Byte][System.Byte]0)))
$btnclear.ForeColor = [System.Drawing.SystemColors]::ControlText
$btnclear.Location = (New-Object -TypeName System.Drawing.Point -ArgumentList @([System.Int32]17,[System.Int32]380))
$btnclear.Name = [System.String]'btnclear'
$btnclear.Size = (New-Object -TypeName System.Drawing.Size -ArgumentList @([System.Int32]196,[System.Int32]25))
$btnclear.TabIndex = [System.Int32]20
$btnclear.Text = [System.String]'Clear Servers'
$btnclear.UseVisualStyleBackColor = $false
$btnclear.add_Click($btnclear_Click)
#
#trackBar1
#
$trackBar1.Location = (New-Object -TypeName System.Drawing.Point -ArgumentList @([System.Int32]228,[System.Int32]352))
$trackBar1.Name = [System.String]'trackBar1'
$trackBar1.Size = (New-Object -TypeName System.Drawing.Size -ArgumentList @([System.Int32]177,[System.Int32]45))
$trackBar1.TabIndex = [System.Int32]2
$trackBar1.add_Scroll($trackBar1_Scroll)
#
#btnProcess
#
$btnProcess.BackColor = [System.Drawing.Color]::LimeGreen
$btnProcess.FlatAppearance.BorderColor = [System.Drawing.Color]::White
$btnProcess.FlatAppearance.MouseDownBackColor = [System.Drawing.Color]::White
$btnProcess.FlatAppearance.MouseOverBackColor = [System.Drawing.Color]::White
$btnProcess.FlatStyle = [System.Windows.Forms.FlatStyle]::Popup
$btnProcess.Font = (New-Object -TypeName System.Drawing.Font -ArgumentList @([System.String]'Microsoft Sans Serif',[System.Single]10,[System.Drawing.FontStyle]::Bold,[System.Drawing.GraphicsUnit]::Point,([System.Byte][System.Byte]0)))
$btnProcess.ForeColor = [System.Drawing.SystemColors]::ControlText
$btnProcess.Location = (New-Object -TypeName System.Drawing.Point -ArgumentList @([System.Int32]17,[System.Int32]350))
$btnProcess.Name = [System.String]'btnProcess'
$btnProcess.Size = (New-Object -TypeName System.Drawing.Size -ArgumentList @([System.Int32]196,[System.Int32]25))
$btnProcess.TabIndex = [System.Int32]16
$btnProcess.Text = [System.String]'Process Servers'
$btnProcess.UseVisualStyleBackColor = $false
$btnProcess.add_Click($btnProcess_Click)
#
#button3
#
$button3.BackColor = [System.Drawing.Color]::LimeGreen
$button3.FlatStyle = [System.Windows.Forms.FlatStyle]::Popup
$button3.Font = (New-Object -TypeName System.Drawing.Font -ArgumentList @([System.String]'Microsoft Sans Serif',[System.Single]10,[System.Drawing.FontStyle]::Bold,[System.Drawing.GraphicsUnit]::Point,([System.Byte][System.Byte]0)))
$button3.ForeColor = [System.Drawing.SystemColors]::ControlText
$button3.Location = (New-Object -TypeName System.Drawing.Point -ArgumentList @([System.Int32]224,[System.Int32]276))
$button3.Name = [System.String]'button3'
$button3.Size = (New-Object -TypeName System.Drawing.Size -ArgumentList @([System.Int32]190,[System.Int32]25))
$button3.TabIndex = [System.Int32]15
$button3.Text = [System.String]'Change VCenter'
$button3.UseVisualStyleBackColor = $false
$button3.add_Click($button3_Click)
#
#groupBox2
#
$groupBox2.Controls.Add($RBUpgrade)
$groupBox2.Controls.Add($RBQueries)
$groupBox2.Controls.Add($RBChange)
$groupBox2.Controls.Add($RBUtilities)
$groupBox2.Controls.Add($GBUpgrade)
$groupBox2.Controls.Add($GBUtilities)
$groupBox2.Controls.Add($GBQueries)
$groupBox2.Controls.Add($GBChange)
$groupBox2.Font = (New-Object -TypeName System.Drawing.Font -ArgumentList @([System.String]'Microsoft Sans Serif',[System.Single]8.25,[System.Drawing.FontStyle]::Bold,[System.Drawing.GraphicsUnit]::Point,([System.Byte][System.Byte]0)))
$groupBox2.Location = (New-Object -TypeName System.Drawing.Point -ArgumentList @([System.Int32]15,[System.Int32]415))
$groupBox2.Name = [System.String]'groupBox2'
$groupBox2.Size = (New-Object -TypeName System.Drawing.Size -ArgumentList @([System.Int32]398,[System.Int32]494))
$groupBox2.TabIndex = [System.Int32]1
$groupBox2.TabStop = $false
$groupBox2.Text = [System.String]'Options:'
$groupBox2.add_Enter($groupBox2_Enter)
#
#RBUpgrade
#
$RBUpgrade.AutoSize = $true
$RBUpgrade.FlatStyle = [System.Windows.Forms.FlatStyle]::Popup
$RBUpgrade.ForeColor = [System.Drawing.SystemColors]::HotTrack
$RBUpgrade.Location = (New-Object -TypeName System.Drawing.Point -ArgumentList @([System.Int32]202,[System.Int32]273))
$RBUpgrade.Name = [System.String]'RBUpgrade'
$RBUpgrade.Size = (New-Object -TypeName System.Drawing.Size -ArgumentList @([System.Int32]117,[System.Int32]17))
$RBUpgrade.TabIndex = [System.Int32]17
$RBUpgrade.TabStop = $true
$RBUpgrade.Text = [System.String]'Click to activate'
$RBUpgrade.UseVisualStyleBackColor = $true
$RBUpgrade.add_CheckedChanged($RBUpgrade_CheckedChanged)
#
#RBQueries
#
$RBQueries.AutoSize = $true
$RBQueries.FlatStyle = [System.Windows.Forms.FlatStyle]::Popup
$RBQueries.ForeColor = [System.Drawing.SystemColors]::HotTrack
$RBQueries.Location = (New-Object -TypeName System.Drawing.Point -ArgumentList @([System.Int32]9,[System.Int32]44))
$RBQueries.Name = [System.String]'RBQueries'
$RBQueries.Size = (New-Object -TypeName System.Drawing.Size -ArgumentList @([System.Int32]117,[System.Int32]17))
$RBQueries.TabIndex = [System.Int32]16
$RBQueries.TabStop = $true
$RBQueries.Text = [System.String]'Click to activate'
$RBQueries.UseVisualStyleBackColor = $true
$RBQueries.add_CheckedChanged($RBQueries_CheckedChanged)
#
#RBChange
#
$RBChange.AutoSize = $true
$RBChange.FlatStyle = [System.Windows.Forms.FlatStyle]::Popup
$RBChange.ForeColor = [System.Drawing.SystemColors]::HotTrack
$RBChange.Location = (New-Object -TypeName System.Drawing.Point -ArgumentList @([System.Int32]202,[System.Int32]44))
$RBChange.Name = [System.String]'RBChange'
$RBChange.Size = (New-Object -TypeName System.Drawing.Size -ArgumentList @([System.Int32]117,[System.Int32]17))
$RBChange.TabIndex = [System.Int32]15
$RBChange.TabStop = $true
$RBChange.Text = [System.String]'Click to activate'
$RBChange.UseVisualStyleBackColor = $true
$RBChange.add_CheckedChanged($RBChange_CheckedChanged)
#
#RBUtilities
#
$RBUtilities.AutoSize = $true
$RBUtilities.FlatStyle = [System.Windows.Forms.FlatStyle]::Popup
$RBUtilities.ForeColor = [System.Drawing.SystemColors]::HotTrack
$RBUtilities.Location = (New-Object -TypeName System.Drawing.Point -ArgumentList @([System.Int32]9,[System.Int32]273))
$RBUtilities.Name = [System.String]'RBUtilities'
$RBUtilities.Size = (New-Object -TypeName System.Drawing.Size -ArgumentList @([System.Int32]117,[System.Int32]17))
$RBUtilities.TabIndex = [System.Int32]12
$RBUtilities.TabStop = $true
$RBUtilities.Text = [System.String]'Click to activate'
$RBUtilities.UseVisualStyleBackColor = $true
$RBUtilities.add_CheckedChanged($RBUtilities_CheckedChanged)
#
#GBUpgrade
#
$GBUpgrade.Controls.Add($BtnAbort)
$GBUpgrade.Controls.Add($button4)
$GBUpgrade.FlatStyle = [System.Windows.Forms.FlatStyle]::Popup
$GBUpgrade.Font = (New-Object -TypeName System.Drawing.Font -ArgumentList @([System.String]'Microsoft Sans Serif',[System.Single]8.25,[System.Drawing.FontStyle]::Bold,[System.Drawing.GraphicsUnit]::Point,([System.Byte][System.Byte]0)))
$GBUpgrade.Location = (New-Object -TypeName System.Drawing.Point -ArgumentList @([System.Int32]202,[System.Int32]299))
$GBUpgrade.Name = [System.String]'GBUpgrade'
$GBUpgrade.Size = (New-Object -TypeName System.Drawing.Size -ArgumentList @([System.Int32]188,[System.Int32]149))
$GBUpgrade.TabIndex = [System.Int32]11
$GBUpgrade.TabStop = $false
$GBUpgrade.Text = [System.String]'Upgrade'
#
#BtnAbort
#
$BtnAbort.FlatStyle = [System.Windows.Forms.FlatStyle]::Popup
$BtnAbort.ForeColor = [System.Drawing.Color]::Red
$BtnAbort.Location = (New-Object -TypeName System.Drawing.Point -ArgumentList @([System.Int32]8,[System.Int32]120))
$BtnAbort.Name = [System.String]'BtnAbort'
$BtnAbort.Size = (New-Object -TypeName System.Drawing.Size -ArgumentList @([System.Int32]171,[System.Int32]23))
$BtnAbort.TabIndex = [System.Int32]14
$BtnAbort.Text = [System.String]'!! Abort !!'
$BtnAbort.UseVisualStyleBackColor = $true
$BtnAbort.add_Click($BtnAbort_Click)
#
#button4
#
$button4.FlatStyle = [System.Windows.Forms.FlatStyle]::Popup
$button4.Location = (New-Object -TypeName System.Drawing.Point -ArgumentList @([System.Int32]11,[System.Int32]60))
$button4.Name = [System.String]'button4'
$button4.Size = (New-Object -TypeName System.Drawing.Size -ArgumentList @([System.Int32]170,[System.Int32]54))
$button4.TabIndex = [System.Int32]13
$button4.Text = [System.String]'Upgrade Tools and Hardware on the Servers listed in the textbox.
'
$button4.UseVisualStyleBackColor = $true
$button4.add_Click($button4_Click)
$button4.add_MouseHover($button4_MouseHover)
#
#GBUtilities
#
$GBUtilities.Controls.Add($RBrdp)
$GBUtilities.Controls.Add($rbping)
$GBUtilities.FlatStyle = [System.Windows.Forms.FlatStyle]::Popup
$GBUtilities.Font = (New-Object -TypeName System.Drawing.Font -ArgumentList @([System.String]'Microsoft Sans Serif',[System.Single]8.25,[System.Drawing.FontStyle]::Bold,[System.Drawing.GraphicsUnit]::Point,([System.Byte][System.Byte]0)))
$GBUtilities.Location = (New-Object -TypeName System.Drawing.Point -ArgumentList @([System.Int32]9,[System.Int32]299))
$GBUtilities.Name = [System.String]'GBUtilities'
$GBUtilities.Size = (New-Object -TypeName System.Drawing.Size -ArgumentList @([System.Int32]180,[System.Int32]149))
$GBUtilities.TabIndex = [System.Int32]3
$GBUtilities.TabStop = $false
$GBUtilities.Text = [System.String]'Utilities'
#
#RBrdp
#
$RBrdp.AutoSize = $true
$RBrdp.FlatAppearance.CheckedBackColor = [System.Drawing.SystemColors]::ControlLightLight
$RBrdp.FlatAppearance.MouseOverBackColor = [System.Drawing.Color]::White
$RBrdp.FlatStyle = [System.Windows.Forms.FlatStyle]::Popup
$RBrdp.Font = (New-Object -TypeName System.Drawing.Font -ArgumentList @([System.String]'Microsoft Sans Serif',[System.Single]8.25,[System.Drawing.FontStyle]::Regular,[System.Drawing.GraphicsUnit]::Point,([System.Byte][System.Byte]0)))
$RBrdp.Location = (New-Object -TypeName System.Drawing.Point -ArgumentList @([System.Int32]6,[System.Int32]42))
$RBrdp.Name = [System.String]'RBrdp'
$RBrdp.Size = (New-Object -TypeName System.Drawing.Size -ArgumentList @([System.Int32]71,[System.Int32]17))
$RBrdp.TabIndex = [System.Int32]1
$RBrdp.TabStop = $true
$RBrdp.Text = [System.String]'RDP Test'
$RBrdp.UseVisualStyleBackColor = $true
#
#rbping
#
$rbping.AutoSize = $true
$rbping.FlatStyle = [System.Windows.Forms.FlatStyle]::Popup
$rbping.Font = (New-Object -TypeName System.Drawing.Font -ArgumentList @([System.String]'Microsoft Sans Serif',[System.Single]8.25,[System.Drawing.FontStyle]::Regular,[System.Drawing.GraphicsUnit]::Point,([System.Byte][System.Byte]0)))
$rbping.Location = (New-Object -TypeName System.Drawing.Point -ArgumentList @([System.Int32]6,[System.Int32]19))
$rbping.Name = [System.String]'rbping'
$rbping.Size = (New-Object -TypeName System.Drawing.Size -ArgumentList @([System.Int32]45,[System.Int32]17))
$rbping.TabIndex = [System.Int32]0
$rbping.TabStop = $true
$rbping.Text = [System.String]'Ping'
$rbping.UseVisualStyleBackColor = $true
#
#GBQueries
#
$GBQueries.Controls.Add($radioButton10)
$GBQueries.Controls.Add($RBNicinfo)
$GBQueries.Controls.Add($BTNgetvmHWversion)
$GBQueries.Controls.Add($rbpowerstate)
$GBQueries.Controls.Add($RBToolsver)
$GBQueries.Controls.Add($RBCurrentHost)
$GBQueries.FlatStyle = [System.Windows.Forms.FlatStyle]::Popup
$GBQueries.Font = (New-Object -TypeName System.Drawing.Font -ArgumentList @([System.String]'Microsoft Sans Serif',[System.Single]8.25,[System.Drawing.FontStyle]::Bold,[System.Drawing.GraphicsUnit]::Point,([System.Byte][System.Byte]0)))
$GBQueries.Location = (New-Object -TypeName System.Drawing.Point -ArgumentList @([System.Int32]9,[System.Int32]67))
$GBQueries.Name = [System.String]'GBQueries'
$GBQueries.Size = (New-Object -TypeName System.Drawing.Size -ArgumentList @([System.Int32]180,[System.Int32]165))
$GBQueries.TabIndex = [System.Int32]2
$GBQueries.TabStop = $false
$GBQueries.Text = [System.String]'Queries:'
#
#radioButton10
#
$radioButton10.AutoSize = $true
$radioButton10.Font = (New-Object -TypeName System.Drawing.Font -ArgumentList @([System.String]'Microsoft Sans Serif',[System.Single]8.25,[System.Drawing.FontStyle]::Regular,[System.Drawing.GraphicsUnit]::Point,([System.Byte][System.Byte]0)))
$radioButton10.Location = (New-Object -TypeName System.Drawing.Point -ArgumentList @([System.Int32]25,[System.Int32]169))
$radioButton10.Name = [System.String]'radioButton10'
$radioButton10.Size = (New-Object -TypeName System.Drawing.Size -ArgumentList @([System.Int32]128,[System.Int32]17))
$radioButton10.TabIndex = [System.Int32]9
$radioButton10.TabStop = $true
$radioButton10.Text = [System.String]'Get VM MAC Address'
$radioButton10.UseVisualStyleBackColor = $true
#
#RBNicinfo
#
$RBNicinfo.AutoSize = $true
$RBNicinfo.FlatStyle = [System.Windows.Forms.FlatStyle]::Popup
$RBNicinfo.Font = (New-Object -TypeName System.Drawing.Font -ArgumentList @([System.String]'Microsoft Sans Serif',[System.Single]8.25,[System.Drawing.FontStyle]::Regular,[System.Drawing.GraphicsUnit]::Point,([System.Byte][System.Byte]0)))
$RBNicinfo.Location = (New-Object -TypeName System.Drawing.Point -ArgumentList @([System.Int32]6,[System.Int32]112))
$RBNicinfo.Name = [System.String]'RBNicinfo'
$RBNicinfo.Size = (New-Object -TypeName System.Drawing.Size -ArgumentList @([System.Int32]97,[System.Int32]17))
$RBNicinfo.TabIndex = [System.Int32]8
$RBNicinfo.TabStop = $true
$RBNicinfo.Text = [System.String]'NIC Information'
$RBNicinfo.UseVisualStyleBackColor = $true
#
#BTNgetvmHWversion
#
$BTNgetvmHWversion.AutoSize = $true
$BTNgetvmHWversion.FlatStyle = [System.Windows.Forms.FlatStyle]::Popup
$BTNgetvmHWversion.Font = (New-Object -TypeName System.Drawing.Font -ArgumentList @([System.String]'Microsoft Sans Serif',[System.Single]8.25,[System.Drawing.FontStyle]::Regular,[System.Drawing.GraphicsUnit]::Point,([System.Byte][System.Byte]0)))
$BTNgetvmHWversion.Location = (New-Object -TypeName System.Drawing.Point -ArgumentList @([System.Int32]6,[System.Int32]20))
$BTNgetvmHWversion.Name = [System.String]'BTNgetvmHWversion'
$BTNgetvmHWversion.Size = (New-Object -TypeName System.Drawing.Size -ArgumentList @([System.Int32]147,[System.Int32]17))
$BTNgetvmHWversion.TabIndex = [System.Int32]0
$BTNgetvmHWversion.TabStop = $true
$BTNgetvmHWversion.Text = [System.String]'Get VM Hardware Version'
$BTNgetvmHWversion.UseVisualStyleBackColor = $true
#
#rbpowerstate
#
$rbpowerstate.AutoSize = $true
$rbpowerstate.FlatStyle = [System.Windows.Forms.FlatStyle]::Popup
$rbpowerstate.Font = (New-Object -TypeName System.Drawing.Font -ArgumentList @([System.String]'Microsoft Sans Serif',[System.Single]8.25,[System.Drawing.FontStyle]::Regular,[System.Drawing.GraphicsUnit]::Point,([System.Byte][System.Byte]0)))
$rbpowerstate.Location = (New-Object -TypeName System.Drawing.Point -ArgumentList @([System.Int32]6,[System.Int32]89))
$rbpowerstate.Name = [System.String]'rbpowerstate'
$rbpowerstate.Size = (New-Object -TypeName System.Drawing.Size -ArgumentList @([System.Int32]121,[System.Int32]17))
$rbpowerstate.TabIndex = [System.Int32]6
$rbpowerstate.TabStop = $true
$rbpowerstate.Text = [System.String]'Get VM Power State'
$rbpowerstate.UseVisualStyleBackColor = $true
#
#RBToolsver
#
$RBToolsver.AutoSize = $true
$RBToolsver.FlatStyle = [System.Windows.Forms.FlatStyle]::Popup
$RBToolsver.Font = (New-Object -TypeName System.Drawing.Font -ArgumentList @([System.String]'Microsoft Sans Serif',[System.Single]8.25,[System.Drawing.FontStyle]::Regular,[System.Drawing.GraphicsUnit]::Point,([System.Byte][System.Byte]0)))
$RBToolsver.Location = (New-Object -TypeName System.Drawing.Point -ArgumentList @([System.Int32]6,[System.Int32]43))
$RBToolsver.Name = [System.String]'RBToolsver'
$RBToolsver.Size = (New-Object -TypeName System.Drawing.Size -ArgumentList @([System.Int32]107,[System.Int32]17))
$RBToolsver.TabIndex = [System.Int32]1
$RBToolsver.TabStop = $true
$RBToolsver.Text = [System.String]'Get VMTools Info'
$RBToolsver.UseVisualStyleBackColor = $true
$RBToolsver.add_CheckedChanged($RBToolsver_CheckedChanged)
#
#RBCurrentHost
#
$RBCurrentHost.AutoSize = $true
$RBCurrentHost.FlatStyle = [System.Windows.Forms.FlatStyle]::Popup
$RBCurrentHost.Font = (New-Object -TypeName System.Drawing.Font -ArgumentList @([System.String]'Microsoft Sans Serif',[System.Single]8.25,[System.Drawing.FontStyle]::Regular,[System.Drawing.GraphicsUnit]::Point,([System.Byte][System.Byte]0)))
$RBCurrentHost.Location = (New-Object -TypeName System.Drawing.Point -ArgumentList @([System.Int32]6,[System.Int32]66))
$RBCurrentHost.Name = [System.String]'RBCurrentHost'
$RBCurrentHost.Size = (New-Object -TypeName System.Drawing.Size -ArgumentList @([System.Int32]122,[System.Int32]17))
$RBCurrentHost.TabIndex = [System.Int32]5
$RBCurrentHost.TabStop = $true
$RBCurrentHost.Text = [System.String]'Get VM Current Host'
$RBCurrentHost.UseVisualStyleBackColor = $true
#
#GBChange
#
$GBChange.Controls.Add($RBRebootHard)
$GBChange.Controls.Add($RBSTopVMHard)
$GBChange.Controls.Add($RBRebootOS)
$GBChange.Controls.Add($RBStartVM)
$GBChange.Controls.Add($RBStopVMOS)
$GBChange.FlatStyle = [System.Windows.Forms.FlatStyle]::Popup
$GBChange.Font = (New-Object -TypeName System.Drawing.Font -ArgumentList @([System.String]'Microsoft Sans Serif',[System.Single]8.25,[System.Drawing.FontStyle]::Bold,[System.Drawing.GraphicsUnit]::Point,([System.Byte][System.Byte]0)))
$GBChange.Location = (New-Object -TypeName System.Drawing.Point -ArgumentList @([System.Int32]204,[System.Int32]67))
$GBChange.Name = [System.String]'GBChange'
$GBChange.Size = (New-Object -TypeName System.Drawing.Size -ArgumentList @([System.Int32]188,[System.Int32]165))
$GBChange.TabIndex = [System.Int32]3
$GBChange.TabStop = $false
$GBChange.Text = [System.String]'Change'
$GBChange.add_Enter($GBChange_Enter)
#
#RBRebootHard
#
$RBRebootHard.AutoSize = $true
$RBRebootHard.FlatStyle = [System.Windows.Forms.FlatStyle]::Popup
$RBRebootHard.Font = (New-Object -TypeName System.Drawing.Font -ArgumentList @([System.String]'Microsoft Sans Serif',[System.Single]8.25,[System.Drawing.FontStyle]::Regular,[System.Drawing.GraphicsUnit]::Point,([System.Byte][System.Byte]0)))
$RBRebootHard.Location = (New-Object -TypeName System.Drawing.Point -ArgumentList @([System.Int32]5,[System.Int32]112))
$RBRebootHard.Name = [System.String]'RBRebootHard'
$RBRebootHard.Size = (New-Object -TypeName System.Drawing.Size -ArgumentList @([System.Int32]179,[System.Int32]17))
$RBRebootHard.TabIndex = [System.Int32]9
$RBRebootHard.TabStop = $true
$RBRebootHard.Text = [System.String]'Restart VM (Wintel Hard Restart)'
$RBRebootHard.UseVisualStyleBackColor = $true
$RBRebootHard.add_CheckedChanged($radioButton1_CheckedChanged)
#
#RBSTopVMHard
#
$RBSTopVMHard.AutoSize = $true
$RBSTopVMHard.FlatStyle = [System.Windows.Forms.FlatStyle]::Popup
$RBSTopVMHard.Font = (New-Object -TypeName System.Drawing.Font -ArgumentList @([System.String]'Microsoft Sans Serif',[System.Single]8.25,[System.Drawing.FontStyle]::Regular,[System.Drawing.GraphicsUnit]::Point,([System.Byte][System.Byte]0)))
$RBSTopVMHard.Location = (New-Object -TypeName System.Drawing.Point -ArgumentList @([System.Int32]5,[System.Int32]66))
$RBSTopVMHard.Name = [System.String]'RBSTopVMHard'
$RBSTopVMHard.Size = (New-Object -TypeName System.Drawing.Size -ArgumentList @([System.Int32]99,[System.Int32]17))
$RBSTopVMHard.TabIndex = [System.Int32]7
$RBSTopVMHard.TabStop = $true
$RBSTopVMHard.Text = [System.String]'Stop VM (Linux)'
$RBSTopVMHard.UseVisualStyleBackColor = $true
#
#RBRebootOS
#
$RBRebootOS.AutoSize = $true
$RBRebootOS.FlatStyle = [System.Windows.Forms.FlatStyle]::Popup
$RBRebootOS.Font = (New-Object -TypeName System.Drawing.Font -ArgumentList @([System.String]'Microsoft Sans Serif',[System.Single]8.25,[System.Drawing.FontStyle]::Regular,[System.Drawing.GraphicsUnit]::Point,([System.Byte][System.Byte]0)))
$RBRebootOS.Location = (New-Object -TypeName System.Drawing.Point -ArgumentList @([System.Int32]5,[System.Int32]89))
$RBRebootOS.Name = [System.String]'RBRebootOS'
$RBRebootOS.Size = (New-Object -TypeName System.Drawing.Size -ArgumentList @([System.Int32]117,[System.Int32]17))
$RBRebootOS.TabIndex = [System.Int32]4
$RBRebootOS.TabStop = $true
$RBRebootOS.Text = [System.String]'Reboot VM (Wintel)'
$RBRebootOS.UseVisualStyleBackColor = $true
#
#RBStartVM
#
$RBStartVM.AutoSize = $true
$RBStartVM.FlatStyle = [System.Windows.Forms.FlatStyle]::Popup
$RBStartVM.Font = (New-Object -TypeName System.Drawing.Font -ArgumentList @([System.String]'Microsoft Sans Serif',[System.Single]8.25,[System.Drawing.FontStyle]::Regular,[System.Drawing.GraphicsUnit]::Point,([System.Byte][System.Byte]0)))
$RBStartVM.Location = (New-Object -TypeName System.Drawing.Point -ArgumentList @([System.Int32]5,[System.Int32]20))
$RBStartVM.Name = [System.String]'RBStartVM'
$RBStartVM.Size = (New-Object -TypeName System.Drawing.Size -ArgumentList @([System.Int32]134,[System.Int32]17))
$RBStartVM.TabIndex = [System.Int32]8
$RBStartVM.Text = [System.String]'Start VM (Wintel/Linux)'
$RBStartVM.UseVisualStyleBackColor = $true
#
#RBStopVMOS
#
$RBStopVMOS.AutoSize = $true
$RBStopVMOS.FlatStyle = [System.Windows.Forms.FlatStyle]::Popup
$RBStopVMOS.Font = (New-Object -TypeName System.Drawing.Font -ArgumentList @([System.String]'Microsoft Sans Serif',[System.Single]8.25,[System.Drawing.FontStyle]::Regular,[System.Drawing.GraphicsUnit]::Point,([System.Byte][System.Byte]0)))
$RBStopVMOS.Location = (New-Object -TypeName System.Drawing.Point -ArgumentList @([System.Int32]5,[System.Int32]43))
$RBStopVMOS.Name = [System.String]'RBStopVMOS'
$RBStopVMOS.Size = (New-Object -TypeName System.Drawing.Size -ArgumentList @([System.Int32]151,[System.Int32]17))
$RBStopVMOS.TabIndex = [System.Int32]2
$RBStopVMOS.TabStop = $true
$RBStopVMOS.Text = [System.String]'Stop VM (Wintel Soft Stop)'
$RBStopVMOS.UseVisualStyleBackColor = $true
#
#textBox1
#
$textBox1.BorderStyle = [System.Windows.Forms.BorderStyle]::FixedSingle
$textBox1.Location = (New-Object -TypeName System.Drawing.Point -ArgumentList @([System.Int32]15,[System.Int32]31))
$textBox1.Multiline = $true
$textBox1.Name = [System.String]'textBox1'
$textBox1.ScrollBars = [System.Windows.Forms.ScrollBars]::Both
$textBox1.Size = (New-Object -TypeName System.Drawing.Size -ArgumentList @([System.Int32]196,[System.Int32]313))
$textBox1.TabIndex = [System.Int32]0
$textBox1.add_TextChanged($textBox1_TextChanged)
#
#label4
#
$label4.AutoSize = $true
$label4.Font = (New-Object -TypeName System.Drawing.Font -ArgumentList @([System.String]'Microsoft Sans Serif',[System.Single]11,[System.Drawing.FontStyle]::Bold,[System.Drawing.GraphicsUnit]::Point,([System.Byte][System.Byte]0)))
$label4.Location = (New-Object -TypeName System.Drawing.Point -ArgumentList @([System.Int32]443,[System.Int32]19))
$label4.Name = [System.String]'label4'
$label4.Size = (New-Object -TypeName System.Drawing.Size -ArgumentList @([System.Int32]131,[System.Int32]18))
$label4.TabIndex = [System.Int32]18
$label4.Text = [System.String]'Console Output:'
#
#textBox2
#
$textBox2.BackColor = [System.Drawing.Color]::RoyalBlue
$textBox2.BorderStyle = [System.Windows.Forms.BorderStyle]::FixedSingle
$textBox2.Font = (New-Object -TypeName System.Drawing.Font -ArgumentList @([System.String]'Microsoft Sans Serif',[System.Single]11,[System.Drawing.FontStyle]::Regular,[System.Drawing.GraphicsUnit]::Point,([System.Byte][System.Byte]0)))
$textBox2.ForeColor = [System.Drawing.SystemColors]::Window
$textBox2.Location = (New-Object -TypeName System.Drawing.Point -ArgumentList @([System.Int32]446,[System.Int32]43))
$textBox2.Multiline = $true
$textBox2.Name = [System.String]'textBox2'
$textBox2.ScrollBars = [System.Windows.Forms.ScrollBars]::Both
$textBox2.Size = (New-Object -TypeName System.Drawing.Size -ArgumentList @([System.Int32]804,[System.Int32]261))
$textBox2.TabIndex = [System.Int32]19
$textBox2.WordWrap = $false
#
#lblprogress
#
$lblprogress.AutoSize = $true
$lblprogress.Font = (New-Object -TypeName System.Drawing.Font -ArgumentList @([System.String]'Microsoft Sans Serif',[System.Single]11,[System.Drawing.FontStyle]::Bold,[System.Drawing.GraphicsUnit]::Point,([System.Byte][System.Byte]0)))
$lblprogress.Location = (New-Object -TypeName System.Drawing.Point -ArgumentList @([System.Int32]656,[System.Int32]19))
$lblprogress.Name = [System.String]'lblprogress'
$lblprogress.Size = (New-Object -TypeName System.Drawing.Size -ArgumentList @([System.Int32]82,[System.Int32]18))
$lblprogress.TabIndex = [System.Int32]20
$lblprogress.Text = [System.String]'Progress:'
#
#label17
#
$label17.AutoSize = $true
$label17.Location = (New-Object -TypeName System.Drawing.Point -ArgumentList @([System.Int32]1150,[System.Int32]914))
$label17.Name = [System.String]'label17'
$label17.Size = (New-Object -TypeName System.Drawing.Size -ArgumentList @([System.Int32]100,[System.Int32]13))
$label17.TabIndex = [System.Int32]22
$label17.Text = [System.String]'Author: Tom Wilson'
#
#label9
#
$label9.AutoSize = $true
$label9.Location = (New-Object -TypeName System.Drawing.Point -ArgumentList @([System.Int32]6,[System.Int32]19))
$label9.Name = [System.String]'label9'
$label9.Size = (New-Object -TypeName System.Drawing.Size -ArgumentList @([System.Int32]153,[System.Int32]18))
$label9.TabIndex = [System.Int32]3
$label9.Text = [System.String]'Number of Threads'
$label9.add_Click($label9_Click)
#
#groupBox4
#
$groupBox4.Controls.Add($ARCHIVECOMPLETED)
$groupBox4.Controls.Add($BTNCounterUpdate)
$groupBox4.Controls.Add($tbcompleted)
$groupBox4.Controls.Add($label14)
$groupBox4.Controls.Add($tbrunning)
$groupBox4.Controls.Add($label16)
$groupBox4.Controls.Add($tbtotals)
$groupBox4.Controls.Add($label15)
$groupBox4.Location = (New-Object -TypeName System.Drawing.Point -ArgumentList @([System.Int32]294,[System.Int32]382))
$groupBox4.Name = [System.String]'groupBox4'
$groupBox4.Size = (New-Object -TypeName System.Drawing.Size -ArgumentList @([System.Int32]437,[System.Int32]152))
$groupBox4.TabIndex = [System.Int32]38
$groupBox4.TabStop = $false
$groupBox4.Text = [System.String]'Thread status:'
#
#ARCHIVECOMPLETED
#
$ARCHIVECOMPLETED.FlatStyle = [System.Windows.Forms.FlatStyle]::Popup
$ARCHIVECOMPLETED.Location = (New-Object -TypeName System.Drawing.Point -ArgumentList @([System.Int32]258,[System.Int32]108))
$ARCHIVECOMPLETED.Name = [System.String]'ARCHIVECOMPLETED'
$ARCHIVECOMPLETED.Size = (New-Object -TypeName System.Drawing.Size -ArgumentList @([System.Int32]163,[System.Int32]23))
$ARCHIVECOMPLETED.TabIndex = [System.Int32]40
$ARCHIVECOMPLETED.Text = [System.String]'<--- Archive'
$ARCHIVECOMPLETED.UseVisualStyleBackColor = $true
$ARCHIVECOMPLETED.add_Click($ARCHIVECOMPLETED_Click)
#
#BTNCounterUpdate
#
$BTNCounterUpdate.FlatStyle = [System.Windows.Forms.FlatStyle]::Popup
$BTNCounterUpdate.Location = (New-Object -TypeName System.Drawing.Point -ArgumentList @([System.Int32]258,[System.Int32]41))
$BTNCounterUpdate.Name = [System.String]'BTNCounterUpdate'
$BTNCounterUpdate.Size = (New-Object -TypeName System.Drawing.Size -ArgumentList @([System.Int32]163,[System.Int32]23))
$BTNCounterUpdate.TabIndex = [System.Int32]39
$BTNCounterUpdate.Text = [System.String]'<--- Update Counters'
$BTNCounterUpdate.UseVisualStyleBackColor = $true
$BTNCounterUpdate.add_Click($BTNCounterUpdate_Click)
#
#tbcompleted
#
$tbcompleted.BackColor = [System.Drawing.Color]::Chartreuse
$tbcompleted.ForeColor = [System.Drawing.SystemColors]::InfoText
$tbcompleted.Location = (New-Object -TypeName System.Drawing.Point -ArgumentList @([System.Int32]142,[System.Int32]110))
$tbcompleted.Name = [System.String]'tbcompleted'
$tbcompleted.Size = (New-Object -TypeName System.Drawing.Size -ArgumentList @([System.Int32]82,[System.Int32]20))
$tbcompleted.TabIndex = [System.Int32]37
$tbcompleted.TextAlign = [System.Windows.Forms.HorizontalAlignment]::Right
#
#label14
#
$label14.AutoSize = $true
$label14.Font = (New-Object -TypeName System.Drawing.Font -ArgumentList @([System.String]'Microsoft Sans Serif',[System.Single]13,[System.Drawing.FontStyle]::Bold,[System.Drawing.GraphicsUnit]::Point,([System.Byte][System.Byte]0)))
$label14.Location = (New-Object -TypeName System.Drawing.Point -ArgumentList @([System.Int32]36,[System.Int32]39))
$label14.Name = [System.String]'label14'
$label14.Size = (New-Object -TypeName System.Drawing.Size -ArgumentList @([System.Int32]62,[System.Int32]22))
$label14.TabIndex = [System.Int32]32
$label14.Text = [System.String]'Total:'
#
#tbrunning
#
$tbrunning.BackColor = [System.Drawing.Color]::Khaki
$tbrunning.ForeColor = [System.Drawing.SystemColors]::InfoText
$tbrunning.Location = (New-Object -TypeName System.Drawing.Point -ArgumentList @([System.Int32]142,[System.Int32]76))
$tbrunning.Name = [System.String]'tbrunning'
$tbrunning.Size = (New-Object -TypeName System.Drawing.Size -ArgumentList @([System.Int32]82,[System.Int32]20))
$tbrunning.TabIndex = [System.Int32]35
$tbrunning.TextAlign = [System.Windows.Forms.HorizontalAlignment]::Right
#
#label16
#
$label16.AutoSize = $true
$label16.Font = (New-Object -TypeName System.Drawing.Font -ArgumentList @([System.String]'Microsoft Sans Serif',[System.Single]13,[System.Drawing.FontStyle]::Bold,[System.Drawing.GraphicsUnit]::Point,([System.Byte][System.Byte]0)))
$label16.ForeColor = [System.Drawing.Color]::LimeGreen
$label16.Location = (New-Object -TypeName System.Drawing.Point -ArgumentList @([System.Int32]36,[System.Int32]108))
$label16.Name = [System.String]'label16'
$label16.Size = (New-Object -TypeName System.Drawing.Size -ArgumentList @([System.Int32]100,[System.Int32]22))
$label16.TabIndex = [System.Int32]36
$label16.Text = [System.String]'Complete:'
#
#tbtotals
#
$tbtotals.ForeColor = [System.Drawing.SystemColors]::InfoText
$tbtotals.Location = (New-Object -TypeName System.Drawing.Point -ArgumentList @([System.Int32]142,[System.Int32]41))
$tbtotals.Name = [System.String]'tbtotals'
$tbtotals.Size = (New-Object -TypeName System.Drawing.Size -ArgumentList @([System.Int32]82,[System.Int32]20))
$tbtotals.TabIndex = [System.Int32]33
$tbtotals.TextAlign = [System.Windows.Forms.HorizontalAlignment]::Right
#
#label15
#
$label15.AutoSize = $true
$label15.Font = (New-Object -TypeName System.Drawing.Font -ArgumentList @([System.String]'Microsoft Sans Serif',[System.Single]13,[System.Drawing.FontStyle]::Bold,[System.Drawing.GraphicsUnit]::Point,([System.Byte][System.Byte]0)))
$label15.ForeColor = [System.Drawing.Color]::Orange
$label15.Location = (New-Object -TypeName System.Drawing.Point -ArgumentList @([System.Int32]36,[System.Int32]73))
$label15.Name = [System.String]'label15'
$label15.Size = (New-Object -TypeName System.Drawing.Size -ArgumentList @([System.Int32]90,[System.Int32]22))
$label15.TabIndex = [System.Int32]34
$label15.Text = [System.String]'Running:'
#
#label5
#
$label5.AutoSize = $true
$label5.Location = (New-Object -TypeName System.Drawing.Point -ArgumentList @([System.Int32]337,[System.Int32]206))
$label5.Name = [System.String]'label5'
$label5.Size = (New-Object -TypeName System.Drawing.Size -ArgumentList @([System.Int32]142,[System.Int32]13))
$label5.TabIndex = [System.Int32]12
$label5.Text = [System.String]'Starting/Check RDP -->'
#
#label1
#
$label1.AutoSize = $true
$label1.Location = (New-Object -TypeName System.Drawing.Point -ArgumentList @([System.Int32]112,[System.Int32]23))
$label1.Name = [System.String]'label1'
$label1.Size = (New-Object -TypeName System.Drawing.Size -ArgumentList @([System.Int32]163,[System.Int32]13))
$label1.TabIndex = [System.Int32]8
$label1.Text = [System.String]'Starting VMs (if needed) -->'
#
#label2
#
$label2.AutoSize = $true
$label2.Location = (New-Object -TypeName System.Drawing.Point -ArgumentList @([System.Int32]112,[System.Int32]206))
$label2.Name = [System.String]'label2'
$label2.Size = (New-Object -TypeName System.Drawing.Size -ArgumentList @([System.Int32]163,[System.Int32]13))
$label2.TabIndex = [System.Int32]9
$label2.Text = [System.String]'Performing HW Upgrade -->'
#
#webBrowser4
#
$webBrowser4.AllowNavigation = $false
$webBrowser4.Location = (New-Object -TypeName System.Drawing.Point -ArgumentList @([System.Int32]69,[System.Int32]43))
$webBrowser4.MinimumSize = (New-Object -TypeName System.Drawing.Size -ArgumentList @([System.Int32]20,[System.Int32]20))
$webBrowser4.Name = [System.String]'webBrowser4'
$webBrowser4.Size = (New-Object -TypeName System.Drawing.Size -ArgumentList @([System.Int32]209,[System.Int32]153))
$webBrowser4.TabIndex = [System.Int32]5
$webBrowser4.Url = (New-Object -TypeName System.Uri -ArgumentList @([System.String]'C:\vmtoolsupdater\logs\startingvms',[System.UriKind]::Absolute))
#
#label7
#
$label7.AutoSize = $true
$label7.Location = (New-Object -TypeName System.Drawing.Point -ArgumentList @([System.Int32]337,[System.Int32]24))
$label7.Name = [System.String]'label7'
$label7.Size = (New-Object -TypeName System.Drawing.Size -ArgumentList @([System.Int32]137,[System.Int32]13))
$label7.TabIndex = [System.Int32]14
$label7.Text = [System.String]'Upgrading VMTools -->'
#
#webBrowser2
#
$webBrowser2.Location = (New-Object -TypeName System.Drawing.Point -ArgumentList @([System.Int32]69,[System.Int32]228))
$webBrowser2.MinimumSize = (New-Object -TypeName System.Drawing.Size -ArgumentList @([System.Int32]20,[System.Int32]20))
$webBrowser2.Name = [System.String]'webBrowser2'
$webBrowser2.Size = (New-Object -TypeName System.Drawing.Size -ArgumentList @([System.Int32]209,[System.Int32]143))
$webBrowser2.TabIndex = [System.Int32]3
$webBrowser2.Url = (New-Object -TypeName System.Uri -ArgumentList @([System.String]'C:\vmtoolsupdater\logs\hardwareupgrade',[System.UriKind]::Absolute))
#
#webBrowser1
#
$webBrowser1.Location = (New-Object -TypeName System.Drawing.Point -ArgumentList @([System.Int32]294,[System.Int32]228))
$webBrowser1.MinimumSize = (New-Object -TypeName System.Drawing.Size -ArgumentList @([System.Int32]20,[System.Int32]20))
$webBrowser1.Name = [System.String]'webBrowser1'
$webBrowser1.Size = (New-Object -TypeName System.Drawing.Size -ArgumentList @([System.Int32]209,[System.Int32]141))
$webBrowser1.TabIndex = [System.Int32]2
$webBrowser1.Url = (New-Object -TypeName System.Uri -ArgumentList @([System.String]'C:\vmtoolsupdater\logs\startingRDPcheck',[System.UriKind]::Absolute))
#
#webBrowser3
#
$webBrowser3.Location = (New-Object -TypeName System.Drawing.Point -ArgumentList @([System.Int32]522,[System.Int32]228))
$webBrowser3.MinimumSize = (New-Object -TypeName System.Drawing.Size -ArgumentList @([System.Int32]20,[System.Int32]20))
$webBrowser3.Name = [System.String]'webBrowser3'
$webBrowser3.Size = (New-Object -TypeName System.Drawing.Size -ArgumentList @([System.Int32]209,[System.Int32]143))
$webBrowser3.TabIndex = [System.Int32]4
$webBrowser3.Url = (New-Object -TypeName System.Uri -ArgumentList @([System.String]'C:\vmtoolsupdater\logs\complete',[System.UriKind]::Absolute))
$webBrowser3.add_DocumentCompleted($webBrowser3_DocumentCompleted)
#
#label8
#
$label8.AutoSize = $true
$label8.Location = (New-Object -TypeName System.Drawing.Point -ArgumentList @([System.Int32]565,[System.Int32]206))
$label8.Name = [System.String]'label8'
$label8.Size = (New-Object -TypeName System.Drawing.Size -ArgumentList @([System.Int32]66,[System.Int32]13))
$label8.TabIndex = [System.Int32]15
$label8.Text = [System.String]'Completed'
#
#webBrowser5
#
$webBrowser5.Location = (New-Object -TypeName System.Drawing.Point -ArgumentList @([System.Int32]294,[System.Int32]44))
$webBrowser5.MinimumSize = (New-Object -TypeName System.Drawing.Size -ArgumentList @([System.Int32]20,[System.Int32]20))
$webBrowser5.Name = [System.String]'webBrowser5'
$webBrowser5.Size = (New-Object -TypeName System.Drawing.Size -ArgumentList @([System.Int32]209,[System.Int32]152))
$webBrowser5.TabIndex = [System.Int32]6
$webBrowser5.Url = (New-Object -TypeName System.Uri -ArgumentList @([System.String]'C:\vmtoolsupdater\logs\toolsupgrade',[System.UriKind]::Absolute))
#
#tbstart
#
$tbstart.BackColor = [System.Drawing.Color]::Khaki
$tbstart.ForeColor = [System.Drawing.SystemColors]::InfoText
$tbstart.Location = (New-Object -TypeName System.Drawing.Point -ArgumentList @([System.Int32]69,[System.Int32]20))
$tbstart.Name = [System.String]'tbstart'
$tbstart.Size = (New-Object -TypeName System.Drawing.Size -ArgumentList @([System.Int32]37,[System.Int32]20))
$tbstart.TabIndex = [System.Int32]23
$tbstart.TextAlign = [System.Windows.Forms.HorizontalAlignment]::Right
#
#webBrowser6
#
$webBrowser6.Location = (New-Object -TypeName System.Drawing.Point -ArgumentList @([System.Int32]522,[System.Int32]44))
$webBrowser6.MinimumSize = (New-Object -TypeName System.Drawing.Size -ArgumentList @([System.Int32]20,[System.Int32]20))
$webBrowser6.Name = [System.String]'webBrowser6'
$webBrowser6.Size = (New-Object -TypeName System.Drawing.Size -ArgumentList @([System.Int32]209,[System.Int32]152))
$webBrowser6.TabIndex = [System.Int32]7
$webBrowser6.Url = (New-Object -TypeName System.Uri -ArgumentList @([System.String]'C:\vmtoolsupdater\logs\stopping',[System.UriKind]::Absolute))
#
#label6
#
$label6.AutoSize = $true
$label6.Location = (New-Object -TypeName System.Drawing.Point -ArgumentList @([System.Int32]565,[System.Int32]24))
$label6.Name = [System.String]'label6'
$label6.Size = (New-Object -TypeName System.Drawing.Size -ArgumentList @([System.Int32]131,[System.Int32]13))
$label6.TabIndex = [System.Int32]13
$label6.Text = [System.String]'Stopping VMGuest -->'
#
#webBrowser7
#
$webBrowser7.Location = (New-Object -TypeName System.Drawing.Point -ArgumentList @([System.Int32]72,[System.Int32]405))
$webBrowser7.MinimumSize = (New-Object -TypeName System.Drawing.Size -ArgumentList @([System.Int32]20,[System.Int32]20))
$webBrowser7.Name = [System.String]'webBrowser7'
$webBrowser7.Size = (New-Object -TypeName System.Drawing.Size -ArgumentList @([System.Int32]209,[System.Int32]126))
$webBrowser7.TabIndex = [System.Int32]16
$webBrowser7.Url = (New-Object -TypeName System.Uri -ArgumentList @([System.String]'C:\vmtoolsupdater\logs\error',[System.UriKind]::Absolute))
#
#label3
#
$label3.AutoSize = $true
$label3.Location = (New-Object -TypeName System.Drawing.Point -ArgumentList @([System.Int32]115,[System.Int32]382))
$label3.Name = [System.String]'label3'
$label3.Size = (New-Object -TypeName System.Drawing.Size -ArgumentList @([System.Int32]34,[System.Int32]13))
$label3.TabIndex = [System.Int32]17
$label3.Text = [System.String]'Error'
$label3.add_Click($label3_Click)
#
#tbhwupgradestart
#
$tbhwupgradestart.BackColor = [System.Drawing.Color]::Khaki
$tbhwupgradestart.ForeColor = [System.Drawing.SystemColors]::InfoText
$tbhwupgradestart.Location = (New-Object -TypeName System.Drawing.Point -ArgumentList @([System.Int32]69,[System.Int32]202))
$tbhwupgradestart.Name = [System.String]'tbhwupgradestart'
$tbhwupgradestart.Size = (New-Object -TypeName System.Drawing.Size -ArgumentList @([System.Int32]37,[System.Int32]20))
$tbhwupgradestart.TabIndex = [System.Int32]26
$tbhwupgradestart.TextAlign = [System.Windows.Forms.HorizontalAlignment]::Right
#
#tbupdatetools
#
$tbupdatetools.BackColor = [System.Drawing.Color]::Khaki
$tbupdatetools.ForeColor = [System.Drawing.SystemColors]::InfoText
$tbupdatetools.Location = (New-Object -TypeName System.Drawing.Point -ArgumentList @([System.Int32]294,[System.Int32]20))
$tbupdatetools.Name = [System.String]'tbupdatetools'
$tbupdatetools.Size = (New-Object -TypeName System.Drawing.Size -ArgumentList @([System.Int32]37,[System.Int32]20))
$tbupdatetools.TabIndex = [System.Int32]27
$tbupdatetools.TextAlign = [System.Windows.Forms.HorizontalAlignment]::Right
#
#tbstartrdp
#
$tbstartrdp.BackColor = [System.Drawing.Color]::Khaki
$tbstartrdp.ForeColor = [System.Drawing.SystemColors]::InfoText
$tbstartrdp.Location = (New-Object -TypeName System.Drawing.Point -ArgumentList @([System.Int32]294,[System.Int32]202))
$tbstartrdp.Name = [System.String]'tbstartrdp'
$tbstartrdp.Size = (New-Object -TypeName System.Drawing.Size -ArgumentList @([System.Int32]37,[System.Int32]20))
$tbstartrdp.TabIndex = [System.Int32]28
$tbstartrdp.TextAlign = [System.Windows.Forms.HorizontalAlignment]::Right
#
#tbstop
#
$tbstop.BackColor = [System.Drawing.Color]::Khaki
$tbstop.ForeColor = [System.Drawing.SystemColors]::InfoText
$tbstop.Location = (New-Object -TypeName System.Drawing.Point -ArgumentList @([System.Int32]522,[System.Int32]20))
$tbstop.Name = [System.String]'tbstop'
$tbstop.Size = (New-Object -TypeName System.Drawing.Size -ArgumentList @([System.Int32]37,[System.Int32]20))
$tbstop.TabIndex = [System.Int32]29
$tbstop.TextAlign = [System.Windows.Forms.HorizontalAlignment]::Right
#
#tbcomplete
#
$tbcomplete.BackColor = [System.Drawing.Color]::Chartreuse
$tbcomplete.ForeColor = [System.Drawing.SystemColors]::InfoText
$tbcomplete.Location = (New-Object -TypeName System.Drawing.Point -ArgumentList @([System.Int32]522,[System.Int32]203))
$tbcomplete.Name = [System.String]'tbcomplete'
$tbcomplete.Size = (New-Object -TypeName System.Drawing.Size -ArgumentList @([System.Int32]37,[System.Int32]20))
$tbcomplete.TabIndex = [System.Int32]30
$tbcomplete.TextAlign = [System.Windows.Forms.HorizontalAlignment]::Right
$tbcomplete.add_TextChanged($textBox10_TextChanged)
#
#tberror
#
$tberror.BackColor = [System.Drawing.Color]::Tomato
$tberror.ForeColor = [System.Drawing.SystemColors]::InfoText
$tberror.Location = (New-Object -TypeName System.Drawing.Point -ArgumentList @([System.Int32]72,[System.Int32]379))
$tberror.Name = [System.String]'tberror'
$tberror.Size = (New-Object -TypeName System.Drawing.Size -ArgumentList @([System.Int32]37,[System.Int32]20))
$tberror.TabIndex = [System.Int32]31
$tberror.TextAlign = [System.Windows.Forms.HorizontalAlignment]::Right
#
#GBThreads
#
$GBThreads.Controls.Add($tberror)
$GBThreads.Controls.Add($tbcomplete)
$GBThreads.Controls.Add($tbstop)
$GBThreads.Controls.Add($tbstartrdp)
$GBThreads.Controls.Add($tbupdatetools)
$GBThreads.Controls.Add($tbhwupgradestart)
$GBThreads.Controls.Add($label3)
$GBThreads.Controls.Add($webBrowser7)
$GBThreads.Controls.Add($label6)
$GBThreads.Controls.Add($webBrowser6)
$GBThreads.Controls.Add($tbstart)
$GBThreads.Controls.Add($webBrowser5)
$GBThreads.Controls.Add($label8)
$GBThreads.Controls.Add($webBrowser3)
$GBThreads.Controls.Add($webBrowser1)
$GBThreads.Controls.Add($webBrowser2)
$GBThreads.Controls.Add($label7)
$GBThreads.Controls.Add($webBrowser4)
$GBThreads.Controls.Add($label2)
$GBThreads.Controls.Add($label1)
$GBThreads.Controls.Add($label5)
$GBThreads.Controls.Add($groupBox4)
$GBThreads.Font = (New-Object -TypeName System.Drawing.Font -ArgumentList @([System.String]'Microsoft Sans Serif',[System.Single]8.25,[System.Drawing.FontStyle]::Bold,[System.Drawing.GraphicsUnit]::Point,([System.Byte][System.Byte]0)))
$GBThreads.Location = (New-Object -TypeName System.Drawing.Point -ArgumentList @([System.Int32]442,[System.Int32]320))
$GBThreads.Name = [System.String]'GBThreads'
$GBThreads.Size = (New-Object -TypeName System.Drawing.Size -ArgumentList @([System.Int32]808,[System.Int32]573))
$GBThreads.TabIndex = [System.Int32]16
$GBThreads.TabStop = $false
$GBThreads.Text = [System.String]'Active Thread Workflow:'
#
#TBThreads
#
$TBThreads.Location = (New-Object -TypeName System.Drawing.Point -ArgumentList @([System.Int32]162,[System.Int32]14))
$TBThreads.Name = [System.String]'TBThreads'
$TBThreads.Size = (New-Object -TypeName System.Drawing.Size -ArgumentList @([System.Int32]32,[System.Int32]20))
$TBThreads.TabIndex = [System.Int32]4
$TBThreads.add_TextChanged($TBThreads_TextChanged)
#
#IntelHostingVMwareUtilityForm
#
$IntelHostingVMwareUtilityForm.AutoSizeMode = [System.Windows.Forms.AutoSizeMode]::GrowAndShrink
$IntelHostingVMwareUtilityForm.BackColor = [System.Drawing.SystemColors]::ControlLight
$IntelHostingVMwareUtilityForm.ClientSize = (New-Object -TypeName System.Drawing.Size -ArgumentList @([System.Int32]1262,[System.Int32]939))
$IntelHostingVMwareUtilityForm.Controls.Add($label17)
$IntelHostingVMwareUtilityForm.Controls.Add($progressBar1)
$IntelHostingVMwareUtilityForm.Controls.Add($lblprogress)
$IntelHostingVMwareUtilityForm.Controls.Add($textBox2)
$IntelHostingVMwareUtilityForm.Controls.Add($label4)
$IntelHostingVMwareUtilityForm.Controls.Add($groupBox1)
$IntelHostingVMwareUtilityForm.Controls.Add($GBThreads)
$IntelHostingVMwareUtilityForm.Name = [System.String]'IntelHostingVMwareUtilityForm'
$IntelHostingVMwareUtilityForm.ShowIcon = $false
$IntelHostingVMwareUtilityForm.StartPosition = [System.Windows.Forms.FormStartPosition]::CenterScreen
$IntelHostingVMwareUtilityForm.Text = [System.String]'Intel Hosting VMWare Utility'
$IntelHostingVMwareUtilityForm.add_Load($IntelHostingVMwareUtilityForm_Load)
$IntelHostingVMwareUtilityForm.add_Leave($IntelHostingVMwareUtilityForm_Leave)
$GBThreadcounter.ResumeLayout($false)
$GBThreadcounter.PerformLayout()
$groupBox3.ResumeLayout($false)
$groupBox3.PerformLayout()
$groupBox1.ResumeLayout($false)
$groupBox1.PerformLayout()
([System.ComponentModel.ISupportInitialize]$trackBar1).EndInit()
$groupBox2.ResumeLayout($false)
$groupBox2.PerformLayout()
$GBUpgrade.ResumeLayout($false)
$GBUtilities.ResumeLayout($false)
$GBUtilities.PerformLayout()
$GBQueries.ResumeLayout($false)
$GBQueries.PerformLayout()
$GBChange.ResumeLayout($false)
$GBChange.PerformLayout()
$groupBox4.ResumeLayout($false)
$groupBox4.PerformLayout()
$GBThreads.ResumeLayout($false)
$GBThreads.PerformLayout()
$IntelHostingVMwareUtilityForm.ResumeLayout($false)
$IntelHostingVMwareUtilityForm.PerformLayout()
Add-Member -InputObject $IntelHostingVMwareUtilityForm -Name base -Value $base -MemberType NoteProperty
Add-Member -InputObject $IntelHostingVMwareUtilityForm -Name progressBar1 -Value $progressBar1 -MemberType NoteProperty
Add-Member -InputObject $IntelHostingVMwareUtilityForm -Name GBThreadcounter -Value $GBThreadcounter -MemberType NoteProperty
Add-Member -InputObject $IntelHostingVMwareUtilityForm -Name lblthreads -Value $lblthreads -MemberType NoteProperty
Add-Member -InputObject $IntelHostingVMwareUtilityForm -Name tbtrackbarval -Value $tbtrackbarval -MemberType NoteProperty
Add-Member -InputObject $IntelHostingVMwareUtilityForm -Name groupBox3 -Value $groupBox3 -MemberType NoteProperty
Add-Member -InputObject $IntelHostingVMwareUtilityForm -Name lblstatus -Value $lblstatus -MemberType NoteProperty
Add-Member -InputObject $IntelHostingVMwareUtilityForm -Name label11 -Value $label11 -MemberType NoteProperty
Add-Member -InputObject $IntelHostingVMwareUtilityForm -Name TBPassword -Value $TBPassword -MemberType NoteProperty
Add-Member -InputObject $IntelHostingVMwareUtilityForm -Name TBUserID -Value $TBUserID -MemberType NoteProperty
Add-Member -InputObject $IntelHostingVMwareUtilityForm -Name label13 -Value $label13 -MemberType NoteProperty
Add-Member -InputObject $IntelHostingVMwareUtilityForm -Name label12 -Value $label12 -MemberType NoteProperty
Add-Member -InputObject $IntelHostingVMwareUtilityForm -Name button2 -Value $button2 -MemberType NoteProperty
Add-Member -InputObject $IntelHostingVMwareUtilityForm -Name label10 -Value $label10 -MemberType NoteProperty
Add-Member -InputObject $IntelHostingVMwareUtilityForm -Name groupBox1 -Value $groupBox1 -MemberType NoteProperty
Add-Member -InputObject $IntelHostingVMwareUtilityForm -Name btnclear -Value $btnclear -MemberType NoteProperty
Add-Member -InputObject $IntelHostingVMwareUtilityForm -Name trackBar1 -Value $trackBar1 -MemberType NoteProperty
Add-Member -InputObject $IntelHostingVMwareUtilityForm -Name btnProcess -Value $btnProcess -MemberType NoteProperty
Add-Member -InputObject $IntelHostingVMwareUtilityForm -Name button3 -Value $button3 -MemberType NoteProperty
Add-Member -InputObject $IntelHostingVMwareUtilityForm -Name groupBox2 -Value $groupBox2 -MemberType NoteProperty
Add-Member -InputObject $IntelHostingVMwareUtilityForm -Name RBUpgrade -Value $RBUpgrade -MemberType NoteProperty
Add-Member -InputObject $IntelHostingVMwareUtilityForm -Name RBQueries -Value $RBQueries -MemberType NoteProperty
Add-Member -InputObject $IntelHostingVMwareUtilityForm -Name RBChange -Value $RBChange -MemberType NoteProperty
Add-Member -InputObject $IntelHostingVMwareUtilityForm -Name RBUtilities -Value $RBUtilities -MemberType NoteProperty
Add-Member -InputObject $IntelHostingVMwareUtilityForm -Name GBUpgrade -Value $GBUpgrade -MemberType NoteProperty
Add-Member -InputObject $IntelHostingVMwareUtilityForm -Name BtnAbort -Value $BtnAbort -MemberType NoteProperty
Add-Member -InputObject $IntelHostingVMwareUtilityForm -Name button4 -Value $button4 -MemberType NoteProperty
Add-Member -InputObject $IntelHostingVMwareUtilityForm -Name GBUtilities -Value $GBUtilities -MemberType NoteProperty
Add-Member -InputObject $IntelHostingVMwareUtilityForm -Name RBrdp -Value $RBrdp -MemberType NoteProperty
Add-Member -InputObject $IntelHostingVMwareUtilityForm -Name rbping -Value $rbping -MemberType NoteProperty
Add-Member -InputObject $IntelHostingVMwareUtilityForm -Name GBQueries -Value $GBQueries -MemberType NoteProperty
Add-Member -InputObject $IntelHostingVMwareUtilityForm -Name radioButton10 -Value $radioButton10 -MemberType NoteProperty
Add-Member -InputObject $IntelHostingVMwareUtilityForm -Name RBNicinfo -Value $RBNicinfo -MemberType NoteProperty
Add-Member -InputObject $IntelHostingVMwareUtilityForm -Name BTNgetvmHWversion -Value $BTNgetvmHWversion -MemberType NoteProperty
Add-Member -InputObject $IntelHostingVMwareUtilityForm -Name rbpowerstate -Value $rbpowerstate -MemberType NoteProperty
Add-Member -InputObject $IntelHostingVMwareUtilityForm -Name RBToolsver -Value $RBToolsver -MemberType NoteProperty
Add-Member -InputObject $IntelHostingVMwareUtilityForm -Name RBCurrentHost -Value $RBCurrentHost -MemberType NoteProperty
Add-Member -InputObject $IntelHostingVMwareUtilityForm -Name GBChange -Value $GBChange -MemberType NoteProperty
Add-Member -InputObject $IntelHostingVMwareUtilityForm -Name RBRebootHard -Value $RBRebootHard -MemberType NoteProperty
Add-Member -InputObject $IntelHostingVMwareUtilityForm -Name RBSTopVMHard -Value $RBSTopVMHard -MemberType NoteProperty
Add-Member -InputObject $IntelHostingVMwareUtilityForm -Name RBRebootOS -Value $RBRebootOS -MemberType NoteProperty
Add-Member -InputObject $IntelHostingVMwareUtilityForm -Name RBStartVM -Value $RBStartVM -MemberType NoteProperty
Add-Member -InputObject $IntelHostingVMwareUtilityForm -Name RBStopVMOS -Value $RBStopVMOS -MemberType NoteProperty
Add-Member -InputObject $IntelHostingVMwareUtilityForm -Name textBox1 -Value $textBox1 -MemberType NoteProperty
Add-Member -InputObject $IntelHostingVMwareUtilityForm -Name label4 -Value $label4 -MemberType NoteProperty
Add-Member -InputObject $IntelHostingVMwareUtilityForm -Name textBox2 -Value $textBox2 -MemberType NoteProperty
Add-Member -InputObject $IntelHostingVMwareUtilityForm -Name lblprogress -Value $lblprogress -MemberType NoteProperty
Add-Member -InputObject $IntelHostingVMwareUtilityForm -Name label17 -Value $label17 -MemberType NoteProperty
Add-Member -InputObject $IntelHostingVMwareUtilityForm -Name label9 -Value $label9 -MemberType NoteProperty
Add-Member -InputObject $IntelHostingVMwareUtilityForm -Name groupBox4 -Value $groupBox4 -MemberType NoteProperty
Add-Member -InputObject $IntelHostingVMwareUtilityForm -Name ARCHIVECOMPLETED -Value $ARCHIVECOMPLETED -MemberType NoteProperty
Add-Member -InputObject $IntelHostingVMwareUtilityForm -Name BTNCounterUpdate -Value $BTNCounterUpdate -MemberType NoteProperty
Add-Member -InputObject $IntelHostingVMwareUtilityForm -Name tbcompleted -Value $tbcompleted -MemberType NoteProperty
Add-Member -InputObject $IntelHostingVMwareUtilityForm -Name label14 -Value $label14 -MemberType NoteProperty
Add-Member -InputObject $IntelHostingVMwareUtilityForm -Name tbrunning -Value $tbrunning -MemberType NoteProperty
Add-Member -InputObject $IntelHostingVMwareUtilityForm -Name label16 -Value $label16 -MemberType NoteProperty
Add-Member -InputObject $IntelHostingVMwareUtilityForm -Name tbtotals -Value $tbtotals -MemberType NoteProperty
Add-Member -InputObject $IntelHostingVMwareUtilityForm -Name label15 -Value $label15 -MemberType NoteProperty
Add-Member -InputObject $IntelHostingVMwareUtilityForm -Name label5 -Value $label5 -MemberType NoteProperty
Add-Member -InputObject $IntelHostingVMwareUtilityForm -Name label1 -Value $label1 -MemberType NoteProperty
Add-Member -InputObject $IntelHostingVMwareUtilityForm -Name label2 -Value $label2 -MemberType NoteProperty
Add-Member -InputObject $IntelHostingVMwareUtilityForm -Name webBrowser4 -Value $webBrowser4 -MemberType NoteProperty
Add-Member -InputObject $IntelHostingVMwareUtilityForm -Name label7 -Value $label7 -MemberType NoteProperty
Add-Member -InputObject $IntelHostingVMwareUtilityForm -Name webBrowser2 -Value $webBrowser2 -MemberType NoteProperty
Add-Member -InputObject $IntelHostingVMwareUtilityForm -Name webBrowser1 -Value $webBrowser1 -MemberType NoteProperty
Add-Member -InputObject $IntelHostingVMwareUtilityForm -Name webBrowser3 -Value $webBrowser3 -MemberType NoteProperty
Add-Member -InputObject $IntelHostingVMwareUtilityForm -Name label8 -Value $label8 -MemberType NoteProperty
Add-Member -InputObject $IntelHostingVMwareUtilityForm -Name webBrowser5 -Value $webBrowser5 -MemberType NoteProperty
Add-Member -InputObject $IntelHostingVMwareUtilityForm -Name tbstart -Value $tbstart -MemberType NoteProperty
Add-Member -InputObject $IntelHostingVMwareUtilityForm -Name webBrowser6 -Value $webBrowser6 -MemberType NoteProperty
Add-Member -InputObject $IntelHostingVMwareUtilityForm -Name label6 -Value $label6 -MemberType NoteProperty
Add-Member -InputObject $IntelHostingVMwareUtilityForm -Name webBrowser7 -Value $webBrowser7 -MemberType NoteProperty
Add-Member -InputObject $IntelHostingVMwareUtilityForm -Name label3 -Value $label3 -MemberType NoteProperty
Add-Member -InputObject $IntelHostingVMwareUtilityForm -Name tbhwupgradestart -Value $tbhwupgradestart -MemberType NoteProperty
Add-Member -InputObject $IntelHostingVMwareUtilityForm -Name tbupdatetools -Value $tbupdatetools -MemberType NoteProperty
Add-Member -InputObject $IntelHostingVMwareUtilityForm -Name tbstartrdp -Value $tbstartrdp -MemberType NoteProperty
Add-Member -InputObject $IntelHostingVMwareUtilityForm -Name tbstop -Value $tbstop -MemberType NoteProperty
Add-Member -InputObject $IntelHostingVMwareUtilityForm -Name tbcomplete -Value $tbcomplete -MemberType NoteProperty
Add-Member -InputObject $IntelHostingVMwareUtilityForm -Name GBThreads -Value $GBThreads -MemberType NoteProperty
Add-Member -InputObject $IntelHostingVMwareUtilityForm -Name TBThreads -Value $TBThreads -MemberType NoteProperty
Add-Member -InputObject $IntelHostingVMwareUtilityForm -Name components -Value $components -MemberType NoteProperty
Add-Member -InputObject $IntelHostingVMwareUtilityForm -Name TBVcenter -Value $TBVcenter -MemberType NoteProperty
Add-Member -InputObject $IntelHostingVMwareUtilityForm -Name tberror -Value $tberror -MemberType NoteProperty
Add-Member -InputObject $IntelHostingVMwareUtilityForm -Name button1 -Value $button1 -MemberType NoteProperty
}
. InitializeComponent
