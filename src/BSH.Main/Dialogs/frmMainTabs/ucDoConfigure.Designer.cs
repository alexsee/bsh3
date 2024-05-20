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
            components = new System.ComponentModel.Container();
            var resources = new System.ComponentModel.ComponentResourceManager(typeof(ucDoConfigure));
            cmdNext = new Button();
            lblTitle = new Label();
            lblDescription = new Label();
            lvSourceFolders = new ListView();
            ColumnHeader1 = new ColumnHeader();
            ilImages = new ImageList(components);
            cmdAdd = new Button();
            cmdDelete = new Button();
            cmdBack = new Button();
            cmdImport = new Button();
            tcSource = new TabControl();
            TabPage1 = new TabPage();
            cmdRefresh = new Button();
            lvBackupDrive = new ListView();
            ColumnHeader2 = new ColumnHeader();
            TabPage2 = new TabPage();
            chkFtpEncryption = new CheckBox();
            Label18 = new Label();
            cboFtpEncoding = new ComboBox();
            cmdFTPCheck = new Button();
            txtFTPPort = new TextBox();
            Label8 = new Label();
            txtFTPPath = new TextBox();
            Label7 = new Label();
            txtFTPPassword = new TextBox();
            Label5 = new Label();
            txtFTPUsername = new TextBox();
            Label4 = new Label();
            txtFTPServer = new TextBox();
            Label2 = new Label();
            TabPage6 = new TabPage();
            txtUNCPassword = new TextBox();
            Label16 = new Label();
            txtUNCUsername = new TextBox();
            Label17 = new Label();
            Label1 = new Label();
            txtUNCPath = new TextBox();
            rdManualMode = new RadioButton();
            rdFullAutomated = new RadioButton();
            lblStatus = new Label();
            ProgressBar1 = new ProgressBar();
            tcStep5 = new TabControl();
            TabPage3 = new TabPage();
            cmdRefresh2 = new Button();
            lvBackupMedia = new ListView();
            ColumnHeader3 = new ColumnHeader();
            TabPage4 = new TabPage();
            chkFtpEncryption2 = new CheckBox();
            cboFtpEncoding2 = new ComboBox();
            Label19 = new Label();
            txtFTPPort2 = new TextBox();
            Label6 = new Label();
            txtFTPPath2 = new TextBox();
            Label9 = new Label();
            txtFTPPass2 = new TextBox();
            Label10 = new Label();
            txtFTPUser2 = new TextBox();
            Label11 = new Label();
            txtFTPServer2 = new TextBox();
            Label12 = new Label();
            TabPage5 = new TabPage();
            cmdBrowse = new Button();
            txtPath = new TextBox();
            lvBackups = new ListView();
            ColumnHeader4 = new ColumnHeader();
            cmdChange = new Button();
            lvSourceDirs = new ListView();
            ColumnHeader5 = new ColumnHeader();
            ColumnHeader6 = new ColumnHeader();
            tbControl = new TabControl();
            tbStep0 = new TabPage();
            TableLayoutPanel1 = new TableLayoutPanel();
            Label3 = new Label();
            Label14 = new Label();
            Label13 = new Label();
            Label15 = new Label();
            Panel3 = new Panel();
            Panel4 = new Panel();
            cmdConfigure = new Button();
            tbStep1 = new TabPage();
            tbStep2 = new TabPage();
            tbStep3 = new TabPage();
            tbProgress = new TabPage();
            tbStep5 = new TabPage();
            tbStep6 = new TabPage();
            tbStep7 = new TabPage();
            Panel1 = new Panel();
            Panel2 = new Panel();
            tcSource.SuspendLayout();
            TabPage1.SuspendLayout();
            TabPage2.SuspendLayout();
            TabPage6.SuspendLayout();
            tcStep5.SuspendLayout();
            TabPage3.SuspendLayout();
            TabPage4.SuspendLayout();
            TabPage5.SuspendLayout();
            tbControl.SuspendLayout();
            tbStep0.SuspendLayout();
            TableLayoutPanel1.SuspendLayout();
            Panel3.SuspendLayout();
            Panel4.SuspendLayout();
            tbStep1.SuspendLayout();
            tbStep2.SuspendLayout();
            tbStep3.SuspendLayout();
            tbProgress.SuspendLayout();
            tbStep5.SuspendLayout();
            tbStep6.SuspendLayout();
            tbStep7.SuspendLayout();
            Panel1.SuspendLayout();
            SuspendLayout();
            // 
            // cmdNext
            // 
            resources.ApplyResources(cmdNext, "cmdNext");
            cmdNext.Name = "cmdNext";
            cmdNext.UseVisualStyleBackColor = true;
            cmdNext.Click += cmdNext_Click;
            // 
            // lblTitle
            // 
            resources.ApplyResources(lblTitle, "lblTitle");
            lblTitle.ForeColor = Color.FromArgb(0, 51, 153);
            lblTitle.Name = "lblTitle";
            // 
            // lblDescription
            // 
            resources.ApplyResources(lblDescription, "lblDescription");
            lblDescription.Name = "lblDescription";
            // 
            // lvSourceFolders
            // 
            resources.ApplyResources(lvSourceFolders, "lvSourceFolders");
            lvSourceFolders.Columns.AddRange(new ColumnHeader[] { ColumnHeader1 });
            lvSourceFolders.FullRowSelect = true;
            lvSourceFolders.HeaderStyle = ColumnHeaderStyle.None;
            lvSourceFolders.MultiSelect = false;
            lvSourceFolders.Name = "lvSourceFolders";
            lvSourceFolders.SmallImageList = ilImages;
            lvSourceFolders.UseCompatibleStateImageBehavior = false;
            lvSourceFolders.View = View.Details;
            lvSourceFolders.SelectedIndexChanged += lvSourceFolders_SelectedIndexChanged;
            // 
            // ColumnHeader1
            // 
            resources.ApplyResources(ColumnHeader1, "ColumnHeader1");
            // 
            // ilImages
            // 
            ilImages.ColorDepth = ColorDepth.Depth32Bit;
            ilImages.ImageStream = (ImageListStreamer)resources.GetObject("ilImages.ImageStream");
            ilImages.TransparentColor = Color.Transparent;
            ilImages.Images.SetKeyName(0, "account_circle_icon_24.png");
            ilImages.Images.SetKeyName(1, "folder_icon_22.png");
            ilImages.Images.SetKeyName(2, "ic_fluent_hard_drive_22_regular.png");
            ilImages.Images.SetKeyName(3, "smb_share_icon_24.png");
            ilImages.Images.SetKeyName(4, "supervisor_account_icon_24.png");
            // 
            // cmdAdd
            // 
            resources.ApplyResources(cmdAdd, "cmdAdd");
            cmdAdd.Name = "cmdAdd";
            cmdAdd.UseVisualStyleBackColor = true;
            cmdAdd.Click += cmdAdd_Click;
            // 
            // cmdDelete
            // 
            resources.ApplyResources(cmdDelete, "cmdDelete");
            cmdDelete.Name = "cmdDelete";
            cmdDelete.UseVisualStyleBackColor = true;
            cmdDelete.Click += cmdDelete_Click;
            // 
            // cmdBack
            // 
            resources.ApplyResources(cmdBack, "cmdBack");
            cmdBack.Name = "cmdBack";
            cmdBack.UseVisualStyleBackColor = true;
            cmdBack.Click += cmdBack_Click;
            // 
            // cmdImport
            // 
            resources.ApplyResources(cmdImport, "cmdImport");
            cmdImport.Name = "cmdImport";
            cmdImport.UseVisualStyleBackColor = true;
            cmdImport.Click += cmdImport_Click;
            // 
            // tcSource
            // 
            resources.ApplyResources(tcSource, "tcSource");
            tcSource.Controls.Add(TabPage1);
            tcSource.Controls.Add(TabPage2);
            tcSource.Controls.Add(TabPage6);
            tcSource.Name = "tcSource";
            tcSource.SelectedIndex = 0;
            // 
            // TabPage1
            // 
            resources.ApplyResources(TabPage1, "TabPage1");
            TabPage1.Controls.Add(cmdRefresh);
            TabPage1.Controls.Add(lvBackupDrive);
            TabPage1.Name = "TabPage1";
            TabPage1.UseVisualStyleBackColor = true;
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
            // TabPage2
            // 
            resources.ApplyResources(TabPage2, "TabPage2");
            TabPage2.Controls.Add(chkFtpEncryption);
            TabPage2.Controls.Add(Label18);
            TabPage2.Controls.Add(cboFtpEncoding);
            TabPage2.Controls.Add(cmdFTPCheck);
            TabPage2.Controls.Add(txtFTPPort);
            TabPage2.Controls.Add(Label8);
            TabPage2.Controls.Add(txtFTPPath);
            TabPage2.Controls.Add(Label7);
            TabPage2.Controls.Add(txtFTPPassword);
            TabPage2.Controls.Add(Label5);
            TabPage2.Controls.Add(txtFTPUsername);
            TabPage2.Controls.Add(Label4);
            TabPage2.Controls.Add(txtFTPServer);
            TabPage2.Controls.Add(Label2);
            TabPage2.Name = "TabPage2";
            TabPage2.UseVisualStyleBackColor = true;
            // 
            // chkFtpEncryption
            // 
            resources.ApplyResources(chkFtpEncryption, "chkFtpEncryption");
            chkFtpEncryption.Name = "chkFtpEncryption";
            chkFtpEncryption.UseVisualStyleBackColor = true;
            // 
            // Label18
            // 
            resources.ApplyResources(Label18, "Label18");
            Label18.Name = "Label18";
            // 
            // cboFtpEncoding
            // 
            resources.ApplyResources(cboFtpEncoding, "cboFtpEncoding");
            cboFtpEncoding.DropDownStyle = ComboBoxStyle.DropDownList;
            cboFtpEncoding.FormattingEnabled = true;
            cboFtpEncoding.Items.AddRange(new object[] { resources.GetString("cboFtpEncoding.Items"), resources.GetString("cboFtpEncoding.Items1") });
            cboFtpEncoding.Name = "cboFtpEncoding";
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
            // Label4
            // 
            resources.ApplyResources(Label4, "Label4");
            Label4.Name = "Label4";
            // 
            // txtFTPServer
            // 
            resources.ApplyResources(txtFTPServer, "txtFTPServer");
            txtFTPServer.Name = "txtFTPServer";
            // 
            // Label2
            // 
            resources.ApplyResources(Label2, "Label2");
            Label2.Name = "Label2";
            // 
            // TabPage6
            // 
            resources.ApplyResources(TabPage6, "TabPage6");
            TabPage6.Controls.Add(txtUNCPassword);
            TabPage6.Controls.Add(Label16);
            TabPage6.Controls.Add(txtUNCUsername);
            TabPage6.Controls.Add(Label17);
            TabPage6.Controls.Add(Label1);
            TabPage6.Controls.Add(txtUNCPath);
            TabPage6.Name = "TabPage6";
            TabPage6.UseVisualStyleBackColor = true;
            // 
            // txtUNCPassword
            // 
            resources.ApplyResources(txtUNCPassword, "txtUNCPassword");
            txtUNCPassword.Name = "txtUNCPassword";
            txtUNCPassword.UseSystemPasswordChar = true;
            // 
            // Label16
            // 
            resources.ApplyResources(Label16, "Label16");
            Label16.Name = "Label16";
            // 
            // txtUNCUsername
            // 
            resources.ApplyResources(txtUNCUsername, "txtUNCUsername");
            txtUNCUsername.Name = "txtUNCUsername";
            // 
            // Label17
            // 
            resources.ApplyResources(Label17, "Label17");
            Label17.Name = "Label17";
            // 
            // Label1
            // 
            resources.ApplyResources(Label1, "Label1");
            Label1.Name = "Label1";
            // 
            // txtUNCPath
            // 
            resources.ApplyResources(txtUNCPath, "txtUNCPath");
            txtUNCPath.Name = "txtUNCPath";
            // 
            // rdManualMode
            // 
            resources.ApplyResources(rdManualMode, "rdManualMode");
            rdManualMode.Name = "rdManualMode";
            rdManualMode.UseVisualStyleBackColor = true;
            // 
            // rdFullAutomated
            // 
            resources.ApplyResources(rdFullAutomated, "rdFullAutomated");
            rdFullAutomated.Checked = true;
            rdFullAutomated.Name = "rdFullAutomated";
            rdFullAutomated.TabStop = true;
            rdFullAutomated.UseVisualStyleBackColor = true;
            // 
            // lblStatus
            // 
            resources.ApplyResources(lblStatus, "lblStatus");
            lblStatus.Name = "lblStatus";
            // 
            // ProgressBar1
            // 
            resources.ApplyResources(ProgressBar1, "ProgressBar1");
            ProgressBar1.MarqueeAnimationSpeed = 50;
            ProgressBar1.Name = "ProgressBar1";
            ProgressBar1.Style = ProgressBarStyle.Marquee;
            // 
            // tcStep5
            // 
            resources.ApplyResources(tcStep5, "tcStep5");
            tcStep5.Controls.Add(TabPage3);
            tcStep5.Controls.Add(TabPage4);
            tcStep5.Controls.Add(TabPage5);
            tcStep5.Name = "tcStep5";
            tcStep5.SelectedIndex = 0;
            // 
            // TabPage3
            // 
            resources.ApplyResources(TabPage3, "TabPage3");
            TabPage3.Controls.Add(cmdRefresh2);
            TabPage3.Controls.Add(lvBackupMedia);
            TabPage3.Name = "TabPage3";
            TabPage3.UseVisualStyleBackColor = true;
            // 
            // cmdRefresh2
            // 
            resources.ApplyResources(cmdRefresh2, "cmdRefresh2");
            cmdRefresh2.Name = "cmdRefresh2";
            cmdRefresh2.UseVisualStyleBackColor = true;
            cmdRefresh2.Click += cmdRefresh2_Click;
            // 
            // lvBackupMedia
            // 
            resources.ApplyResources(lvBackupMedia, "lvBackupMedia");
            lvBackupMedia.Columns.AddRange(new ColumnHeader[] { ColumnHeader3 });
            lvBackupMedia.FullRowSelect = true;
            lvBackupMedia.Groups.AddRange(new ListViewGroup[] { (ListViewGroup)resources.GetObject("lvBackupMedia.Groups"), (ListViewGroup)resources.GetObject("lvBackupMedia.Groups1"), (ListViewGroup)resources.GetObject("lvBackupMedia.Groups2") });
            lvBackupMedia.HeaderStyle = ColumnHeaderStyle.None;
            lvBackupMedia.Name = "lvBackupMedia";
            lvBackupMedia.SmallImageList = ilImages;
            lvBackupMedia.UseCompatibleStateImageBehavior = false;
            lvBackupMedia.View = View.Details;
            // 
            // ColumnHeader3
            // 
            resources.ApplyResources(ColumnHeader3, "ColumnHeader3");
            // 
            // TabPage4
            // 
            resources.ApplyResources(TabPage4, "TabPage4");
            TabPage4.Controls.Add(chkFtpEncryption2);
            TabPage4.Controls.Add(cboFtpEncoding2);
            TabPage4.Controls.Add(Label19);
            TabPage4.Controls.Add(txtFTPPort2);
            TabPage4.Controls.Add(Label6);
            TabPage4.Controls.Add(txtFTPPath2);
            TabPage4.Controls.Add(Label9);
            TabPage4.Controls.Add(txtFTPPass2);
            TabPage4.Controls.Add(Label10);
            TabPage4.Controls.Add(txtFTPUser2);
            TabPage4.Controls.Add(Label11);
            TabPage4.Controls.Add(txtFTPServer2);
            TabPage4.Controls.Add(Label12);
            TabPage4.Name = "TabPage4";
            TabPage4.UseVisualStyleBackColor = true;
            // 
            // chkFtpEncryption2
            // 
            resources.ApplyResources(chkFtpEncryption2, "chkFtpEncryption2");
            chkFtpEncryption2.Name = "chkFtpEncryption2";
            chkFtpEncryption2.UseVisualStyleBackColor = true;
            // 
            // cboFtpEncoding2
            // 
            resources.ApplyResources(cboFtpEncoding2, "cboFtpEncoding2");
            cboFtpEncoding2.DropDownStyle = ComboBoxStyle.DropDownList;
            cboFtpEncoding2.FormattingEnabled = true;
            cboFtpEncoding2.Items.AddRange(new object[] { resources.GetString("cboFtpEncoding2.Items"), resources.GetString("cboFtpEncoding2.Items1") });
            cboFtpEncoding2.Name = "cboFtpEncoding2";
            // 
            // Label19
            // 
            resources.ApplyResources(Label19, "Label19");
            Label19.Name = "Label19";
            // 
            // txtFTPPort2
            // 
            resources.ApplyResources(txtFTPPort2, "txtFTPPort2");
            txtFTPPort2.Name = "txtFTPPort2";
            // 
            // Label6
            // 
            resources.ApplyResources(Label6, "Label6");
            Label6.Name = "Label6";
            // 
            // txtFTPPath2
            // 
            resources.ApplyResources(txtFTPPath2, "txtFTPPath2");
            txtFTPPath2.Name = "txtFTPPath2";
            // 
            // Label9
            // 
            resources.ApplyResources(Label9, "Label9");
            Label9.Name = "Label9";
            // 
            // txtFTPPass2
            // 
            resources.ApplyResources(txtFTPPass2, "txtFTPPass2");
            txtFTPPass2.Name = "txtFTPPass2";
            txtFTPPass2.UseSystemPasswordChar = true;
            // 
            // Label10
            // 
            resources.ApplyResources(Label10, "Label10");
            Label10.Name = "Label10";
            // 
            // txtFTPUser2
            // 
            resources.ApplyResources(txtFTPUser2, "txtFTPUser2");
            txtFTPUser2.Name = "txtFTPUser2";
            // 
            // Label11
            // 
            resources.ApplyResources(Label11, "Label11");
            Label11.Name = "Label11";
            // 
            // txtFTPServer2
            // 
            resources.ApplyResources(txtFTPServer2, "txtFTPServer2");
            txtFTPServer2.Name = "txtFTPServer2";
            // 
            // Label12
            // 
            resources.ApplyResources(Label12, "Label12");
            Label12.Name = "Label12";
            // 
            // TabPage5
            // 
            resources.ApplyResources(TabPage5, "TabPage5");
            TabPage5.Controls.Add(cmdBrowse);
            TabPage5.Controls.Add(txtPath);
            TabPage5.Name = "TabPage5";
            TabPage5.UseVisualStyleBackColor = true;
            // 
            // cmdBrowse
            // 
            resources.ApplyResources(cmdBrowse, "cmdBrowse");
            cmdBrowse.Name = "cmdBrowse";
            cmdBrowse.UseVisualStyleBackColor = true;
            cmdBrowse.Click += cmdBrowse_Click;
            // 
            // txtPath
            // 
            resources.ApplyResources(txtPath, "txtPath");
            txtPath.Name = "txtPath";
            // 
            // lvBackups
            // 
            resources.ApplyResources(lvBackups, "lvBackups");
            lvBackups.Columns.AddRange(new ColumnHeader[] { ColumnHeader4 });
            lvBackups.FullRowSelect = true;
            lvBackups.Name = "lvBackups";
            lvBackups.SmallImageList = ilImages;
            lvBackups.UseCompatibleStateImageBehavior = false;
            lvBackups.View = View.Details;
            // 
            // ColumnHeader4
            // 
            resources.ApplyResources(ColumnHeader4, "ColumnHeader4");
            // 
            // cmdChange
            // 
            resources.ApplyResources(cmdChange, "cmdChange");
            cmdChange.Name = "cmdChange";
            cmdChange.UseVisualStyleBackColor = true;
            cmdChange.Click += cmdChange_Click;
            // 
            // lvSourceDirs
            // 
            resources.ApplyResources(lvSourceDirs, "lvSourceDirs");
            lvSourceDirs.Columns.AddRange(new ColumnHeader[] { ColumnHeader5, ColumnHeader6 });
            lvSourceDirs.FullRowSelect = true;
            lvSourceDirs.Name = "lvSourceDirs";
            lvSourceDirs.UseCompatibleStateImageBehavior = false;
            lvSourceDirs.View = View.Details;
            // 
            // ColumnHeader5
            // 
            resources.ApplyResources(ColumnHeader5, "ColumnHeader5");
            // 
            // ColumnHeader6
            // 
            resources.ApplyResources(ColumnHeader6, "ColumnHeader6");
            // 
            // tbControl
            // 
            resources.ApplyResources(tbControl, "tbControl");
            tbControl.Controls.Add(tbStep0);
            tbControl.Controls.Add(tbStep1);
            tbControl.Controls.Add(tbStep2);
            tbControl.Controls.Add(tbStep3);
            tbControl.Controls.Add(tbProgress);
            tbControl.Controls.Add(tbStep5);
            tbControl.Controls.Add(tbStep6);
            tbControl.Controls.Add(tbStep7);
            tbControl.Name = "tbControl";
            tbControl.SelectedIndex = 0;
            // 
            // tbStep0
            // 
            resources.ApplyResources(tbStep0, "tbStep0");
            tbStep0.BackColor = Color.White;
            tbStep0.Controls.Add(TableLayoutPanel1);
            tbStep0.Name = "tbStep0";
            // 
            // TableLayoutPanel1
            // 
            resources.ApplyResources(TableLayoutPanel1, "TableLayoutPanel1");
            TableLayoutPanel1.Controls.Add(Label3, 0, 0);
            TableLayoutPanel1.Controls.Add(Label14, 0, 1);
            TableLayoutPanel1.Controls.Add(Label13, 1, 1);
            TableLayoutPanel1.Controls.Add(Label15, 1, 0);
            TableLayoutPanel1.Controls.Add(Panel3, 0, 2);
            TableLayoutPanel1.Controls.Add(Panel4, 1, 2);
            TableLayoutPanel1.Name = "TableLayoutPanel1";
            // 
            // Label3
            // 
            resources.ApplyResources(Label3, "Label3");
            Label3.ForeColor = Color.FromArgb(0, 51, 153);
            Label3.Name = "Label3";
            // 
            // Label14
            // 
            resources.ApplyResources(Label14, "Label14");
            Label14.Name = "Label14";
            // 
            // Label13
            // 
            resources.ApplyResources(Label13, "Label13");
            Label13.Name = "Label13";
            // 
            // Label15
            // 
            resources.ApplyResources(Label15, "Label15");
            Label15.ForeColor = Color.FromArgb(0, 51, 153);
            Label15.Name = "Label15";
            // 
            // Panel3
            // 
            resources.ApplyResources(Panel3, "Panel3");
            Panel3.Controls.Add(cmdImport);
            Panel3.Name = "Panel3";
            // 
            // Panel4
            // 
            resources.ApplyResources(Panel4, "Panel4");
            Panel4.Controls.Add(cmdConfigure);
            Panel4.Name = "Panel4";
            // 
            // cmdConfigure
            // 
            resources.ApplyResources(cmdConfigure, "cmdConfigure");
            cmdConfigure.Name = "cmdConfigure";
            cmdConfigure.UseVisualStyleBackColor = true;
            cmdConfigure.Click += cmdConfigure_Click;
            // 
            // tbStep1
            // 
            resources.ApplyResources(tbStep1, "tbStep1");
            tbStep1.BackColor = Color.White;
            tbStep1.Controls.Add(cmdDelete);
            tbStep1.Controls.Add(lvSourceFolders);
            tbStep1.Controls.Add(cmdAdd);
            tbStep1.Name = "tbStep1";
            // 
            // tbStep2
            // 
            resources.ApplyResources(tbStep2, "tbStep2");
            tbStep2.BackColor = Color.White;
            tbStep2.Controls.Add(tcSource);
            tbStep2.Name = "tbStep2";
            // 
            // tbStep3
            // 
            resources.ApplyResources(tbStep3, "tbStep3");
            tbStep3.BackColor = Color.White;
            tbStep3.Controls.Add(rdManualMode);
            tbStep3.Controls.Add(rdFullAutomated);
            tbStep3.Name = "tbStep3";
            // 
            // tbProgress
            // 
            resources.ApplyResources(tbProgress, "tbProgress");
            tbProgress.BackColor = Color.White;
            tbProgress.Controls.Add(lblStatus);
            tbProgress.Controls.Add(ProgressBar1);
            tbProgress.Name = "tbProgress";
            // 
            // tbStep5
            // 
            resources.ApplyResources(tbStep5, "tbStep5");
            tbStep5.BackColor = Color.White;
            tbStep5.Controls.Add(tcStep5);
            tbStep5.Name = "tbStep5";
            // 
            // tbStep6
            // 
            resources.ApplyResources(tbStep6, "tbStep6");
            tbStep6.BackColor = Color.White;
            tbStep6.Controls.Add(lvBackups);
            tbStep6.Name = "tbStep6";
            // 
            // tbStep7
            // 
            resources.ApplyResources(tbStep7, "tbStep7");
            tbStep7.BackColor = Color.White;
            tbStep7.Controls.Add(cmdChange);
            tbStep7.Controls.Add(lvSourceDirs);
            tbStep7.Name = "tbStep7";
            // 
            // Panel1
            // 
            resources.ApplyResources(Panel1, "Panel1");
            Panel1.Controls.Add(lblTitle);
            Panel1.Controls.Add(lblDescription);
            Panel1.Name = "Panel1";
            // 
            // Panel2
            // 
            resources.ApplyResources(Panel2, "Panel2");
            Panel2.Name = "Panel2";
            // 
            // ucDoConfigure
            // 
            resources.ApplyResources(this, "$this");
            AutoScaleMode = AutoScaleMode.Dpi;
            BackColor = Color.White;
            Controls.Add(Panel1);
            Controls.Add(Panel2);
            Controls.Add(cmdBack);
            Controls.Add(cmdNext);
            Controls.Add(tbControl);
            DoubleBuffered = true;
            Name = "ucDoConfigure";
            tcSource.ResumeLayout(false);
            TabPage1.ResumeLayout(false);
            TabPage2.ResumeLayout(false);
            TabPage2.PerformLayout();
            TabPage6.ResumeLayout(false);
            TabPage6.PerformLayout();
            tcStep5.ResumeLayout(false);
            TabPage3.ResumeLayout(false);
            TabPage4.ResumeLayout(false);
            TabPage4.PerformLayout();
            TabPage5.ResumeLayout(false);
            TabPage5.PerformLayout();
            tbControl.ResumeLayout(false);
            tbStep0.ResumeLayout(false);
            TableLayoutPanel1.ResumeLayout(false);
            Panel3.ResumeLayout(false);
            Panel4.ResumeLayout(false);
            tbStep1.ResumeLayout(false);
            tbStep2.ResumeLayout(false);
            tbStep3.ResumeLayout(false);
            tbStep3.PerformLayout();
            tbProgress.ResumeLayout(false);
            tbStep5.ResumeLayout(false);
            tbStep6.ResumeLayout(false);
            tbStep7.ResumeLayout(false);
            Panel1.ResumeLayout(false);
            Panel1.PerformLayout();
            ResumeLayout(false);
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