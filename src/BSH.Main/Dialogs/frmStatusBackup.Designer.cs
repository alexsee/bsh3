using System.Diagnostics;
using System.Windows.Forms;
using BSH.Main.Properties;

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
            var resources = new System.ComponentModel.ComponentResourceManager(typeof(frmStatusBackup));
            lblStatus = new Label();
            pbarTotal = new ProgressBar();
            lblFiles = new Label();
            Panel1 = new Panel();
            plShutdown = new Panel();
            cboOptions = new ComboBox();
            chkOptions = new CheckBox();
            Label1 = new Label();
            cmdCancel = new Button();
            PictureBox1 = new PictureBox();
            lblFile = new Label();
            Panel1.SuspendLayout();
            plShutdown.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)PictureBox1).BeginInit();
            SuspendLayout();
            // 
            // lblStatus
            // 
            resources.ApplyResources(lblStatus, "lblStatus");
            lblStatus.BackColor = System.Drawing.Color.Transparent;
            lblStatus.Name = "lblStatus";
            // 
            // pbarTotal
            // 
            resources.ApplyResources(pbarTotal, "pbarTotal");
            pbarTotal.Name = "pbarTotal";
            // 
            // lblFiles
            // 
            resources.ApplyResources(lblFiles, "lblFiles");
            lblFiles.AutoEllipsis = true;
            lblFiles.BackColor = System.Drawing.Color.Transparent;
            lblFiles.Name = "lblFiles";
            // 
            // Panel1
            // 
            resources.ApplyResources(Panel1, "Panel1");
            Panel1.BackColor = System.Drawing.SystemColors.Control;
            Panel1.Controls.Add(plShutdown);
            Panel1.Controls.Add(Label1);
            Panel1.Controls.Add(cmdCancel);
            Panel1.Name = "Panel1";
            // 
            // plShutdown
            // 
            resources.ApplyResources(plShutdown, "plShutdown");
            plShutdown.Controls.Add(cboOptions);
            plShutdown.Controls.Add(chkOptions);
            plShutdown.Name = "plShutdown";
            // 
            // cboOptions
            // 
            resources.ApplyResources(cboOptions, "cboOptions");
            cboOptions.DropDownStyle = ComboBoxStyle.DropDownList;
            cboOptions.FormattingEnabled = true;
            cboOptions.Items.AddRange(new object[] { resources.GetString("cboOptions.Items"), resources.GetString("cboOptions.Items1") });
            cboOptions.Name = "cboOptions";
            // 
            // chkOptions
            // 
            resources.ApplyResources(chkOptions, "chkOptions");
            chkOptions.Name = "chkOptions";
            chkOptions.UseVisualStyleBackColor = true;
            chkOptions.CheckedChanged += chkOptions_CheckedChanged;
            // 
            // Label1
            // 
            resources.ApplyResources(Label1, "Label1");
            Label1.BackColor = System.Drawing.Color.DarkGray;
            Label1.Name = "Label1";
            // 
            // cmdCancel
            // 
            resources.ApplyResources(cmdCancel, "cmdCancel");
            cmdCancel.Name = "cmdCancel";
            cmdCancel.UseVisualStyleBackColor = true;
            cmdCancel.Click += cmdCancel_Click;
            // 
            // PictureBox1
            // 
            resources.ApplyResources(PictureBox1, "PictureBox1");
            PictureBox1.BackColor = System.Drawing.Color.Transparent;
            PictureBox1.Image = Resources.settings_backup_restore_icon_48;
            PictureBox1.Name = "PictureBox1";
            PictureBox1.TabStop = false;
            // 
            // lblFile
            // 
            resources.ApplyResources(lblFile, "lblFile");
            lblFile.AutoEllipsis = true;
            lblFile.BackColor = System.Drawing.Color.Transparent;
            lblFile.Name = "lblFile";
            // 
            // frmStatusBackup
            // 
            resources.ApplyResources(this, "$this");
            AutoScaleMode = AutoScaleMode.Dpi;
            BackColor = System.Drawing.Color.White;
            Controls.Add(lblFile);
            Controls.Add(Panel1);
            Controls.Add(lblFiles);
            Controls.Add(pbarTotal);
            Controls.Add(lblStatus);
            Controls.Add(PictureBox1);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            Name = "frmStatusBackup";
            Panel1.ResumeLayout(false);
            plShutdown.ResumeLayout(false);
            plShutdown.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)PictureBox1).EndInit();
            ResumeLayout(false);
            PerformLayout();
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