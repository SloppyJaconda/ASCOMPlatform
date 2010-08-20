﻿namespace PyxisLE_Control
{
    partial class MainForm
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.StatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.label1 = new System.Windows.Forms.Label();
            this.SkyPA_TB = new System.Windows.Forms.TextBox();
            this.HomeBTN = new System.Windows.Forms.Button();
            this.SetPA_BTN = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.AbsoluteMove_TB = new System.Windows.Forms.TextBox();
            this.AbsoluteMove_BTN = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.RotatorDiagram = new System.Windows.Forms.PictureBox();
            this.RelativeIncrement_TB = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.RelativeForward_BTN = new System.Windows.Forms.Button();
            this.RelativeReverse_Btn = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label6 = new System.Windows.Forms.Label();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showDeviceDiagramToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deviceToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.advancedSetupToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.diagnosticsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ablueToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.checkForUpdatesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deviceDocumentationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.errorProvider1 = new System.Windows.Forms.ErrorProvider(this.components);
            this.label2 = new System.Windows.Forms.Label();
            this.statusStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.RotatorDiagram)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider1)).BeginInit();
            this.SuspendLayout();
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.StatusLabel});
            this.statusStrip1.Location = new System.Drawing.Point(0, 490);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(376, 22);
            this.statusStrip1.TabIndex = 0;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // StatusLabel
            // 
            this.StatusLabel.Name = "StatusLabel";
            this.StatusLabel.Size = new System.Drawing.Size(77, 17);
            this.StatusLabel.Text = "Device Status";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(31, 36);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(163, 20);
            this.label1.TabIndex = 1;
            this.label1.Text = "Sky Position Angle:";
            // 
            // SkyPA_TB
            // 
            this.SkyPA_TB.Cursor = System.Windows.Forms.Cursors.No;
            this.SkyPA_TB.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.SkyPA_TB.Location = new System.Drawing.Point(192, 33);
            this.SkyPA_TB.Name = "SkyPA_TB";
            this.SkyPA_TB.ReadOnly = true;
            this.SkyPA_TB.Size = new System.Drawing.Size(73, 26);
            this.SkyPA_TB.TabIndex = 2;
            this.SkyPA_TB.TabStop = false;
            // 
            // HomeBTN
            // 
            this.HomeBTN.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.HomeBTN.Location = new System.Drawing.Point(125, 337);
            this.HomeBTN.Name = "HomeBTN";
            this.HomeBTN.Size = new System.Drawing.Size(73, 23);
            this.HomeBTN.TabIndex = 4;
            this.HomeBTN.Text = "Home";
            this.HomeBTN.UseVisualStyleBackColor = true;
            this.HomeBTN.Click += new System.EventHandler(this.HomeBTN_Click);
            // 
            // SetPA_BTN
            // 
            this.SetPA_BTN.Location = new System.Drawing.Point(271, 35);
            this.SetPA_BTN.Name = "SetPA_BTN";
            this.SetPA_BTN.Size = new System.Drawing.Size(75, 23);
            this.SetPA_BTN.TabIndex = 5;
            this.SetPA_BTN.Text = "Change...";
            this.SetPA_BTN.UseVisualStyleBackColor = true;
            this.SetPA_BTN.Click += new System.EventHandler(this.SetPA_BTN_Click);
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(32, 374);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(155, 13);
            this.label3.TabIndex = 6;
            this.label3.Text = "Move to Absolute Sky PA:";
            // 
            // AbsoluteMove_TB
            // 
            this.AbsoluteMove_TB.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.AbsoluteMove_TB.Location = new System.Drawing.Point(199, 371);
            this.AbsoluteMove_TB.Name = "AbsoluteMove_TB";
            this.AbsoluteMove_TB.Size = new System.Drawing.Size(73, 20);
            this.AbsoluteMove_TB.TabIndex = 7;
            this.AbsoluteMove_TB.Text = "0";
            this.AbsoluteMove_TB.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.AbsoluteMove_TB.TextChanged += new System.EventHandler(this.AbsoluteMove_TB_TextChanged);
            this.AbsoluteMove_TB.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.AbsoluteMove_TB_KeyPress);
            this.AbsoluteMove_TB.Validating += new System.ComponentModel.CancelEventHandler(this.AbsoluteMove_TB_Validating);
            // 
            // AbsoluteMove_BTN
            // 
            this.AbsoluteMove_BTN.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.AbsoluteMove_BTN.Location = new System.Drawing.Point(297, 369);
            this.AbsoluteMove_BTN.Name = "AbsoluteMove_BTN";
            this.AbsoluteMove_BTN.Size = new System.Drawing.Size(37, 23);
            this.AbsoluteMove_BTN.TabIndex = 8;
            this.AbsoluteMove_BTN.Text = "Go";
            this.AbsoluteMove_BTN.UseVisualStyleBackColor = true;
            this.AbsoluteMove_BTN.Click += new System.EventHandler(this.AbsoluteMove_BTN_Click);
            // 
            // label4
            // 
            this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(32, 342);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(87, 13);
            this.label4.TabIndex = 9;
            this.label4.Text = "Home Device:";
            // 
            // RotatorDiagram
            // 
            this.RotatorDiagram.Cursor = System.Windows.Forms.Cursors.Cross;
            this.RotatorDiagram.Location = new System.Drawing.Point(51, 71);
            this.RotatorDiagram.Name = "RotatorDiagram";
            this.RotatorDiagram.Size = new System.Drawing.Size(260, 260);
            this.RotatorDiagram.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.RotatorDiagram.TabIndex = 10;
            this.RotatorDiagram.TabStop = false;
            this.RotatorDiagram.Click += new System.EventHandler(this.RotatorDiagram_Click);
            this.RotatorDiagram.Paint += new System.Windows.Forms.PaintEventHandler(this.RotatorDiagram_Paint);
            // 
            // RelativeIncrement_TB
            // 
            this.RelativeIncrement_TB.Location = new System.Drawing.Point(121, 35);
            this.RelativeIncrement_TB.Name = "RelativeIncrement_TB";
            this.RelativeIncrement_TB.Size = new System.Drawing.Size(71, 20);
            this.RelativeIncrement_TB.TabIndex = 11;
            this.RelativeIncrement_TB.Text = "10";
            this.RelativeIncrement_TB.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.RelativeIncrement_TB.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.RelativeIncrement_TB_KeyPress);
            this.RelativeIncrement_TB.Validating += new System.ComponentModel.CancelEventHandler(this.RelativeIncrement_TB_Validating);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(126, 17);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(63, 13);
            this.label5.TabIndex = 12;
            this.label5.Text = "Increment";
            // 
            // RelativeForward_BTN
            // 
            this.RelativeForward_BTN.Location = new System.Drawing.Point(208, 34);
            this.RelativeForward_BTN.Name = "RelativeForward_BTN";
            this.RelativeForward_BTN.Size = new System.Drawing.Size(90, 23);
            this.RelativeForward_BTN.TabIndex = 13;
            this.RelativeForward_BTN.Text = ">>";
            this.RelativeForward_BTN.UseVisualStyleBackColor = true;
            this.RelativeForward_BTN.Click += new System.EventHandler(this.RelativeForward_BTN_Click);
            // 
            // RelativeReverse_Btn
            // 
            this.RelativeReverse_Btn.Location = new System.Drawing.Point(9, 34);
            this.RelativeReverse_Btn.Name = "RelativeReverse_Btn";
            this.RelativeReverse_Btn.Size = new System.Drawing.Size(96, 23);
            this.RelativeReverse_Btn.TabIndex = 14;
            this.RelativeReverse_Btn.Text = "<<";
            this.RelativeReverse_Btn.UseVisualStyleBackColor = true;
            this.RelativeReverse_Btn.Click += new System.EventHandler(this.RelativeReverse_Btn_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.groupBox1.Controls.Add(this.label6);
            this.groupBox1.Controls.Add(this.RelativeReverse_Btn);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.RelativeForward_BTN);
            this.groupBox1.Controls.Add(this.RelativeIncrement_TB);
            this.groupBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox1.Location = new System.Drawing.Point(35, 399);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(307, 74);
            this.groupBox1.TabIndex = 15;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Relative Move";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.BackColor = System.Drawing.Color.White;
            this.label6.Location = new System.Drawing.Point(179, 38);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(12, 13);
            this.label6.TabIndex = 18;
            this.label6.Text = "°";
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.viewToolStripMenuItem,
            this.deviceToolStripMenuItem,
            this.helpToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(376, 24);
            this.menuStrip1.TabIndex = 16;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(92, 22);
            this.exitToolStripMenuItem.Text = "Exit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // viewToolStripMenuItem
            // 
            this.viewToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.showDeviceDiagramToolStripMenuItem});
            this.viewToolStripMenuItem.Name = "viewToolStripMenuItem";
            this.viewToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            this.viewToolStripMenuItem.Text = "View";
            // 
            // showDeviceDiagramToolStripMenuItem
            // 
            this.showDeviceDiagramToolStripMenuItem.Name = "showDeviceDiagramToolStripMenuItem";
            this.showDeviceDiagramToolStripMenuItem.Size = new System.Drawing.Size(223, 22);
            this.showDeviceDiagramToolStripMenuItem.Text = "Show/Hide Rotator Diagram";
            this.showDeviceDiagramToolStripMenuItem.Click += new System.EventHandler(this.showDeviceDiagramToolStripMenuItem_Click);
            // 
            // deviceToolStripMenuItem
            // 
            this.deviceToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.advancedSetupToolStripMenuItem,
            this.diagnosticsToolStripMenuItem});
            this.deviceToolStripMenuItem.Name = "deviceToolStripMenuItem";
            this.deviceToolStripMenuItem.Size = new System.Drawing.Size(54, 20);
            this.deviceToolStripMenuItem.Text = "Device";
            // 
            // advancedSetupToolStripMenuItem
            // 
            this.advancedSetupToolStripMenuItem.Name = "advancedSetupToolStripMenuItem";
            this.advancedSetupToolStripMenuItem.Size = new System.Drawing.Size(160, 22);
            this.advancedSetupToolStripMenuItem.Text = "Advanced Setup";
            this.advancedSetupToolStripMenuItem.Click += new System.EventHandler(this.advancedSetupToolStripMenuItem_Click);
            // 
            // diagnosticsToolStripMenuItem
            // 
            this.diagnosticsToolStripMenuItem.Name = "diagnosticsToolStripMenuItem";
            this.diagnosticsToolStripMenuItem.Size = new System.Drawing.Size(160, 22);
            this.diagnosticsToolStripMenuItem.Text = "Diagnostics";
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ablueToolStripMenuItem,
            this.checkForUpdatesToolStripMenuItem,
            this.deviceDocumentationToolStripMenuItem});
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            this.helpToolStripMenuItem.Text = "Help";
            // 
            // ablueToolStripMenuItem
            // 
            this.ablueToolStripMenuItem.Name = "ablueToolStripMenuItem";
            this.ablueToolStripMenuItem.Size = new System.Drawing.Size(195, 22);
            this.ablueToolStripMenuItem.Text = "About";
            // 
            // checkForUpdatesToolStripMenuItem
            // 
            this.checkForUpdatesToolStripMenuItem.Name = "checkForUpdatesToolStripMenuItem";
            this.checkForUpdatesToolStripMenuItem.Size = new System.Drawing.Size(195, 22);
            this.checkForUpdatesToolStripMenuItem.Text = "Check for Updates";
            // 
            // deviceDocumentationToolStripMenuItem
            // 
            this.deviceDocumentationToolStripMenuItem.Name = "deviceDocumentationToolStripMenuItem";
            this.deviceDocumentationToolStripMenuItem.Size = new System.Drawing.Size(195, 22);
            this.deviceDocumentationToolStripMenuItem.Text = "Device Documentation";
            // 
            // errorProvider1
            // 
            this.errorProvider1.ContainerControl = this;
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label2.AutoSize = true;
            this.label2.BackColor = System.Drawing.Color.White;
            this.label2.Location = new System.Drawing.Point(243, 374);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(11, 13);
            this.label2.TabIndex = 17;
            this.label2.Text = "°";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(376, 512);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.AbsoluteMove_BTN);
            this.Controls.Add(this.AbsoluteMove_TB);
            this.Controls.Add(this.HomeBTN);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.SetPA_BTN);
            this.Controls.Add(this.SkyPA_TB);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.menuStrip1);
            this.Controls.Add(this.RotatorDiagram);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "MainForm";
            this.Text = "Pyxis LE Control";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.RotatorDiagram)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel StatusLabel;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox SkyPA_TB;
        private System.Windows.Forms.Button HomeBTN;
        private System.Windows.Forms.Button SetPA_BTN;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox AbsoluteMove_TB;
        private System.Windows.Forms.Button AbsoluteMove_BTN;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.PictureBox RotatorDiagram;
        private System.Windows.Forms.TextBox RelativeIncrement_TB;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button RelativeForward_BTN;
        private System.Windows.Forms.Button RelativeReverse_Btn;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem deviceToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem advancedSetupToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem diagnosticsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem ablueToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem checkForUpdatesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem deviceDocumentationToolStripMenuItem;
        private System.Windows.Forms.ErrorProvider errorProvider1;
        private System.Windows.Forms.ToolStripMenuItem viewToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem showDeviceDiagramToolStripMenuItem;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label6;
    }
}