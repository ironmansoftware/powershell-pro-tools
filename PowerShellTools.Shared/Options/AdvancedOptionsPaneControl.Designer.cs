namespace PowerShellToolsPro.Options
{
    partial class AdvancedOptionsPaneControl
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AdvancedOptionsPaneControl));
            this.chkPackage = new System.Windows.Forms.CheckBox();
            this.cmoEntryScript = new System.Windows.Forms.ComboBox();
            this.lblEntryPoint = new System.Windows.Forms.Label();
            this.chkBundle = new System.Windows.Forms.CheckBox();
            this.lblEntryPointDesc = new System.Windows.Forms.Label();
            this.lblBundle = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.tabPackagingSettings = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.tabSettings = new System.Windows.Forms.TabPage();
            this.cmoPowerShellVersion = new System.Windows.Forms.ComboBox();
            this.label22 = new System.Windows.Forms.Label();
            this.label23 = new System.Windows.Forms.Label();
            this.cmoPlatform = new System.Windows.Forms.ComboBox();
            this.txtPowerShellArgs = new System.Windows.Forms.TextBox();
            this.label21 = new System.Windows.Forms.Label();
            this.label20 = new System.Windows.Forms.Label();
            this.chkHighDpiSupport = new System.Windows.Forms.CheckBox();
            this.cmoPackageType = new System.Windows.Forms.ComboBox();
            this.label16 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.btnExploreIcon = new System.Windows.Forms.Button();
            this.chkObfuscate = new System.Windows.Forms.CheckBox();
            this.label15 = new System.Windows.Forms.Label();
            this.txtIcon = new System.Windows.Forms.TextBox();
            this.chkHideConsoleWindow = new System.Windows.Forms.CheckBox();
            this.label14 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.chkRequireElevation = new System.Windows.Forms.CheckBox();
            this.cmoDotNetVersion = new System.Windows.Forms.ComboBox();
            this.label12 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.chkPackageModules = new System.Windows.Forms.CheckBox();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.label13 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.txtFileVersion = new System.Windows.Forms.TextBox();
            this.txtFileDescription = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.txtProductName = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.txtProductVersion = new System.Windows.Forms.TextBox();
            this.label10 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.txtCopyright = new System.Windows.Forms.TextBox();
            this.tabService = new System.Windows.Forms.TabPage();
            this.label19 = new System.Windows.Forms.Label();
            this.txtServiceDisplayName = new System.Windows.Forms.TextBox();
            this.label18 = new System.Windows.Forms.Label();
            this.label17 = new System.Windows.Forms.Label();
            this.txtServiceName = new System.Windows.Forms.TextBox();
            this.tabPackagingSettings.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabSettings.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.tabService.SuspendLayout();
            this.SuspendLayout();
            // 
            // chkPackage
            // 
            this.chkPackage.AutoSize = true;
            this.chkPackage.Location = new System.Drawing.Point(34, 414);
            this.chkPackage.Name = "chkPackage";
            this.chkPackage.Size = new System.Drawing.Size(199, 24);
            this.chkPackage.TabIndex = 0;
            this.chkPackage.Text = "Package as executable";
            this.chkPackage.UseVisualStyleBackColor = true;
            this.chkPackage.CheckedChanged += new System.EventHandler(this.chkPackage_CheckedChanged);
            // 
            // cmoEntryScript
            // 
            this.cmoEntryScript.FormattingEnabled = true;
            this.cmoEntryScript.Location = new System.Drawing.Point(34, 92);
            this.cmoEntryScript.Name = "cmoEntryScript";
            this.cmoEntryScript.Size = new System.Drawing.Size(528, 28);
            this.cmoEntryScript.TabIndex = 1;
            this.cmoEntryScript.SelectedIndexChanged += new System.EventHandler(this.cmoEntryScript_SelectedIndexChanged);
            // 
            // lblEntryPoint
            // 
            this.lblEntryPoint.AutoSize = true;
            this.lblEntryPoint.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblEntryPoint.Location = new System.Drawing.Point(21, 23);
            this.lblEntryPoint.Name = "lblEntryPoint";
            this.lblEntryPoint.Size = new System.Drawing.Size(97, 20);
            this.lblEntryPoint.TabIndex = 2;
            this.lblEntryPoint.Text = "Entry Point";
            // 
            // chkBundle
            // 
            this.chkBundle.AutoSize = true;
            this.chkBundle.Location = new System.Drawing.Point(34, 263);
            this.chkBundle.Name = "chkBundle";
            this.chkBundle.Size = new System.Drawing.Size(85, 24);
            this.chkBundle.TabIndex = 3;
            this.chkBundle.Text = "Bundle";
            this.chkBundle.UseVisualStyleBackColor = true;
            this.chkBundle.CheckedChanged += new System.EventHandler(this.chkBundle_CheckedChanged);
            // 
            // lblEntryPointDesc
            // 
            this.lblEntryPointDesc.AutoSize = true;
            this.lblEntryPointDesc.Location = new System.Drawing.Point(32, 60);
            this.lblEntryPointDesc.Name = "lblEntryPointDesc";
            this.lblEntryPointDesc.Size = new System.Drawing.Size(373, 20);
            this.lblEntryPointDesc.TabIndex = 4;
            this.lblEntryPointDesc.Text = "Defines the entry point of the bundle or executable. \r\n";
            // 
            // lblBundle
            // 
            this.lblBundle.AutoSize = true;
            this.lblBundle.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblBundle.Location = new System.Drawing.Point(18, 143);
            this.lblBundle.Name = "lblBundle";
            this.lblBundle.Size = new System.Drawing.Size(65, 20);
            this.lblBundle.TabIndex = 5;
            this.lblBundle.Text = "Bundle";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(32, 182);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(508, 60);
            this.label1.TabIndex = 6;
            this.label1.Text = "Specifies whether to bundle dot source files included by the entry point.\r\nThis s" +
    "etting is recursive. Dot source files that contain other dot sourced\r\nfiles will" +
    " also be included.";
            // 
            // tabPackagingSettings
            // 
            this.tabPackagingSettings.Controls.Add(this.tabPage1);
            this.tabPackagingSettings.Controls.Add(this.tabSettings);
            this.tabPackagingSettings.Controls.Add(this.tabPage2);
            this.tabPackagingSettings.Controls.Add(this.tabService);
            this.tabPackagingSettings.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabPackagingSettings.Location = new System.Drawing.Point(0, 0);
            this.tabPackagingSettings.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.tabPackagingSettings.Name = "tabPackagingSettings";
            this.tabPackagingSettings.SelectedIndex = 0;
            this.tabPackagingSettings.Size = new System.Drawing.Size(836, 745);
            this.tabPackagingSettings.TabIndex = 35;
            // 
            // tabPage1
            // 
            this.tabPage1.AutoScroll = true;
            this.tabPage1.Controls.Add(this.lblEntryPoint);
            this.tabPage1.Controls.Add(this.label3);
            this.tabPage1.Controls.Add(this.lblEntryPointDesc);
            this.tabPage1.Controls.Add(this.label2);
            this.tabPage1.Controls.Add(this.lblBundle);
            this.tabPage1.Controls.Add(this.chkBundle);
            this.tabPage1.Controls.Add(this.chkPackage);
            this.tabPage1.Controls.Add(this.label1);
            this.tabPage1.Controls.Add(this.cmoEntryScript);
            this.tabPage1.Location = new System.Drawing.Point(4, 29);
            this.tabPage1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.tabPage1.Size = new System.Drawing.Size(828, 712);
            this.tabPage1.TabIndex = 3;
            this.tabPage1.Text = "Packaging Settings";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(32, 335);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(428, 60);
            this.label3.TabIndex = 8;
            this.label3.Text = "Outputs an executable that contains the script. Launching\r\nthe executable will ex" +
    "ecute the script. This can be combined\r\nwith bundling to merge several scripts i" +
    "nto an executable.";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(18, 303);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(196, 20);
            this.label2.TabIndex = 7;
            this.label2.Text = "Package as Executable";
            // 
            // tabSettings
            // 
            this.tabSettings.AutoScroll = true;
            this.tabSettings.Controls.Add(this.cmoPowerShellVersion);
            this.tabSettings.Controls.Add(this.label22);
            this.tabSettings.Controls.Add(this.label23);
            this.tabSettings.Controls.Add(this.cmoPlatform);
            this.tabSettings.Controls.Add(this.txtPowerShellArgs);
            this.tabSettings.Controls.Add(this.label21);
            this.tabSettings.Controls.Add(this.label20);
            this.tabSettings.Controls.Add(this.chkHighDpiSupport);
            this.tabSettings.Controls.Add(this.cmoPackageType);
            this.tabSettings.Controls.Add(this.label16);
            this.tabSettings.Controls.Add(this.label4);
            this.tabSettings.Controls.Add(this.btnExploreIcon);
            this.tabSettings.Controls.Add(this.chkObfuscate);
            this.tabSettings.Controls.Add(this.label15);
            this.tabSettings.Controls.Add(this.txtIcon);
            this.tabSettings.Controls.Add(this.chkHideConsoleWindow);
            this.tabSettings.Controls.Add(this.label14);
            this.tabSettings.Controls.Add(this.label5);
            this.tabSettings.Controls.Add(this.chkRequireElevation);
            this.tabSettings.Controls.Add(this.cmoDotNetVersion);
            this.tabSettings.Controls.Add(this.label12);
            this.tabSettings.Controls.Add(this.label7);
            this.tabSettings.Controls.Add(this.chkPackageModules);
            this.tabSettings.Location = new System.Drawing.Point(4, 29);
            this.tabSettings.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.tabSettings.Name = "tabSettings";
            this.tabSettings.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.tabSettings.Size = new System.Drawing.Size(828, 712);
            this.tabSettings.TabIndex = 0;
            this.tabSettings.Text = "Executable Properties";
            this.tabSettings.UseVisualStyleBackColor = true;
            // 
            // cmoPowerShellVersion
            // 
            this.cmoPowerShellVersion.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmoPowerShellVersion.FormattingEnabled = true;
            this.cmoPowerShellVersion.Location = new System.Drawing.Point(33, 140);
            this.cmoPowerShellVersion.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.cmoPowerShellVersion.Name = "cmoPowerShellVersion";
            this.cmoPowerShellVersion.Size = new System.Drawing.Size(314, 28);
            this.cmoPowerShellVersion.TabIndex = 45;
            this.cmoPowerShellVersion.SelectedIndexChanged += new System.EventHandler(this.cmoPowerShellVersion_SelectedIndexChanged);
            // 
            // label22
            // 
            this.label22.AutoSize = true;
            this.label22.Location = new System.Drawing.Point(30, 105);
            this.label22.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label22.Name = "label22";
            this.label22.Size = new System.Drawing.Size(146, 20);
            this.label22.TabIndex = 46;
            this.label22.Text = "PowerShell Version";
            // 
            // label23
            // 
            this.label23.AutoSize = true;
            this.label23.Location = new System.Drawing.Point(423, 105);
            this.label23.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label23.Name = "label23";
            this.label23.Size = new System.Drawing.Size(68, 20);
            this.label23.TabIndex = 44;
            this.label23.Text = "Platform";
            // 
            // cmoPlatform
            // 
            this.cmoPlatform.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmoPlatform.FormattingEnabled = true;
            this.cmoPlatform.Items.AddRange(new object[] {
            "x64",
            "x86"});
            this.cmoPlatform.Location = new System.Drawing.Point(426, 142);
            this.cmoPlatform.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.cmoPlatform.Name = "cmoPlatform";
            this.cmoPlatform.Size = new System.Drawing.Size(343, 28);
            this.cmoPlatform.TabIndex = 43;
            this.cmoPlatform.SelectedIndexChanged += new System.EventHandler(this.cmoPlatform_SelectedIndexChanged);
            // 
            // txtPowerShellArgs
            // 
            this.txtPowerShellArgs.Location = new System.Drawing.Point(426, 608);
            this.txtPowerShellArgs.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txtPowerShellArgs.Multiline = true;
            this.txtPowerShellArgs.Name = "txtPowerShellArgs";
            this.txtPowerShellArgs.Size = new System.Drawing.Size(342, 106);
            this.txtPowerShellArgs.TabIndex = 40;
            this.txtPowerShellArgs.TextChanged += new System.EventHandler(this.TxtPowerShellArgs_TextChanged);
            // 
            // label21
            // 
            this.label21.AutoSize = true;
            this.label21.Location = new System.Drawing.Point(422, 445);
            this.label21.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label21.Name = "label21";
            this.label21.Size = new System.Drawing.Size(374, 140);
            this.label21.TabIndex = 39;
            this.label21.Text = resources.GetString("label21.Text");
            // 
            // label20
            // 
            this.label20.AutoSize = true;
            this.label20.Location = new System.Drawing.Point(422, 328);
            this.label20.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label20.Name = "label20";
            this.label20.Size = new System.Drawing.Size(324, 60);
            this.label20.TabIndex = 38;
            this.label20.Text = "High DPI support for WinForm applications. \r\nRequires .NET framework v4.7 or high" +
    "er and \r\nonly executes on Windows 10 or newer.";
            // 
            // chkHighDpiSupport
            // 
            this.chkHighDpiSupport.AutoSize = true;
            this.chkHighDpiSupport.Location = new System.Drawing.Point(426, 405);
            this.chkHighDpiSupport.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.chkHighDpiSupport.Name = "chkHighDpiSupport";
            this.chkHighDpiSupport.Size = new System.Drawing.Size(160, 24);
            this.chkHighDpiSupport.TabIndex = 37;
            this.chkHighDpiSupport.Text = "High DPI Support";
            this.chkHighDpiSupport.UseVisualStyleBackColor = true;
            this.chkHighDpiSupport.CheckedChanged += new System.EventHandler(this.ChkHighDpiSupport_CheckedChanged);
            // 
            // cmoPackageType
            // 
            this.cmoPackageType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmoPackageType.FormattingEnabled = true;
            this.cmoPackageType.Items.AddRange(new object[] {
            "Console",
            "Service"});
            this.cmoPackageType.Location = new System.Drawing.Point(426, 54);
            this.cmoPackageType.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.cmoPackageType.Name = "cmoPackageType";
            this.cmoPackageType.Size = new System.Drawing.Size(342, 28);
            this.cmoPackageType.TabIndex = 36;
            this.cmoPackageType.SelectedIndexChanged += new System.EventHandler(this.cmoPackageType_SelectedIndexChanged);
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Location = new System.Drawing.Point(422, 17);
            this.label16.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(96, 20);
            this.label16.TabIndex = 35;
            this.label16.Text = "Output Type";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(423, 225);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(392, 40);
            this.label4.TabIndex = 10;
            this.label4.Text = "Obfuscates the .NET executable containing the script. \r\nThe string containing the" +
    " script is also obfuscated.";
            // 
            // btnExploreIcon
            // 
            this.btnExploreIcon.Location = new System.Drawing.Point(336, 598);
            this.btnExploreIcon.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnExploreIcon.Name = "btnExploreIcon";
            this.btnExploreIcon.Size = new System.Drawing.Size(54, 35);
            this.btnExploreIcon.TabIndex = 34;
            this.btnExploreIcon.Text = "...";
            this.btnExploreIcon.UseVisualStyleBackColor = true;
            this.btnExploreIcon.Click += new System.EventHandler(this.btnExploreIcon_Click);
            // 
            // chkObfuscate
            // 
            this.chkObfuscate.AutoSize = true;
            this.chkObfuscate.Location = new System.Drawing.Point(426, 283);
            this.chkObfuscate.Name = "chkObfuscate";
            this.chkObfuscate.Size = new System.Drawing.Size(190, 24);
            this.chkObfuscate.TabIndex = 9;
            this.chkObfuscate.Text = "Obfuscate executable";
            this.chkObfuscate.UseVisualStyleBackColor = true;
            this.chkObfuscate.CheckedChanged += new System.EventHandler(this.chkObfuscate_CheckedChanged);
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Location = new System.Drawing.Point(24, 563);
            this.label15.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(40, 20);
            this.label15.TabIndex = 32;
            this.label15.Text = "Icon";
            // 
            // txtIcon
            // 
            this.txtIcon.Location = new System.Drawing.Point(27, 598);
            this.txtIcon.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txtIcon.Name = "txtIcon";
            this.txtIcon.Size = new System.Drawing.Size(283, 26);
            this.txtIcon.TabIndex = 31;
            this.txtIcon.TextChanged += new System.EventHandler(this.txtIcon_TextChanged);
            // 
            // chkHideConsoleWindow
            // 
            this.chkHideConsoleWindow.AutoSize = true;
            this.chkHideConsoleWindow.Location = new System.Drawing.Point(27, 265);
            this.chkHideConsoleWindow.Name = "chkHideConsoleWindow";
            this.chkHideConsoleWindow.Size = new System.Drawing.Size(190, 24);
            this.chkHideConsoleWindow.TabIndex = 11;
            this.chkHideConsoleWindow.Text = "Hide Console Window";
            this.chkHideConsoleWindow.UseVisualStyleBackColor = true;
            this.chkHideConsoleWindow.CheckedChanged += new System.EventHandler(this.checkBox1_CheckedChanged);
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(24, 445);
            this.label14.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(364, 40);
            this.label14.TabIndex = 30;
            this.label14.Text = "Adds an application manifest that forces elevation \r\nwhen running the resulting e" +
    "xecutable.";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(24, 206);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(324, 40);
            this.label5.TabIndex = 12;
            this.label5.Text = "Whether or not to show the console window. \r\nUseful to disable for GUI applicatio" +
    "ns. ";
            // 
            // chkRequireElevation
            // 
            this.chkRequireElevation.AutoSize = true;
            this.chkRequireElevation.Location = new System.Drawing.Point(26, 517);
            this.chkRequireElevation.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.chkRequireElevation.Name = "chkRequireElevation";
            this.chkRequireElevation.Size = new System.Drawing.Size(268, 24);
            this.chkRequireElevation.TabIndex = 26;
            this.chkRequireElevation.Text = "Require Elevation (Administrator)";
            this.chkRequireElevation.UseVisualStyleBackColor = true;
            this.chkRequireElevation.CheckedChanged += new System.EventHandler(this.chkRequireElevation_CheckedChanged);
            // 
            // cmoDotNetVersion
            // 
            this.cmoDotNetVersion.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmoDotNetVersion.FormattingEnabled = true;
            this.cmoDotNetVersion.Items.AddRange(new object[] {
            "net462",
            "net470",
            "net471",
            "net472",
            "net48",
            "netcoreapp31",
            "net5.0",
            "net6.0"});
            this.cmoDotNetVersion.Location = new System.Drawing.Point(30, 54);
            this.cmoDotNetVersion.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.cmoDotNetVersion.Name = "cmoDotNetVersion";
            this.cmoDotNetVersion.Size = new System.Drawing.Size(314, 28);
            this.cmoDotNetVersion.TabIndex = 13;
            this.cmoDotNetVersion.SelectedIndexChanged += new System.EventHandler(this.cmoDotNetVersion_SelectedIndexChanged);
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(24, 308);
            this.label12.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(366, 60);
            this.label12.TabIndex = 28;
            this.label12.Text = "Embeds modules referenced using Import-Module \r\nwithin the resulting executable s" +
    "o there is no need \r\nto deploy them to the target machine.";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(27, 17);
            this.label7.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(102, 20);
            this.label7.TabIndex = 15;
            this.label7.Text = ".NET Version";
            // 
            // chkPackageModules
            // 
            this.chkPackageModules.AutoSize = true;
            this.chkPackageModules.Location = new System.Drawing.Point(27, 395);
            this.chkPackageModules.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.chkPackageModules.Name = "chkPackageModules";
            this.chkPackageModules.Size = new System.Drawing.Size(161, 24);
            this.chkPackageModules.TabIndex = 27;
            this.chkPackageModules.Text = "Package Modules";
            this.chkPackageModules.UseVisualStyleBackColor = true;
            this.chkPackageModules.CheckedChanged += new System.EventHandler(this.chkPackageModules_CheckedChanged);
            // 
            // tabPage2
            // 
            this.tabPage2.AutoScroll = true;
            this.tabPage2.Controls.Add(this.label13);
            this.tabPage2.Controls.Add(this.label6);
            this.tabPage2.Controls.Add(this.txtFileVersion);
            this.tabPage2.Controls.Add(this.txtFileDescription);
            this.tabPage2.Controls.Add(this.label8);
            this.tabPage2.Controls.Add(this.txtProductName);
            this.tabPage2.Controls.Add(this.label9);
            this.tabPage2.Controls.Add(this.txtProductVersion);
            this.tabPage2.Controls.Add(this.label10);
            this.tabPage2.Controls.Add(this.label11);
            this.tabPage2.Controls.Add(this.txtCopyright);
            this.tabPage2.Location = new System.Drawing.Point(4, 29);
            this.tabPage2.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.tabPage2.Size = new System.Drawing.Size(828, 712);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "File Properties";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(9, 25);
            this.label13.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(396, 40);
            this.label13.TabIndex = 29;
            this.label13.Text = "Specify file details below that will show up in the details \r\nfor the executable." +
    "";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(9, 158);
            this.label6.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(92, 20);
            this.label6.TabIndex = 16;
            this.label6.Text = "File Version";
            // 
            // txtFileVersion
            // 
            this.txtFileVersion.Location = new System.Drawing.Point(14, 188);
            this.txtFileVersion.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txtFileVersion.Name = "txtFileVersion";
            this.txtFileVersion.Size = new System.Drawing.Size(374, 26);
            this.txtFileVersion.TabIndex = 17;
            this.txtFileVersion.TextChanged += new System.EventHandler(this.textBox1_TextChanged);
            // 
            // txtFileDescription
            // 
            this.txtFileDescription.Location = new System.Drawing.Point(14, 117);
            this.txtFileDescription.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txtFileDescription.Name = "txtFileDescription";
            this.txtFileDescription.Size = new System.Drawing.Size(374, 26);
            this.txtFileDescription.TabIndex = 18;
            this.txtFileDescription.TextChanged += new System.EventHandler(this.txtFileDescription_TextChanged);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(10, 88);
            this.label8.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(118, 20);
            this.label8.TabIndex = 19;
            this.label8.Text = "File Description";
            // 
            // txtProductName
            // 
            this.txtProductName.Location = new System.Drawing.Point(14, 248);
            this.txtProductName.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txtProductName.Name = "txtProductName";
            this.txtProductName.Size = new System.Drawing.Size(374, 26);
            this.txtProductName.TabIndex = 20;
            this.txtProductName.TextChanged += new System.EventHandler(this.txtProductName_TextChanged);
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(9, 223);
            this.label9.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(110, 20);
            this.label9.TabIndex = 21;
            this.label9.Text = "Product Name";
            // 
            // txtProductVersion
            // 
            this.txtProductVersion.Location = new System.Drawing.Point(14, 325);
            this.txtProductVersion.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txtProductVersion.Name = "txtProductVersion";
            this.txtProductVersion.Size = new System.Drawing.Size(374, 26);
            this.txtProductVersion.TabIndex = 22;
            this.txtProductVersion.TextChanged += new System.EventHandler(this.txtProductVersion_TextChanged);
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(12, 294);
            this.label10.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(122, 20);
            this.label10.TabIndex = 23;
            this.label10.Text = "Product Version";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(12, 375);
            this.label11.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(76, 20);
            this.label11.TabIndex = 25;
            this.label11.Text = "Copyright";
            // 
            // txtCopyright
            // 
            this.txtCopyright.Location = new System.Drawing.Point(14, 406);
            this.txtCopyright.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txtCopyright.Name = "txtCopyright";
            this.txtCopyright.Size = new System.Drawing.Size(374, 26);
            this.txtCopyright.TabIndex = 24;
            this.txtCopyright.TextChanged += new System.EventHandler(this.txtCopyright_TextChanged);
            // 
            // tabService
            // 
            this.tabService.AutoScroll = true;
            this.tabService.Controls.Add(this.label19);
            this.tabService.Controls.Add(this.txtServiceDisplayName);
            this.tabService.Controls.Add(this.label18);
            this.tabService.Controls.Add(this.label17);
            this.tabService.Controls.Add(this.txtServiceName);
            this.tabService.Location = new System.Drawing.Point(4, 29);
            this.tabService.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.tabService.Name = "tabService";
            this.tabService.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.tabService.Size = new System.Drawing.Size(828, 712);
            this.tabService.TabIndex = 2;
            this.tabService.Text = "Service Properties";
            this.tabService.UseVisualStyleBackColor = true;
            // 
            // label19
            // 
            this.label19.AutoSize = true;
            this.label19.Location = new System.Drawing.Point(27, 31);
            this.label19.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label19.Name = "label19";
            this.label19.Size = new System.Drawing.Size(435, 40);
            this.label19.TabIndex = 4;
            this.label19.Text = "Settings for a PowerShell service. Change the Output Type \r\non the Executable Pro" +
    "perties switch to a PowerShell Service.";
            // 
            // txtServiceDisplayName
            // 
            this.txtServiceDisplayName.Location = new System.Drawing.Point(27, 217);
            this.txtServiceDisplayName.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txtServiceDisplayName.Name = "txtServiceDisplayName";
            this.txtServiceDisplayName.Size = new System.Drawing.Size(322, 26);
            this.txtServiceDisplayName.TabIndex = 3;
            this.txtServiceDisplayName.TextChanged += new System.EventHandler(this.txtServiceDisplayName_TextChanged);
            // 
            // label18
            // 
            this.label18.AutoSize = true;
            this.label18.Location = new System.Drawing.Point(22, 177);
            this.label18.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label18.Name = "label18";
            this.label18.Size = new System.Drawing.Size(162, 20);
            this.label18.TabIndex = 2;
            this.label18.Text = "Service Display Name";
            // 
            // label17
            // 
            this.label17.AutoSize = true;
            this.label17.Location = new System.Drawing.Point(22, 88);
            this.label17.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(107, 20);
            this.label17.TabIndex = 1;
            this.label17.Text = "Service Name";
            // 
            // txtServiceName
            // 
            this.txtServiceName.Location = new System.Drawing.Point(27, 125);
            this.txtServiceName.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txtServiceName.Name = "txtServiceName";
            this.txtServiceName.Size = new System.Drawing.Size(322, 26);
            this.txtServiceName.TabIndex = 0;
            this.txtServiceName.TextChanged += new System.EventHandler(this.txtServiceName_TextChanged);
            // 
            // AdvancedOptionsPaneControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tabPackagingSettings);
            this.Name = "AdvancedOptionsPaneControl";
            this.Size = new System.Drawing.Size(836, 745);
            this.Load += new System.EventHandler(this.AdvancedOptionsPaneControl_Load);
            this.tabPackagingSettings.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            this.tabSettings.ResumeLayout(false);
            this.tabSettings.PerformLayout();
            this.tabPage2.ResumeLayout(false);
            this.tabPage2.PerformLayout();
            this.tabService.ResumeLayout(false);
            this.tabService.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.CheckBox chkPackage;
        private System.Windows.Forms.ComboBox cmoEntryScript;
        private System.Windows.Forms.Label lblEntryPoint;
        private System.Windows.Forms.CheckBox chkBundle;
        private System.Windows.Forms.Label lblEntryPointDesc;
        private System.Windows.Forms.Label lblBundle;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.CheckBox chkObfuscate;
        private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.CheckBox chkHideConsoleWindow;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.ComboBox cmoDotNetVersion;
        private System.Windows.Forms.TextBox txtFileVersion;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.CheckBox chkRequireElevation;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.TextBox txtCopyright;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.TextBox txtProductVersion;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TextBox txtProductName;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox txtFileDescription;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.CheckBox chkPackageModules;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.Button btnExploreIcon;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.TextBox txtIcon;
        private System.Windows.Forms.TabControl tabPackagingSettings;
        private System.Windows.Forms.TabPage tabSettings;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.ComboBox cmoPackageType;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.TabPage tabService;
        private System.Windows.Forms.Label label17;
        private System.Windows.Forms.TextBox txtServiceName;
        private System.Windows.Forms.TextBox txtServiceDisplayName;
        private System.Windows.Forms.Label label18;
        private System.Windows.Forms.Label label19;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.Label label20;
        private System.Windows.Forms.CheckBox chkHighDpiSupport;
        private System.Windows.Forms.Label label21;
        private System.Windows.Forms.TextBox txtPowerShellArgs;
        private System.Windows.Forms.Label label23;
        private System.Windows.Forms.ComboBox cmoPlatform;
        private System.Windows.Forms.ComboBox cmoPowerShellVersion;
        private System.Windows.Forms.Label label22;
    }
}
