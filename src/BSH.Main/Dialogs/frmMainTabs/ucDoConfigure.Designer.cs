using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace Brightbits.BSH.Main
{
    public partial class ucDoConfigure : UserControl
    {

        // UserControl überschreibt den Löschvorgang, um die Komponentenliste zu bereinigen.
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ucDoConfigure));
            System.Windows.Forms.ListViewGroup listViewGroup1 = new System.Windows.Forms.ListViewGroup("Festplatten", System.Windows.Forms.HorizontalAlignment.Left);
            System.Windows.Forms.ListViewGroup listViewGroup2 = new System.Windows.Forms.ListViewGroup("Wechseldatenträger", System.Windows.Forms.HorizontalAlignment.Left);
            System.Windows.Forms.ListViewGroup listViewGroup3 = new System.Windows.Forms.ListViewGroup("Netzwerk", System.Windows.Forms.HorizontalAlignment.Left);
            System.Windows.Forms.ListViewGroup listViewGroup4 = new System.Windows.Forms.ListViewGroup("Festplatten", System.Windows.Forms.HorizontalAlignment.Left);
            System.Windows.Forms.ListViewGroup listViewGroup5 = new System.Windows.Forms.ListViewGroup("Wechseldatenträger", System.Windows.Forms.HorizontalAlignment.Left);
            System.Windows.Forms.ListViewGroup listViewGroup6 = new System.Windows.Forms.ListViewGroup("Netzwerk", System.Windows.Forms.HorizontalAlignment.Left);
            this.cmdNext = new System.Windows.Forms.Button();
            this.lblTitle = new System.Windows.Forms.Label();
            this.lblDescription = new System.Windows.Forms.Label();
            this.lvSourceFolders = new System.Windows.Forms.ListView();
            this.ColumnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.ilImages = new System.Windows.Forms.ImageList(this.components);
            this.cmdAdd = new System.Windows.Forms.Button();
            this.cmdDelete = new System.Windows.Forms.Button();
            this.cmdBack = new System.Windows.Forms.Button();
            this.cmdImport = new System.Windows.Forms.Button();
            this.tcSource = new System.Windows.Forms.TabControl();
            this.TabPage1 = new System.Windows.Forms.TabPage();
            this.cmdRefresh = new System.Windows.Forms.Button();
            this.lvBackupDrive = new System.Windows.Forms.ListView();
            this.ColumnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.TabPage2 = new System.Windows.Forms.TabPage();
            this.chkFtpEncryption = new System.Windows.Forms.CheckBox();
            this.Label18 = new System.Windows.Forms.Label();
            this.cboFtpEncoding = new System.Windows.Forms.ComboBox();
            this.cmdFTPCheck = new System.Windows.Forms.Button();
            this.txtFTPPort = new System.Windows.Forms.TextBox();
            this.Label8 = new System.Windows.Forms.Label();
            this.txtFTPPath = new System.Windows.Forms.TextBox();
            this.Label7 = new System.Windows.Forms.Label();
            this.txtFTPPassword = new System.Windows.Forms.TextBox();
            this.Label5 = new System.Windows.Forms.Label();
            this.txtFTPUsername = new System.Windows.Forms.TextBox();
            this.Label4 = new System.Windows.Forms.Label();
            this.txtFTPServer = new System.Windows.Forms.TextBox();
            this.Label2 = new System.Windows.Forms.Label();
            this.TabPage6 = new System.Windows.Forms.TabPage();
            this.txtUNCPassword = new System.Windows.Forms.TextBox();
            this.Label16 = new System.Windows.Forms.Label();
            this.txtUNCUsername = new System.Windows.Forms.TextBox();
            this.Label17 = new System.Windows.Forms.Label();
            this.Label1 = new System.Windows.Forms.Label();
            this.txtUNCPath = new System.Windows.Forms.TextBox();
            this.rdManualMode = new System.Windows.Forms.RadioButton();
            this.rdFullAutomated = new System.Windows.Forms.RadioButton();
            this.lblStatus = new System.Windows.Forms.Label();
            this.ProgressBar1 = new System.Windows.Forms.ProgressBar();
            this.tcStep5 = new System.Windows.Forms.TabControl();
            this.TabPage3 = new System.Windows.Forms.TabPage();
            this.cmdRefresh2 = new System.Windows.Forms.Button();
            this.lvBackupMedia = new System.Windows.Forms.ListView();
            this.ColumnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.TabPage4 = new System.Windows.Forms.TabPage();
            this.chkFtpEncryption2 = new System.Windows.Forms.CheckBox();
            this.cboFtpEncoding2 = new System.Windows.Forms.ComboBox();
            this.Label19 = new System.Windows.Forms.Label();
            this.txtFTPPort2 = new System.Windows.Forms.TextBox();
            this.Label6 = new System.Windows.Forms.Label();
            this.txtFTPPath2 = new System.Windows.Forms.TextBox();
            this.Label9 = new System.Windows.Forms.Label();
            this.txtFTPPass2 = new System.Windows.Forms.TextBox();
            this.Label10 = new System.Windows.Forms.Label();
            this.txtFTPUser2 = new System.Windows.Forms.TextBox();
            this.Label11 = new System.Windows.Forms.Label();
            this.txtFTPServer2 = new System.Windows.Forms.TextBox();
            this.Label12 = new System.Windows.Forms.Label();
            this.TabPage5 = new System.Windows.Forms.TabPage();
            this.cmdBrowse = new System.Windows.Forms.Button();
            this.txtPath = new System.Windows.Forms.TextBox();
            this.lvBackups = new System.Windows.Forms.ListView();
            this.ColumnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.cmdChange = new System.Windows.Forms.Button();
            this.lvSourceDirs = new System.Windows.Forms.ListView();
            this.ColumnHeader5 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.ColumnHeader6 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.tbControl = new System.Windows.Forms.TabControl();
            this.tbStep0 = new System.Windows.Forms.TabPage();
            this.TableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.Label3 = new System.Windows.Forms.Label();
            this.Label14 = new System.Windows.Forms.Label();
            this.Label13 = new System.Windows.Forms.Label();
            this.Label15 = new System.Windows.Forms.Label();
            this.Panel3 = new System.Windows.Forms.Panel();
            this.Panel4 = new System.Windows.Forms.Panel();
            this.cmdConfigure = new System.Windows.Forms.Button();
            this.tbStep1 = new System.Windows.Forms.TabPage();
            this.tbStep2 = new System.Windows.Forms.TabPage();
            this.tbStep3 = new System.Windows.Forms.TabPage();
            this.tbProgress = new System.Windows.Forms.TabPage();
            this.tbStep5 = new System.Windows.Forms.TabPage();
            this.tbStep6 = new System.Windows.Forms.TabPage();
            this.tbStep7 = new System.Windows.Forms.TabPage();
            this.Panel1 = new System.Windows.Forms.Panel();
            this.Panel2 = new System.Windows.Forms.Panel();
            this.tcSource.SuspendLayout();
            this.TabPage1.SuspendLayout();
            this.TabPage2.SuspendLayout();
            this.TabPage6.SuspendLayout();
            this.tcStep5.SuspendLayout();
            this.TabPage3.SuspendLayout();
            this.TabPage4.SuspendLayout();
            this.TabPage5.SuspendLayout();
            this.tbControl.SuspendLayout();
            this.tbStep0.SuspendLayout();
            this.TableLayoutPanel1.SuspendLayout();
            this.Panel3.SuspendLayout();
            this.Panel4.SuspendLayout();
            this.tbStep1.SuspendLayout();
            this.tbStep2.SuspendLayout();
            this.tbStep3.SuspendLayout();
            this.tbProgress.SuspendLayout();
            this.tbStep5.SuspendLayout();
            this.tbStep6.SuspendLayout();
            this.tbStep7.SuspendLayout();
            this.Panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // cmdNext
            // 
            this.cmdNext.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmdNext.Location = new System.Drawing.Point(862, 495);
            this.cmdNext.Margin = new System.Windows.Forms.Padding(4);
            this.cmdNext.Name = "cmdNext";
            this.cmdNext.Size = new System.Drawing.Size(122, 48);
            this.cmdNext.TabIndex = 9;
            this.cmdNext.Text = "&Weiter";
            this.cmdNext.UseVisualStyleBackColor = true;
            this.cmdNext.Visible = false;
            this.cmdNext.Click += new System.EventHandler(this.cmdNext_Click);
            // 
            // lblTitle
            // 
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(153)))));
            this.lblTitle.Location = new System.Drawing.Point(57, 21);
            this.lblTitle.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(0, 32);
            this.lblTitle.TabIndex = 11;
            // 
            // lblDescription
            // 
            this.lblDescription.Location = new System.Drawing.Point(58, 58);
            this.lblDescription.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblDescription.Name = "lblDescription";
            this.lblDescription.Size = new System.Drawing.Size(932, 58);
            this.lblDescription.TabIndex = 12;
            // 
            // lvSourceFolders
            // 
            this.lvSourceFolders.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.ColumnHeader1});
            this.lvSourceFolders.FullRowSelect = true;
            this.lvSourceFolders.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.lvSourceFolders.HideSelection = false;
            this.lvSourceFolders.Location = new System.Drawing.Point(57, 62);
            this.lvSourceFolders.Margin = new System.Windows.Forms.Padding(4);
            this.lvSourceFolders.MultiSelect = false;
            this.lvSourceFolders.Name = "lvSourceFolders";
            this.lvSourceFolders.Size = new System.Drawing.Size(763, 330);
            this.lvSourceFolders.SmallImageList = this.ilImages;
            this.lvSourceFolders.TabIndex = 14;
            this.lvSourceFolders.UseCompatibleStateImageBehavior = false;
            this.lvSourceFolders.View = System.Windows.Forms.View.Details;
            this.lvSourceFolders.SelectedIndexChanged += new System.EventHandler(this.lvSourceFolders_SelectedIndexChanged);
            // 
            // ColumnHeader1
            // 
            this.ColumnHeader1.Width = 450;
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
            // cmdAdd
            // 
            this.cmdAdd.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.cmdAdd.Location = new System.Drawing.Point(831, 62);
            this.cmdAdd.Margin = new System.Windows.Forms.Padding(4);
            this.cmdAdd.Name = "cmdAdd";
            this.cmdAdd.Size = new System.Drawing.Size(153, 39);
            this.cmdAdd.TabIndex = 15;
            this.cmdAdd.Text = "&Hinzufügen";
            this.cmdAdd.UseVisualStyleBackColor = true;
            this.cmdAdd.Click += new System.EventHandler(this.cmdAdd_Click);
            // 
            // cmdDelete
            // 
            this.cmdDelete.Enabled = false;
            this.cmdDelete.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.cmdDelete.Location = new System.Drawing.Point(831, 110);
            this.cmdDelete.Margin = new System.Windows.Forms.Padding(4);
            this.cmdDelete.Name = "cmdDelete";
            this.cmdDelete.Size = new System.Drawing.Size(153, 39);
            this.cmdDelete.TabIndex = 16;
            this.cmdDelete.Text = "&Löschen";
            this.cmdDelete.UseVisualStyleBackColor = true;
            this.cmdDelete.Click += new System.EventHandler(this.cmdDelete_Click);
            // 
            // cmdBack
            // 
            this.cmdBack.Enabled = false;
            this.cmdBack.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmdBack.Location = new System.Drawing.Point(732, 495);
            this.cmdBack.Margin = new System.Windows.Forms.Padding(4);
            this.cmdBack.Name = "cmdBack";
            this.cmdBack.Size = new System.Drawing.Size(122, 48);
            this.cmdBack.TabIndex = 17;
            this.cmdBack.Text = "&Zurück";
            this.cmdBack.UseVisualStyleBackColor = true;
            this.cmdBack.Visible = false;
            this.cmdBack.Click += new System.EventHandler(this.cmdBack_Click);
            // 
            // cmdImport
            // 
            this.cmdImport.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.cmdImport.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmdImport.Location = new System.Drawing.Point(162, 1);
            this.cmdImport.Margin = new System.Windows.Forms.Padding(4);
            this.cmdImport.Name = "cmdImport";
            this.cmdImport.Size = new System.Drawing.Size(153, 48);
            this.cmdImport.TabIndex = 18;
            this.cmdImport.Text = "&Importieren";
            this.cmdImport.UseVisualStyleBackColor = true;
            this.cmdImport.Click += new System.EventHandler(this.cmdImport_Click);
            // 
            // tcSource
            // 
            this.tcSource.Controls.Add(this.TabPage1);
            this.tcSource.Controls.Add(this.TabPage2);
            this.tcSource.Controls.Add(this.TabPage6);
            this.tcSource.Location = new System.Drawing.Point(57, 62);
            this.tcSource.Margin = new System.Windows.Forms.Padding(4);
            this.tcSource.Name = "tcSource";
            this.tcSource.SelectedIndex = 0;
            this.tcSource.Size = new System.Drawing.Size(916, 322);
            this.tcSource.TabIndex = 16;
            // 
            // TabPage1
            // 
            this.TabPage1.Controls.Add(this.cmdRefresh);
            this.TabPage1.Controls.Add(this.lvBackupDrive);
            this.TabPage1.Location = new System.Drawing.Point(4, 34);
            this.TabPage1.Margin = new System.Windows.Forms.Padding(4);
            this.TabPage1.Name = "TabPage1";
            this.TabPage1.Padding = new System.Windows.Forms.Padding(4);
            this.TabPage1.Size = new System.Drawing.Size(908, 284);
            this.TabPage1.TabIndex = 0;
            this.TabPage1.Text = "Datenträger";
            this.TabPage1.UseVisualStyleBackColor = true;
            // 
            // cmdRefresh
            // 
            this.cmdRefresh.Location = new System.Drawing.Point(742, 9);
            this.cmdRefresh.Margin = new System.Windows.Forms.Padding(4);
            this.cmdRefresh.Name = "cmdRefresh";
            this.cmdRefresh.Size = new System.Drawing.Size(153, 39);
            this.cmdRefresh.TabIndex = 15;
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
            this.lvBackupDrive.Location = new System.Drawing.Point(9, 9);
            this.lvBackupDrive.Margin = new System.Windows.Forms.Padding(4);
            this.lvBackupDrive.Name = "lvBackupDrive";
            this.lvBackupDrive.Size = new System.Drawing.Size(722, 264);
            this.lvBackupDrive.SmallImageList = this.ilImages;
            this.lvBackupDrive.TabIndex = 14;
            this.lvBackupDrive.UseCompatibleStateImageBehavior = false;
            this.lvBackupDrive.View = System.Windows.Forms.View.Details;
            // 
            // ColumnHeader2
            // 
            this.ColumnHeader2.Width = 450;
            // 
            // TabPage2
            // 
            this.TabPage2.Controls.Add(this.chkFtpEncryption);
            this.TabPage2.Controls.Add(this.Label18);
            this.TabPage2.Controls.Add(this.cboFtpEncoding);
            this.TabPage2.Controls.Add(this.cmdFTPCheck);
            this.TabPage2.Controls.Add(this.txtFTPPort);
            this.TabPage2.Controls.Add(this.Label8);
            this.TabPage2.Controls.Add(this.txtFTPPath);
            this.TabPage2.Controls.Add(this.Label7);
            this.TabPage2.Controls.Add(this.txtFTPPassword);
            this.TabPage2.Controls.Add(this.Label5);
            this.TabPage2.Controls.Add(this.txtFTPUsername);
            this.TabPage2.Controls.Add(this.Label4);
            this.TabPage2.Controls.Add(this.txtFTPServer);
            this.TabPage2.Controls.Add(this.Label2);
            this.TabPage2.Location = new System.Drawing.Point(4, 34);
            this.TabPage2.Margin = new System.Windows.Forms.Padding(4);
            this.TabPage2.Name = "TabPage2";
            this.TabPage2.Padding = new System.Windows.Forms.Padding(4);
            this.TabPage2.Size = new System.Drawing.Size(908, 284);
            this.TabPage2.TabIndex = 1;
            this.TabPage2.Text = "FTP-Server";
            this.TabPage2.UseVisualStyleBackColor = true;
            // 
            // chkFtpEncryption
            // 
            this.chkFtpEncryption.Location = new System.Drawing.Point(29, 199);
            this.chkFtpEncryption.Name = "chkFtpEncryption";
            this.chkFtpEncryption.Size = new System.Drawing.Size(411, 40);
            this.chkFtpEncryption.TabIndex = 105;
            this.chkFtpEncryption.Text = "Unverschlüsselte Verbindung erzwingen";
            this.chkFtpEncryption.UseVisualStyleBackColor = true;
            // 
            // Label18
            // 
            this.Label18.AutoSize = true;
            this.Label18.Location = new System.Drawing.Point(24, 156);
            this.Label18.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.Label18.Name = "Label18";
            this.Label18.Size = new System.Drawing.Size(90, 25);
            this.Label18.TabIndex = 104;
            this.Label18.Text = "Encoding:";
            // 
            // cboFtpEncoding
            // 
            this.cboFtpEncoding.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboFtpEncoding.FormattingEnabled = true;
            this.cboFtpEncoding.Items.AddRange(new object[] {
            "ISO-8859-1",
            "UTF-8"});
            this.cboFtpEncoding.Location = new System.Drawing.Point(194, 148);
            this.cboFtpEncoding.Name = "cboFtpEncoding";
            this.cboFtpEncoding.Size = new System.Drawing.Size(304, 33);
            this.cboFtpEncoding.TabIndex = 103;
            // 
            // cmdFTPCheck
            // 
            this.cmdFTPCheck.Location = new System.Drawing.Point(748, 152);
            this.cmdFTPCheck.Margin = new System.Windows.Forms.Padding(4);
            this.cmdFTPCheck.Name = "cmdFTPCheck";
            this.cmdFTPCheck.Size = new System.Drawing.Size(122, 34);
            this.cmdFTPCheck.TabIndex = 102;
            this.cmdFTPCheck.Text = "&Testen";
            this.cmdFTPCheck.UseVisualStyleBackColor = true;
            this.cmdFTPCheck.Click += new System.EventHandler(this.cmdFTPCheck_Click);
            // 
            // txtFTPPort
            // 
            this.txtFTPPort.Location = new System.Drawing.Point(802, 32);
            this.txtFTPPort.Margin = new System.Windows.Forms.Padding(4);
            this.txtFTPPort.Name = "txtFTPPort";
            this.txtFTPPort.Size = new System.Drawing.Size(67, 31);
            this.txtFTPPort.TabIndex = 101;
            this.txtFTPPort.Text = "21";
            // 
            // Label8
            // 
            this.Label8.AutoSize = true;
            this.Label8.Location = new System.Drawing.Point(746, 36);
            this.Label8.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.Label8.Name = "Label8";
            this.Label8.Size = new System.Drawing.Size(48, 25);
            this.Label8.TabIndex = 100;
            this.Label8.Text = "Port:";
            // 
            // txtFTPPath
            // 
            this.txtFTPPath.Location = new System.Drawing.Point(194, 110);
            this.txtFTPPath.Margin = new System.Windows.Forms.Padding(4);
            this.txtFTPPath.Name = "txtFTPPath";
            this.txtFTPPath.Size = new System.Drawing.Size(676, 31);
            this.txtFTPPath.TabIndex = 99;
            // 
            // Label7
            // 
            this.Label7.AutoSize = true;
            this.Label7.Location = new System.Drawing.Point(24, 114);
            this.Label7.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.Label7.Name = "Label7";
            this.Label7.Size = new System.Drawing.Size(102, 25);
            this.Label7.TabIndex = 98;
            this.Label7.Text = "Verzeichnis:";
            // 
            // txtFTPPassword
            // 
            this.txtFTPPassword.Location = new System.Drawing.Point(614, 70);
            this.txtFTPPassword.Margin = new System.Windows.Forms.Padding(4);
            this.txtFTPPassword.Name = "txtFTPPassword";
            this.txtFTPPassword.Size = new System.Drawing.Size(256, 31);
            this.txtFTPPassword.TabIndex = 97;
            this.txtFTPPassword.UseSystemPasswordChar = true;
            // 
            // Label5
            // 
            this.Label5.AutoSize = true;
            this.Label5.Location = new System.Drawing.Point(508, 75);
            this.Label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.Label5.Name = "Label5";
            this.Label5.Size = new System.Drawing.Size(91, 25);
            this.Label5.TabIndex = 96;
            this.Label5.Text = "Kennwort:";
            // 
            // txtFTPUsername
            // 
            this.txtFTPUsername.Location = new System.Drawing.Point(194, 70);
            this.txtFTPUsername.Margin = new System.Windows.Forms.Padding(4);
            this.txtFTPUsername.Name = "txtFTPUsername";
            this.txtFTPUsername.Size = new System.Drawing.Size(304, 31);
            this.txtFTPUsername.TabIndex = 95;
            // 
            // Label4
            // 
            this.Label4.AutoSize = true;
            this.Label4.Location = new System.Drawing.Point(24, 75);
            this.Label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.Label4.Name = "Label4";
            this.Label4.Size = new System.Drawing.Size(128, 25);
            this.Label4.TabIndex = 94;
            this.Label4.Text = "Benutzername:";
            // 
            // txtFTPServer
            // 
            this.txtFTPServer.Location = new System.Drawing.Point(194, 32);
            this.txtFTPServer.Margin = new System.Windows.Forms.Padding(4);
            this.txtFTPServer.Name = "txtFTPServer";
            this.txtFTPServer.Size = new System.Drawing.Size(546, 31);
            this.txtFTPServer.TabIndex = 93;
            // 
            // Label2
            // 
            this.Label2.AutoSize = true;
            this.Label2.Location = new System.Drawing.Point(24, 36);
            this.Label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.Label2.Name = "Label2";
            this.Label2.Size = new System.Drawing.Size(65, 25);
            this.Label2.TabIndex = 92;
            this.Label2.Text = "Server:";
            // 
            // TabPage6
            // 
            this.TabPage6.Controls.Add(this.txtUNCPassword);
            this.TabPage6.Controls.Add(this.Label16);
            this.TabPage6.Controls.Add(this.txtUNCUsername);
            this.TabPage6.Controls.Add(this.Label17);
            this.TabPage6.Controls.Add(this.Label1);
            this.TabPage6.Controls.Add(this.txtUNCPath);
            this.TabPage6.Location = new System.Drawing.Point(4, 34);
            this.TabPage6.Margin = new System.Windows.Forms.Padding(4);
            this.TabPage6.Name = "TabPage6";
            this.TabPage6.Padding = new System.Windows.Forms.Padding(4);
            this.TabPage6.Size = new System.Drawing.Size(908, 284);
            this.TabPage6.TabIndex = 2;
            this.TabPage6.Text = "UNC-Pfad";
            this.TabPage6.UseVisualStyleBackColor = true;
            // 
            // txtUNCPassword
            // 
            this.txtUNCPassword.Location = new System.Drawing.Point(198, 158);
            this.txtUNCPassword.Margin = new System.Windows.Forms.Padding(4);
            this.txtUNCPassword.Name = "txtUNCPassword";
            this.txtUNCPassword.Size = new System.Drawing.Size(304, 31);
            this.txtUNCPassword.TabIndex = 101;
            this.txtUNCPassword.UseSystemPasswordChar = true;
            // 
            // Label16
            // 
            this.Label16.AutoSize = true;
            this.Label16.Location = new System.Drawing.Point(28, 162);
            this.Label16.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.Label16.Name = "Label16";
            this.Label16.Size = new System.Drawing.Size(91, 25);
            this.Label16.TabIndex = 100;
            this.Label16.Text = "Kennwort:";
            // 
            // txtUNCUsername
            // 
            this.txtUNCUsername.Location = new System.Drawing.Point(198, 114);
            this.txtUNCUsername.Margin = new System.Windows.Forms.Padding(4);
            this.txtUNCUsername.Name = "txtUNCUsername";
            this.txtUNCUsername.Size = new System.Drawing.Size(304, 31);
            this.txtUNCUsername.TabIndex = 99;
            // 
            // Label17
            // 
            this.Label17.AutoSize = true;
            this.Label17.Location = new System.Drawing.Point(28, 118);
            this.Label17.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.Label17.Name = "Label17";
            this.Label17.Size = new System.Drawing.Size(128, 25);
            this.Label17.TabIndex = 98;
            this.Label17.Text = "Benutzername:";
            // 
            // Label1
            // 
            this.Label1.AutoSize = true;
            this.Label1.Location = new System.Drawing.Point(28, 28);
            this.Label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.Label1.Name = "Label1";
            this.Label1.Size = new System.Drawing.Size(191, 25);
            this.Label1.TabIndex = 1;
            this.Label1.Text = "Manuelle Pfadeingabe:";
            // 
            // txtUNCPath
            // 
            this.txtUNCPath.Location = new System.Drawing.Point(33, 66);
            this.txtUNCPath.Margin = new System.Windows.Forms.Padding(4);
            this.txtUNCPath.Name = "txtUNCPath";
            this.txtUNCPath.Size = new System.Drawing.Size(836, 31);
            this.txtUNCPath.TabIndex = 0;
            // 
            // rdManualMode
            // 
            this.rdManualMode.AutoSize = true;
            this.rdManualMode.CheckAlign = System.Drawing.ContentAlignment.TopLeft;
            this.rdManualMode.Location = new System.Drawing.Point(57, 198);
            this.rdManualMode.Margin = new System.Windows.Forms.Padding(4);
            this.rdManualMode.Name = "rdManualMode";
            this.rdManualMode.Size = new System.Drawing.Size(358, 29);
            this.rdManualMode.TabIndex = 1;
            this.rdManualMode.Text = "Detailierte Einstellungen jetzt vornehmen";
            this.rdManualMode.UseVisualStyleBackColor = true;
            // 
            // rdFullAutomated
            // 
            this.rdFullAutomated.AutoSize = true;
            this.rdFullAutomated.CheckAlign = System.Drawing.ContentAlignment.TopLeft;
            this.rdFullAutomated.Checked = true;
            this.rdFullAutomated.Location = new System.Drawing.Point(57, 100);
            this.rdFullAutomated.Margin = new System.Windows.Forms.Padding(4);
            this.rdFullAutomated.Name = "rdFullAutomated";
            this.rdFullAutomated.Size = new System.Drawing.Size(852, 79);
            this.rdFullAutomated.TabIndex = 0;
            this.rdFullAutomated.TabStop = true;
            this.rdFullAutomated.Text = resources.GetString("rdFullAutomated.Text");
            this.rdFullAutomated.UseVisualStyleBackColor = true;
            // 
            // lblStatus
            // 
            this.lblStatus.Location = new System.Drawing.Point(33, 218);
            this.lblStatus.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(972, 22);
            this.lblStatus.TabIndex = 1;
            this.lblStatus.Text = "Backup Service Home wird eingerichtet...";
            this.lblStatus.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // ProgressBar1
            // 
            this.ProgressBar1.Location = new System.Drawing.Point(312, 249);
            this.ProgressBar1.Margin = new System.Windows.Forms.Padding(4);
            this.ProgressBar1.MarqueeAnimationSpeed = 50;
            this.ProgressBar1.Name = "ProgressBar1";
            this.ProgressBar1.Size = new System.Drawing.Size(416, 24);
            this.ProgressBar1.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
            this.ProgressBar1.TabIndex = 0;
            // 
            // tcStep5
            // 
            this.tcStep5.Controls.Add(this.TabPage3);
            this.tcStep5.Controls.Add(this.TabPage4);
            this.tcStep5.Controls.Add(this.TabPage5);
            this.tcStep5.Location = new System.Drawing.Point(57, 62);
            this.tcStep5.Margin = new System.Windows.Forms.Padding(4);
            this.tcStep5.Name = "tcStep5";
            this.tcStep5.SelectedIndex = 0;
            this.tcStep5.Size = new System.Drawing.Size(927, 322);
            this.tcStep5.TabIndex = 17;
            // 
            // TabPage3
            // 
            this.TabPage3.Controls.Add(this.cmdRefresh2);
            this.TabPage3.Controls.Add(this.lvBackupMedia);
            this.TabPage3.Location = new System.Drawing.Point(4, 34);
            this.TabPage3.Margin = new System.Windows.Forms.Padding(4);
            this.TabPage3.Name = "TabPage3";
            this.TabPage3.Padding = new System.Windows.Forms.Padding(4);
            this.TabPage3.Size = new System.Drawing.Size(919, 284);
            this.TabPage3.TabIndex = 0;
            this.TabPage3.Text = "Datenträger";
            this.TabPage3.UseVisualStyleBackColor = true;
            // 
            // cmdRefresh2
            // 
            this.cmdRefresh2.Location = new System.Drawing.Point(753, 9);
            this.cmdRefresh2.Margin = new System.Windows.Forms.Padding(4);
            this.cmdRefresh2.Name = "cmdRefresh2";
            this.cmdRefresh2.Size = new System.Drawing.Size(153, 39);
            this.cmdRefresh2.TabIndex = 15;
            this.cmdRefresh2.Text = "&Aktualisieren";
            this.cmdRefresh2.UseVisualStyleBackColor = true;
            this.cmdRefresh2.Click += new System.EventHandler(this.cmdRefresh2_Click);
            // 
            // lvBackupMedia
            // 
            this.lvBackupMedia.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.ColumnHeader3});
            this.lvBackupMedia.FullRowSelect = true;
            listViewGroup4.Header = "Festplatten";
            listViewGroup4.Name = "Local";
            listViewGroup5.Header = "Wechseldatenträger";
            listViewGroup5.Name = "Removable";
            listViewGroup6.Header = "Netzwerk";
            listViewGroup6.Name = "network";
            this.lvBackupMedia.Groups.AddRange(new System.Windows.Forms.ListViewGroup[] {
            listViewGroup4,
            listViewGroup5,
            listViewGroup6});
            this.lvBackupMedia.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.lvBackupMedia.HideSelection = false;
            this.lvBackupMedia.Location = new System.Drawing.Point(9, 9);
            this.lvBackupMedia.Margin = new System.Windows.Forms.Padding(4);
            this.lvBackupMedia.Name = "lvBackupMedia";
            this.lvBackupMedia.Size = new System.Drawing.Size(733, 264);
            this.lvBackupMedia.SmallImageList = this.ilImages;
            this.lvBackupMedia.TabIndex = 14;
            this.lvBackupMedia.UseCompatibleStateImageBehavior = false;
            this.lvBackupMedia.View = System.Windows.Forms.View.Details;
            // 
            // ColumnHeader3
            // 
            this.ColumnHeader3.Width = 450;
            // 
            // TabPage4
            // 
            this.TabPage4.Controls.Add(this.chkFtpEncryption2);
            this.TabPage4.Controls.Add(this.cboFtpEncoding2);
            this.TabPage4.Controls.Add(this.Label19);
            this.TabPage4.Controls.Add(this.txtFTPPort2);
            this.TabPage4.Controls.Add(this.Label6);
            this.TabPage4.Controls.Add(this.txtFTPPath2);
            this.TabPage4.Controls.Add(this.Label9);
            this.TabPage4.Controls.Add(this.txtFTPPass2);
            this.TabPage4.Controls.Add(this.Label10);
            this.TabPage4.Controls.Add(this.txtFTPUser2);
            this.TabPage4.Controls.Add(this.Label11);
            this.TabPage4.Controls.Add(this.txtFTPServer2);
            this.TabPage4.Controls.Add(this.Label12);
            this.TabPage4.Location = new System.Drawing.Point(4, 34);
            this.TabPage4.Margin = new System.Windows.Forms.Padding(4);
            this.TabPage4.Name = "TabPage4";
            this.TabPage4.Padding = new System.Windows.Forms.Padding(4);
            this.TabPage4.Size = new System.Drawing.Size(919, 284);
            this.TabPage4.TabIndex = 1;
            this.TabPage4.Text = "FTP-Server";
            this.TabPage4.UseVisualStyleBackColor = true;
            // 
            // chkFtpEncryption2
            // 
            this.chkFtpEncryption2.Location = new System.Drawing.Point(32, 197);
            this.chkFtpEncryption2.Name = "chkFtpEncryption2";
            this.chkFtpEncryption2.Size = new System.Drawing.Size(411, 40);
            this.chkFtpEncryption2.TabIndex = 104;
            this.chkFtpEncryption2.Text = "Unverschlüsselte Verbindung erzwingen";
            this.chkFtpEncryption2.UseVisualStyleBackColor = true;
            // 
            // cboFtpEncoding2
            // 
            this.cboFtpEncoding2.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboFtpEncoding2.FormattingEnabled = true;
            this.cboFtpEncoding2.Items.AddRange(new object[] {
            "ISO-8859-1",
            "UTF-8"});
            this.cboFtpEncoding2.Location = new System.Drawing.Point(196, 148);
            this.cboFtpEncoding2.Name = "cboFtpEncoding2";
            this.cboFtpEncoding2.Size = new System.Drawing.Size(304, 33);
            this.cboFtpEncoding2.TabIndex = 103;
            // 
            // Label19
            // 
            this.Label19.AutoSize = true;
            this.Label19.Location = new System.Drawing.Point(27, 152);
            this.Label19.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.Label19.Name = "Label19";
            this.Label19.Size = new System.Drawing.Size(90, 25);
            this.Label19.TabIndex = 102;
            this.Label19.Text = "Encoding:";
            // 
            // txtFTPPort2
            // 
            this.txtFTPPort2.Location = new System.Drawing.Point(806, 32);
            this.txtFTPPort2.Margin = new System.Windows.Forms.Padding(4);
            this.txtFTPPort2.Name = "txtFTPPort2";
            this.txtFTPPort2.Size = new System.Drawing.Size(67, 31);
            this.txtFTPPort2.TabIndex = 101;
            this.txtFTPPort2.Text = "21";
            // 
            // Label6
            // 
            this.Label6.AutoSize = true;
            this.Label6.Location = new System.Drawing.Point(748, 36);
            this.Label6.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.Label6.Name = "Label6";
            this.Label6.Size = new System.Drawing.Size(48, 25);
            this.Label6.TabIndex = 100;
            this.Label6.Text = "Port:";
            // 
            // txtFTPPath2
            // 
            this.txtFTPPath2.Location = new System.Drawing.Point(196, 110);
            this.txtFTPPath2.Margin = new System.Windows.Forms.Padding(4);
            this.txtFTPPath2.Name = "txtFTPPath2";
            this.txtFTPPath2.Size = new System.Drawing.Size(676, 31);
            this.txtFTPPath2.TabIndex = 99;
            // 
            // Label9
            // 
            this.Label9.AutoSize = true;
            this.Label9.Location = new System.Drawing.Point(27, 114);
            this.Label9.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.Label9.Name = "Label9";
            this.Label9.Size = new System.Drawing.Size(102, 25);
            this.Label9.TabIndex = 98;
            this.Label9.Text = "Verzeichnis:";
            // 
            // txtFTPPass2
            // 
            this.txtFTPPass2.Location = new System.Drawing.Point(616, 70);
            this.txtFTPPass2.Margin = new System.Windows.Forms.Padding(4);
            this.txtFTPPass2.Name = "txtFTPPass2";
            this.txtFTPPass2.Size = new System.Drawing.Size(256, 31);
            this.txtFTPPass2.TabIndex = 97;
            this.txtFTPPass2.UseSystemPasswordChar = true;
            // 
            // Label10
            // 
            this.Label10.AutoSize = true;
            this.Label10.Location = new System.Drawing.Point(512, 75);
            this.Label10.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.Label10.Name = "Label10";
            this.Label10.Size = new System.Drawing.Size(91, 25);
            this.Label10.TabIndex = 96;
            this.Label10.Text = "Kennwort:";
            // 
            // txtFTPUser2
            // 
            this.txtFTPUser2.Location = new System.Drawing.Point(196, 70);
            this.txtFTPUser2.Margin = new System.Windows.Forms.Padding(4);
            this.txtFTPUser2.Name = "txtFTPUser2";
            this.txtFTPUser2.Size = new System.Drawing.Size(304, 31);
            this.txtFTPUser2.TabIndex = 95;
            // 
            // Label11
            // 
            this.Label11.AutoSize = true;
            this.Label11.Location = new System.Drawing.Point(27, 75);
            this.Label11.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.Label11.Name = "Label11";
            this.Label11.Size = new System.Drawing.Size(128, 25);
            this.Label11.TabIndex = 94;
            this.Label11.Text = "Benutzername:";
            // 
            // txtFTPServer2
            // 
            this.txtFTPServer2.Location = new System.Drawing.Point(196, 32);
            this.txtFTPServer2.Margin = new System.Windows.Forms.Padding(4);
            this.txtFTPServer2.Name = "txtFTPServer2";
            this.txtFTPServer2.Size = new System.Drawing.Size(546, 31);
            this.txtFTPServer2.TabIndex = 93;
            // 
            // Label12
            // 
            this.Label12.AutoSize = true;
            this.Label12.Location = new System.Drawing.Point(27, 36);
            this.Label12.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.Label12.Name = "Label12";
            this.Label12.Size = new System.Drawing.Size(65, 25);
            this.Label12.TabIndex = 92;
            this.Label12.Text = "Server:";
            // 
            // TabPage5
            // 
            this.TabPage5.Controls.Add(this.cmdBrowse);
            this.TabPage5.Controls.Add(this.txtPath);
            this.TabPage5.Location = new System.Drawing.Point(4, 34);
            this.TabPage5.Margin = new System.Windows.Forms.Padding(4);
            this.TabPage5.Name = "TabPage5";
            this.TabPage5.Padding = new System.Windows.Forms.Padding(4);
            this.TabPage5.Size = new System.Drawing.Size(919, 284);
            this.TabPage5.TabIndex = 2;
            this.TabPage5.Text = "Manueller Pfad";
            this.TabPage5.UseVisualStyleBackColor = true;
            // 
            // cmdBrowse
            // 
            this.cmdBrowse.Location = new System.Drawing.Point(712, 75);
            this.cmdBrowse.Margin = new System.Windows.Forms.Padding(4);
            this.cmdBrowse.Name = "cmdBrowse";
            this.cmdBrowse.Size = new System.Drawing.Size(156, 39);
            this.cmdBrowse.TabIndex = 10;
            this.cmdBrowse.Text = "&Durchsuchen";
            this.cmdBrowse.UseVisualStyleBackColor = true;
            this.cmdBrowse.Click += new System.EventHandler(this.cmdBrowse_Click);
            // 
            // txtPath
            // 
            this.txtPath.Location = new System.Drawing.Point(30, 32);
            this.txtPath.Margin = new System.Windows.Forms.Padding(4);
            this.txtPath.Name = "txtPath";
            this.txtPath.Size = new System.Drawing.Size(836, 31);
            this.txtPath.TabIndex = 0;
            // 
            // lvBackups
            // 
            this.lvBackups.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.ColumnHeader4});
            this.lvBackups.FullRowSelect = true;
            this.lvBackups.HideSelection = false;
            this.lvBackups.Location = new System.Drawing.Point(57, 64);
            this.lvBackups.Margin = new System.Windows.Forms.Padding(4);
            this.lvBackups.Name = "lvBackups";
            this.lvBackups.Size = new System.Drawing.Size(925, 326);
            this.lvBackups.SmallImageList = this.ilImages;
            this.lvBackups.TabIndex = 1;
            this.lvBackups.UseCompatibleStateImageBehavior = false;
            this.lvBackups.View = System.Windows.Forms.View.Details;
            // 
            // ColumnHeader4
            // 
            this.ColumnHeader4.Text = "Benutzername";
            this.ColumnHeader4.Width = 579;
            // 
            // cmdChange
            // 
            this.cmdChange.Location = new System.Drawing.Point(870, 354);
            this.cmdChange.Margin = new System.Windows.Forms.Padding(4);
            this.cmdChange.Name = "cmdChange";
            this.cmdChange.Size = new System.Drawing.Size(114, 39);
            this.cmdChange.TabIndex = 19;
            this.cmdChange.Text = "&Ändern";
            this.cmdChange.UseVisualStyleBackColor = true;
            this.cmdChange.Click += new System.EventHandler(this.cmdChange_Click);
            // 
            // lvSourceDirs
            // 
            this.lvSourceDirs.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.ColumnHeader5,
            this.ColumnHeader6});
            this.lvSourceDirs.FullRowSelect = true;
            this.lvSourceDirs.HideSelection = false;
            this.lvSourceDirs.Location = new System.Drawing.Point(57, 62);
            this.lvSourceDirs.Margin = new System.Windows.Forms.Padding(4);
            this.lvSourceDirs.Name = "lvSourceDirs";
            this.lvSourceDirs.Size = new System.Drawing.Size(925, 282);
            this.lvSourceDirs.TabIndex = 1;
            this.lvSourceDirs.UseCompatibleStateImageBehavior = false;
            this.lvSourceDirs.View = System.Windows.Forms.View.Details;
            // 
            // ColumnHeader5
            // 
            this.ColumnHeader5.Text = "Neuer Speicherort";
            this.ColumnHeader5.Width = 260;
            // 
            // ColumnHeader6
            // 
            this.ColumnHeader6.Text = "Alter Speicherort";
            this.ColumnHeader6.Width = 341;
            // 
            // tbControl
            // 
            this.tbControl.Controls.Add(this.tbStep0);
            this.tbControl.Controls.Add(this.tbStep1);
            this.tbControl.Controls.Add(this.tbStep2);
            this.tbControl.Controls.Add(this.tbStep3);
            this.tbControl.Controls.Add(this.tbProgress);
            this.tbControl.Controls.Add(this.tbStep5);
            this.tbControl.Controls.Add(this.tbStep6);
            this.tbControl.Controls.Add(this.tbStep7);
            this.tbControl.Location = new System.Drawing.Point(-6, 42);
            this.tbControl.Margin = new System.Windows.Forms.Padding(4);
            this.tbControl.Name = "tbControl";
            this.tbControl.SelectedIndex = 0;
            this.tbControl.Size = new System.Drawing.Size(1094, 444);
            this.tbControl.TabIndex = 26;
            // 
            // tbStep0
            // 
            this.tbStep0.BackColor = System.Drawing.Color.White;
            this.tbStep0.Controls.Add(this.TableLayoutPanel1);
            this.tbStep0.Location = new System.Drawing.Point(4, 34);
            this.tbStep0.Margin = new System.Windows.Forms.Padding(4);
            this.tbStep0.Name = "tbStep0";
            this.tbStep0.Padding = new System.Windows.Forms.Padding(4);
            this.tbStep0.Size = new System.Drawing.Size(1086, 406);
            this.tbStep0.TabIndex = 7;
            this.tbStep0.Text = "tbStep0";
            // 
            // TableLayoutPanel1
            // 
            this.TableLayoutPanel1.ColumnCount = 2;
            this.TableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.TableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.TableLayoutPanel1.Controls.Add(this.Label3, 0, 0);
            this.TableLayoutPanel1.Controls.Add(this.Label14, 0, 1);
            this.TableLayoutPanel1.Controls.Add(this.Label13, 1, 1);
            this.TableLayoutPanel1.Controls.Add(this.Label15, 1, 0);
            this.TableLayoutPanel1.Controls.Add(this.Panel3, 0, 2);
            this.TableLayoutPanel1.Controls.Add(this.Panel4, 1, 2);
            this.TableLayoutPanel1.Location = new System.Drawing.Point(32, 62);
            this.TableLayoutPanel1.Margin = new System.Windows.Forms.Padding(4);
            this.TableLayoutPanel1.Name = "TableLayoutPanel1";
            this.TableLayoutPanel1.RowCount = 3;
            this.TableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 15.97938F));
            this.TableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 84.02062F));
            this.TableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 57F));
            this.TableLayoutPanel1.Size = new System.Drawing.Size(982, 322);
            this.TableLayoutPanel1.TabIndex = 26;
            // 
            // Label3
            // 
            this.Label3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Label3.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Label3.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(153)))));
            this.Label3.Location = new System.Drawing.Point(4, 0);
            this.Label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.Label3.Name = "Label3";
            this.Label3.Size = new System.Drawing.Size(483, 42);
            this.Label3.TabIndex = 20;
            this.Label3.Text = "Wiederherstellen einer Sicherung";
            this.Label3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // Label14
            // 
            this.Label14.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Label14.Location = new System.Drawing.Point(4, 42);
            this.Label14.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.Label14.Name = "Label14";
            this.Label14.Size = new System.Drawing.Size(483, 222);
            this.Label14.TabIndex = 22;
            this.Label14.Text = "Wenn Sie bereits eine Sicherung auf einem externen Datenträger oder auf einem FTP" +
    "-Server erstellt haben und diese Sicherung nun wiederherstellen möchten, dann kl" +
    "icken Sie auf \"Importieren\".";
            this.Label14.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // Label13
            // 
            this.Label13.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Label13.Location = new System.Drawing.Point(495, 42);
            this.Label13.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.Label13.Name = "Label13";
            this.Label13.Size = new System.Drawing.Size(483, 222);
            this.Label13.TabIndex = 24;
            this.Label13.Text = "Sie möchten Backup Service Home 3 für diesen Computer einrichten und Sicherungen " +
    "durchführen , dann klicken Sie auf \"Konfigurieren\".";
            this.Label13.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // Label15
            // 
            this.Label15.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Label15.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Label15.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(153)))));
            this.Label15.Location = new System.Drawing.Point(495, 0);
            this.Label15.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.Label15.Name = "Label15";
            this.Label15.Size = new System.Drawing.Size(483, 42);
            this.Label15.TabIndex = 23;
            this.Label15.Text = "Erste Einrichtung";
            this.Label15.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // Panel3
            // 
            this.Panel3.Controls.Add(this.cmdImport);
            this.Panel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Panel3.Location = new System.Drawing.Point(4, 268);
            this.Panel3.Margin = new System.Windows.Forms.Padding(4);
            this.Panel3.Name = "Panel3";
            this.Panel3.Size = new System.Drawing.Size(483, 50);
            this.Panel3.TabIndex = 26;
            // 
            // Panel4
            // 
            this.Panel4.Controls.Add(this.cmdConfigure);
            this.Panel4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Panel4.Location = new System.Drawing.Point(495, 268);
            this.Panel4.Margin = new System.Windows.Forms.Padding(4);
            this.Panel4.Name = "Panel4";
            this.Panel4.Size = new System.Drawing.Size(483, 50);
            this.Panel4.TabIndex = 27;
            // 
            // cmdConfigure
            // 
            this.cmdConfigure.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.cmdConfigure.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmdConfigure.Location = new System.Drawing.Point(165, 1);
            this.cmdConfigure.Margin = new System.Windows.Forms.Padding(4);
            this.cmdConfigure.Name = "cmdConfigure";
            this.cmdConfigure.Size = new System.Drawing.Size(153, 48);
            this.cmdConfigure.TabIndex = 25;
            this.cmdConfigure.Text = "&Konfigurieren";
            this.cmdConfigure.UseVisualStyleBackColor = true;
            this.cmdConfigure.Click += new System.EventHandler(this.cmdConfigure_Click);
            // 
            // tbStep1
            // 
            this.tbStep1.BackColor = System.Drawing.Color.White;
            this.tbStep1.Controls.Add(this.cmdDelete);
            this.tbStep1.Controls.Add(this.lvSourceFolders);
            this.tbStep1.Controls.Add(this.cmdAdd);
            this.tbStep1.Location = new System.Drawing.Point(4, 34);
            this.tbStep1.Margin = new System.Windows.Forms.Padding(4);
            this.tbStep1.Name = "tbStep1";
            this.tbStep1.Padding = new System.Windows.Forms.Padding(4);
            this.tbStep1.Size = new System.Drawing.Size(1086, 406);
            this.tbStep1.TabIndex = 0;
            this.tbStep1.Text = "Step1";
            // 
            // tbStep2
            // 
            this.tbStep2.BackColor = System.Drawing.Color.White;
            this.tbStep2.Controls.Add(this.tcSource);
            this.tbStep2.Location = new System.Drawing.Point(4, 34);
            this.tbStep2.Margin = new System.Windows.Forms.Padding(4);
            this.tbStep2.Name = "tbStep2";
            this.tbStep2.Padding = new System.Windows.Forms.Padding(4);
            this.tbStep2.Size = new System.Drawing.Size(1086, 406);
            this.tbStep2.TabIndex = 3;
            this.tbStep2.Text = "Step2";
            // 
            // tbStep3
            // 
            this.tbStep3.BackColor = System.Drawing.Color.White;
            this.tbStep3.Controls.Add(this.rdManualMode);
            this.tbStep3.Controls.Add(this.rdFullAutomated);
            this.tbStep3.Location = new System.Drawing.Point(4, 34);
            this.tbStep3.Margin = new System.Windows.Forms.Padding(4);
            this.tbStep3.Name = "tbStep3";
            this.tbStep3.Padding = new System.Windows.Forms.Padding(4);
            this.tbStep3.Size = new System.Drawing.Size(1086, 406);
            this.tbStep3.TabIndex = 1;
            this.tbStep3.Text = "Step3";
            // 
            // tbProgress
            // 
            this.tbProgress.BackColor = System.Drawing.Color.White;
            this.tbProgress.Controls.Add(this.lblStatus);
            this.tbProgress.Controls.Add(this.ProgressBar1);
            this.tbProgress.Location = new System.Drawing.Point(4, 34);
            this.tbProgress.Margin = new System.Windows.Forms.Padding(4);
            this.tbProgress.Name = "tbProgress";
            this.tbProgress.Padding = new System.Windows.Forms.Padding(4);
            this.tbProgress.Size = new System.Drawing.Size(1086, 406);
            this.tbProgress.TabIndex = 2;
            this.tbProgress.Text = "Progress";
            // 
            // tbStep5
            // 
            this.tbStep5.BackColor = System.Drawing.Color.White;
            this.tbStep5.Controls.Add(this.tcStep5);
            this.tbStep5.Location = new System.Drawing.Point(4, 34);
            this.tbStep5.Margin = new System.Windows.Forms.Padding(4);
            this.tbStep5.Name = "tbStep5";
            this.tbStep5.Padding = new System.Windows.Forms.Padding(4);
            this.tbStep5.Size = new System.Drawing.Size(1086, 406);
            this.tbStep5.TabIndex = 6;
            this.tbStep5.Text = "Step5";
            // 
            // tbStep6
            // 
            this.tbStep6.BackColor = System.Drawing.Color.White;
            this.tbStep6.Controls.Add(this.lvBackups);
            this.tbStep6.Location = new System.Drawing.Point(4, 34);
            this.tbStep6.Margin = new System.Windows.Forms.Padding(4);
            this.tbStep6.Name = "tbStep6";
            this.tbStep6.Padding = new System.Windows.Forms.Padding(4);
            this.tbStep6.Size = new System.Drawing.Size(1086, 406);
            this.tbStep6.TabIndex = 5;
            this.tbStep6.Text = "Step6";
            // 
            // tbStep7
            // 
            this.tbStep7.BackColor = System.Drawing.Color.White;
            this.tbStep7.Controls.Add(this.cmdChange);
            this.tbStep7.Controls.Add(this.lvSourceDirs);
            this.tbStep7.Location = new System.Drawing.Point(4, 34);
            this.tbStep7.Margin = new System.Windows.Forms.Padding(4);
            this.tbStep7.Name = "tbStep7";
            this.tbStep7.Padding = new System.Windows.Forms.Padding(4);
            this.tbStep7.Size = new System.Drawing.Size(1086, 406);
            this.tbStep7.TabIndex = 4;
            this.tbStep7.Text = "Step7";
            // 
            // Panel1
            // 
            this.Panel1.Controls.Add(this.lblTitle);
            this.Panel1.Controls.Add(this.lblDescription);
            this.Panel1.Location = new System.Drawing.Point(-6, 4);
            this.Panel1.Margin = new System.Windows.Forms.Padding(4);
            this.Panel1.Name = "Panel1";
            this.Panel1.Size = new System.Drawing.Size(1056, 126);
            this.Panel1.TabIndex = 27;
            // 
            // Panel2
            // 
            this.Panel2.Location = new System.Drawing.Point(0, 471);
            this.Panel2.Margin = new System.Windows.Forms.Padding(4);
            this.Panel2.Name = "Panel2";
            this.Panel2.Size = new System.Drawing.Size(1050, 15);
            this.Panel2.TabIndex = 28;
            // 
            // ucDoConfigure
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(144F, 144F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.BackColor = System.Drawing.Color.White;
            this.Controls.Add(this.Panel1);
            this.Controls.Add(this.Panel2);
            this.Controls.Add(this.cmdBack);
            this.Controls.Add(this.cmdNext);
            this.Controls.Add(this.tbControl);
            this.DoubleBuffered = true;
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "ucDoConfigure";
            this.Size = new System.Drawing.Size(1050, 570);
            this.tcSource.ResumeLayout(false);
            this.TabPage1.ResumeLayout(false);
            this.TabPage2.ResumeLayout(false);
            this.TabPage2.PerformLayout();
            this.TabPage6.ResumeLayout(false);
            this.TabPage6.PerformLayout();
            this.tcStep5.ResumeLayout(false);
            this.TabPage3.ResumeLayout(false);
            this.TabPage4.ResumeLayout(false);
            this.TabPage4.PerformLayout();
            this.TabPage5.ResumeLayout(false);
            this.TabPage5.PerformLayout();
            this.tbControl.ResumeLayout(false);
            this.tbStep0.ResumeLayout(false);
            this.TableLayoutPanel1.ResumeLayout(false);
            this.Panel3.ResumeLayout(false);
            this.Panel4.ResumeLayout(false);
            this.tbStep1.ResumeLayout(false);
            this.tbStep2.ResumeLayout(false);
            this.tbStep3.ResumeLayout(false);
            this.tbStep3.PerformLayout();
            this.tbProgress.ResumeLayout(false);
            this.tbStep5.ResumeLayout(false);
            this.tbStep6.ResumeLayout(false);
            this.tbStep7.ResumeLayout(false);
            this.Panel1.ResumeLayout(false);
            this.Panel1.PerformLayout();
            this.ResumeLayout(false);

        }

        internal Button cmdNext;
        internal Label lblTitle;
        internal Label lblDescription;
        internal ListView lvSourceFolders;
        internal Button cmdAdd;
        internal Button cmdDelete;
        internal Button cmdBack;
        internal Button cmdImport;
        internal ImageList ilImages;
        internal ColumnHeader ColumnHeader1;
        internal Button cmdRefresh;
        internal ListView lvBackupDrive;
        internal ColumnHeader ColumnHeader2;
        internal TabControl tcSource;
        internal TabPage TabPage1;
        internal TabPage TabPage2;
        internal RadioButton rdManualMode;
        internal RadioButton rdFullAutomated;
        internal Button cmdFTPCheck;
        internal TextBox txtFTPPort;
        internal Label Label8;
        internal TextBox txtFTPPath;
        internal Label Label7;
        internal TextBox txtFTPPassword;
        internal Label Label5;
        internal TextBox txtFTPUsername;
        internal Label Label4;
        internal TextBox txtFTPServer;
        internal Label Label2;
        internal Label lblStatus;
        internal ProgressBar ProgressBar1;
        internal TabControl tcStep5;
        internal TabPage TabPage3;
        internal Button cmdRefresh2;
        internal ListView lvBackupMedia;
        internal ColumnHeader ColumnHeader3;
        internal TabPage TabPage4;
        internal TextBox txtFTPPort2;
        internal Label Label6;
        internal TextBox txtFTPPath2;
        internal Label Label9;
        internal TextBox txtFTPPass2;
        internal Label Label10;
        internal TextBox txtFTPUser2;
        internal Label Label11;
        internal TextBox txtFTPServer2;
        internal Label Label12;
        internal ListView lvBackups;
        internal ColumnHeader ColumnHeader4;
        internal TabPage TabPage5;
        internal Button cmdBrowse;
        internal TextBox txtPath;
        internal ListView lvSourceDirs;
        internal ColumnHeader ColumnHeader5;
        internal ColumnHeader ColumnHeader6;
        internal Button cmdChange;
        internal TabControl tbControl;
        internal TabPage tbStep1;
        internal TabPage tbStep3;
        internal TabPage tbProgress;
        internal TabPage tbStep2;
        internal TabPage tbStep7;
        internal TabPage tbStep6;
        internal TabPage tbStep5;
        internal Panel Panel1;
        internal Panel Panel2;
        internal TabPage tbStep0;
        internal Button cmdConfigure;
        internal Label Label13;
        internal Label Label15;
        internal Label Label14;
        internal Label Label3;
        internal TableLayoutPanel TableLayoutPanel1;
        internal Panel Panel3;
        internal Panel Panel4;
        internal TabPage TabPage6;
        internal Label Label1;
        internal TextBox txtUNCPath;
        internal TextBox txtUNCPassword;
        internal Label Label16;
        internal TextBox txtUNCUsername;
        internal Label Label17;
        internal Label Label18;
        internal ComboBox cboFtpEncoding;
        internal ComboBox cboFtpEncoding2;
        internal Label Label19;
        private CheckBox chkFtpEncryption;
        private CheckBox chkFtpEncryption2;
    }
}