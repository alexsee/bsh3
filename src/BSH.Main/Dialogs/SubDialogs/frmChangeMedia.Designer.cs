using System.Diagnostics;
using System.Windows.Forms;

namespace Brightbits.BSH.Main
{
    public partial class frmChangeMedia : Form
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
            components = new System.ComponentModel.Container();
            var resources = new System.ComponentModel.ComponentResourceManager(typeof(frmChangeMedia));
            Panel1 = new Panel();
            Button1 = new Button();
            Label1 = new Label();
            cmdCancel = new Button();
            Label2 = new Label();
            Label3 = new Label();
            PictureBox1 = new PictureBox();
            cboMedia = new ComboBox();
            Label4 = new Label();
            plDevice = new Panel();
            cmdRefresh = new Button();
            lvBackupDrive = new ListView();
            ColumnHeader2 = new ColumnHeader();
            ilImages = new ImageList(components);
            plFTP = new Panel();
            chkFtpEncryption = new CheckBox();
            cboFtpEncoding = new ComboBox();
            Label10 = new Label();
            cmdFTPCheck = new Button();
            txtFTPPort = new TextBox();
            Label8 = new Label();
            txtFTPPath = new TextBox();
            Label7 = new Label();
            txtFTPPassword = new TextBox();
            Label5 = new Label();
            txtFTPUsername = new TextBox();
            Label6 = new Label();
            txtFTPServer = new TextBox();
            Label9 = new Label();
            Panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)PictureBox1).BeginInit();
            plDevice.SuspendLayout();
            plFTP.SuspendLayout();
            SuspendLayout();
            // 
            // Panel1
            // 
            resources.ApplyResources(Panel1, "Panel1");
            Panel1.BackColor = System.Drawing.SystemColors.Control;
            Panel1.Controls.Add(Button1);
            Panel1.Controls.Add(Label1);
            Panel1.Controls.Add(cmdCancel);
            Panel1.Name = "Panel1";
            // 
            // Button1
            // 
            resources.ApplyResources(Button1, "Button1");
            Button1.Name = "Button1";
            Button1.UseVisualStyleBackColor = true;
            Button1.Click += Button1_Click;
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
            cmdCancel.DialogResult = DialogResult.Cancel;
            cmdCancel.Name = "cmdCancel";
            cmdCancel.UseVisualStyleBackColor = true;
            // 
            // Label2
            // 
            resources.ApplyResources(Label2, "Label2");
            Label2.ForeColor = System.Drawing.Color.FromArgb(0, 51, 153);
            Label2.Name = "Label2";
            // 
            // Label3
            // 
            resources.ApplyResources(Label3, "Label3");
            Label3.Name = "Label3";
            // 
            // PictureBox1
            // 
            resources.ApplyResources(PictureBox1, "PictureBox1");
            PictureBox1.Image = Main.Properties.Resources.storage_icon_48;
            PictureBox1.Name = "PictureBox1";
            PictureBox1.TabStop = false;
            // 
            // cboMedia
            // 
            resources.ApplyResources(cboMedia, "cboMedia");
            cboMedia.DropDownStyle = ComboBoxStyle.DropDownList;
            cboMedia.FormattingEnabled = true;
            cboMedia.Items.AddRange(new object[] { resources.GetString("cboMedia.Items"), resources.GetString("cboMedia.Items1") });
            cboMedia.Name = "cboMedia";
            cboMedia.SelectedIndexChanged += cboMedia_SelectedIndexChanged;
            // 
            // Label4
            // 
            resources.ApplyResources(Label4, "Label4");
            Label4.BackColor = System.Drawing.Color.Transparent;
            Label4.Name = "Label4";
            // 
            // plDevice
            // 
            resources.ApplyResources(plDevice, "plDevice");
            plDevice.BackColor = System.Drawing.Color.Transparent;
            plDevice.Controls.Add(cmdRefresh);
            plDevice.Controls.Add(lvBackupDrive);
            plDevice.Name = "plDevice";
            // 
            // cmdRefresh
            // 
            resources.ApplyResources(cmdRefresh, "cmdRefresh");
            cmdRefresh.Name = "cmdRefresh";
            cmdRefresh.UseVisualStyleBackColor = true;
            cmdRefresh.Click += cmdRefresh_Click;
            // 
            // lvBackupDrive
            // 
            resources.ApplyResources(lvBackupDrive, "lvBackupDrive");
            lvBackupDrive.Columns.AddRange(new ColumnHeader[] { ColumnHeader2 });
            lvBackupDrive.FullRowSelect = true;
            lvBackupDrive.Groups.AddRange(new ListViewGroup[] { (ListViewGroup)resources.GetObject("lvBackupDrive.Groups"), (ListViewGroup)resources.GetObject("lvBackupDrive.Groups1"), (ListViewGroup)resources.GetObject("lvBackupDrive.Groups2") });
            lvBackupDrive.HeaderStyle = ColumnHeaderStyle.None;
            lvBackupDrive.Name = "lvBackupDrive";
            lvBackupDrive.SmallImageList = ilImages;
            lvBackupDrive.UseCompatibleStateImageBehavior = false;
            lvBackupDrive.View = View.Details;
            // 
            // ColumnHeader2
            // 
            resources.ApplyResources(ColumnHeader2, "ColumnHeader2");
            // 
            // ilImages
            // 
            ilImages.ColorDepth = ColorDepth.Depth32Bit;
            ilImages.ImageStream = (ImageListStreamer)resources.GetObject("ilImages.ImageStream");
            ilImages.TransparentColor = System.Drawing.Color.Transparent;
            ilImages.Images.SetKeyName(0, "user-home.png");
            ilImages.Images.SetKeyName(1, "folder.png");
            ilImages.Images.SetKeyName(2, "drive-harddisk.png");
            ilImages.Images.SetKeyName(3, "drive-removable-media.png");
            ilImages.Images.SetKeyName(4, "system-users.png");
            // 
            // plFTP
            // 
            resources.ApplyResources(plFTP, "plFTP");
            plFTP.BackColor = System.Drawing.Color.Transparent;
            plFTP.Controls.Add(chkFtpEncryption);
            plFTP.Controls.Add(cboFtpEncoding);
            plFTP.Controls.Add(Label10);
            plFTP.Controls.Add(cmdFTPCheck);
            plFTP.Controls.Add(txtFTPPort);
            plFTP.Controls.Add(Label8);
            plFTP.Controls.Add(txtFTPPath);
            plFTP.Controls.Add(Label7);
            plFTP.Controls.Add(txtFTPPassword);
            plFTP.Controls.Add(Label5);
            plFTP.Controls.Add(txtFTPUsername);
            plFTP.Controls.Add(Label6);
            plFTP.Controls.Add(txtFTPServer);
            plFTP.Controls.Add(Label9);
            plFTP.Name = "plFTP";
            // 
            // chkFtpEncryption
            // 
            resources.ApplyResources(chkFtpEncryption, "chkFtpEncryption");
            chkFtpEncryption.Name = "chkFtpEncryption";
            chkFtpEncryption.UseVisualStyleBackColor = true;
            // 
            // cboFtpEncoding
            // 
            resources.ApplyResources(cboFtpEncoding, "cboFtpEncoding");
            cboFtpEncoding.DropDownStyle = ComboBoxStyle.DropDownList;
            cboFtpEncoding.FormattingEnabled = true;
            cboFtpEncoding.Items.AddRange(new object[] { resources.GetString("cboFtpEncoding.Items"), resources.GetString("cboFtpEncoding.Items1") });
            cboFtpEncoding.Name = "cboFtpEncoding";
            // 
            // Label10
            // 
            resources.ApplyResources(Label10, "Label10");
            Label10.Name = "Label10";
            // 
            // cmdFTPCheck
            // 
            resources.ApplyResources(cmdFTPCheck, "cmdFTPCheck");
            cmdFTPCheck.Name = "cmdFTPCheck";
            cmdFTPCheck.UseVisualStyleBackColor = true;
            cmdFTPCheck.Click += cmdFTPCheck_Click;
            // 
            // txtFTPPort
            // 
            resources.ApplyResources(txtFTPPort, "txtFTPPort");
            txtFTPPort.Name = "txtFTPPort";
            // 
            // Label8
            // 
            resources.ApplyResources(Label8, "Label8");
            Label8.Name = "Label8";
            // 
            // txtFTPPath
            // 
            resources.ApplyResources(txtFTPPath, "txtFTPPath");
            txtFTPPath.Name = "txtFTPPath";
            // 
            // Label7
            // 
            resources.ApplyResources(Label7, "Label7");
            Label7.Name = "Label7";
            // 
            // txtFTPPassword
            // 
            resources.ApplyResources(txtFTPPassword, "txtFTPPassword");
            txtFTPPassword.Name = "txtFTPPassword";
            txtFTPPassword.UseSystemPasswordChar = true;
            // 
            // Label5
            // 
            resources.ApplyResources(Label5, "Label5");
            Label5.Name = "Label5";
            // 
            // txtFTPUsername
            // 
            resources.ApplyResources(txtFTPUsername, "txtFTPUsername");
            txtFTPUsername.Name = "txtFTPUsername";
            // 
            // Label6
            // 
            resources.ApplyResources(Label6, "Label6");
            Label6.Name = "Label6";
            // 
            // txtFTPServer
            // 
            resources.ApplyResources(txtFTPServer, "txtFTPServer");
            txtFTPServer.Name = "txtFTPServer";
            // 
            // Label9
            // 
            resources.ApplyResources(Label9, "Label9");
            Label9.Name = "Label9";
            // 
            // frmChangeMedia
            // 
            resources.ApplyResources(this, "$this");
            AutoScaleMode = AutoScaleMode.Dpi;
            BackColor = System.Drawing.Color.White;
            Controls.Add(cboMedia);
            Controls.Add(Label4);
            Controls.Add(Label3);
            Controls.Add(PictureBox1);
            Controls.Add(Label2);
            Controls.Add(Panel1);
            Controls.Add(plFTP);
            Controls.Add(plDevice);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "frmChangeMedia";
            Load += frmChangeMedia_Load;
            Panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)PictureBox1).EndInit();
            plDevice.ResumeLayout(false);
            plFTP.ResumeLayout(false);
            plFTP.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        internal Panel Panel1;
        internal Label Label1;
        internal Button cmdCancel;
        internal Label Label2;
        internal Button Button1;
        internal Label Label3;
        internal PictureBox PictureBox1;
        internal ComboBox cboMedia;
        internal Label Label4;
        internal Panel plDevice;
        internal Panel plFTP;
        internal Button cmdFTPCheck;
        internal TextBox txtFTPPort;
        internal Label Label8;
        internal TextBox txtFTPPath;
        internal Label Label7;
        internal TextBox txtFTPPassword;
        internal Label Label5;
        internal TextBox txtFTPUsername;
        internal Label Label6;
        internal TextBox txtFTPServer;
        internal Label Label9;
        internal Button cmdRefresh;
        internal ListView lvBackupDrive;
        internal ColumnHeader ColumnHeader2;
        internal ImageList ilImages;
        internal ComboBox cboFtpEncoding;
        internal Label Label10;
        private CheckBox chkFtpEncryption;
    }
}