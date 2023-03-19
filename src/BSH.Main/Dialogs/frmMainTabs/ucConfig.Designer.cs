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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ucConfig));
            this.ImageList1 = new System.Windows.Forms.ImageList(this.components);
            this.ColumnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.ColumnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.ColumnHeader6 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.ColumnHeader7 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.TabPage4 = new System.Windows.Forms.TabPage();
            this.chkWaitOnMediaInteractive = new System.Windows.Forms.CheckBox();
            this.chkRemind = new System.Windows.Forms.CheckBox();
            this.Label9 = new System.Windows.Forms.Label();
            this.nudRemind = new System.Windows.Forms.NumericUpDown();
            this.chkInfoBackupDone = new System.Windows.Forms.CheckBox();
            this.chkShowLocalized = new System.Windows.Forms.CheckBox();
            this.chkAbortWhenNotAvailable = new System.Windows.Forms.CheckBox();
            this.chkRemindSpace = new System.Windows.Forms.CheckBox();
            this.chkDeactivateAutoBackupsWhenAkku = new System.Windows.Forms.CheckBox();
            this.Label14 = new System.Windows.Forms.Label();
            this.txtRemindSpace = new System.Windows.Forms.NumericUpDown();
            this.TabPage3 = new System.Windows.Forms.TabPage();
            this.cmdDeactivateEncrypt = new System.Windows.Forms.Button();
            this.plCompressEncrypt = new System.Windows.Forms.Panel();
            this.cmdExcludeCompress = new System.Windows.Forms.Button();
            this.lblCompressionLevel = new System.Windows.Forms.Label();
            this.tbCompressionLevel = new System.Windows.Forms.TrackBar();
            this.rdEncrypt = new System.Windows.Forms.RadioButton();
            this.rdNoCompress = new System.Windows.Forms.RadioButton();
            this.rdCompress = new System.Windows.Forms.RadioButton();
            this.cmdEncrypt = new System.Windows.Forms.Button();
            this.TabPage2 = new System.Windows.Forms.TabPage();
            this.chkDoPastBackups = new System.Windows.Forms.CheckBox();
            this.cmdEditSchedule = new System.Windows.Forms.Button();
            this.rbMB = new System.Windows.Forms.RadioButton();
            this.rbFAB = new System.Windows.Forms.RadioButton();
            this.rbTSB = new System.Windows.Forms.RadioButton();
            this.TabPage1 = new System.Windows.Forms.TabPage();
            this.cmdFilter = new System.Windows.Forms.Button();
            this.lvSource = new System.Windows.Forms.ListView();
            this.ColumnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.cmdAddSource = new System.Windows.Forms.Button();
            this.cmdDeleteSource = new System.Windows.Forms.Button();
            this.tcOptions = new System.Windows.Forms.TabControl();
            this.TabPage5 = new System.Windows.Forms.TabPage();
            this.cmdChangeBackupMedia = new System.Windows.Forms.Button();
            this.Label1 = new System.Windows.Forms.Label();
            this.cboMedia = new System.Windows.Forms.ComboBox();
            this.Label2 = new System.Windows.Forms.Label();
            this.plDevice = new System.Windows.Forms.Panel();
            this.plUNCAuthentication = new System.Windows.Forms.Panel();
            this.Label11 = new System.Windows.Forms.Label();
            this.txtUNCPassword = new System.Windows.Forms.TextBox();
            this.Label10 = new System.Windows.Forms.Label();
            this.txtUNCUsername = new System.Windows.Forms.TextBox();
            this.Label15 = new System.Windows.Forms.Label();
            this.cmdChange = new System.Windows.Forms.Button();
            this.txtBackupPath = new System.Windows.Forms.TextBox();
            this.plFTP = new System.Windows.Forms.Panel();
            this.chkFtpEncryption = new System.Windows.Forms.CheckBox();
            this.Label12 = new System.Windows.Forms.Label();
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
            this.Label3 = new System.Windows.Forms.Label();
            this.cmdOK = new System.Windows.Forms.Button();
            this.ToolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.plExcludeCompress = new System.Windows.Forms.Panel();
            this.cmdCloseExcludeCompress = new System.Windows.Forms.Button();
            this.cmdDeleteExcludeCompress = new System.Windows.Forms.Button();
            this.cmdAddExcludeCompress = new System.Windows.Forms.Button();
            this.lstExcludeCompress = new System.Windows.Forms.ListBox();
            this.Label6 = new System.Windows.Forms.Label();
            this.TabPage4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudRemind)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtRemindSpace)).BeginInit();
            this.TabPage3.SuspendLayout();
            this.plCompressEncrypt.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.tbCompressionLevel)).BeginInit();
            this.TabPage2.SuspendLayout();
            this.TabPage1.SuspendLayout();
            this.tcOptions.SuspendLayout();
            this.TabPage5.SuspendLayout();
            this.plDevice.SuspendLayout();
            this.plUNCAuthentication.SuspendLayout();
            this.plFTP.SuspendLayout();
            this.plExcludeCompress.SuspendLayout();
            this.SuspendLayout();
            // 
            // ImageList1
            // 
            this.ImageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("ImageList1.ImageStream")));
            this.ImageList1.TransparentColor = System.Drawing.Color.Transparent;
            this.ImageList1.Images.SetKeyName(0, "fixed");
            this.ImageList1.Images.SetKeyName(1, "removable");
            this.ImageList1.Images.SetKeyName(2, "remote");
            // 
            // ColumnHeader3
            // 
            this.ColumnHeader3.Width = 464;
            // 
            // ColumnHeader4
            // 
            this.ColumnHeader4.Width = 464;
            // 
            // ColumnHeader6
            // 
            this.ColumnHeader6.Width = 521;
            // 
            // ColumnHeader7
            // 
            this.ColumnHeader7.Width = 521;
            // 
            // TabPage4
            // 
            this.TabPage4.BackColor = System.Drawing.Color.White;
            this.TabPage4.Controls.Add(this.chkWaitOnMediaInteractive);
            this.TabPage4.Controls.Add(this.chkRemind);
            this.TabPage4.Controls.Add(this.Label9);
            this.TabPage4.Controls.Add(this.nudRemind);
            this.TabPage4.Controls.Add(this.chkInfoBackupDone);
            this.TabPage4.Controls.Add(this.chkShowLocalized);
            this.TabPage4.Controls.Add(this.chkAbortWhenNotAvailable);
            this.TabPage4.Controls.Add(this.chkRemindSpace);
            this.TabPage4.Controls.Add(this.chkDeactivateAutoBackupsWhenAkku);
            this.TabPage4.Controls.Add(this.Label14);
            this.TabPage4.Controls.Add(this.txtRemindSpace);
            this.TabPage4.Location = new System.Drawing.Point(4, 37);
            this.TabPage4.Margin = new System.Windows.Forms.Padding(4);
            this.TabPage4.Name = "TabPage4";
            this.TabPage4.Padding = new System.Windows.Forms.Padding(4);
            this.TabPage4.Size = new System.Drawing.Size(936, 439);
            this.TabPage4.TabIndex = 3;
            this.TabPage4.Text = "Weitere";
            // 
            // chkWaitOnMediaInteractive
            // 
            this.chkWaitOnMediaInteractive.AutoSize = true;
            this.chkWaitOnMediaInteractive.BackColor = System.Drawing.Color.Transparent;
            this.chkWaitOnMediaInteractive.Location = new System.Drawing.Point(32, 270);
            this.chkWaitOnMediaInteractive.Margin = new System.Windows.Forms.Padding(4);
            this.chkWaitOnMediaInteractive.Name = "chkWaitOnMediaInteractive";
            this.chkWaitOnMediaInteractive.Size = new System.Drawing.Size(540, 32);
            this.chkWaitOnMediaInteractive.TabIndex = 124;
            this.chkWaitOnMediaInteractive.Text = "Aufforderung bei fehlendem Sicherungsmedium anzeigen";
            this.chkWaitOnMediaInteractive.UseVisualStyleBackColor = false;
            // 
            // chkRemind
            // 
            this.chkRemind.AutoSize = true;
            this.chkRemind.BackColor = System.Drawing.Color.Transparent;
            this.chkRemind.Location = new System.Drawing.Point(32, 308);
            this.chkRemind.Margin = new System.Windows.Forms.Padding(4);
            this.chkRemind.Name = "chkRemind";
            this.chkRemind.Size = new System.Drawing.Size(165, 32);
            this.chkRemind.TabIndex = 121;
            this.chkRemind.Text = "Erinnern, wenn";
            this.chkRemind.UseVisualStyleBackColor = false;
            this.chkRemind.CheckedChanged += new System.EventHandler(this.chkRemind_CheckedChanged);
            // 
            // Label9
            // 
            this.Label9.AutoSize = true;
            this.Label9.BackColor = System.Drawing.Color.Transparent;
            this.Label9.Location = new System.Drawing.Point(380, 306);
            this.Label9.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.Label9.Name = "Label9";
            this.Label9.Size = new System.Drawing.Size(377, 28);
            this.Label9.TabIndex = 122;
            this.Label9.Text = "Tage keine Sicherung durchgeführt wurde.";
            // 
            // nudRemind
            // 
            this.nudRemind.Enabled = false;
            this.nudRemind.Location = new System.Drawing.Point(219, 306);
            this.nudRemind.Margin = new System.Windows.Forms.Padding(4);
            this.nudRemind.Maximum = new decimal(new int[] {
            1410065407,
            2,
            0,
            0});
            this.nudRemind.Name = "nudRemind";
            this.nudRemind.Size = new System.Drawing.Size(130, 33);
            this.nudRemind.TabIndex = 123;
            // 
            // chkInfoBackupDone
            // 
            this.chkInfoBackupDone.AutoSize = true;
            this.chkInfoBackupDone.BackColor = System.Drawing.Color.Transparent;
            this.chkInfoBackupDone.Location = new System.Drawing.Point(32, 232);
            this.chkInfoBackupDone.Margin = new System.Windows.Forms.Padding(4);
            this.chkInfoBackupDone.Name = "chkInfoBackupDone";
            this.chkInfoBackupDone.Size = new System.Drawing.Size(564, 32);
            this.chkInfoBackupDone.TabIndex = 120;
            this.chkInfoBackupDone.Text = "Information anzeigen, dass eine Sicherung abgeschlossen ist.";
            this.chkInfoBackupDone.UseVisualStyleBackColor = false;
            // 
            // chkShowLocalized
            // 
            this.chkShowLocalized.AutoSize = true;
            this.chkShowLocalized.BackColor = System.Drawing.Color.Transparent;
            this.chkShowLocalized.Location = new System.Drawing.Point(32, 195);
            this.chkShowLocalized.Margin = new System.Windows.Forms.Padding(4);
            this.chkShowLocalized.Name = "chkShowLocalized";
            this.chkShowLocalized.Size = new System.Drawing.Size(315, 32);
            this.chkShowLocalized.TabIndex = 118;
            this.chkShowLocalized.Text = "Ordnerlokalisierungen anzeigen";
            this.chkShowLocalized.UseVisualStyleBackColor = false;
            // 
            // chkAbortWhenNotAvailable
            // 
            this.chkAbortWhenNotAvailable.AutoSize = true;
            this.chkAbortWhenNotAvailable.BackColor = System.Drawing.Color.Transparent;
            this.chkAbortWhenNotAvailable.Location = new System.Drawing.Point(32, 158);
            this.chkAbortWhenNotAvailable.Margin = new System.Windows.Forms.Padding(4);
            this.chkAbortWhenNotAvailable.Name = "chkAbortWhenNotAvailable";
            this.chkAbortWhenNotAvailable.Size = new System.Drawing.Size(604, 32);
            this.chkAbortWhenNotAvailable.TabIndex = 114;
            this.chkAbortWhenNotAvailable.Text = "Auf Medium warten, wenn Sicherungsmedium nicht verfügbar ist.";
            this.chkAbortWhenNotAvailable.UseVisualStyleBackColor = false;
            // 
            // chkRemindSpace
            // 
            this.chkRemindSpace.AutoSize = true;
            this.chkRemindSpace.BackColor = System.Drawing.Color.Transparent;
            this.chkRemindSpace.Location = new System.Drawing.Point(32, 22);
            this.chkRemindSpace.Margin = new System.Windows.Forms.Padding(4);
            this.chkRemindSpace.Name = "chkRemindSpace";
            this.chkRemindSpace.Size = new System.Drawing.Size(720, 32);
            this.chkRemindSpace.TabIndex = 108;
            this.chkRemindSpace.Text = "Warnen, wenn weniger Speicherplatz auf dem Backupmedium verfügbar ist als:";
            this.chkRemindSpace.UseVisualStyleBackColor = false;
            this.chkRemindSpace.CheckedChanged += new System.EventHandler(this.chkRemindSpace_CheckedChanged);
            // 
            // chkDeactivateAutoBackupsWhenAkku
            // 
            this.chkDeactivateAutoBackupsWhenAkku.AutoSize = true;
            this.chkDeactivateAutoBackupsWhenAkku.BackColor = System.Drawing.Color.Transparent;
            this.chkDeactivateAutoBackupsWhenAkku.Location = new System.Drawing.Point(32, 120);
            this.chkDeactivateAutoBackupsWhenAkku.Margin = new System.Windows.Forms.Padding(4);
            this.chkDeactivateAutoBackupsWhenAkku.Name = "chkDeactivateAutoBackupsWhenAkku";
            this.chkDeactivateAutoBackupsWhenAkku.Size = new System.Drawing.Size(516, 32);
            this.chkDeactivateAutoBackupsWhenAkku.TabIndex = 112;
            this.chkDeactivateAutoBackupsWhenAkku.Text = "Im Akkubetrieb automatische Sicherungen ausschalten.";
            this.chkDeactivateAutoBackupsWhenAkku.UseVisualStyleBackColor = false;
            // 
            // Label14
            // 
            this.Label14.AutoSize = true;
            this.Label14.BackColor = System.Drawing.Color.Transparent;
            this.Label14.Location = new System.Drawing.Point(254, 63);
            this.Label14.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.Label14.Name = "Label14";
            this.Label14.Size = new System.Drawing.Size(41, 28);
            this.Label14.TabIndex = 110;
            this.Label14.Text = "MB";
            // 
            // txtRemindSpace
            // 
            this.txtRemindSpace.Enabled = false;
            this.txtRemindSpace.Location = new System.Drawing.Point(69, 60);
            this.txtRemindSpace.Margin = new System.Windows.Forms.Padding(4);
            this.txtRemindSpace.Maximum = new decimal(new int[] {
            1410065407,
            2,
            0,
            0});
            this.txtRemindSpace.Name = "txtRemindSpace";
            this.txtRemindSpace.Size = new System.Drawing.Size(176, 33);
            this.txtRemindSpace.TabIndex = 111;
            // 
            // TabPage3
            // 
            this.TabPage3.BackColor = System.Drawing.Color.White;
            this.TabPage3.Controls.Add(this.cmdDeactivateEncrypt);
            this.TabPage3.Controls.Add(this.plCompressEncrypt);
            this.TabPage3.Controls.Add(this.cmdEncrypt);
            this.TabPage3.Location = new System.Drawing.Point(4, 37);
            this.TabPage3.Margin = new System.Windows.Forms.Padding(4);
            this.TabPage3.Name = "TabPage3";
            this.TabPage3.Padding = new System.Windows.Forms.Padding(4);
            this.TabPage3.Size = new System.Drawing.Size(936, 439);
            this.TabPage3.TabIndex = 2;
            this.TabPage3.Text = "Sicherungsoptionen";
            // 
            // cmdDeactivateEncrypt
            // 
            this.cmdDeactivateEncrypt.Location = new System.Drawing.Point(206, 369);
            this.cmdDeactivateEncrypt.Margin = new System.Windows.Forms.Padding(4);
            this.cmdDeactivateEncrypt.Name = "cmdDeactivateEncrypt";
            this.cmdDeactivateEncrypt.Size = new System.Drawing.Size(155, 48);
            this.cmdDeactivateEncrypt.TabIndex = 105;
            this.cmdDeactivateEncrypt.Tag = "";
            this.cmdDeactivateEncrypt.Text = "Ausschalten";
            this.cmdDeactivateEncrypt.UseVisualStyleBackColor = true;
            this.cmdDeactivateEncrypt.Click += new System.EventHandler(this.cmdDeactivateEncrypt_Click);
            // 
            // plCompressEncrypt
            // 
            this.plCompressEncrypt.BackColor = System.Drawing.Color.Transparent;
            this.plCompressEncrypt.Controls.Add(this.cmdExcludeCompress);
            this.plCompressEncrypt.Controls.Add(this.lblCompressionLevel);
            this.plCompressEncrypt.Controls.Add(this.tbCompressionLevel);
            this.plCompressEncrypt.Controls.Add(this.rdEncrypt);
            this.plCompressEncrypt.Controls.Add(this.rdNoCompress);
            this.plCompressEncrypt.Controls.Add(this.rdCompress);
            this.plCompressEncrypt.Location = new System.Drawing.Point(6, 8);
            this.plCompressEncrypt.Margin = new System.Windows.Forms.Padding(4);
            this.plCompressEncrypt.Name = "plCompressEncrypt";
            this.plCompressEncrypt.Size = new System.Drawing.Size(916, 353);
            this.plCompressEncrypt.TabIndex = 107;
            // 
            // cmdExcludeCompress
            // 
            this.cmdExcludeCompress.Location = new System.Drawing.Point(732, 124);
            this.cmdExcludeCompress.Margin = new System.Windows.Forms.Padding(4);
            this.cmdExcludeCompress.Name = "cmdExcludeCompress";
            this.cmdExcludeCompress.Size = new System.Drawing.Size(152, 48);
            this.cmdExcludeCompress.TabIndex = 108;
            this.cmdExcludeCompress.Text = "Ausschließen";
            this.cmdExcludeCompress.UseVisualStyleBackColor = true;
            this.cmdExcludeCompress.Click += new System.EventHandler(this.cmdExcludeCompress_Click);
            // 
            // lblCompressionLevel
            // 
            this.lblCompressionLevel.AutoSize = true;
            this.lblCompressionLevel.Location = new System.Drawing.Point(57, 192);
            this.lblCompressionLevel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblCompressionLevel.Name = "lblCompressionLevel";
            this.lblCompressionLevel.Size = new System.Drawing.Size(0, 28);
            this.lblCompressionLevel.TabIndex = 107;
            // 
            // tbCompressionLevel
            // 
            this.tbCompressionLevel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbCompressionLevel.BackColor = System.Drawing.Color.White;
            this.tbCompressionLevel.Enabled = false;
            this.tbCompressionLevel.LargeChange = 1;
            this.tbCompressionLevel.Location = new System.Drawing.Point(50, 124);
            this.tbCompressionLevel.Margin = new System.Windows.Forms.Padding(4);
            this.tbCompressionLevel.Maximum = 9;
            this.tbCompressionLevel.Name = "tbCompressionLevel";
            this.tbCompressionLevel.Size = new System.Drawing.Size(674, 69);
            this.tbCompressionLevel.TabIndex = 106;
            this.tbCompressionLevel.TickStyle = System.Windows.Forms.TickStyle.TopLeft;
            this.tbCompressionLevel.ValueChanged += new System.EventHandler(this.tbCompressionLevel_ValueChanged);
            // 
            // rdEncrypt
            // 
            this.rdEncrypt.AutoSize = true;
            this.rdEncrypt.CheckAlign = System.Drawing.ContentAlignment.TopLeft;
            this.rdEncrypt.Location = new System.Drawing.Point(26, 236);
            this.rdEncrypt.Margin = new System.Windows.Forms.Padding(4);
            this.rdEncrypt.Name = "rdEncrypt";
            this.rdEncrypt.Size = new System.Drawing.Size(881, 116);
            this.rdEncrypt.TabIndex = 102;
            this.rdEncrypt.Text = resources.GetString("rdEncrypt.Text");
            this.rdEncrypt.UseVisualStyleBackColor = true;
            this.rdEncrypt.CheckedChanged += new System.EventHandler(this.rdEncrypt_CheckedChanged);
            // 
            // rdNoCompress
            // 
            this.rdNoCompress.AutoSize = true;
            this.rdNoCompress.Checked = true;
            this.rdNoCompress.Location = new System.Drawing.Point(26, 14);
            this.rdNoCompress.Margin = new System.Windows.Forms.Padding(4);
            this.rdNoCompress.Name = "rdNoCompress";
            this.rdNoCompress.Size = new System.Drawing.Size(392, 32);
            this.rdNoCompress.TabIndex = 104;
            this.rdNoCompress.TabStop = true;
            this.rdNoCompress.Text = "Keine Kompression oder Verschlüsselung";
            this.rdNoCompress.UseVisualStyleBackColor = true;
            // 
            // rdCompress
            // 
            this.rdCompress.AutoSize = true;
            this.rdCompress.Location = new System.Drawing.Point(26, 74);
            this.rdCompress.Margin = new System.Windows.Forms.Padding(4);
            this.rdCompress.Name = "rdCompress";
            this.rdCompress.Size = new System.Drawing.Size(582, 32);
            this.rdCompress.TabIndex = 103;
            this.rdCompress.Text = "Datensicherung komprimieren (erfordert mehr Rechenleistung)";
            this.rdCompress.UseVisualStyleBackColor = true;
            this.rdCompress.CheckedChanged += new System.EventHandler(this.rdCompress_CheckedChanged);
            // 
            // cmdEncrypt
            // 
            this.cmdEncrypt.Enabled = false;
            this.cmdEncrypt.Location = new System.Drawing.Point(56, 369);
            this.cmdEncrypt.Margin = new System.Windows.Forms.Padding(4);
            this.cmdEncrypt.Name = "cmdEncrypt";
            this.cmdEncrypt.Size = new System.Drawing.Size(142, 48);
            this.cmdEncrypt.TabIndex = 97;
            this.cmdEncrypt.Tag = "";
            this.cmdEncrypt.Text = "Einrichten";
            this.cmdEncrypt.UseVisualStyleBackColor = true;
            this.cmdEncrypt.Click += new System.EventHandler(this.cmdEncrypt_Click);
            // 
            // TabPage2
            // 
            this.TabPage2.BackColor = System.Drawing.Color.White;
            this.TabPage2.Controls.Add(this.chkDoPastBackups);
            this.TabPage2.Controls.Add(this.cmdEditSchedule);
            this.TabPage2.Controls.Add(this.rbMB);
            this.TabPage2.Controls.Add(this.rbFAB);
            this.TabPage2.Controls.Add(this.rbTSB);
            this.TabPage2.Location = new System.Drawing.Point(4, 37);
            this.TabPage2.Margin = new System.Windows.Forms.Padding(4);
            this.TabPage2.Name = "TabPage2";
            this.TabPage2.Padding = new System.Windows.Forms.Padding(4);
            this.TabPage2.Size = new System.Drawing.Size(936, 439);
            this.TabPage2.TabIndex = 1;
            this.TabPage2.Text = "Modus";
            // 
            // chkDoPastBackups
            // 
            this.chkDoPastBackups.AutoSize = true;
            this.chkDoPastBackups.BackColor = System.Drawing.Color.Transparent;
            this.chkDoPastBackups.Enabled = false;
            this.chkDoPastBackups.Location = new System.Drawing.Point(204, 257);
            this.chkDoPastBackups.Margin = new System.Windows.Forms.Padding(4);
            this.chkDoPastBackups.Name = "chkDoPastBackups";
            this.chkDoPastBackups.Size = new System.Drawing.Size(329, 32);
            this.chkDoPastBackups.TabIndex = 99;
            this.chkDoPastBackups.Text = "Verpasste Sicherungen nachholen";
            this.chkDoPastBackups.UseVisualStyleBackColor = false;
            // 
            // cmdEditSchedule
            // 
            this.cmdEditSchedule.Enabled = false;
            this.cmdEditSchedule.Location = new System.Drawing.Point(57, 248);
            this.cmdEditSchedule.Margin = new System.Windows.Forms.Padding(4);
            this.cmdEditSchedule.Name = "cmdEditSchedule";
            this.cmdEditSchedule.Size = new System.Drawing.Size(123, 48);
            this.cmdEditSchedule.TabIndex = 98;
            this.cmdEditSchedule.Tag = "";
            this.cmdEditSchedule.Text = "Zeitplan";
            this.cmdEditSchedule.UseVisualStyleBackColor = true;
            this.cmdEditSchedule.Click += new System.EventHandler(this.cmdEditSchedule_Click);
            // 
            // rbMB
            // 
            this.rbMB.AutoSize = true;
            this.rbMB.BackColor = System.Drawing.Color.Transparent;
            this.rbMB.CheckAlign = System.Drawing.ContentAlignment.TopLeft;
            this.rbMB.Location = new System.Drawing.Point(32, 311);
            this.rbMB.Margin = new System.Windows.Forms.Padding(4);
            this.rbMB.Name = "rbMB";
            this.rbMB.Size = new System.Drawing.Size(364, 60);
            this.rbMB.TabIndex = 94;
            this.rbMB.Text = "Manuelle Sicherung\r\nSie erstellen die Sicherungen manuell.";
            this.rbMB.UseVisualStyleBackColor = false;
            // 
            // rbFAB
            // 
            this.rbFAB.AutoSize = true;
            this.rbFAB.BackColor = System.Drawing.Color.Transparent;
            this.rbFAB.CheckAlign = System.Drawing.ContentAlignment.TopLeft;
            this.rbFAB.Location = new System.Drawing.Point(32, 22);
            this.rbFAB.Margin = new System.Windows.Forms.Padding(4);
            this.rbFAB.Name = "rbFAB";
            this.rbFAB.Size = new System.Drawing.Size(878, 116);
            this.rbFAB.TabIndex = 92;
            this.rbFAB.Text = resources.GetString("rbFAB.Text");
            this.rbFAB.UseVisualStyleBackColor = false;
            // 
            // rbTSB
            // 
            this.rbTSB.AutoSize = true;
            this.rbTSB.BackColor = System.Drawing.Color.Transparent;
            this.rbTSB.CheckAlign = System.Drawing.ContentAlignment.TopLeft;
            this.rbTSB.Location = new System.Drawing.Point(32, 149);
            this.rbTSB.Margin = new System.Windows.Forms.Padding(4);
            this.rbTSB.Name = "rbTSB";
            this.rbTSB.Size = new System.Drawing.Size(822, 88);
            this.rbTSB.TabIndex = 93;
            this.rbTSB.Text = "Zeitplangesteuerte Sicherung\r\nSie legen fest, wann eine Datensicherung durchgefüh" +
    "rt wird und wie lange diese verfügbar \r\nbleiben.";
            this.rbTSB.UseVisualStyleBackColor = false;
            this.rbTSB.CheckedChanged += new System.EventHandler(this.rbTSB_CheckedChanged);
            // 
            // TabPage1
            // 
            this.TabPage1.BackColor = System.Drawing.Color.White;
            this.TabPage1.Controls.Add(this.cmdFilter);
            this.TabPage1.Controls.Add(this.lvSource);
            this.TabPage1.Controls.Add(this.cmdAddSource);
            this.TabPage1.Controls.Add(this.cmdDeleteSource);
            this.TabPage1.Location = new System.Drawing.Point(4, 37);
            this.TabPage1.Margin = new System.Windows.Forms.Padding(4);
            this.TabPage1.Name = "TabPage1";
            this.TabPage1.Padding = new System.Windows.Forms.Padding(4);
            this.TabPage1.Size = new System.Drawing.Size(936, 439);
            this.TabPage1.TabIndex = 0;
            this.TabPage1.Text = "Quellverzeichnisse";
            // 
            // cmdFilter
            // 
            this.cmdFilter.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdFilter.Location = new System.Drawing.Point(676, 374);
            this.cmdFilter.Margin = new System.Windows.Forms.Padding(4);
            this.cmdFilter.Name = "cmdFilter";
            this.cmdFilter.Size = new System.Drawing.Size(231, 48);
            this.cmdFilter.TabIndex = 116;
            this.cmdFilter.Tag = "";
            this.cmdFilter.Text = "Dateien ausschließen";
            this.cmdFilter.UseVisualStyleBackColor = true;
            this.cmdFilter.Click += new System.EventHandler(this.cmdFilter_Click);
            // 
            // lvSource
            // 
            this.lvSource.AllowDrop = true;
            this.lvSource.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lvSource.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.ColumnHeader1});
            this.lvSource.FullRowSelect = true;
            this.lvSource.HideSelection = false;
            this.lvSource.Location = new System.Drawing.Point(22, 20);
            this.lvSource.Margin = new System.Windows.Forms.Padding(4);
            this.lvSource.Name = "lvSource";
            this.lvSource.Size = new System.Drawing.Size(885, 337);
            this.lvSource.SmallImageList = this.ImageList1;
            this.lvSource.Sorting = System.Windows.Forms.SortOrder.Ascending;
            this.lvSource.TabIndex = 73;
            this.lvSource.UseCompatibleStateImageBehavior = false;
            this.lvSource.View = System.Windows.Forms.View.Details;
            this.lvSource.SelectedIndexChanged += new System.EventHandler(this.lvSource_SelectedIndexChanged);
            this.lvSource.DragDrop += new System.Windows.Forms.DragEventHandler(this.lvSource_DragDrop);
            this.lvSource.DragEnter += new System.Windows.Forms.DragEventHandler(this.lvSource_DragEnter);
            // 
            // ColumnHeader1
            // 
            this.ColumnHeader1.Text = "Verzeichnis";
            this.ColumnHeader1.Width = 522;
            // 
            // cmdAddSource
            // 
            this.cmdAddSource.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.cmdAddSource.Location = new System.Drawing.Point(22, 374);
            this.cmdAddSource.Margin = new System.Windows.Forms.Padding(4);
            this.cmdAddSource.Name = "cmdAddSource";
            this.cmdAddSource.Size = new System.Drawing.Size(147, 48);
            this.cmdAddSource.TabIndex = 74;
            this.cmdAddSource.Text = "Hinzufügen";
            this.cmdAddSource.UseVisualStyleBackColor = true;
            this.cmdAddSource.Click += new System.EventHandler(this.cmdAddSource_Click);
            // 
            // cmdDeleteSource
            // 
            this.cmdDeleteSource.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.cmdDeleteSource.Enabled = false;
            this.cmdDeleteSource.Location = new System.Drawing.Point(177, 374);
            this.cmdDeleteSource.Margin = new System.Windows.Forms.Padding(4);
            this.cmdDeleteSource.Name = "cmdDeleteSource";
            this.cmdDeleteSource.Size = new System.Drawing.Size(125, 48);
            this.cmdDeleteSource.TabIndex = 75;
            this.cmdDeleteSource.Text = "Löschen";
            this.cmdDeleteSource.UseVisualStyleBackColor = true;
            this.cmdDeleteSource.Click += new System.EventHandler(this.cmdDeleteSource_Click);
            // 
            // tcOptions
            // 
            this.tcOptions.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tcOptions.Controls.Add(this.TabPage1);
            this.tcOptions.Controls.Add(this.TabPage5);
            this.tcOptions.Controls.Add(this.TabPage2);
            this.tcOptions.Controls.Add(this.TabPage3);
            this.tcOptions.Controls.Add(this.TabPage4);
            this.tcOptions.Location = new System.Drawing.Point(27, 33);
            this.tcOptions.Margin = new System.Windows.Forms.Padding(4);
            this.tcOptions.Name = "tcOptions";
            this.tcOptions.SelectedIndex = 0;
            this.tcOptions.Size = new System.Drawing.Size(944, 480);
            this.tcOptions.TabIndex = 85;
            // 
            // TabPage5
            // 
            this.TabPage5.BackColor = System.Drawing.Color.White;
            this.TabPage5.Controls.Add(this.cmdChangeBackupMedia);
            this.TabPage5.Controls.Add(this.Label1);
            this.TabPage5.Controls.Add(this.cboMedia);
            this.TabPage5.Controls.Add(this.Label2);
            this.TabPage5.Controls.Add(this.plDevice);
            this.TabPage5.Controls.Add(this.plFTP);
            this.TabPage5.Location = new System.Drawing.Point(4, 37);
            this.TabPage5.Margin = new System.Windows.Forms.Padding(4);
            this.TabPage5.Name = "TabPage5";
            this.TabPage5.Size = new System.Drawing.Size(936, 439);
            this.TabPage5.TabIndex = 4;
            this.TabPage5.Text = "Medium";
            // 
            // cmdChangeBackupMedia
            // 
            this.cmdChangeBackupMedia.Location = new System.Drawing.Point(32, 344);
            this.cmdChangeBackupMedia.Margin = new System.Windows.Forms.Padding(4);
            this.cmdChangeBackupMedia.Name = "cmdChangeBackupMedia";
            this.cmdChangeBackupMedia.Size = new System.Drawing.Size(144, 48);
            this.cmdChangeBackupMedia.TabIndex = 97;
            this.cmdChangeBackupMedia.Text = "Wechseln";
            this.cmdChangeBackupMedia.UseVisualStyleBackColor = true;
            this.cmdChangeBackupMedia.Click += new System.EventHandler(this.Button1_Click);
            // 
            // Label1
            // 
            this.Label1.AutoSize = true;
            this.Label1.BackColor = System.Drawing.Color.Transparent;
            this.Label1.Location = new System.Drawing.Point(213, 348);
            this.Label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.Label1.Name = "Label1";
            this.Label1.Size = new System.Drawing.Size(518, 56);
            this.Label1.TabIndex = 96;
            this.Label1.Text = "Ermöglicht das Wechseln des Sicherungsmediums auf ein \r\nneues (leeres) Medium ohn" +
    "e die Einstellungen zu verlieren.";
            // 
            // cboMedia
            // 
            this.cboMedia.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboMedia.FormattingEnabled = true;
            this.cboMedia.Items.AddRange(new object[] {
            "Verzeichnis oder Laufwerk",
            "Server (FTP)"});
            this.cboMedia.Location = new System.Drawing.Point(218, 22);
            this.cboMedia.Margin = new System.Windows.Forms.Padding(4);
            this.cboMedia.Name = "cboMedia";
            this.cboMedia.Size = new System.Drawing.Size(478, 36);
            this.cboMedia.TabIndex = 95;
            this.cboMedia.SelectedIndexChanged += new System.EventHandler(this.cboMedia_SelectedIndexChanged);
            // 
            // Label2
            // 
            this.Label2.AutoSize = true;
            this.Label2.BackColor = System.Drawing.Color.Transparent;
            this.Label2.Location = new System.Drawing.Point(27, 27);
            this.Label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.Label2.Name = "Label2";
            this.Label2.Size = new System.Drawing.Size(183, 28);
            this.Label2.TabIndex = 92;
            this.Label2.Text = "Sicherungsmedium:";
            // 
            // plDevice
            // 
            this.plDevice.BackColor = System.Drawing.Color.Transparent;
            this.plDevice.Controls.Add(this.plUNCAuthentication);
            this.plDevice.Controls.Add(this.Label15);
            this.plDevice.Controls.Add(this.cmdChange);
            this.plDevice.Controls.Add(this.txtBackupPath);
            this.plDevice.Location = new System.Drawing.Point(32, 69);
            this.plDevice.Margin = new System.Windows.Forms.Padding(4);
            this.plDevice.Name = "plDevice";
            this.plDevice.Size = new System.Drawing.Size(872, 188);
            this.plDevice.TabIndex = 94;
            // 
            // plUNCAuthentication
            // 
            this.plUNCAuthentication.Controls.Add(this.Label11);
            this.plUNCAuthentication.Controls.Add(this.txtUNCPassword);
            this.plUNCAuthentication.Controls.Add(this.Label10);
            this.plUNCAuthentication.Controls.Add(this.txtUNCUsername);
            this.plUNCAuthentication.Location = new System.Drawing.Point(40, 52);
            this.plUNCAuthentication.Margin = new System.Windows.Forms.Padding(4);
            this.plUNCAuthentication.Name = "plUNCAuthentication";
            this.plUNCAuthentication.Size = new System.Drawing.Size(696, 90);
            this.plUNCAuthentication.TabIndex = 85;
            // 
            // Label11
            // 
            this.Label11.AutoSize = true;
            this.Label11.Location = new System.Drawing.Point(4, 46);
            this.Label11.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.Label11.Name = "Label11";
            this.Label11.Size = new System.Drawing.Size(100, 28);
            this.Label11.TabIndex = 84;
            this.Label11.Text = "Kennwort:";
            // 
            // txtUNCPassword
            // 
            this.txtUNCPassword.BackColor = System.Drawing.Color.White;
            this.txtUNCPassword.Location = new System.Drawing.Point(146, 42);
            this.txtUNCPassword.Margin = new System.Windows.Forms.Padding(4);
            this.txtUNCPassword.Name = "txtUNCPassword";
            this.txtUNCPassword.Size = new System.Drawing.Size(548, 33);
            this.txtUNCPassword.TabIndex = 83;
            this.txtUNCPassword.UseSystemPasswordChar = true;
            // 
            // Label10
            // 
            this.Label10.AutoSize = true;
            this.Label10.Location = new System.Drawing.Point(4, 8);
            this.Label10.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.Label10.Name = "Label10";
            this.Label10.Size = new System.Drawing.Size(140, 28);
            this.Label10.TabIndex = 82;
            this.Label10.Text = "Benutzername:";
            // 
            // txtUNCUsername
            // 
            this.txtUNCUsername.BackColor = System.Drawing.Color.White;
            this.txtUNCUsername.Location = new System.Drawing.Point(146, 3);
            this.txtUNCUsername.Margin = new System.Windows.Forms.Padding(4);
            this.txtUNCUsername.Name = "txtUNCUsername";
            this.txtUNCUsername.Size = new System.Drawing.Size(548, 33);
            this.txtUNCUsername.TabIndex = 81;
            // 
            // Label15
            // 
            this.Label15.AutoSize = true;
            this.Label15.Location = new System.Drawing.Point(45, 10);
            this.Label15.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.Label15.Name = "Label15";
            this.Label15.Size = new System.Drawing.Size(117, 28);
            this.Label15.TabIndex = 80;
            this.Label15.Text = "Speicherort:";
            // 
            // cmdChange
            // 
            this.cmdChange.Location = new System.Drawing.Point(746, 6);
            this.cmdChange.Margin = new System.Windows.Forms.Padding(4);
            this.cmdChange.Name = "cmdChange";
            this.cmdChange.Size = new System.Drawing.Size(122, 48);
            this.cmdChange.TabIndex = 79;
            this.cmdChange.Tag = "";
            this.cmdChange.Text = "&Ändern";
            this.cmdChange.UseVisualStyleBackColor = true;
            this.cmdChange.Click += new System.EventHandler(this.cmdChange_Click);
            // 
            // txtBackupPath
            // 
            this.txtBackupPath.BackColor = System.Drawing.Color.White;
            this.txtBackupPath.Location = new System.Drawing.Point(186, 6);
            this.txtBackupPath.Margin = new System.Windows.Forms.Padding(4);
            this.txtBackupPath.Name = "txtBackupPath";
            this.txtBackupPath.ReadOnly = true;
            this.txtBackupPath.Size = new System.Drawing.Size(548, 33);
            this.txtBackupPath.TabIndex = 78;
            this.txtBackupPath.TextChanged += new System.EventHandler(this.txtBackupPath_TextChanged);
            // 
            // plFTP
            // 
            this.plFTP.BackColor = System.Drawing.Color.Transparent;
            this.plFTP.Controls.Add(this.chkFtpEncryption);
            this.plFTP.Controls.Add(this.Label12);
            this.plFTP.Controls.Add(this.cboFtpEncoding);
            this.plFTP.Controls.Add(this.cmdFTPCheck);
            this.plFTP.Controls.Add(this.txtFTPPort);
            this.plFTP.Controls.Add(this.Label8);
            this.plFTP.Controls.Add(this.txtFTPPath);
            this.plFTP.Controls.Add(this.Label7);
            this.plFTP.Controls.Add(this.txtFTPPassword);
            this.plFTP.Controls.Add(this.Label5);
            this.plFTP.Controls.Add(this.txtFTPUsername);
            this.plFTP.Controls.Add(this.Label4);
            this.plFTP.Controls.Add(this.txtFTPServer);
            this.plFTP.Controls.Add(this.Label3);
            this.plFTP.Location = new System.Drawing.Point(32, 69);
            this.plFTP.Margin = new System.Windows.Forms.Padding(4);
            this.plFTP.Name = "plFTP";
            this.plFTP.Size = new System.Drawing.Size(872, 228);
            this.plFTP.TabIndex = 93;
            this.plFTP.Visible = false;
            // 
            // chkFtpEncryption
            // 
            this.chkFtpEncryption.Location = new System.Drawing.Point(457, 181);
            this.chkFtpEncryption.Name = "chkFtpEncryption";
            this.chkFtpEncryption.Size = new System.Drawing.Size(411, 40);
            this.chkFtpEncryption.TabIndex = 94;
            this.chkFtpEncryption.Text = "Unverschlüsselte Verbindung erzwingen";
            this.chkFtpEncryption.UseVisualStyleBackColor = true;
            // 
            // Label12
            // 
            this.Label12.AutoSize = true;
            this.Label12.Location = new System.Drawing.Point(45, 186);
            this.Label12.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.Label12.Name = "Label12";
            this.Label12.Size = new System.Drawing.Size(98, 28);
            this.Label12.TabIndex = 93;
            this.Label12.Text = "Encoding:";
            // 
            // cboFtpEncoding
            // 
            this.cboFtpEncoding.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboFtpEncoding.FormattingEnabled = true;
            this.cboFtpEncoding.Items.AddRange(new object[] {
            "ISO-8859-1",
            "UTF-8"});
            this.cboFtpEncoding.Location = new System.Drawing.Point(186, 178);
            this.cboFtpEncoding.Name = "cboFtpEncoding";
            this.cboFtpEncoding.Size = new System.Drawing.Size(248, 36);
            this.cboFtpEncoding.TabIndex = 92;
            // 
            // cmdFTPCheck
            // 
            this.cmdFTPCheck.Location = new System.Drawing.Point(675, 6);
            this.cmdFTPCheck.Margin = new System.Windows.Forms.Padding(4);
            this.cmdFTPCheck.Name = "cmdFTPCheck";
            this.cmdFTPCheck.Size = new System.Drawing.Size(122, 34);
            this.cmdFTPCheck.TabIndex = 91;
            this.cmdFTPCheck.Text = "&Testen";
            this.cmdFTPCheck.UseVisualStyleBackColor = true;
            this.cmdFTPCheck.Click += new System.EventHandler(this.cmdFTPCheck_Click);
            // 
            // txtFTPPort
            // 
            this.txtFTPPort.Location = new System.Drawing.Point(592, 6);
            this.txtFTPPort.Margin = new System.Windows.Forms.Padding(4);
            this.txtFTPPort.Name = "txtFTPPort";
            this.txtFTPPort.Size = new System.Drawing.Size(72, 33);
            this.txtFTPPort.TabIndex = 90;
            // 
            // Label8
            // 
            this.Label8.AutoSize = true;
            this.Label8.Location = new System.Drawing.Point(536, 10);
            this.Label8.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.Label8.Name = "Label8";
            this.Label8.Size = new System.Drawing.Size(52, 28);
            this.Label8.TabIndex = 89;
            this.Label8.Text = "Port:";
            // 
            // txtFTPPath
            // 
            this.txtFTPPath.Location = new System.Drawing.Point(186, 138);
            this.txtFTPPath.Margin = new System.Windows.Forms.Padding(4);
            this.txtFTPPath.Name = "txtFTPPath";
            this.txtFTPPath.Size = new System.Drawing.Size(478, 33);
            this.txtFTPPath.TabIndex = 88;
            // 
            // Label7
            // 
            this.Label7.AutoSize = true;
            this.Label7.Location = new System.Drawing.Point(45, 142);
            this.Label7.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.Label7.Name = "Label7";
            this.Label7.Size = new System.Drawing.Size(112, 28);
            this.Label7.TabIndex = 87;
            this.Label7.Text = "Verzeichnis:";
            // 
            // txtFTPPassword
            // 
            this.txtFTPPassword.Location = new System.Drawing.Point(186, 94);
            this.txtFTPPassword.Margin = new System.Windows.Forms.Padding(4);
            this.txtFTPPassword.Name = "txtFTPPassword";
            this.txtFTPPassword.Size = new System.Drawing.Size(478, 33);
            this.txtFTPPassword.TabIndex = 86;
            this.txtFTPPassword.UseSystemPasswordChar = true;
            // 
            // Label5
            // 
            this.Label5.AutoSize = true;
            this.Label5.Location = new System.Drawing.Point(45, 99);
            this.Label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.Label5.Name = "Label5";
            this.Label5.Size = new System.Drawing.Size(100, 28);
            this.Label5.TabIndex = 85;
            this.Label5.Text = "Kennwort:";
            // 
            // txtFTPUsername
            // 
            this.txtFTPUsername.Location = new System.Drawing.Point(186, 51);
            this.txtFTPUsername.Margin = new System.Windows.Forms.Padding(4);
            this.txtFTPUsername.Name = "txtFTPUsername";
            this.txtFTPUsername.Size = new System.Drawing.Size(478, 33);
            this.txtFTPUsername.TabIndex = 84;
            // 
            // Label4
            // 
            this.Label4.AutoSize = true;
            this.Label4.Location = new System.Drawing.Point(45, 56);
            this.Label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.Label4.Name = "Label4";
            this.Label4.Size = new System.Drawing.Size(140, 28);
            this.Label4.TabIndex = 83;
            this.Label4.Text = "Benutzername:";
            // 
            // txtFTPServer
            // 
            this.txtFTPServer.Location = new System.Drawing.Point(186, 6);
            this.txtFTPServer.Margin = new System.Windows.Forms.Padding(4);
            this.txtFTPServer.Name = "txtFTPServer";
            this.txtFTPServer.Size = new System.Drawing.Size(338, 33);
            this.txtFTPServer.TabIndex = 82;
            // 
            // Label3
            // 
            this.Label3.AutoSize = true;
            this.Label3.Location = new System.Drawing.Point(45, 10);
            this.Label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.Label3.Name = "Label3";
            this.Label3.Size = new System.Drawing.Size(71, 28);
            this.Label3.TabIndex = 81;
            this.Label3.Text = "Server:";
            // 
            // cmdOK
            // 
            this.cmdOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdOK.Location = new System.Drawing.Point(801, 512);
            this.cmdOK.Margin = new System.Windows.Forms.Padding(4);
            this.cmdOK.Name = "cmdOK";
            this.cmdOK.Size = new System.Drawing.Size(166, 48);
            this.cmdOK.TabIndex = 86;
            this.cmdOK.Text = "&Speichern";
            this.cmdOK.UseVisualStyleBackColor = true;
            this.cmdOK.Click += new System.EventHandler(this.cmdOK_Click);
            // 
            // plExcludeCompress
            // 
            this.plExcludeCompress.BackColor = System.Drawing.Color.White;
            this.plExcludeCompress.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.plExcludeCompress.Controls.Add(this.cmdCloseExcludeCompress);
            this.plExcludeCompress.Controls.Add(this.cmdDeleteExcludeCompress);
            this.plExcludeCompress.Controls.Add(this.cmdAddExcludeCompress);
            this.plExcludeCompress.Controls.Add(this.lstExcludeCompress);
            this.plExcludeCompress.Controls.Add(this.Label6);
            this.plExcludeCompress.Location = new System.Drawing.Point(98, 27);
            this.plExcludeCompress.Margin = new System.Windows.Forms.Padding(4);
            this.plExcludeCompress.Name = "plExcludeCompress";
            this.plExcludeCompress.Size = new System.Drawing.Size(800, 436);
            this.plExcludeCompress.TabIndex = 87;
            this.plExcludeCompress.Visible = false;
            // 
            // cmdCloseExcludeCompress
            // 
            this.cmdCloseExcludeCompress.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdCloseExcludeCompress.Location = new System.Drawing.Point(650, 364);
            this.cmdCloseExcludeCompress.Margin = new System.Windows.Forms.Padding(4);
            this.cmdCloseExcludeCompress.Name = "cmdCloseExcludeCompress";
            this.cmdCloseExcludeCompress.Size = new System.Drawing.Size(112, 48);
            this.cmdCloseExcludeCompress.TabIndex = 5;
            this.cmdCloseExcludeCompress.Text = "&OK";
            this.cmdCloseExcludeCompress.UseVisualStyleBackColor = true;
            this.cmdCloseExcludeCompress.Click += new System.EventHandler(this.cmdCloseExcludeCompress_Click);
            // 
            // cmdDeleteExcludeCompress
            // 
            this.cmdDeleteExcludeCompress.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.cmdDeleteExcludeCompress.Enabled = false;
            this.cmdDeleteExcludeCompress.Location = new System.Drawing.Point(191, 364);
            this.cmdDeleteExcludeCompress.Margin = new System.Windows.Forms.Padding(4);
            this.cmdDeleteExcludeCompress.Name = "cmdDeleteExcludeCompress";
            this.cmdDeleteExcludeCompress.Size = new System.Drawing.Size(150, 48);
            this.cmdDeleteExcludeCompress.TabIndex = 4;
            this.cmdDeleteExcludeCompress.Text = "Löschen";
            this.cmdDeleteExcludeCompress.UseVisualStyleBackColor = true;
            this.cmdDeleteExcludeCompress.Click += new System.EventHandler(this.cmdDeleteExcludeCompress_Click);
            // 
            // cmdAddExcludeCompress
            // 
            this.cmdAddExcludeCompress.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.cmdAddExcludeCompress.Location = new System.Drawing.Point(33, 364);
            this.cmdAddExcludeCompress.Margin = new System.Windows.Forms.Padding(4);
            this.cmdAddExcludeCompress.Name = "cmdAddExcludeCompress";
            this.cmdAddExcludeCompress.Size = new System.Drawing.Size(150, 48);
            this.cmdAddExcludeCompress.TabIndex = 3;
            this.cmdAddExcludeCompress.Text = "Hinzufügen";
            this.cmdAddExcludeCompress.UseVisualStyleBackColor = true;
            this.cmdAddExcludeCompress.Click += new System.EventHandler(this.cmdAddExcludeCompress_Click);
            // 
            // lstExcludeCompress
            // 
            this.lstExcludeCompress.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lstExcludeCompress.FormattingEnabled = true;
            this.lstExcludeCompress.IntegralHeight = false;
            this.lstExcludeCompress.ItemHeight = 28;
            this.lstExcludeCompress.Location = new System.Drawing.Point(33, 76);
            this.lstExcludeCompress.Margin = new System.Windows.Forms.Padding(4);
            this.lstExcludeCompress.Name = "lstExcludeCompress";
            this.lstExcludeCompress.Size = new System.Drawing.Size(727, 262);
            this.lstExcludeCompress.Sorted = true;
            this.lstExcludeCompress.TabIndex = 2;
            this.lstExcludeCompress.SelectedIndexChanged += new System.EventHandler(this.lstExcludeCompress_SelectedIndexChanged);
            // 
            // Label6
            // 
            this.Label6.AutoSize = true;
            this.Label6.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Label6.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(153)))));
            this.Label6.Location = new System.Drawing.Point(27, 27);
            this.Label6.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.Label6.Name = "Label6";
            this.Label6.Size = new System.Drawing.Size(465, 32);
            this.Label6.TabIndex = 1;
            this.Label6.Text = "Dateitypen von Kompression ausschließen";
            // 
            // ucConfig
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(144F, 144F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoScroll = true;
            this.Controls.Add(this.plExcludeCompress);
            this.Controls.Add(this.cmdOK);
            this.Controls.Add(this.tcOptions);
            this.DoubleBuffered = true;
            this.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "ucConfig";
            this.Size = new System.Drawing.Size(1004, 588);
            this.TabPage4.ResumeLayout(false);
            this.TabPage4.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudRemind)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtRemindSpace)).EndInit();
            this.TabPage3.ResumeLayout(false);
            this.plCompressEncrypt.ResumeLayout(false);
            this.plCompressEncrypt.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.tbCompressionLevel)).EndInit();
            this.TabPage2.ResumeLayout(false);
            this.TabPage2.PerformLayout();
            this.TabPage1.ResumeLayout(false);
            this.tcOptions.ResumeLayout(false);
            this.TabPage5.ResumeLayout(false);
            this.TabPage5.PerformLayout();
            this.plDevice.ResumeLayout(false);
            this.plDevice.PerformLayout();
            this.plUNCAuthentication.ResumeLayout(false);
            this.plUNCAuthentication.PerformLayout();
            this.plFTP.ResumeLayout(false);
            this.plFTP.PerformLayout();
            this.plExcludeCompress.ResumeLayout(false);
            this.plExcludeCompress.PerformLayout();
            this.ResumeLayout(false);

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
        internal TrackBar tbCompressionLevel;
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
    }
}