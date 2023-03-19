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
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.ListViewGroup listViewGroup1 = new System.Windows.Forms.ListViewGroup("Festplatten", System.Windows.Forms.HorizontalAlignment.Left);
            System.Windows.Forms.ListViewGroup listViewGroup2 = new System.Windows.Forms.ListViewGroup("Wechseldatenträger", System.Windows.Forms.HorizontalAlignment.Left);
            System.Windows.Forms.ListViewGroup listViewGroup3 = new System.Windows.Forms.ListViewGroup("Netzwerk", System.Windows.Forms.HorizontalAlignment.Left);
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmChangeMedia));
            this.Panel1 = new System.Windows.Forms.Panel();
            this.Button1 = new System.Windows.Forms.Button();
            this.Label1 = new System.Windows.Forms.Label();
            this.cmdCancel = new System.Windows.Forms.Button();
            this.Label2 = new System.Windows.Forms.Label();
            this.Label3 = new System.Windows.Forms.Label();
            this.PictureBox1 = new System.Windows.Forms.PictureBox();
            this.cboMedia = new System.Windows.Forms.ComboBox();
            this.Label4 = new System.Windows.Forms.Label();
            this.plDevice = new System.Windows.Forms.Panel();
            this.cmdRefresh = new System.Windows.Forms.Button();
            this.lvBackupDrive = new System.Windows.Forms.ListView();
            this.ColumnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.ilImages = new System.Windows.Forms.ImageList(this.components);
            this.plFTP = new System.Windows.Forms.Panel();
            this.chkFtpEncryption = new System.Windows.Forms.CheckBox();
            this.cboFtpEncoding = new System.Windows.Forms.ComboBox();
            this.Label10 = new System.Windows.Forms.Label();
            this.cmdFTPCheck = new System.Windows.Forms.Button();
            this.txtFTPPort = new System.Windows.Forms.TextBox();
            this.Label8 = new System.Windows.Forms.Label();
            this.txtFTPPath = new System.Windows.Forms.TextBox();
            this.Label7 = new System.Windows.Forms.Label();
            this.txtFTPPassword = new System.Windows.Forms.TextBox();
            this.Label5 = new System.Windows.Forms.Label();
            this.txtFTPUsername = new System.Windows.Forms.TextBox();
            this.Label6 = new System.Windows.Forms.Label();
            this.txtFTPServer = new System.Windows.Forms.TextBox();
            this.Label9 = new System.Windows.Forms.Label();
            this.Panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.PictureBox1)).BeginInit();
            this.plDevice.SuspendLayout();
            this.plFTP.SuspendLayout();
            this.SuspendLayout();
            // 
            // Panel1
            // 
            this.Panel1.BackColor = System.Drawing.SystemColors.Control;
            this.Panel1.Controls.Add(this.Button1);
            this.Panel1.Controls.Add(this.Label1);
            this.Panel1.Controls.Add(this.cmdCancel);
            this.Panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.Panel1.Location = new System.Drawing.Point(0, 528);
            this.Panel1.Margin = new System.Windows.Forms.Padding(4);
            this.Panel1.Name = "Panel1";
            this.Panel1.Size = new System.Drawing.Size(936, 68);
            this.Panel1.TabIndex = 6;
            // 
            // Button1
            // 
            this.Button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.Button1.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Button1.Location = new System.Drawing.Point(620, 15);
            this.Button1.Margin = new System.Windows.Forms.Padding(4);
            this.Button1.Name = "Button1";
            this.Button1.Size = new System.Drawing.Size(140, 39);
            this.Button1.TabIndex = 0;
            this.Button1.Text = "&Verwenden";
            this.Button1.UseVisualStyleBackColor = true;
            this.Button1.Click += new System.EventHandler(this.Button1_Click);
            // 
            // Label1
            // 
            this.Label1.BackColor = System.Drawing.Color.DarkGray;
            this.Label1.Dock = System.Windows.Forms.DockStyle.Top;
            this.Label1.Location = new System.Drawing.Point(0, 0);
            this.Label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.Label1.Name = "Label1";
            this.Label1.Size = new System.Drawing.Size(936, 2);
            this.Label1.TabIndex = 5;
            // 
            // cmdCancel
            // 
            this.cmdCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cmdCancel.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmdCancel.Location = new System.Drawing.Point(768, 15);
            this.cmdCancel.Margin = new System.Windows.Forms.Padding(4);
            this.cmdCancel.Name = "cmdCancel";
            this.cmdCancel.Size = new System.Drawing.Size(140, 39);
            this.cmdCancel.TabIndex = 1;
            this.cmdCancel.Text = "&Abbrechen";
            this.cmdCancel.UseVisualStyleBackColor = true;
            // 
            // Label2
            // 
            this.Label2.AutoSize = true;
            this.Label2.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Label2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(153)))));
            this.Label2.Location = new System.Drawing.Point(27, 27);
            this.Label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.Label2.Name = "Label2";
            this.Label2.Size = new System.Drawing.Size(343, 32);
            this.Label2.TabIndex = 0;
            this.Label2.Text = "Wechseln des Backupmediums";
            // 
            // Label3
            // 
            this.Label3.Location = new System.Drawing.Point(92, 87);
            this.Label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.Label3.Name = "Label3";
            this.Label3.Size = new System.Drawing.Size(788, 116);
            this.Label3.TabIndex = 8;
            this.Label3.Text = "Sie können nun das alte Sicherungsmedium vom Computer trennen.\r\n\r\nSchließen Sie n" +
    "un das neue (leere) Sicherungsmedium an den Computer an und wählen Sie dieses au" +
    "s der Liste aus.";
            // 
            // PictureBox1
            // 
            this.PictureBox1.Image = global::BSH.Main.Properties.Resources.storage_icon_48;
            this.PictureBox1.Location = new System.Drawing.Point(33, 87);
            this.PictureBox1.Margin = new System.Windows.Forms.Padding(4);
            this.PictureBox1.Name = "PictureBox1";
            this.PictureBox1.Size = new System.Drawing.Size(48, 48);
            this.PictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.PictureBox1.TabIndex = 7;
            this.PictureBox1.TabStop = false;
            // 
            // cboMedia
            // 
            this.cboMedia.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cboMedia.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboMedia.FormattingEnabled = true;
            this.cboMedia.Items.AddRange(new object[] {
            "Verzeichnis oder Laufwerk",
            "Server (FTP)"});
            this.cboMedia.Location = new System.Drawing.Point(214, 219);
            this.cboMedia.Margin = new System.Windows.Forms.Padding(4);
            this.cboMedia.Name = "cboMedia";
            this.cboMedia.Size = new System.Drawing.Size(691, 36);
            this.cboMedia.TabIndex = 98;
            this.cboMedia.SelectedIndexChanged += new System.EventHandler(this.cboMedia_SelectedIndexChanged);
            // 
            // Label4
            // 
            this.Label4.AutoSize = true;
            this.Label4.BackColor = System.Drawing.Color.Transparent;
            this.Label4.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Label4.Location = new System.Drawing.Point(32, 224);
            this.Label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.Label4.Name = "Label4";
            this.Label4.Size = new System.Drawing.Size(183, 28);
            this.Label4.TabIndex = 96;
            this.Label4.Text = "Sicherungsmedium:";
            // 
            // plDevice
            // 
            this.plDevice.BackColor = System.Drawing.Color.Transparent;
            this.plDevice.Controls.Add(this.cmdRefresh);
            this.plDevice.Controls.Add(this.lvBackupDrive);
            this.plDevice.Location = new System.Drawing.Point(28, 266);
            this.plDevice.Margin = new System.Windows.Forms.Padding(4);
            this.plDevice.Name = "plDevice";
            this.plDevice.Size = new System.Drawing.Size(879, 188);
            this.plDevice.TabIndex = 97;
            this.plDevice.Visible = false;
            // 
            // cmdRefresh
            // 
            this.cmdRefresh.Location = new System.Drawing.Point(736, -2);
            this.cmdRefresh.Margin = new System.Windows.Forms.Padding(4);
            this.cmdRefresh.Name = "cmdRefresh";
            this.cmdRefresh.Size = new System.Drawing.Size(143, 39);
            this.cmdRefresh.TabIndex = 17;
            this.cmdRefresh.Text = "&Aktualisieren";
            this.cmdRefresh.UseVisualStyleBackColor = true;
            this.cmdRefresh.Click += new System.EventHandler(this.cmdRefresh_Click);
            // 
            // lvBackupDrive
            // 
            this.lvBackupDrive.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.ColumnHeader2});
            this.lvBackupDrive.FullRowSelect = true;
            listViewGroup1.Header = "Festplatten";
            listViewGroup1.Name = "Local";
            listViewGroup2.Header = "Wechseldatenträger";
            listViewGroup2.Name = "Removable";
            listViewGroup3.Header = "Netzwerk";
            listViewGroup3.Name = "network";
            this.lvBackupDrive.Groups.AddRange(new System.Windows.Forms.ListViewGroup[] {
            listViewGroup1,
            listViewGroup2,
            listViewGroup3});
            this.lvBackupDrive.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.lvBackupDrive.HideSelection = false;
            this.lvBackupDrive.Location = new System.Drawing.Point(0, 0);
            this.lvBackupDrive.Margin = new System.Windows.Forms.Padding(4);
            this.lvBackupDrive.Name = "lvBackupDrive";
            this.lvBackupDrive.Size = new System.Drawing.Size(728, 186);
            this.lvBackupDrive.SmallImageList = this.ilImages;
            this.lvBackupDrive.TabIndex = 16;
            this.lvBackupDrive.UseCompatibleStateImageBehavior = false;
            this.lvBackupDrive.View = System.Windows.Forms.View.Details;
            // 
            // ColumnHeader2
            // 
            this.ColumnHeader2.Width = 400;
            // 
            // ilImages
            // 
            this.ilImages.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("ilImages.ImageStream")));
            this.ilImages.TransparentColor = System.Drawing.Color.Transparent;
            this.ilImages.Images.SetKeyName(0, "user-home.png");
            this.ilImages.Images.SetKeyName(1, "folder.png");
            this.ilImages.Images.SetKeyName(2, "drive-harddisk.png");
            this.ilImages.Images.SetKeyName(3, "drive-removable-media.png");
            this.ilImages.Images.SetKeyName(4, "system-users.png");
            // 
            // plFTP
            // 
            this.plFTP.BackColor = System.Drawing.Color.Transparent;
            this.plFTP.Controls.Add(this.chkFtpEncryption);
            this.plFTP.Controls.Add(this.cboFtpEncoding);
            this.plFTP.Controls.Add(this.Label10);
            this.plFTP.Controls.Add(this.cmdFTPCheck);
            this.plFTP.Controls.Add(this.txtFTPPort);
            this.plFTP.Controls.Add(this.Label8);
            this.plFTP.Controls.Add(this.txtFTPPath);
            this.plFTP.Controls.Add(this.Label7);
            this.plFTP.Controls.Add(this.txtFTPPassword);
            this.plFTP.Controls.Add(this.Label5);
            this.plFTP.Controls.Add(this.txtFTPUsername);
            this.plFTP.Controls.Add(this.Label6);
            this.plFTP.Controls.Add(this.txtFTPServer);
            this.plFTP.Controls.Add(this.Label9);
            this.plFTP.Location = new System.Drawing.Point(28, 266);
            this.plFTP.Margin = new System.Windows.Forms.Padding(4);
            this.plFTP.Name = "plFTP";
            this.plFTP.Size = new System.Drawing.Size(879, 236);
            this.plFTP.TabIndex = 99;
            this.plFTP.Visible = false;
            // 
            // chkFtpEncryption
            // 
            this.chkFtpEncryption.Location = new System.Drawing.Point(464, 195);
            this.chkFtpEncryption.Name = "chkFtpEncryption";
            this.chkFtpEncryption.Size = new System.Drawing.Size(398, 40);
            this.chkFtpEncryption.TabIndex = 95;
            this.chkFtpEncryption.Text = "Unverschlüsselte Verbindung erzwingen";
            this.chkFtpEncryption.UseVisualStyleBackColor = true;
            // 
            // cboFtpEncoding
            // 
            this.cboFtpEncoding.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboFtpEncoding.FormattingEnabled = true;
            this.cboFtpEncoding.Items.AddRange(new object[] {
            "ISO-8859-1",
            "UTF-8"});
            this.cboFtpEncoding.Location = new System.Drawing.Point(186, 197);
            this.cboFtpEncoding.Name = "cboFtpEncoding";
            this.cboFtpEncoding.Size = new System.Drawing.Size(259, 36);
            this.cboFtpEncoding.TabIndex = 93;
            // 
            // Label10
            // 
            this.Label10.AutoSize = true;
            this.Label10.Location = new System.Drawing.Point(45, 200);
            this.Label10.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.Label10.Name = "Label10";
            this.Label10.Size = new System.Drawing.Size(98, 28);
            this.Label10.TabIndex = 92;
            this.Label10.Text = "Encoding:";
            // 
            // cmdFTPCheck
            // 
            this.cmdFTPCheck.Location = new System.Drawing.Point(672, 21);
            this.cmdFTPCheck.Margin = new System.Windows.Forms.Padding(4);
            this.cmdFTPCheck.Name = "cmdFTPCheck";
            this.cmdFTPCheck.Size = new System.Drawing.Size(190, 38);
            this.cmdFTPCheck.TabIndex = 91;
            this.cmdFTPCheck.Text = "&Testen";
            this.cmdFTPCheck.UseVisualStyleBackColor = true;
            this.cmdFTPCheck.Click += new System.EventHandler(this.cmdFTPCheck_Click);
            // 
            // txtFTPPort
            // 
            this.txtFTPPort.Location = new System.Drawing.Point(592, 21);
            this.txtFTPPort.Margin = new System.Windows.Forms.Padding(4);
            this.txtFTPPort.Name = "txtFTPPort";
            this.txtFTPPort.Size = new System.Drawing.Size(72, 33);
            this.txtFTPPort.TabIndex = 90;
            // 
            // Label8
            // 
            this.Label8.AutoSize = true;
            this.Label8.Location = new System.Drawing.Point(536, 26);
            this.Label8.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.Label8.Name = "Label8";
            this.Label8.Size = new System.Drawing.Size(52, 28);
            this.Label8.TabIndex = 89;
            this.Label8.Text = "Port:";
            // 
            // txtFTPPath
            // 
            this.txtFTPPath.Location = new System.Drawing.Point(186, 153);
            this.txtFTPPath.Margin = new System.Windows.Forms.Padding(4);
            this.txtFTPPath.Name = "txtFTPPath";
            this.txtFTPPath.Size = new System.Drawing.Size(478, 33);
            this.txtFTPPath.TabIndex = 88;
            // 
            // Label7
            // 
            this.Label7.AutoSize = true;
            this.Label7.Location = new System.Drawing.Point(45, 158);
            this.Label7.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.Label7.Name = "Label7";
            this.Label7.Size = new System.Drawing.Size(112, 28);
            this.Label7.TabIndex = 87;
            this.Label7.Text = "Verzeichnis:";
            // 
            // txtFTPPassword
            // 
            this.txtFTPPassword.Location = new System.Drawing.Point(186, 110);
            this.txtFTPPassword.Margin = new System.Windows.Forms.Padding(4);
            this.txtFTPPassword.Name = "txtFTPPassword";
            this.txtFTPPassword.Size = new System.Drawing.Size(478, 33);
            this.txtFTPPassword.TabIndex = 86;
            this.txtFTPPassword.UseSystemPasswordChar = true;
            // 
            // Label5
            // 
            this.Label5.AutoSize = true;
            this.Label5.Location = new System.Drawing.Point(45, 114);
            this.Label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.Label5.Name = "Label5";
            this.Label5.Size = new System.Drawing.Size(100, 28);
            this.Label5.TabIndex = 85;
            this.Label5.Text = "Kennwort:";
            // 
            // txtFTPUsername
            // 
            this.txtFTPUsername.Location = new System.Drawing.Point(186, 66);
            this.txtFTPUsername.Margin = new System.Windows.Forms.Padding(4);
            this.txtFTPUsername.Name = "txtFTPUsername";
            this.txtFTPUsername.Size = new System.Drawing.Size(478, 33);
            this.txtFTPUsername.TabIndex = 84;
            // 
            // Label6
            // 
            this.Label6.AutoSize = true;
            this.Label6.Location = new System.Drawing.Point(45, 70);
            this.Label6.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.Label6.Name = "Label6";
            this.Label6.Size = new System.Drawing.Size(140, 28);
            this.Label6.TabIndex = 83;
            this.Label6.Text = "Benutzername:";
            // 
            // txtFTPServer
            // 
            this.txtFTPServer.Location = new System.Drawing.Point(186, 21);
            this.txtFTPServer.Margin = new System.Windows.Forms.Padding(4);
            this.txtFTPServer.Name = "txtFTPServer";
            this.txtFTPServer.Size = new System.Drawing.Size(338, 33);
            this.txtFTPServer.TabIndex = 82;
            // 
            // Label9
            // 
            this.Label9.AutoSize = true;
            this.Label9.Location = new System.Drawing.Point(45, 26);
            this.Label9.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.Label9.Name = "Label9";
            this.Label9.Size = new System.Drawing.Size(71, 28);
            this.Label9.TabIndex = 81;
            this.Label9.Text = "Server:";
            // 
            // frmChangeMedia
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(144F, 144F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(936, 596);
            this.Controls.Add(this.cboMedia);
            this.Controls.Add(this.Label4);
            this.Controls.Add(this.Label3);
            this.Controls.Add(this.PictureBox1);
            this.Controls.Add(this.Label2);
            this.Controls.Add(this.Panel1);
            this.Controls.Add(this.plFTP);
            this.Controls.Add(this.plDevice);
            this.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmChangeMedia";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Wechseln des Backupmediums";
            this.Load += new System.EventHandler(this.frmChangeMedia_Load);
            this.Panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.PictureBox1)).EndInit();
            this.plDevice.ResumeLayout(false);
            this.plFTP.ResumeLayout(false);
            this.plFTP.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

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