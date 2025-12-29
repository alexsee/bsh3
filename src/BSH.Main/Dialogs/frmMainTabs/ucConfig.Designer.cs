using System.Diagnostics;
using System.Windows.Forms;

namespace Brightbits.BSH.Main
{
    public partial class ucConfig : UserControl
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
            var resources = new System.ComponentModel.ComponentResourceManager(typeof(ucConfig));
            ImageList1 = new ImageList(components);
            ColumnHeader3 = new ColumnHeader();
            ColumnHeader4 = new ColumnHeader();
            ColumnHeader6 = new ColumnHeader();
            ColumnHeader7 = new ColumnHeader();
            TabPage4 = new TabPage();
            chkWaitOnMediaInteractive = new CheckBox();
            chkRemind = new CheckBox();
            Label9 = new Label();
            nudRemind = new NumericUpDown();
            chkInfoBackupDone = new CheckBox();
            chkShowLocalized = new CheckBox();
            chkAbortWhenNotAvailable = new CheckBox();
            chkRemindSpace = new CheckBox();
            chkDeactivateAutoBackupsWhenAkku = new CheckBox();
            Label14 = new Label();
            txtRemindSpace = new NumericUpDown();
            TabPage3 = new TabPage();
            cmdDeactivateEncrypt = new Button();
            plCompressEncrypt = new Panel();
            cmdExcludeCompress = new Button();
            lblCompressionLevel = new Label();
            rdEncrypt = new RadioButton();
            rdNoCompress = new RadioButton();
            rdCompress = new RadioButton();
            cmdEncrypt = new Button();
            TabPage2 = new TabPage();
            lblScheduleWarning = new Label();
            chkDoPastBackups = new CheckBox();
            cmdEditSchedule = new Button();
            rbMB = new RadioButton();
            rbFAB = new RadioButton();
            rbTSB = new RadioButton();
            TabPage1 = new TabPage();
            cmdFilter = new Button();
            lvSource = new ListView();
            ColumnHeader1 = new ColumnHeader();
            cmdAddSource = new Button();
            cmdDeleteSource = new Button();
            tcOptions = new TabControl();
            TabPage5 = new TabPage();
            cmdChangeBackupMedia = new Button();
            Label1 = new Label();
            cboMedia = new ComboBox();
            Label2 = new Label();
            plDevice = new Panel();
            plUNCAuthentication = new Panel();
            Label11 = new Label();
            txtUNCPassword = new TextBox();
            Label10 = new Label();
            txtUNCUsername = new TextBox();
            Label15 = new Label();
            cmdChange = new Button();
            txtBackupPath = new TextBox();
            plFTP = new Panel();
            chkFtpEncryption = new CheckBox();
            Label12 = new Label();
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
            Label3 = new Label();
            cmdOK = new Button();
            ToolTip1 = new ToolTip(components);
            plExcludeCompress = new Panel();
            cmdCloseExcludeCompress = new Button();
            cmdDeleteExcludeCompress = new Button();
            cmdAddExcludeCompress = new Button();
            lstExcludeCompress = new ListBox();
            Label6 = new Label();
            TabPage4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)nudRemind).BeginInit();
            ((System.ComponentModel.ISupportInitialize)txtRemindSpace).BeginInit();
            TabPage3.SuspendLayout();
            plCompressEncrypt.SuspendLayout();
            TabPage2.SuspendLayout();
            TabPage1.SuspendLayout();
            tcOptions.SuspendLayout();
            TabPage5.SuspendLayout();
            plDevice.SuspendLayout();
            plUNCAuthentication.SuspendLayout();
            plFTP.SuspendLayout();
            plExcludeCompress.SuspendLayout();
            SuspendLayout();
            // 
            // ImageList1
            // 
            ImageList1.ColorDepth = ColorDepth.Depth32Bit;
            ImageList1.ImageStream = (ImageListStreamer)resources.GetObject("ImageList1.ImageStream");
            ImageList1.TransparentColor = System.Drawing.Color.Transparent;
            ImageList1.Images.SetKeyName(0, "fixed");
            ImageList1.Images.SetKeyName(1, "removable");
            ImageList1.Images.SetKeyName(2, "remote");
            // 
            // ColumnHeader3
            // 
            resources.ApplyResources(ColumnHeader3, "ColumnHeader3");
            // 
            // ColumnHeader4
            // 
            resources.ApplyResources(ColumnHeader4, "ColumnHeader4");
            // 
            // ColumnHeader6
            // 
            resources.ApplyResources(ColumnHeader6, "ColumnHeader6");
            // 
            // ColumnHeader7
            // 
            resources.ApplyResources(ColumnHeader7, "ColumnHeader7");
            // 
            // TabPage4
            // 
            resources.ApplyResources(TabPage4, "TabPage4");
            TabPage4.BackColor = System.Drawing.Color.White;
            TabPage4.Controls.Add(chkWaitOnMediaInteractive);
            TabPage4.Controls.Add(chkRemind);
            TabPage4.Controls.Add(Label9);
            TabPage4.Controls.Add(nudRemind);
            TabPage4.Controls.Add(chkInfoBackupDone);
            TabPage4.Controls.Add(chkShowLocalized);
            TabPage4.Controls.Add(chkAbortWhenNotAvailable);
            TabPage4.Controls.Add(chkRemindSpace);
            TabPage4.Controls.Add(chkDeactivateAutoBackupsWhenAkku);
            TabPage4.Controls.Add(Label14);
            TabPage4.Controls.Add(txtRemindSpace);
            TabPage4.Name = "TabPage4";
            ToolTip1.SetToolTip(TabPage4, resources.GetString("TabPage4.ToolTip"));
            // 
            // chkWaitOnMediaInteractive
            // 
            resources.ApplyResources(chkWaitOnMediaInteractive, "chkWaitOnMediaInteractive");
            chkWaitOnMediaInteractive.BackColor = System.Drawing.Color.Transparent;
            chkWaitOnMediaInteractive.Name = "chkWaitOnMediaInteractive";
            ToolTip1.SetToolTip(chkWaitOnMediaInteractive, resources.GetString("chkWaitOnMediaInteractive.ToolTip"));
            chkWaitOnMediaInteractive.UseVisualStyleBackColor = false;
            // 
            // chkRemind
            // 
            resources.ApplyResources(chkRemind, "chkRemind");
            chkRemind.BackColor = System.Drawing.Color.Transparent;
            chkRemind.Name = "chkRemind";
            ToolTip1.SetToolTip(chkRemind, resources.GetString("chkRemind.ToolTip"));
            chkRemind.UseVisualStyleBackColor = false;
            chkRemind.CheckedChanged += chkRemind_CheckedChanged;
            // 
            // Label9
            // 
            resources.ApplyResources(Label9, "Label9");
            Label9.BackColor = System.Drawing.Color.Transparent;
            Label9.Name = "Label9";
            ToolTip1.SetToolTip(Label9, resources.GetString("Label9.ToolTip"));
            // 
            // nudRemind
            // 
            resources.ApplyResources(nudRemind, "nudRemind");
            nudRemind.Maximum = new decimal(new int[] { 1410065407, 2, 0, 0 });
            nudRemind.Name = "nudRemind";
            ToolTip1.SetToolTip(nudRemind, resources.GetString("nudRemind.ToolTip"));
            // 
            // chkInfoBackupDone
            // 
            resources.ApplyResources(chkInfoBackupDone, "chkInfoBackupDone");
            chkInfoBackupDone.BackColor = System.Drawing.Color.Transparent;
            chkInfoBackupDone.Name = "chkInfoBackupDone";
            ToolTip1.SetToolTip(chkInfoBackupDone, resources.GetString("chkInfoBackupDone.ToolTip"));
            chkInfoBackupDone.UseVisualStyleBackColor = false;
            // 
            // chkShowLocalized
            // 
            resources.ApplyResources(chkShowLocalized, "chkShowLocalized");
            chkShowLocalized.BackColor = System.Drawing.Color.Transparent;
            chkShowLocalized.Name = "chkShowLocalized";
            ToolTip1.SetToolTip(chkShowLocalized, resources.GetString("chkShowLocalized.ToolTip"));
            chkShowLocalized.UseVisualStyleBackColor = false;
            // 
            // chkAbortWhenNotAvailable
            // 
            resources.ApplyResources(chkAbortWhenNotAvailable, "chkAbortWhenNotAvailable");
            chkAbortWhenNotAvailable.BackColor = System.Drawing.Color.Transparent;
            chkAbortWhenNotAvailable.Name = "chkAbortWhenNotAvailable";
            ToolTip1.SetToolTip(chkAbortWhenNotAvailable, resources.GetString("chkAbortWhenNotAvailable.ToolTip"));
            chkAbortWhenNotAvailable.UseVisualStyleBackColor = false;
            // 
            // chkRemindSpace
            // 
            resources.ApplyResources(chkRemindSpace, "chkRemindSpace");
            chkRemindSpace.BackColor = System.Drawing.Color.Transparent;
            chkRemindSpace.Name = "chkRemindSpace";
            ToolTip1.SetToolTip(chkRemindSpace, resources.GetString("chkRemindSpace.ToolTip"));
            chkRemindSpace.UseVisualStyleBackColor = false;
            chkRemindSpace.CheckedChanged += chkRemindSpace_CheckedChanged;
            // 
            // chkDeactivateAutoBackupsWhenAkku
            // 
            resources.ApplyResources(chkDeactivateAutoBackupsWhenAkku, "chkDeactivateAutoBackupsWhenAkku");
            chkDeactivateAutoBackupsWhenAkku.BackColor = System.Drawing.Color.Transparent;
            chkDeactivateAutoBackupsWhenAkku.Name = "chkDeactivateAutoBackupsWhenAkku";
            ToolTip1.SetToolTip(chkDeactivateAutoBackupsWhenAkku, resources.GetString("chkDeactivateAutoBackupsWhenAkku.ToolTip"));
            chkDeactivateAutoBackupsWhenAkku.UseVisualStyleBackColor = false;
            // 
            // Label14
            // 
            resources.ApplyResources(Label14, "Label14");
            Label14.BackColor = System.Drawing.Color.Transparent;
            Label14.Name = "Label14";
            ToolTip1.SetToolTip(Label14, resources.GetString("Label14.ToolTip"));
            // 
            // txtRemindSpace
            // 
            resources.ApplyResources(txtRemindSpace, "txtRemindSpace");
            txtRemindSpace.Maximum = new decimal(new int[] { 1410065407, 2, 0, 0 });
            txtRemindSpace.Name = "txtRemindSpace";
            ToolTip1.SetToolTip(txtRemindSpace, resources.GetString("txtRemindSpace.ToolTip"));
            // 
            // TabPage3
            // 
            resources.ApplyResources(TabPage3, "TabPage3");
            TabPage3.BackColor = System.Drawing.Color.White;
            TabPage3.Controls.Add(cmdDeactivateEncrypt);
            TabPage3.Controls.Add(plCompressEncrypt);
            TabPage3.Controls.Add(cmdEncrypt);
            TabPage3.Name = "TabPage3";
            ToolTip1.SetToolTip(TabPage3, resources.GetString("TabPage3.ToolTip"));
            // 
            // cmdDeactivateEncrypt
            // 
            resources.ApplyResources(cmdDeactivateEncrypt, "cmdDeactivateEncrypt");
            cmdDeactivateEncrypt.Name = "cmdDeactivateEncrypt";
            cmdDeactivateEncrypt.Tag = "";
            ToolTip1.SetToolTip(cmdDeactivateEncrypt, resources.GetString("cmdDeactivateEncrypt.ToolTip"));
            cmdDeactivateEncrypt.UseVisualStyleBackColor = true;
            cmdDeactivateEncrypt.Click += cmdDeactivateEncrypt_Click;
            // 
            // plCompressEncrypt
            // 
            resources.ApplyResources(plCompressEncrypt, "plCompressEncrypt");
            plCompressEncrypt.BackColor = System.Drawing.Color.Transparent;
            plCompressEncrypt.Controls.Add(cmdExcludeCompress);
            plCompressEncrypt.Controls.Add(lblCompressionLevel);
            plCompressEncrypt.Controls.Add(rdEncrypt);
            plCompressEncrypt.Controls.Add(rdNoCompress);
            plCompressEncrypt.Controls.Add(rdCompress);
            plCompressEncrypt.Name = "plCompressEncrypt";
            ToolTip1.SetToolTip(plCompressEncrypt, resources.GetString("plCompressEncrypt.ToolTip"));
            // 
            // cmdExcludeCompress
            // 
            resources.ApplyResources(cmdExcludeCompress, "cmdExcludeCompress");
            cmdExcludeCompress.Name = "cmdExcludeCompress";
            ToolTip1.SetToolTip(cmdExcludeCompress, resources.GetString("cmdExcludeCompress.ToolTip"));
            cmdExcludeCompress.UseVisualStyleBackColor = true;
            cmdExcludeCompress.Click += cmdExcludeCompress_Click;
            // 
            // lblCompressionLevel
            // 
            resources.ApplyResources(lblCompressionLevel, "lblCompressionLevel");
            lblCompressionLevel.Name = "lblCompressionLevel";
            ToolTip1.SetToolTip(lblCompressionLevel, resources.GetString("lblCompressionLevel.ToolTip"));
            // 
            // rdEncrypt
            // 
            resources.ApplyResources(rdEncrypt, "rdEncrypt");
            rdEncrypt.Name = "rdEncrypt";
            ToolTip1.SetToolTip(rdEncrypt, resources.GetString("rdEncrypt.ToolTip"));
            rdEncrypt.UseVisualStyleBackColor = true;
            rdEncrypt.CheckedChanged += rdEncrypt_CheckedChanged;
            // 
            // rdNoCompress
            // 
            resources.ApplyResources(rdNoCompress, "rdNoCompress");
            rdNoCompress.Checked = true;
            rdNoCompress.Name = "rdNoCompress";
            rdNoCompress.TabStop = true;
            ToolTip1.SetToolTip(rdNoCompress, resources.GetString("rdNoCompress.ToolTip"));
            rdNoCompress.UseVisualStyleBackColor = true;
            // 
            // rdCompress
            // 
            resources.ApplyResources(rdCompress, "rdCompress");
            rdCompress.Name = "rdCompress";
            ToolTip1.SetToolTip(rdCompress, resources.GetString("rdCompress.ToolTip"));
            rdCompress.UseVisualStyleBackColor = true;
            rdCompress.CheckedChanged += rdCompress_CheckedChanged;
            // 
            // cmdEncrypt
            // 
            resources.ApplyResources(cmdEncrypt, "cmdEncrypt");
            cmdEncrypt.Name = "cmdEncrypt";
            cmdEncrypt.Tag = "";
            ToolTip1.SetToolTip(cmdEncrypt, resources.GetString("cmdEncrypt.ToolTip"));
            cmdEncrypt.UseVisualStyleBackColor = true;
            cmdEncrypt.Click += cmdEncrypt_Click;
            // 
            // TabPage2
            // 
            resources.ApplyResources(TabPage2, "TabPage2");
            TabPage2.BackColor = System.Drawing.Color.White;
            TabPage2.Controls.Add(lblScheduleWarning);
            TabPage2.Controls.Add(chkDoPastBackups);
            TabPage2.Controls.Add(cmdEditSchedule);
            TabPage2.Controls.Add(rbMB);
            TabPage2.Controls.Add(rbFAB);
            TabPage2.Controls.Add(rbTSB);
            TabPage2.Name = "TabPage2";
            ToolTip1.SetToolTip(TabPage2, resources.GetString("TabPage2.ToolTip"));
            // 
            // lblScheduleWarning
            // 
            lblScheduleWarning.AutoSize = true;
            lblScheduleWarning.BackColor = System.Drawing.Color.Transparent;
            lblScheduleWarning.ForeColor = System.Drawing.Color.FromArgb(192, 0, 0);
            lblScheduleWarning.Location = new System.Drawing.Point(57, 308);
            lblScheduleWarning.MaximumSize = new System.Drawing.Size(820, 0);
            lblScheduleWarning.Name = "lblScheduleWarning";
            lblScheduleWarning.Size = new System.Drawing.Size(0, 25);
            lblScheduleWarning.TabIndex = 99;
            lblScheduleWarning.Visible = false;
            // 
            // chkDoPastBackups
            // 
            resources.ApplyResources(chkDoPastBackups, "chkDoPastBackups");
            chkDoPastBackups.BackColor = System.Drawing.Color.Transparent;
            chkDoPastBackups.Name = "chkDoPastBackups";
            ToolTip1.SetToolTip(chkDoPastBackups, resources.GetString("chkDoPastBackups.ToolTip"));
            chkDoPastBackups.UseVisualStyleBackColor = false;
            // 
            // cmdEditSchedule
            // 
            resources.ApplyResources(cmdEditSchedule, "cmdEditSchedule");
            cmdEditSchedule.Name = "cmdEditSchedule";
            cmdEditSchedule.Tag = "";
            ToolTip1.SetToolTip(cmdEditSchedule, resources.GetString("cmdEditSchedule.ToolTip"));
            cmdEditSchedule.UseVisualStyleBackColor = true;
            cmdEditSchedule.Click += cmdEditSchedule_Click;
            // 
            // rbMB
            // 
            resources.ApplyResources(rbMB, "rbMB");
            rbMB.BackColor = System.Drawing.Color.Transparent;
            rbMB.Name = "rbMB";
            ToolTip1.SetToolTip(rbMB, resources.GetString("rbMB.ToolTip"));
            rbMB.UseVisualStyleBackColor = false;
            // 
            // rbFAB
            // 
            resources.ApplyResources(rbFAB, "rbFAB");
            rbFAB.BackColor = System.Drawing.Color.Transparent;
            rbFAB.Name = "rbFAB";
            ToolTip1.SetToolTip(rbFAB, resources.GetString("rbFAB.ToolTip"));
            rbFAB.UseVisualStyleBackColor = false;
            // 
            // rbTSB
            // 
            resources.ApplyResources(rbTSB, "rbTSB");
            rbTSB.BackColor = System.Drawing.Color.Transparent;
            rbTSB.Name = "rbTSB";
            ToolTip1.SetToolTip(rbTSB, resources.GetString("rbTSB.ToolTip"));
            rbTSB.UseVisualStyleBackColor = false;
            rbTSB.CheckedChanged += rbTSB_CheckedChanged;
            // 
            // TabPage1
            // 
            resources.ApplyResources(TabPage1, "TabPage1");
            TabPage1.BackColor = System.Drawing.Color.White;
            TabPage1.Controls.Add(cmdFilter);
            TabPage1.Controls.Add(lvSource);
            TabPage1.Controls.Add(cmdAddSource);
            TabPage1.Controls.Add(cmdDeleteSource);
            TabPage1.Name = "TabPage1";
            ToolTip1.SetToolTip(TabPage1, resources.GetString("TabPage1.ToolTip"));
            // 
            // cmdFilter
            // 
            resources.ApplyResources(cmdFilter, "cmdFilter");
            cmdFilter.Name = "cmdFilter";
            cmdFilter.Tag = "";
            ToolTip1.SetToolTip(cmdFilter, resources.GetString("cmdFilter.ToolTip"));
            cmdFilter.UseVisualStyleBackColor = true;
            cmdFilter.Click += cmdFilter_Click;
            // 
            // lvSource
            // 
            resources.ApplyResources(lvSource, "lvSource");
            lvSource.AllowDrop = true;
            lvSource.Columns.AddRange(new ColumnHeader[] { ColumnHeader1 });
            lvSource.FullRowSelect = true;
            lvSource.Name = "lvSource";
            lvSource.SmallImageList = ImageList1;
            lvSource.Sorting = SortOrder.Ascending;
            ToolTip1.SetToolTip(lvSource, resources.GetString("lvSource.ToolTip"));
            lvSource.UseCompatibleStateImageBehavior = false;
            lvSource.View = View.Details;
            lvSource.SelectedIndexChanged += lvSource_SelectedIndexChanged;
            lvSource.DragDrop += lvSource_DragDrop;
            lvSource.DragEnter += lvSource_DragEnter;
            // 
            // ColumnHeader1
            // 
            resources.ApplyResources(ColumnHeader1, "ColumnHeader1");
            // 
            // cmdAddSource
            // 
            resources.ApplyResources(cmdAddSource, "cmdAddSource");
            cmdAddSource.Name = "cmdAddSource";
            ToolTip1.SetToolTip(cmdAddSource, resources.GetString("cmdAddSource.ToolTip"));
            cmdAddSource.UseVisualStyleBackColor = true;
            cmdAddSource.Click += cmdAddSource_Click;
            // 
            // cmdDeleteSource
            // 
            resources.ApplyResources(cmdDeleteSource, "cmdDeleteSource");
            cmdDeleteSource.Name = "cmdDeleteSource";
            ToolTip1.SetToolTip(cmdDeleteSource, resources.GetString("cmdDeleteSource.ToolTip"));
            cmdDeleteSource.UseVisualStyleBackColor = true;
            cmdDeleteSource.Click += cmdDeleteSource_Click;
            // 
            // tcOptions
            // 
            resources.ApplyResources(tcOptions, "tcOptions");
            tcOptions.Controls.Add(TabPage1);
            tcOptions.Controls.Add(TabPage5);
            tcOptions.Controls.Add(TabPage2);
            tcOptions.Controls.Add(TabPage3);
            tcOptions.Controls.Add(TabPage4);
            tcOptions.Name = "tcOptions";
            tcOptions.SelectedIndex = 0;
            ToolTip1.SetToolTip(tcOptions, resources.GetString("tcOptions.ToolTip"));
            // 
            // TabPage5
            // 
            resources.ApplyResources(TabPage5, "TabPage5");
            TabPage5.BackColor = System.Drawing.Color.White;
            TabPage5.Controls.Add(cmdChangeBackupMedia);
            TabPage5.Controls.Add(Label1);
            TabPage5.Controls.Add(cboMedia);
            TabPage5.Controls.Add(Label2);
            TabPage5.Controls.Add(plDevice);
            TabPage5.Controls.Add(plFTP);
            TabPage5.Name = "TabPage5";
            ToolTip1.SetToolTip(TabPage5, resources.GetString("TabPage5.ToolTip"));
            // 
            // cmdChangeBackupMedia
            // 
            resources.ApplyResources(cmdChangeBackupMedia, "cmdChangeBackupMedia");
            cmdChangeBackupMedia.Name = "cmdChangeBackupMedia";
            ToolTip1.SetToolTip(cmdChangeBackupMedia, resources.GetString("cmdChangeBackupMedia.ToolTip"));
            cmdChangeBackupMedia.UseVisualStyleBackColor = true;
            cmdChangeBackupMedia.Click += Button1_Click;
            // 
            // Label1
            // 
            resources.ApplyResources(Label1, "Label1");
            Label1.BackColor = System.Drawing.Color.Transparent;
            Label1.Name = "Label1";
            ToolTip1.SetToolTip(Label1, resources.GetString("Label1.ToolTip"));
            // 
            // cboMedia
            // 
            resources.ApplyResources(cboMedia, "cboMedia");
            cboMedia.DropDownStyle = ComboBoxStyle.DropDownList;
            cboMedia.FormattingEnabled = true;
            cboMedia.Items.AddRange(new object[] { resources.GetString("cboMedia.Items"), resources.GetString("cboMedia.Items1") });
            cboMedia.Name = "cboMedia";
            ToolTip1.SetToolTip(cboMedia, resources.GetString("cboMedia.ToolTip"));
            cboMedia.SelectedIndexChanged += cboMedia_SelectedIndexChanged;
            // 
            // Label2
            // 
            resources.ApplyResources(Label2, "Label2");
            Label2.BackColor = System.Drawing.Color.Transparent;
            Label2.Name = "Label2";
            ToolTip1.SetToolTip(Label2, resources.GetString("Label2.ToolTip"));
            // 
            // plDevice
            // 
            resources.ApplyResources(plDevice, "plDevice");
            plDevice.BackColor = System.Drawing.Color.Transparent;
            plDevice.Controls.Add(plUNCAuthentication);
            plDevice.Controls.Add(Label15);
            plDevice.Controls.Add(cmdChange);
            plDevice.Controls.Add(txtBackupPath);
            plDevice.Name = "plDevice";
            ToolTip1.SetToolTip(plDevice, resources.GetString("plDevice.ToolTip"));
            // 
            // plUNCAuthentication
            // 
            resources.ApplyResources(plUNCAuthentication, "plUNCAuthentication");
            plUNCAuthentication.Controls.Add(Label11);
            plUNCAuthentication.Controls.Add(txtUNCPassword);
            plUNCAuthentication.Controls.Add(Label10);
            plUNCAuthentication.Controls.Add(txtUNCUsername);
            plUNCAuthentication.Name = "plUNCAuthentication";
            ToolTip1.SetToolTip(plUNCAuthentication, resources.GetString("plUNCAuthentication.ToolTip"));
            // 
            // Label11
            // 
            resources.ApplyResources(Label11, "Label11");
            Label11.Name = "Label11";
            ToolTip1.SetToolTip(Label11, resources.GetString("Label11.ToolTip"));
            // 
            // txtUNCPassword
            // 
            resources.ApplyResources(txtUNCPassword, "txtUNCPassword");
            txtUNCPassword.BackColor = System.Drawing.Color.White;
            txtUNCPassword.Name = "txtUNCPassword";
            ToolTip1.SetToolTip(txtUNCPassword, resources.GetString("txtUNCPassword.ToolTip"));
            txtUNCPassword.UseSystemPasswordChar = true;
            // 
            // Label10
            // 
            resources.ApplyResources(Label10, "Label10");
            Label10.Name = "Label10";
            ToolTip1.SetToolTip(Label10, resources.GetString("Label10.ToolTip"));
            // 
            // txtUNCUsername
            // 
            resources.ApplyResources(txtUNCUsername, "txtUNCUsername");
            txtUNCUsername.BackColor = System.Drawing.Color.White;
            txtUNCUsername.Name = "txtUNCUsername";
            ToolTip1.SetToolTip(txtUNCUsername, resources.GetString("txtUNCUsername.ToolTip"));
            // 
            // Label15
            // 
            resources.ApplyResources(Label15, "Label15");
            Label15.Name = "Label15";
            ToolTip1.SetToolTip(Label15, resources.GetString("Label15.ToolTip"));
            // 
            // cmdChange
            // 
            resources.ApplyResources(cmdChange, "cmdChange");
            cmdChange.Name = "cmdChange";
            cmdChange.Tag = "";
            ToolTip1.SetToolTip(cmdChange, resources.GetString("cmdChange.ToolTip"));
            cmdChange.UseVisualStyleBackColor = true;
            cmdChange.Click += cmdChange_Click;
            // 
            // txtBackupPath
            // 
            resources.ApplyResources(txtBackupPath, "txtBackupPath");
            txtBackupPath.BackColor = System.Drawing.Color.White;
            txtBackupPath.Name = "txtBackupPath";
            txtBackupPath.ReadOnly = true;
            ToolTip1.SetToolTip(txtBackupPath, resources.GetString("txtBackupPath.ToolTip"));
            txtBackupPath.TextChanged += txtBackupPath_TextChanged;
            // 
            // plFTP
            // 
            resources.ApplyResources(plFTP, "plFTP");
            plFTP.BackColor = System.Drawing.Color.Transparent;
            plFTP.Controls.Add(chkFtpEncryption);
            plFTP.Controls.Add(Label12);
            plFTP.Controls.Add(cboFtpEncoding);
            plFTP.Controls.Add(cmdFTPCheck);
            plFTP.Controls.Add(txtFTPPort);
            plFTP.Controls.Add(Label8);
            plFTP.Controls.Add(txtFTPPath);
            plFTP.Controls.Add(Label7);
            plFTP.Controls.Add(txtFTPPassword);
            plFTP.Controls.Add(Label5);
            plFTP.Controls.Add(txtFTPUsername);
            plFTP.Controls.Add(Label4);
            plFTP.Controls.Add(txtFTPServer);
            plFTP.Controls.Add(Label3);
            plFTP.Name = "plFTP";
            ToolTip1.SetToolTip(plFTP, resources.GetString("plFTP.ToolTip"));
            // 
            // chkFtpEncryption
            // 
            resources.ApplyResources(chkFtpEncryption, "chkFtpEncryption");
            chkFtpEncryption.Name = "chkFtpEncryption";
            ToolTip1.SetToolTip(chkFtpEncryption, resources.GetString("chkFtpEncryption.ToolTip"));
            chkFtpEncryption.UseVisualStyleBackColor = true;
            // 
            // Label12
            // 
            resources.ApplyResources(Label12, "Label12");
            Label12.Name = "Label12";
            ToolTip1.SetToolTip(Label12, resources.GetString("Label12.ToolTip"));
            // 
            // cboFtpEncoding
            // 
            resources.ApplyResources(cboFtpEncoding, "cboFtpEncoding");
            cboFtpEncoding.DropDownStyle = ComboBoxStyle.DropDownList;
            cboFtpEncoding.FormattingEnabled = true;
            cboFtpEncoding.Items.AddRange(new object[] { resources.GetString("cboFtpEncoding.Items"), resources.GetString("cboFtpEncoding.Items1") });
            cboFtpEncoding.Name = "cboFtpEncoding";
            ToolTip1.SetToolTip(cboFtpEncoding, resources.GetString("cboFtpEncoding.ToolTip"));
            // 
            // cmdFTPCheck
            // 
            resources.ApplyResources(cmdFTPCheck, "cmdFTPCheck");
            cmdFTPCheck.Name = "cmdFTPCheck";
            ToolTip1.SetToolTip(cmdFTPCheck, resources.GetString("cmdFTPCheck.ToolTip"));
            cmdFTPCheck.UseVisualStyleBackColor = true;
            cmdFTPCheck.Click += cmdFTPCheck_Click;
            // 
            // txtFTPPort
            // 
            resources.ApplyResources(txtFTPPort, "txtFTPPort");
            txtFTPPort.Name = "txtFTPPort";
            ToolTip1.SetToolTip(txtFTPPort, resources.GetString("txtFTPPort.ToolTip"));
            // 
            // Label8
            // 
            resources.ApplyResources(Label8, "Label8");
            Label8.Name = "Label8";
            ToolTip1.SetToolTip(Label8, resources.GetString("Label8.ToolTip"));
            // 
            // txtFTPPath
            // 
            resources.ApplyResources(txtFTPPath, "txtFTPPath");
            txtFTPPath.Name = "txtFTPPath";
            ToolTip1.SetToolTip(txtFTPPath, resources.GetString("txtFTPPath.ToolTip"));
            // 
            // Label7
            // 
            resources.ApplyResources(Label7, "Label7");
            Label7.Name = "Label7";
            ToolTip1.SetToolTip(Label7, resources.GetString("Label7.ToolTip"));
            // 
            // txtFTPPassword
            // 
            resources.ApplyResources(txtFTPPassword, "txtFTPPassword");
            txtFTPPassword.Name = "txtFTPPassword";
            ToolTip1.SetToolTip(txtFTPPassword, resources.GetString("txtFTPPassword.ToolTip"));
            txtFTPPassword.UseSystemPasswordChar = true;
            // 
            // Label5
            // 
            resources.ApplyResources(Label5, "Label5");
            Label5.Name = "Label5";
            ToolTip1.SetToolTip(Label5, resources.GetString("Label5.ToolTip"));
            // 
            // txtFTPUsername
            // 
            resources.ApplyResources(txtFTPUsername, "txtFTPUsername");
            txtFTPUsername.Name = "txtFTPUsername";
            ToolTip1.SetToolTip(txtFTPUsername, resources.GetString("txtFTPUsername.ToolTip"));
            // 
            // Label4
            // 
            resources.ApplyResources(Label4, "Label4");
            Label4.Name = "Label4";
            ToolTip1.SetToolTip(Label4, resources.GetString("Label4.ToolTip"));
            // 
            // txtFTPServer
            // 
            resources.ApplyResources(txtFTPServer, "txtFTPServer");
            txtFTPServer.Name = "txtFTPServer";
            ToolTip1.SetToolTip(txtFTPServer, resources.GetString("txtFTPServer.ToolTip"));
            // 
            // Label3
            // 
            resources.ApplyResources(Label3, "Label3");
            Label3.Name = "Label3";
            ToolTip1.SetToolTip(Label3, resources.GetString("Label3.ToolTip"));
            // 
            // cmdOK
            // 
            resources.ApplyResources(cmdOK, "cmdOK");
            cmdOK.Name = "cmdOK";
            ToolTip1.SetToolTip(cmdOK, resources.GetString("cmdOK.ToolTip"));
            cmdOK.UseVisualStyleBackColor = true;
            cmdOK.Click += cmdOK_Click;
            // 
            // plExcludeCompress
            // 
            resources.ApplyResources(plExcludeCompress, "plExcludeCompress");
            plExcludeCompress.BackColor = System.Drawing.Color.White;
            plExcludeCompress.BorderStyle = BorderStyle.FixedSingle;
            plExcludeCompress.Controls.Add(cmdCloseExcludeCompress);
            plExcludeCompress.Controls.Add(cmdDeleteExcludeCompress);
            plExcludeCompress.Controls.Add(cmdAddExcludeCompress);
            plExcludeCompress.Controls.Add(lstExcludeCompress);
            plExcludeCompress.Controls.Add(Label6);
            plExcludeCompress.Name = "plExcludeCompress";
            ToolTip1.SetToolTip(plExcludeCompress, resources.GetString("plExcludeCompress.ToolTip"));
            // 
            // cmdCloseExcludeCompress
            // 
            resources.ApplyResources(cmdCloseExcludeCompress, "cmdCloseExcludeCompress");
            cmdCloseExcludeCompress.Name = "cmdCloseExcludeCompress";
            ToolTip1.SetToolTip(cmdCloseExcludeCompress, resources.GetString("cmdCloseExcludeCompress.ToolTip"));
            cmdCloseExcludeCompress.UseVisualStyleBackColor = true;
            cmdCloseExcludeCompress.Click += cmdCloseExcludeCompress_Click;
            // 
            // cmdDeleteExcludeCompress
            // 
            resources.ApplyResources(cmdDeleteExcludeCompress, "cmdDeleteExcludeCompress");
            cmdDeleteExcludeCompress.Name = "cmdDeleteExcludeCompress";
            ToolTip1.SetToolTip(cmdDeleteExcludeCompress, resources.GetString("cmdDeleteExcludeCompress.ToolTip"));
            cmdDeleteExcludeCompress.UseVisualStyleBackColor = true;
            cmdDeleteExcludeCompress.Click += cmdDeleteExcludeCompress_Click;
            // 
            // cmdAddExcludeCompress
            // 
            resources.ApplyResources(cmdAddExcludeCompress, "cmdAddExcludeCompress");
            cmdAddExcludeCompress.Name = "cmdAddExcludeCompress";
            ToolTip1.SetToolTip(cmdAddExcludeCompress, resources.GetString("cmdAddExcludeCompress.ToolTip"));
            cmdAddExcludeCompress.UseVisualStyleBackColor = true;
            cmdAddExcludeCompress.Click += cmdAddExcludeCompress_Click;
            // 
            // lstExcludeCompress
            // 
            resources.ApplyResources(lstExcludeCompress, "lstExcludeCompress");
            lstExcludeCompress.FormattingEnabled = true;
            lstExcludeCompress.Name = "lstExcludeCompress";
            lstExcludeCompress.Sorted = true;
            ToolTip1.SetToolTip(lstExcludeCompress, resources.GetString("lstExcludeCompress.ToolTip"));
            lstExcludeCompress.SelectedIndexChanged += lstExcludeCompress_SelectedIndexChanged;
            // 
            // Label6
            // 
            resources.ApplyResources(Label6, "Label6");
            Label6.ForeColor = System.Drawing.Color.FromArgb(0, 51, 153);
            Label6.Name = "Label6";
            ToolTip1.SetToolTip(Label6, resources.GetString("Label6.ToolTip"));
            // 
            // ucConfig
            // 
            resources.ApplyResources(this, "$this");
            AutoScaleMode = AutoScaleMode.Dpi;
            Controls.Add(plExcludeCompress);
            Controls.Add(cmdOK);
            Controls.Add(tcOptions);
            DoubleBuffered = true;
            Name = "ucConfig";
            ToolTip1.SetToolTip(this, resources.GetString("$this.ToolTip"));
            TabPage4.ResumeLayout(false);
            TabPage4.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)nudRemind).EndInit();
            ((System.ComponentModel.ISupportInitialize)txtRemindSpace).EndInit();
            TabPage3.ResumeLayout(false);
            plCompressEncrypt.ResumeLayout(false);
            plCompressEncrypt.PerformLayout();
            TabPage2.ResumeLayout(false);
            TabPage2.PerformLayout();
            TabPage1.ResumeLayout(false);
            tcOptions.ResumeLayout(false);
            TabPage5.ResumeLayout(false);
            TabPage5.PerformLayout();
            plDevice.ResumeLayout(false);
            plDevice.PerformLayout();
            plUNCAuthentication.ResumeLayout(false);
            plUNCAuthentication.PerformLayout();
            plFTP.ResumeLayout(false);
            plFTP.PerformLayout();
            plExcludeCompress.ResumeLayout(false);
            plExcludeCompress.PerformLayout();
            ResumeLayout(false);
        }

        internal ColumnHeader ColumnHeader3;
        internal ColumnHeader ColumnHeader4;
        internal ColumnHeader ColumnHeader6;
        internal ImageList ImageList1;
        internal ColumnHeader ColumnHeader7;
        internal TabPage TabPage4;
        internal CheckBox chkAbortWhenNotAvailable;
        internal CheckBox chkRemindSpace;
        internal CheckBox chkDeactivateAutoBackupsWhenAkku;
        internal Label Label14;
        internal NumericUpDown txtRemindSpace;
        internal TabPage TabPage3;
        internal Button cmdDeactivateEncrypt;
        internal Panel plCompressEncrypt;
        internal Label lblCompressionLevel;
        internal RadioButton rdNoCompress;
        internal RadioButton rdCompress;
        internal Button cmdEncrypt;
        internal RadioButton rdEncrypt;
        internal TabPage TabPage2;
        internal Button cmdEditSchedule;
        internal RadioButton rbMB;
        internal RadioButton rbFAB;
        internal RadioButton rbTSB;
        internal TabPage TabPage1;
        internal ListView lvSource;
        internal ColumnHeader ColumnHeader1;
        internal Button cmdAddSource;
        internal Button cmdDeleteSource;
        internal TabControl tcOptions;
        internal TabPage TabPage5;
        internal ComboBox cboMedia;
        internal Label Label2;
        internal Panel plDevice;
        internal Label Label15;
        internal Button cmdChange;
        internal TextBox txtBackupPath;
        internal Panel plFTP;
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
        internal Label Label3;
        internal CheckBox chkShowLocalized;
        internal CheckBox chkDoPastBackups;
        internal CheckBox chkInfoBackupDone;
        internal Button cmdFilter;
        internal Button cmdOK;
        internal ToolTip ToolTip1;
        internal Button cmdChangeBackupMedia;
        internal Label Label1;
        internal Button cmdExcludeCompress;
        internal Panel plExcludeCompress;
        internal ListBox lstExcludeCompress;
        internal Label Label6;
        internal Button cmdDeleteExcludeCompress;
        internal Button cmdAddExcludeCompress;
        internal Button cmdCloseExcludeCompress;
        internal CheckBox chkRemind;
        internal Label Label9;
        internal NumericUpDown nudRemind;
        internal Panel plUNCAuthentication;
        internal Label Label11;
        internal TextBox txtUNCPassword;
        internal Label Label10;
        internal TextBox txtUNCUsername;
        internal CheckBox chkWaitOnMediaInteractive;
        internal Label Label12;
        internal ComboBox cboFtpEncoding;
        private CheckBox chkFtpEncryption;
        internal Label lblScheduleWarning;
    }
}