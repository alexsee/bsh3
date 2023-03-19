using System.Diagnostics;
using System.Windows.Forms;

namespace Brightbits.BSH.Main
{
    public partial class frmStatusBackup : Form
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmStatusBackup));
            this.lblStatus = new System.Windows.Forms.Label();
            this.pbarTotal = new System.Windows.Forms.ProgressBar();
            this.lblFiles = new System.Windows.Forms.Label();
            this.Panel1 = new System.Windows.Forms.Panel();
            this.plShutdown = new System.Windows.Forms.Panel();
            this.cboOptions = new System.Windows.Forms.ComboBox();
            this.chkOptions = new System.Windows.Forms.CheckBox();
            this.Label1 = new System.Windows.Forms.Label();
            this.cmdCancel = new System.Windows.Forms.Button();
            this.PictureBox1 = new System.Windows.Forms.PictureBox();
            this.lblFile = new System.Windows.Forms.Label();
            this.Panel1.SuspendLayout();
            this.plShutdown.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.PictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // lblStatus
            // 
            this.lblStatus.AutoSize = true;
            this.lblStatus.BackColor = System.Drawing.Color.Transparent;
            this.lblStatus.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblStatus.Location = new System.Drawing.Point(98, 21);
            this.lblStatus.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(128, 28);
            this.lblStatus.TabIndex = 0;
            this.lblStatus.Text = "Initialisieren...";
            // 
            // pbarTotal
            // 
            this.pbarTotal.Location = new System.Drawing.Point(102, 54);
            this.pbarTotal.Margin = new System.Windows.Forms.Padding(4);
            this.pbarTotal.Name = "pbarTotal";
            this.pbarTotal.Size = new System.Drawing.Size(600, 26);
            this.pbarTotal.TabIndex = 1;
            // 
            // lblFiles
            // 
            this.lblFiles.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblFiles.AutoEllipsis = true;
            this.lblFiles.BackColor = System.Drawing.Color.Transparent;
            this.lblFiles.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblFiles.Location = new System.Drawing.Point(98, 88);
            this.lblFiles.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblFiles.Name = "lblFiles";
            this.lblFiles.Size = new System.Drawing.Size(604, 38);
            this.lblFiles.TabIndex = 2;
            // 
            // Panel1
            // 
            this.Panel1.BackColor = System.Drawing.SystemColors.Control;
            this.Panel1.Controls.Add(this.plShutdown);
            this.Panel1.Controls.Add(this.Label1);
            this.Panel1.Controls.Add(this.cmdCancel);
            this.Panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.Panel1.Location = new System.Drawing.Point(0, 170);
            this.Panel1.Margin = new System.Windows.Forms.Padding(4);
            this.Panel1.Name = "Panel1";
            this.Panel1.Size = new System.Drawing.Size(730, 68);
            this.Panel1.TabIndex = 3;
            // 
            // plShutdown
            // 
            this.plShutdown.Controls.Add(this.cboOptions);
            this.plShutdown.Controls.Add(this.chkOptions);
            this.plShutdown.Location = new System.Drawing.Point(21, 9);
            this.plShutdown.Margin = new System.Windows.Forms.Padding(4);
            this.plShutdown.Name = "plShutdown";
            this.plShutdown.Size = new System.Drawing.Size(542, 44);
            this.plShutdown.TabIndex = 8;
            this.plShutdown.Visible = false;
            // 
            // cboOptions
            // 
            this.cboOptions.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboOptions.Enabled = false;
            this.cboOptions.FormattingEnabled = true;
            this.cboOptions.Items.AddRange(new object[] {
            "PC nach Sicherung herunterfahren",
            "PC nach Sicherung in Ruhezustand"});
            this.cboOptions.Location = new System.Drawing.Point(42, 6);
            this.cboOptions.Margin = new System.Windows.Forms.Padding(4);
            this.cboOptions.Name = "cboOptions";
            this.cboOptions.Size = new System.Drawing.Size(488, 33);
            this.cboOptions.TabIndex = 7;
            // 
            // chkOptions
            // 
            this.chkOptions.AutoSize = true;
            this.chkOptions.Location = new System.Drawing.Point(10, 12);
            this.chkOptions.Margin = new System.Windows.Forms.Padding(4);
            this.chkOptions.Name = "chkOptions";
            this.chkOptions.Size = new System.Drawing.Size(22, 21);
            this.chkOptions.TabIndex = 6;
            this.chkOptions.UseVisualStyleBackColor = true;
            this.chkOptions.CheckedChanged += new System.EventHandler(this.chkOptions_CheckedChanged);
            // 
            // Label1
            // 
            this.Label1.BackColor = System.Drawing.Color.DarkGray;
            this.Label1.Dock = System.Windows.Forms.DockStyle.Top;
            this.Label1.Location = new System.Drawing.Point(0, 0);
            this.Label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.Label1.Name = "Label1";
            this.Label1.Size = new System.Drawing.Size(730, 2);
            this.Label1.TabIndex = 5;
            // 
            // cmdCancel
            // 
            this.cmdCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdCancel.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmdCancel.Location = new System.Drawing.Point(562, 9);
            this.cmdCancel.Margin = new System.Windows.Forms.Padding(4);
            this.cmdCancel.Name = "cmdCancel";
            this.cmdCancel.Size = new System.Drawing.Size(140, 45);
            this.cmdCancel.TabIndex = 0;
            this.cmdCancel.Text = "&Abbrechen";
            this.cmdCancel.UseVisualStyleBackColor = true;
            this.cmdCancel.Click += new System.EventHandler(this.cmdCancel_Click);
            // 
            // PictureBox1
            // 
            this.PictureBox1.BackColor = System.Drawing.Color.Transparent;
            this.PictureBox1.Image = global::BSH.Main.Properties.Resources.settings_backup_restore_icon_48;
            this.PictureBox1.Location = new System.Drawing.Point(32, 21);
            this.PictureBox1.Margin = new System.Windows.Forms.Padding(4);
            this.PictureBox1.Name = "PictureBox1";
            this.PictureBox1.Size = new System.Drawing.Size(48, 44);
            this.PictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.PictureBox1.TabIndex = 0;
            this.PictureBox1.TabStop = false;
            // 
            // lblFile
            // 
            this.lblFile.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblFile.AutoEllipsis = true;
            this.lblFile.BackColor = System.Drawing.Color.Transparent;
            this.lblFile.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblFile.Location = new System.Drawing.Point(98, 126);
            this.lblFile.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblFile.Name = "lblFile";
            this.lblFile.Size = new System.Drawing.Size(604, 27);
            this.lblFile.TabIndex = 4;
            // 
            // frmStatusBackup
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(144F, 144F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(730, 238);
            this.Controls.Add(this.lblFile);
            this.Controls.Add(this.Panel1);
            this.Controls.Add(this.lblFiles);
            this.Controls.Add(this.pbarTotal);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.PictureBox1);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MaximizeBox = false;
            this.Name = "frmStatusBackup";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Datensicherung";
            this.Panel1.ResumeLayout(false);
            this.plShutdown.ResumeLayout(false);
            this.plShutdown.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.PictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        internal PictureBox PictureBox1;
        internal Label lblStatus;
        internal ProgressBar pbarTotal;
        internal Label lblFiles;
        internal Panel Panel1;
        internal Button cmdCancel;
        internal Label Label1;
        internal Label lblFile;
        internal ComboBox cboOptions;
        internal CheckBox chkOptions;
        internal Panel plShutdown;
    }
}