namespace BSH.Controls.UI
{
    partial class dlgInputBox
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
            this.txtInput = new System.Windows.Forms.TextBox();
            this.lblText = new System.Windows.Forms.Label();
            this.Panel1 = new System.Windows.Forms.Panel();
            this.btnCancel = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.btnOk = new System.Windows.Forms.Button();
            this.lblTitle = new System.Windows.Forms.Label();
            this.Panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // txtInput
            // 
            this.txtInput.Location = new System.Drawing.Point(21, 167);
            this.txtInput.Name = "txtInput";
            this.txtInput.Size = new System.Drawing.Size(718, 31);
            this.txtInput.TabIndex = 2;
            this.txtInput.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtInput_KeyDown);
            // 
            // lblText
            // 
            this.lblText.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblText.Location = new System.Drawing.Point(17, 68);
            this.lblText.Name = "lblText";
            this.lblText.Size = new System.Drawing.Size(722, 73);
            this.lblText.TabIndex = 3;
            this.lblText.Text = "# Description";
            // 
            // Panel1
            // 
            this.Panel1.BackColor = System.Drawing.SystemColors.Control;
            this.Panel1.Controls.Add(this.btnCancel);
            this.Panel1.Controls.Add(this.label2);
            this.Panel1.Controls.Add(this.btnOk);
            this.Panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.Panel1.Location = new System.Drawing.Point(0, 235);
            this.Panel1.Name = "Panel1";
            this.Panel1.Size = new System.Drawing.Size(758, 68);
            this.Panel1.TabIndex = 83;
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Font = new System.Drawing.Font("Segoe UI", 9.75F);
            this.btnCancel.Location = new System.Drawing.Point(453, 12);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(140, 39);
            this.btnCancel.TabIndex = 6;
            this.btnCancel.Text = "&Abbrechen";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            this.label2.BackColor = System.Drawing.Color.DarkGray;
            this.label2.Dock = System.Windows.Forms.DockStyle.Top;
            this.label2.Location = new System.Drawing.Point(0, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(758, 2);
            this.label2.TabIndex = 5;
            // 
            // btnOk
            // 
            this.btnOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOk.Font = new System.Drawing.Font("Segoe UI", 9.75F);
            this.btnOk.Location = new System.Drawing.Point(599, 12);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(140, 39);
            this.btnOk.TabIndex = 1;
            this.btnOk.Text = "&OK";
            this.btnOk.UseVisualStyleBackColor = true;
            // 
            // lblTitle
            // 
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.lblTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(153)))));
            this.lblTitle.Location = new System.Drawing.Point(15, 18);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(81, 32);
            this.lblTitle.TabIndex = 85;
            this.lblTitle.Text = "# Title";
            // 
            // dlgInputBox
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(144F, 144F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(758, 303);
            this.Controls.Add(this.lblTitle);
            this.Controls.Add(this.Panel1);
            this.Controls.Add(this.lblText);
            this.Controls.Add(this.txtInput);
            this.Font = new System.Drawing.Font("Calibri", 9.75F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "dlgInputBox";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "# Title";
            this.Panel1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        internal System.Windows.Forms.Panel Panel1;
        internal System.Windows.Forms.Button btnCancel;
        internal System.Windows.Forms.Label label2;
        internal System.Windows.Forms.Button btnOk;
        internal System.Windows.Forms.Label lblTitle;
        internal System.Windows.Forms.Label lblText;
        internal System.Windows.Forms.TextBox txtInput;
    }
}