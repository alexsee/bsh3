using System.Diagnostics;
using System.Windows.Forms;

namespace Brightbits.BSH.Main
{
    public partial class frmFileOverrides : Form
    {

        // Das Formular überschreibt den Löschvorgang, um die Komponentenliste zu bereinigen.
        [DebuggerNonUserCode()]
        protected override void Dispose(bool disposing)
        {
            if (disposing && components is object)
            {
                components.Dispose();
            }

            base.Dispose(disposing);
        }

        // Wird vom Windows Form-Designer benötigt.
        private System.ComponentModel.IContainer components;

        // Hinweis: Die folgende Prozedur ist für den Windows Form-Designer erforderlich.
        // Das Bearbeiten ist mit dem Windows Form-Designer möglich.  
        // Das Bearbeiten mit dem Code-Editor ist nicht möglich.
        [DebuggerStepThrough()]
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmFileOverrides));
            this.Panel1 = new System.Windows.Forms.Panel();
            this.chkAllConflicts = new System.Windows.Forms.CheckBox();
            this.Label1 = new System.Windows.Forms.Label();
            this.cmdCancel = new System.Windows.Forms.Button();
            this.Label2 = new System.Windows.Forms.Label();
            this.lblText = new System.Windows.Forms.Label();
            this.lblOverride = new System.Windows.Forms.Label();
            this.lblOverride2 = new System.Windows.Forms.Label();
            this.Label5 = new System.Windows.Forms.Label();
            this.Label6 = new System.Windows.Forms.Label();
            this.plReplace = new System.Windows.Forms.Panel();
            this.lblFileDateChanged1 = new System.Windows.Forms.Label();
            this.lblFileSize1 = new System.Windows.Forms.Label();
            this.lblFileName1 = new System.Windows.Forms.Label();
            this.picIco1 = new System.Windows.Forms.PictureBox();
            this.plCancel = new System.Windows.Forms.Panel();
            this.lblFileDateChanged2 = new System.Windows.Forms.Label();
            this.lblFileSize2 = new System.Windows.Forms.Label();
            this.lblFileName2 = new System.Windows.Forms.Label();
            this.picIco2 = new System.Windows.Forms.PictureBox();
            this.Panel1.SuspendLayout();
            this.plReplace.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picIco1)).BeginInit();
            this.plCancel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picIco2)).BeginInit();
            this.SuspendLayout();
            // 
            // Panel1
            // 
            this.Panel1.BackColor = System.Drawing.SystemColors.Control;
            this.Panel1.Controls.Add(this.chkAllConflicts);
            this.Panel1.Controls.Add(this.Label1);
            this.Panel1.Controls.Add(this.cmdCancel);
            this.Panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.Panel1.Location = new System.Drawing.Point(0, 445);
            this.Panel1.Name = "Panel1";
            this.Panel1.Size = new System.Drawing.Size(660, 68);
            this.Panel1.TabIndex = 82;
            // 
            // chkAllConflicts
            // 
            this.chkAllConflicts.AutoSize = true;
            this.chkAllConflicts.Location = new System.Drawing.Point(22, 22);
            this.chkAllConflicts.Name = "chkAllConflicts";
            this.chkAllConflicts.Size = new System.Drawing.Size(290, 28);
            this.chkAllConflicts.TabIndex = 6;
            this.chkAllConflicts.Text = "Für alle Konflikte übernehmen";
            this.chkAllConflicts.UseVisualStyleBackColor = true;
            // 
            // Label1
            // 
            this.Label1.BackColor = System.Drawing.Color.DarkGray;
            this.Label1.Dock = System.Windows.Forms.DockStyle.Top;
            this.Label1.Location = new System.Drawing.Point(0, 0);
            this.Label1.Name = "Label1";
            this.Label1.Size = new System.Drawing.Size(660, 2);
            this.Label1.TabIndex = 5;
            // 
            // cmdCancel
            // 
            this.cmdCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cmdCancel.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmdCancel.Location = new System.Drawing.Point(498, 14);
            this.cmdCancel.Name = "cmdCancel";
            this.cmdCancel.Size = new System.Drawing.Size(140, 39);
            this.cmdCancel.TabIndex = 1;
            this.cmdCancel.Text = "&Abbrechen";
            this.cmdCancel.UseVisualStyleBackColor = true;
            // 
            // Label2
            // 
            this.Label2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.Label2.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Label2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(153)))));
            this.Label2.Location = new System.Drawing.Point(16, 20);
            this.Label2.Name = "Label2";
            this.Label2.Size = new System.Drawing.Size(622, 70);
            this.Label2.TabIndex = 84;
            this.Label2.Text = "Es befindet sich bereits eine Datei desselben Namens an diesem Ort.";
            // 
            // lblText
            // 
            this.lblText.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblText.Location = new System.Drawing.Point(18, 100);
            this.lblText.Name = "lblText";
            this.lblText.Size = new System.Drawing.Size(621, 28);
            this.lblText.TabIndex = 85;
            this.lblText.Text = "Klicken Sie auf die Datei, die Sie behalten möchten.";
            // 
            // lblOverride
            // 
            this.lblOverride.AutoSize = true;
            this.lblOverride.BackColor = System.Drawing.Color.Transparent;
            this.lblOverride.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblOverride.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(153)))));
            this.lblOverride.Location = new System.Drawing.Point(63, 6);
            this.lblOverride.Name = "lblOverride";
            this.lblOverride.Size = new System.Drawing.Size(255, 32);
            this.lblOverride.TabIndex = 86;
            this.lblOverride.Text = "Kopieren und ersetzen";
            this.lblOverride.Click += new System.EventHandler(this.lblOverride_Click);
            // 
            // lblOverride2
            // 
            this.lblOverride2.BackColor = System.Drawing.Color.Transparent;
            this.lblOverride2.ForeColor = System.Drawing.Color.Black;
            this.lblOverride2.Location = new System.Drawing.Point(64, 42);
            this.lblOverride2.Name = "lblOverride2";
            this.lblOverride2.Size = new System.Drawing.Size(537, 28);
            this.lblOverride2.TabIndex = 87;
            this.lblOverride2.Text = "Datei im Zielordner durch Datei in Sicherung ersetzen:\r\n";
            this.lblOverride2.Click += new System.EventHandler(this.lblOverride2_Click);
            // 
            // Label5
            // 
            this.Label5.AutoSize = true;
            this.Label5.BackColor = System.Drawing.Color.Transparent;
            this.Label5.ForeColor = System.Drawing.Color.Black;
            this.Label5.Location = new System.Drawing.Point(64, 44);
            this.Label5.Name = "Label5";
            this.Label5.Size = new System.Drawing.Size(307, 24);
            this.Label5.TabIndex = 89;
            this.Label5.Text = "Es werden keine Dateien verändert.";
            this.Label5.Click += new System.EventHandler(this.Label5_Click);
            // 
            // Label6
            // 
            this.Label6.AutoSize = true;
            this.Label6.BackColor = System.Drawing.Color.Transparent;
            this.Label6.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Label6.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(153)))));
            this.Label6.Location = new System.Drawing.Point(63, 6);
            this.Label6.Name = "Label6";
            this.Label6.Size = new System.Drawing.Size(172, 32);
            this.Label6.TabIndex = 88;
            this.Label6.Text = "Nicht kopieren";
            this.Label6.Click += new System.EventHandler(this.Label6_Click);
            // 
            // plReplace
            // 
            this.plReplace.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.plReplace.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("plReplace.BackgroundImage")));
            this.plReplace.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.plReplace.Controls.Add(this.lblFileDateChanged1);
            this.plReplace.Controls.Add(this.lblFileSize1);
            this.plReplace.Controls.Add(this.lblFileName1);
            this.plReplace.Controls.Add(this.picIco1);
            this.plReplace.Controls.Add(this.lblOverride);
            this.plReplace.Controls.Add(this.lblOverride2);
            this.plReplace.Location = new System.Drawing.Point(22, 132);
            this.plReplace.Name = "plReplace";
            this.plReplace.Size = new System.Drawing.Size(615, 136);
            this.plReplace.TabIndex = 90;
            this.plReplace.MouseClick += new System.Windows.Forms.MouseEventHandler(this.plReplace_MouseClick);
            // 
            // lblFileDateChanged1
            // 
            this.lblFileDateChanged1.AutoSize = true;
            this.lblFileDateChanged1.Font = new System.Drawing.Font("Calibri", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblFileDateChanged1.Location = new System.Drawing.Point(128, 111);
            this.lblFileDateChanged1.Name = "lblFileDateChanged1";
            this.lblFileDateChanged1.Size = new System.Drawing.Size(138, 22);
            this.lblFileDateChanged1.TabIndex = 91;
            this.lblFileDateChanged1.Text = "Änderungsdatum:";
            this.lblFileDateChanged1.Click += new System.EventHandler(this.lblFileDateChanged1_Click);
            // 
            // lblFileSize1
            // 
            this.lblFileSize1.AutoSize = true;
            this.lblFileSize1.Font = new System.Drawing.Font("Calibri", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblFileSize1.Location = new System.Drawing.Point(128, 90);
            this.lblFileSize1.Name = "lblFileSize1";
            this.lblFileSize1.Size = new System.Drawing.Size(61, 22);
            this.lblFileSize1.TabIndex = 90;
            this.lblFileSize1.Text = "Größe:";
            this.lblFileSize1.Click += new System.EventHandler(this.lblFileSize1_Click);
            // 
            // lblFileName1
            // 
            this.lblFileName1.AutoSize = true;
            this.lblFileName1.Font = new System.Drawing.Font("Calibri", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblFileName1.Location = new System.Drawing.Point(128, 68);
            this.lblFileName1.Name = "lblFileName1";
            this.lblFileName1.Size = new System.Drawing.Size(130, 24);
            this.lblFileName1.TabIndex = 89;
            this.lblFileName1.Text = "Dateiname.txt";
            this.lblFileName1.Click += new System.EventHandler(this.lblFileName1_Click);
            // 
            // picIco1
            // 
            this.picIco1.Location = new System.Drawing.Point(69, 68);
            this.picIco1.Name = "picIco1";
            this.picIco1.Size = new System.Drawing.Size(48, 48);
            this.picIco1.TabIndex = 88;
            this.picIco1.TabStop = false;
            this.picIco1.Click += new System.EventHandler(this.picIco1_Click);
            // 
            // plCancel
            // 
            this.plCancel.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("plCancel.BackgroundImage")));
            this.plCancel.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.plCancel.Controls.Add(this.lblFileDateChanged2);
            this.plCancel.Controls.Add(this.lblFileSize2);
            this.plCancel.Controls.Add(this.lblFileName2);
            this.plCancel.Controls.Add(this.picIco2);
            this.plCancel.Controls.Add(this.Label6);
            this.plCancel.Controls.Add(this.Label5);
            this.plCancel.Location = new System.Drawing.Point(22, 274);
            this.plCancel.Name = "plCancel";
            this.plCancel.Size = new System.Drawing.Size(615, 147);
            this.plCancel.TabIndex = 91;
            this.plCancel.MouseClick += new System.Windows.Forms.MouseEventHandler(this.plCancel_MouseClick);
            // 
            // lblFileDateChanged2
            // 
            this.lblFileDateChanged2.AutoSize = true;
            this.lblFileDateChanged2.Font = new System.Drawing.Font("Calibri", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblFileDateChanged2.Location = new System.Drawing.Point(128, 114);
            this.lblFileDateChanged2.Name = "lblFileDateChanged2";
            this.lblFileDateChanged2.Size = new System.Drawing.Size(138, 22);
            this.lblFileDateChanged2.TabIndex = 95;
            this.lblFileDateChanged2.Text = "Änderungsdatum:";
            this.lblFileDateChanged2.Click += new System.EventHandler(this.lblFileDateChanged2_Click);
            // 
            // lblFileSize2
            // 
            this.lblFileSize2.AutoSize = true;
            this.lblFileSize2.Font = new System.Drawing.Font("Calibri", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblFileSize2.Location = new System.Drawing.Point(128, 93);
            this.lblFileSize2.Name = "lblFileSize2";
            this.lblFileSize2.Size = new System.Drawing.Size(61, 22);
            this.lblFileSize2.TabIndex = 94;
            this.lblFileSize2.Text = "Größe:";
            this.lblFileSize2.Click += new System.EventHandler(this.lblFileSize2_Click);
            // 
            // lblFileName2
            // 
            this.lblFileName2.AutoSize = true;
            this.lblFileName2.Font = new System.Drawing.Font("Calibri", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblFileName2.Location = new System.Drawing.Point(128, 69);
            this.lblFileName2.Name = "lblFileName2";
            this.lblFileName2.Size = new System.Drawing.Size(130, 24);
            this.lblFileName2.TabIndex = 93;
            this.lblFileName2.Text = "Dateiname.txt";
            this.lblFileName2.Click += new System.EventHandler(this.lblFileName2_Click);
            // 
            // picIco2
            // 
            this.picIco2.Location = new System.Drawing.Point(69, 69);
            this.picIco2.Name = "picIco2";
            this.picIco2.Size = new System.Drawing.Size(48, 48);
            this.picIco2.TabIndex = 92;
            this.picIco2.TabStop = false;
            this.picIco2.Click += new System.EventHandler(this.picIco2_Click);
            // 
            // frmFileOverrides
            // 
            this.AcceptButton = this.cmdCancel;
            this.AutoScaleDimensions = new System.Drawing.SizeF(144F, 144F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(660, 513);
            this.Controls.Add(this.plCancel);
            this.Controls.Add(this.plReplace);
            this.Controls.Add(this.lblText);
            this.Controls.Add(this.Label2);
            this.Controls.Add(this.Panel1);
            this.Font = new System.Drawing.Font("Calibri", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ForeColor = System.Drawing.Color.Black;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmFileOverrides";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Datei überschreiben?";
            this.Shown += new System.EventHandler(this.frmFileOverrides_Shown);
            this.Panel1.ResumeLayout(false);
            this.Panel1.PerformLayout();
            this.plReplace.ResumeLayout(false);
            this.plReplace.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picIco1)).EndInit();
            this.plCancel.ResumeLayout(false);
            this.plCancel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picIco2)).EndInit();
            this.ResumeLayout(false);

        }

        internal Panel Panel1;
        internal Label Label1;
        internal Button cmdCancel;
        internal Label Label2;
        internal Label lblText;
        internal Label lblOverride;
        internal Label lblOverride2;
        internal Label Label5;
        internal Label Label6;
        internal Panel plReplace;
        internal Panel plCancel;
        internal Label lblFileDateChanged1;
        internal Label lblFileSize1;
        internal Label lblFileName1;
        internal PictureBox picIco1;
        internal Label lblFileDateChanged2;
        internal Label lblFileSize2;
        internal Label lblFileName2;
        internal PictureBox picIco2;
        internal CheckBox chkAllConflicts;
    }
}