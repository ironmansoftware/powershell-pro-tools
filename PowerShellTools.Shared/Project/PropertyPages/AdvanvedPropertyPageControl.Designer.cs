namespace PowerShellTools.Project.PropertyPages
{
    partial class AdvanvedPropertyPageControl
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
            this.label1 = new System.Windows.Forms.Label();
            this.linkLabel1 = new System.Windows.Forms.LinkLabel();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.lblEntryPoint = new System.Windows.Forms.Label();
            this.chkPackage = new System.Windows.Forms.CheckBox();
            this.cmoEntryScript = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.chkBundle = new System.Windows.Forms.CheckBox();
            this.lblBundle = new System.Windows.Forms.Label();
            this.lblEntryPointDesc = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 10);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(368, 52);
            this.label1.TabIndex = 0;
            this.label1.Text = "Advanced properties are available in PowerShell Pro Tools for Visual Studio. \r\n\r\n" +
    "For more information, visit\r\n ";
            // 
            // linkLabel1
            // 
            this.linkLabel1.AutoSize = true;
            this.linkLabel1.Location = new System.Drawing.Point(137, 37);
            this.linkLabel1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.linkLabel1.Name = "linkLabel1";
            this.linkLabel1.Size = new System.Drawing.Size(80, 13);
            this.linkLabel1.TabIndex = 1;
            this.linkLabel1.TabStop = true;
            this.linkLabel1.Text = "IronmanSoftware.com";
            this.linkLabel1.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel1_LinkClicked);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.lblEntryPoint);
            this.groupBox1.Controls.Add(this.chkPackage);
            this.groupBox1.Controls.Add(this.cmoEntryScript);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.chkBundle);
            this.groupBox1.Controls.Add(this.lblBundle);
            this.groupBox1.Controls.Add(this.lblEntryPointDesc);
            this.groupBox1.Enabled = false;
            this.groupBox1.Location = new System.Drawing.Point(14, 67);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.groupBox1.Size = new System.Drawing.Size(388, 318);
            this.groupBox1.TabIndex = 9;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Packaging Settings";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(21, 228);
            this.label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(292, 39);
            this.label3.TabIndex = 8;
            this.label3.Text = "Outputs an executable that contains the script. Launching\r\nthe executable will ex" +
    "ecute the script. This can be combined\r\nwith bundling to merge several scripts i" +
    "nto an executable.";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(12, 207);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(141, 13);
            this.label2.TabIndex = 7;
            this.label2.Text = "Package as Executable";
            // 
            // lblEntryPoint
            // 
            this.lblEntryPoint.AutoSize = true;
            this.lblEntryPoint.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblEntryPoint.Location = new System.Drawing.Point(14, 25);
            this.lblEntryPoint.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblEntryPoint.Name = "lblEntryPoint";
            this.lblEntryPoint.Size = new System.Drawing.Size(69, 13);
            this.lblEntryPoint.TabIndex = 2;
            this.lblEntryPoint.Text = "Entry Point";
            // 
            // chkPackage
            // 
            this.chkPackage.AutoSize = true;
            this.chkPackage.Enabled = false;
            this.chkPackage.Location = new System.Drawing.Point(23, 279);
            this.chkPackage.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.chkPackage.Name = "chkPackage";
            this.chkPackage.Size = new System.Drawing.Size(138, 17);
            this.chkPackage.TabIndex = 0;
            this.chkPackage.Text = "Package as executable";
            this.chkPackage.UseVisualStyleBackColor = true;
            // 
            // cmoEntryScript
            // 
            this.cmoEntryScript.Enabled = false;
            this.cmoEntryScript.FormattingEnabled = true;
            this.cmoEntryScript.Location = new System.Drawing.Point(23, 70);
            this.cmoEntryScript.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.cmoEntryScript.Name = "cmoEntryScript";
            this.cmoEntryScript.Size = new System.Drawing.Size(353, 21);
            this.cmoEntryScript.TabIndex = 1;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(21, 128);
            this.label4.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(342, 39);
            this.label4.TabIndex = 6;
            this.label4.Text = "Specifies whether to bundle dot source files included by the entry point.\r\nThis s" +
    "etting is recursive. Dot source files that contain other dot sourced\r\nfiles will" +
    " also be included.";
            // 
            // chkBundle
            // 
            this.chkBundle.AutoSize = true;
            this.chkBundle.Enabled = false;
            this.chkBundle.Location = new System.Drawing.Point(23, 181);
            this.chkBundle.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.chkBundle.Name = "chkBundle";
            this.chkBundle.Size = new System.Drawing.Size(59, 17);
            this.chkBundle.TabIndex = 3;
            this.chkBundle.Text = "Bundle";
            this.chkBundle.UseVisualStyleBackColor = true;
            // 
            // lblBundle
            // 
            this.lblBundle.AutoSize = true;
            this.lblBundle.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblBundle.Location = new System.Drawing.Point(12, 103);
            this.lblBundle.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblBundle.Name = "lblBundle";
            this.lblBundle.Size = new System.Drawing.Size(46, 13);
            this.lblBundle.TabIndex = 5;
            this.lblBundle.Text = "Bundle";
            // 
            // lblEntryPointDesc
            // 
            this.lblEntryPointDesc.AutoSize = true;
            this.lblEntryPointDesc.Location = new System.Drawing.Point(21, 49);
            this.lblEntryPointDesc.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblEntryPointDesc.Name = "lblEntryPointDesc";
            this.lblEntryPointDesc.Size = new System.Drawing.Size(251, 13);
            this.lblEntryPointDesc.TabIndex = 4;
            this.lblEntryPointDesc.Text = "Defines the entry point of the bundle or executable. \r\n";
            // 
            // AdvanvedPropertyPageControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.linkLabel1);
            this.Controls.Add(this.label1);
            this.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.Name = "AdvanvedPropertyPageControl";
            this.Size = new System.Drawing.Size(734, 597);
            this.Load += new System.EventHandler(this.AdvanvedPropertyPageControl_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.LinkLabel linkLabel1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label lblEntryPoint;
        private System.Windows.Forms.CheckBox chkPackage;
        private System.Windows.Forms.ComboBox cmoEntryScript;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.CheckBox chkBundle;
        private System.Windows.Forms.Label lblBundle;
        private System.Windows.Forms.Label lblEntryPointDesc;
    }
}
