namespace ASCOM.Simulator
{
	partial class frmSetup
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmSetup));
            this.cmdOK = new System.Windows.Forms.Button();
            this.cmdCancel = new System.Windows.Forms.Button();
            this.lblStepSize = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.chkCanReverse = new System.Windows.Forms.CheckBox();
            this.txtRotationRate = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.chkReverse = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.TxtSyncOfffset = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // cmdOK
            // 
            this.cmdOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdOK.BackColor = System.Drawing.Color.White;
            this.cmdOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.cmdOK.Location = new System.Drawing.Point(117, 151);
            this.cmdOK.Name = "cmdOK";
            this.cmdOK.Size = new System.Drawing.Size(75, 23);
            this.cmdOK.TabIndex = 0;
            this.cmdOK.Text = "OK";
            this.cmdOK.UseVisualStyleBackColor = false;
            this.cmdOK.Click += new System.EventHandler(this.cmdOK_Click);
            // 
            // cmdCancel
            // 
            this.cmdCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdCancel.BackColor = System.Drawing.Color.White;
            this.cmdCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cmdCancel.Location = new System.Drawing.Point(36, 151);
            this.cmdCancel.Name = "cmdCancel";
            this.cmdCancel.Size = new System.Drawing.Size(75, 23);
            this.cmdCancel.TabIndex = 1;
            this.cmdCancel.Text = "Cancel";
            this.cmdCancel.UseVisualStyleBackColor = false;
            this.cmdCancel.Click += new System.EventHandler(this.cmdCancel_Click);
            // 
            // lblStepSize
            // 
            this.lblStepSize.BackColor = System.Drawing.Color.Transparent;
            this.lblStepSize.ForeColor = System.Drawing.Color.White;
            this.lblStepSize.Location = new System.Drawing.Point(30, 89);
            this.lblStepSize.Name = "lblStepSize";
            this.lblStepSize.Size = new System.Drawing.Size(179, 13);
            this.lblStepSize.TabIndex = 2;
            this.lblStepSize.Text = "<runtime>";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.BackColor = System.Drawing.Color.Transparent;
            this.label2.ForeColor = System.Drawing.Color.White;
            this.label2.Location = new System.Drawing.Point(28, 22);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(71, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Rotation rate:";
            // 
            // chkCanReverse
            // 
            this.chkCanReverse.AutoSize = true;
            this.chkCanReverse.ForeColor = System.Drawing.Color.White;
            this.chkCanReverse.Location = new System.Drawing.Point(32, 116);
            this.chkCanReverse.Name = "chkCanReverse";
            this.chkCanReverse.Size = new System.Drawing.Size(85, 17);
            this.chkCanReverse.TabIndex = 4;
            this.chkCanReverse.Text = "CanReverse";
            this.chkCanReverse.UseVisualStyleBackColor = true;
            this.chkCanReverse.CheckedChanged += new System.EventHandler(this.chkCanReverse_CheckedChanged);
            // 
            // txtRotationRate
            // 
            this.txtRotationRate.Location = new System.Drawing.Point(105, 19);
            this.txtRotationRate.Name = "txtRotationRate";
            this.txtRotationRate.Size = new System.Drawing.Size(41, 20);
            this.txtRotationRate.TabIndex = 6;
            this.txtRotationRate.TextChanged += new System.EventHandler(this.txtRotationRate_TextChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.BackColor = System.Drawing.Color.Transparent;
            this.label4.ForeColor = System.Drawing.Color.White;
            this.label4.Location = new System.Drawing.Point(152, 22);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(47, 13);
            this.label4.TabIndex = 8;
            this.label4.Text = "deg/sec";
            // 
            // chkReverse
            // 
            this.chkReverse.AutoSize = true;
            this.chkReverse.BackColor = System.Drawing.Color.Transparent;
            this.chkReverse.ForeColor = System.Drawing.Color.White;
            this.chkReverse.Location = new System.Drawing.Point(135, 116);
            this.chkReverse.Name = "chkReverse";
            this.chkReverse.Size = new System.Drawing.Size(66, 17);
            this.chkReverse.TabIndex = 9;
            this.chkReverse.Text = "Reverse";
            this.chkReverse.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.BackColor = System.Drawing.Color.Transparent;
            this.label1.ForeColor = System.Drawing.Color.White;
            this.label1.Location = new System.Drawing.Point(152, 54);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(25, 13);
            this.label1.TabIndex = 12;
            this.label1.Text = "deg";
            // 
            // TxtSyncOfffset
            // 
            this.TxtSyncOfffset.Location = new System.Drawing.Point(105, 51);
            this.TxtSyncOfffset.Name = "TxtSyncOfffset";
            this.TxtSyncOfffset.Size = new System.Drawing.Size(41, 20);
            this.TxtSyncOfffset.TabIndex = 11;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.BackColor = System.Drawing.Color.Transparent;
            this.label3.ForeColor = System.Drawing.Color.White;
            this.label3.Location = new System.Drawing.Point(36, 54);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(63, 13);
            this.label3.TabIndex = 10;
            this.label3.Text = "Sync offset:";
            // 
            // frmSetup
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.ClientSize = new System.Drawing.Size(227, 186);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.TxtSyncOfffset);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.chkReverse);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.txtRotationRate);
            this.Controls.Add(this.chkCanReverse);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.lblStepSize);
            this.Controls.Add(this.cmdCancel);
            this.Controls.Add(this.cmdOK);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmSetup";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Rotator Simulator Setup";
            this.Load += new System.EventHandler(this.frmSetup_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button cmdOK;
		private System.Windows.Forms.Button cmdCancel;
		private System.Windows.Forms.Label lblStepSize;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.CheckBox chkCanReverse;
		private System.Windows.Forms.TextBox txtRotationRate;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.CheckBox chkReverse;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox TxtSyncOfffset;
        private System.Windows.Forms.Label label3;
    }
}