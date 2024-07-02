namespace PowerShellTools.Options
{
    partial class GeneralOptionsControl
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(GeneralOptionsControl));
            this.chkOverrideExecutionPolicy = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.chkMultiLineRepl = new System.Windows.Forms.CheckBox();
            this.label2 = new System.Windows.Forms.Label();
            this.cmoPowerShellVersion = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.chkLoadProfiles = new System.Windows.Forms.CheckBox();
            this.label4 = new System.Windows.Forms.Label();
            this.chkTabComplete = new System.Windows.Forms.CheckBox();
            this.label8 = new System.Windows.Forms.Label();
            this.chkSta = new System.Windows.Forms.CheckBox();
            this.chkDontDisplayLicenseInfo = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // chkOverrideExecutionPolicy
            // 
            this.chkOverrideExecutionPolicy.AutoSize = true;
            this.chkOverrideExecutionPolicy.Location = new System.Drawing.Point(8, 4);
            this.chkOverrideExecutionPolicy.Margin = new System.Windows.Forms.Padding(4);
            this.chkOverrideExecutionPolicy.Name = "chkOverrideExecutionPolicy";
            this.chkOverrideExecutionPolicy.Size = new System.Drawing.Size(261, 21);
            this.chkOverrideExecutionPolicy.TabIndex = 0;
            this.chkOverrideExecutionPolicy.Text = "Enable Unrestricted Execution Policy";
            this.chkOverrideExecutionPolicy.UseVisualStyleBackColor = true;
            this.chkOverrideExecutionPolicy.CheckedChanged += new System.EventHandler(this.chkOverrideExecutionPolicy_CheckedChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(8, 28);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(576, 51);
            this.label1.TabIndex = 1;
            this.label1.Text = resources.GetString("label1.Text");
            // 
            // chkMultiLineRepl
            // 
            this.chkMultiLineRepl.AutoSize = true;
            this.chkMultiLineRepl.Location = new System.Drawing.Point(8, 91);
            this.chkMultiLineRepl.Margin = new System.Windows.Forms.Padding(4);
            this.chkMultiLineRepl.Name = "chkMultiLineRepl";
            this.chkMultiLineRepl.Size = new System.Drawing.Size(174, 21);
            this.chkMultiLineRepl.TabIndex = 3;
            this.chkMultiLineRepl.Text = "Multiline REPL Window";
            this.chkMultiLineRepl.UseVisualStyleBackColor = true;
            this.chkMultiLineRepl.CheckedChanged += new System.EventHandler(this.chkMultiLineRepl_CheckedChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(8, 116);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(673, 17);
            this.label2.TabIndex = 4;
            this.label2.Text = "When unchecked, pressing enter invokes the command line in the REPL Window rather tha" +
    "n starting a new line.";
            // 
            // cmoPowerShellVersion
            // 
            this.cmoPowerShellVersion.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmoPowerShellVersion.FormattingEnabled = true;
            this.cmoPowerShellVersion.Location = new System.Drawing.Point(8, 167);
            this.cmoPowerShellVersion.Margin = new System.Windows.Forms.Padding(4);
            this.cmoPowerShellVersion.Name = "cmoPowerShellVersion";
            this.cmoPowerShellVersion.Size = new System.Drawing.Size(451, 24);
            this.cmoPowerShellVersion.TabIndex = 5;
            this.cmoPowerShellVersion.SelectedIndexChanged += new System.EventHandler(this.cmoPowerShellVersion_SelectedIndexChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(5, 148);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(130, 17);
            this.label3.TabIndex = 6;
            this.label3.Text = "PowerShell Version";
            // 
            // chkLoadProfiles
            // 
            this.chkLoadProfiles.AutoSize = true;
            this.chkLoadProfiles.Location = new System.Drawing.Point(8, 217);
            this.chkLoadProfiles.Margin = new System.Windows.Forms.Padding(4);
            this.chkLoadProfiles.Name = "chkLoadProfiles";
            this.chkLoadProfiles.Size = new System.Drawing.Size(167, 21);
            this.chkLoadProfiles.TabIndex = 7;
            this.chkLoadProfiles.Text = "Load Profiles on Start";
            this.chkLoadProfiles.UseVisualStyleBackColor = true;
            this.chkLoadProfiles.CheckedChanged += new System.EventHandler(this.chkLoadProfiles_CheckedChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(8, 241);
            this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(413, 17);
            this.label4.TabIndex = 8;
            this.label4.Text = "When unchecked, the host service will not load any profiles on startup.";
            // 
            // chkTabComplete
            // 
            this.chkTabComplete.AutoSize = true;
            this.chkTabComplete.Location = new System.Drawing.Point(8, 272);
            this.chkTabComplete.Margin = new System.Windows.Forms.Padding(4);
            this.chkTabComplete.Name = "chkTabComplete";
            this.chkTabComplete.Size = new System.Drawing.Size(118, 21);
            this.chkTabComplete.TabIndex = 13;
            this.chkTabComplete.Text = "Tab Complete";
            this.chkTabComplete.UseVisualStyleBackColor = true;
            this.chkTabComplete.CheckedChanged += new System.EventHandler(this.chkTabComplete_CheckedChanged);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(8, 297);
            this.label8.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(354, 17);
            this.label8.TabIndex = 15;
            this.label8.Text = "When checked, enable tab complete for PowerShell scripts.";
            // 
            // chkSta
            // 
            this.chkSta.AutoSize = true;
            this.chkSta.Location = new System.Drawing.Point(8, 328);
            this.chkSta.Name = "chkSta";
            this.chkSta.Size = new System.Drawing.Size(282, 21);
            this.chkSta.TabIndex = 16;
            this.chkSta.Text = "Single Threaded Apartment State (STA)";
            this.chkSta.UseVisualStyleBackColor = true;
            this.chkSta.CheckedChanged += new System.EventHandler(this.chkSta_CheckedChanged);
            // 
            // chkDisplayLicenseInfo
            // 
            this.chkDontDisplayLicenseInfo.AutoSize = true;
            this.chkDontDisplayLicenseInfo.Location = new System.Drawing.Point(8, 353);
            this.chkDontDisplayLicenseInfo.Name = "chkDontDisplayLicenseInfo";
            this.chkDontDisplayLicenseInfo.Size = new System.Drawing.Size(282, 21);
            this.chkDontDisplayLicenseInfo.TabIndex = 16;
            this.chkDontDisplayLicenseInfo.Text = "Don't Display License Info on Startup";
            this.chkDontDisplayLicenseInfo.UseVisualStyleBackColor = true;
            this.chkDontDisplayLicenseInfo.CheckedChanged += new System.EventHandler(this.chkDisplayLicenseInfo_CheckedChanged);
            // 
            // GeneralOptionsControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.Controls.Add(this.chkSta);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.chkTabComplete);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.chkLoadProfiles);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.cmoPowerShellVersion);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.chkMultiLineRepl);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.chkOverrideExecutionPolicy);
            this.Controls.Add(this.chkDontDisplayLicenseInfo);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "GeneralOptionsControl";
            this.Size = new System.Drawing.Size(924, 645);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox chkOverrideExecutionPolicy;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox chkMultiLineRepl;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox cmoPowerShellVersion;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.CheckBox chkLoadProfiles;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.CheckBox chkTabComplete;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.CheckBox chkSta;
        private System.Windows.Forms.CheckBox chkDontDisplayLicenseInfo;
    }
}
