namespace CodaClientVSIX
{
    partial class CodaOptionsForm
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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.chkVoteDown = new System.Windows.Forms.CheckBox();
            this.chkVoteUp = new System.Windows.Forms.CheckBox();
            this.chkTshootComment = new System.Windows.Forms.CheckBox();
            this.chkTroubleshoot = new System.Windows.Forms.CheckBox();
            this.chkErrorReport = new System.Windows.Forms.CheckBox();
            this.chkErrorEdits = new System.Windows.Forms.CheckBox();
            this.cboFrequency = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.chkVoteDown);
            this.groupBox1.Controls.Add(this.chkVoteUp);
            this.groupBox1.Controls.Add(this.chkTshootComment);
            this.groupBox1.Controls.Add(this.chkTroubleshoot);
            this.groupBox1.Controls.Add(this.chkErrorReport);
            this.groupBox1.Controls.Add(this.chkErrorEdits);
            this.groupBox1.Controls.Add(this.cboFrequency);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(434, 179);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Notifications";
            // 
            // chkVoteDown
            // 
            this.chkVoteDown.AutoSize = true;
            this.chkVoteDown.Location = new System.Drawing.Point(19, 155);
            this.chkVoteDown.Name = "chkVoteDown";
            this.chkVoteDown.Size = new System.Drawing.Size(79, 17);
            this.chkVoteDown.TabIndex = 2;
            this.chkVoteDown.Text = "Vote Down";
            this.chkVoteDown.UseVisualStyleBackColor = true;
            // 
            // chkVoteUp
            // 
            this.chkVoteUp.AutoSize = true;
            this.chkVoteUp.Location = new System.Drawing.Point(19, 132);
            this.chkVoteUp.Name = "chkVoteUp";
            this.chkVoteUp.Size = new System.Drawing.Size(65, 17);
            this.chkVoteUp.TabIndex = 2;
            this.chkVoteUp.Text = "Vote Up";
            this.chkVoteUp.UseVisualStyleBackColor = true;
            // 
            // chkTshootComment
            // 
            this.chkTshootComment.AutoSize = true;
            this.chkTshootComment.Location = new System.Drawing.Point(19, 109);
            this.chkTshootComment.Name = "chkTshootComment";
            this.chkTshootComment.Size = new System.Drawing.Size(171, 17);
            this.chkTshootComment.TabIndex = 2;
            this.chkTshootComment.Text = "Troubleshoot Comment Posted";
            this.chkTshootComment.UseVisualStyleBackColor = true;
            // 
            // chkTroubleshoot
            // 
            this.chkTroubleshoot.AutoSize = true;
            this.chkTroubleshoot.Location = new System.Drawing.Point(19, 86);
            this.chkTroubleshoot.Name = "chkTroubleshoot";
            this.chkTroubleshoot.Size = new System.Drawing.Size(124, 17);
            this.chkTroubleshoot.TabIndex = 2;
            this.chkTroubleshoot.Text = "Troubleshoot Posted";
            this.chkTroubleshoot.UseVisualStyleBackColor = true;
            // 
            // chkErrorReport
            // 
            this.chkErrorReport.AutoSize = true;
            this.chkErrorReport.Location = new System.Drawing.Point(19, 63);
            this.chkErrorReport.Name = "chkErrorReport";
            this.chkErrorReport.Size = new System.Drawing.Size(88, 17);
            this.chkErrorReport.TabIndex = 2;
            this.chkErrorReport.Text = "Error Reports";
            this.chkErrorReport.UseVisualStyleBackColor = true;
            // 
            // chkErrorEdits
            // 
            this.chkErrorEdits.AutoSize = true;
            this.chkErrorEdits.Location = new System.Drawing.Point(19, 40);
            this.chkErrorEdits.Name = "chkErrorEdits";
            this.chkErrorEdits.Size = new System.Drawing.Size(95, 17);
            this.chkErrorEdits.TabIndex = 2;
            this.chkErrorEdits.Text = "Error Log Edits";
            this.chkErrorEdits.UseVisualStyleBackColor = true;
            // 
            // cboFrequency
            // 
            this.cboFrequency.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboFrequency.FormattingEnabled = true;
            this.cboFrequency.Items.AddRange(new object[] {
            "Immediate",
            "Daily",
            "Weekly"});
            this.cboFrequency.Location = new System.Drawing.Point(82, 13);
            this.cboFrequency.Name = "cboFrequency";
            this.cboFrequency.Size = new System.Drawing.Size(121, 21);
            this.cboFrequency.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(16, 16);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(60, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Frequency:";
            // 
            // btnOK
            // 
            this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOK.Location = new System.Drawing.Point(12, 206);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(75, 23);
            this.btnOK.TabIndex = 1;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(156, 206);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 2;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // CodaOptionsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(243, 243);
            this.ControlBox = false;
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.groupBox1);
            this.Name = "CodaOptionsForm";
            this.Text = "CodaEA Options";
            this.Load += new System.EventHandler(this.CodaOptionsForm_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.CheckBox chkErrorReport;
        private System.Windows.Forms.CheckBox chkErrorEdits;
        private System.Windows.Forms.ComboBox cboFrequency;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.CheckBox chkVoteDown;
        private System.Windows.Forms.CheckBox chkVoteUp;
        private System.Windows.Forms.CheckBox chkTshootComment;
        private System.Windows.Forms.CheckBox chkTroubleshoot;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
    }
}