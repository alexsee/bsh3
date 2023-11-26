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
            tbCompressionLevel = new TrackBar();
            rdEncrypt = new RadioButton();
            rdNoCompress = new RadioButton();
            rdCompress = new RadioButton();
            cmdEncrypt = new Button();
            TabPage2 = new TabPage();
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
            ((System.ComponentModel.ISupportInitialize)tbCompressionLevel).BeginInit();
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
            ColumnHeader3.Width = 464;
            // 
            // ColumnHeader4
            // 
            ColumnHeader4.Width = 464;
            // 
            // ColumnHeader6
            // 
            ColumnHeader6.Width = 521;
            // 
            // ColumnHeader7
            // 
            ColumnHeader7.Width = 521;
            // 
            // TabPage4
            // 
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
            TabPage4.Location = new System.Drawing.Point(4, 34);
            TabPage4.Margin = new Padding(4);
            TabPage4.Name = "TabPage4";
            TabPage4.Padding = new Padding(4);
            TabPage4.Size = new System.Drawing.Size(936, 442);
            TabPage4.TabIndex = 3;
            TabPage4.Text = "Weitere";
            // 
            // chkWaitOnMediaInteractive
            // 
            chkWaitOnMediaInteractive.AutoSize = true;
            chkWaitOnMediaInteractive.BackColor = System.Drawing.Color.Transparent;
            chkWaitOnMediaInteractive.Location = new System.Drawing.Point(32, 270);
            chkWaitOnMediaInteractive.Margin = new Padding(4);
            chkWaitOnMediaInteractive.Name = "chkWaitOnMediaInteractive";
            chkWaitOnMediaInteractive.Size = new System.Drawing.Size(540, 32);
            chkWaitOnMediaInteractive.TabIndex = 124;
            chkWaitOnMediaInteractive.Text = "Aufforderung bei fehlendem Sicherungsmedium anzeigen";
            chkWaitOnMediaInteractive.UseVisualStyleBackColor = false;
            // 
            // chkRemind
            // 
            chkRemind.AutoSize = true;
            chkRemind.BackColor = System.Drawing.Color.Transparent;
            chkRemind.Location = new System.Drawing.Point(32, 308);
            chkRemind.Margin = new Padding(4);
            chkRemind.Name = "chkRemind";
            chkRemind.Size = new System.Drawing.Size(165, 32);
            chkRemind.TabIndex = 121;
            chkRemind.Text = "Erinnern, wenn";
            chkRemind.UseVisualStyleBackColor = false;
            chkRemind.CheckedChanged += chkRemind_CheckedChanged;
            // 
            // Label9
            // 
            Label9.AutoSize = true;
            Label9.BackColor = System.Drawing.Color.Transparent;
            Label9.Location = new System.Drawing.Point(380, 306);
            Label9.Margin = new Padding(4, 0, 4, 0);
            Label9.Name = "Label9";
            Label9.Size = new System.Drawing.Size(377, 28);
            Label9.TabIndex = 122;
            Label9.Text = "Tage keine Sicherung durchgeführt wurde.";
            // 
            // nudRemind
            // 
            nudRemind.Enabled = false;
            nudRemind.Location = new System.Drawing.Point(219, 306);
            nudRemind.Margin = new Padding(4);
            nudRemind.Maximum = new decimal(new int[] { 1410065407, 2, 0, 0 });
            nudRemind.Name = "nudRemind";
            nudRemind.Size = new System.Drawing.Size(130, 33);
            nudRemind.TabIndex = 123;
            // 
            // chkInfoBackupDone
            // 
            chkInfoBackupDone.AutoSize = true;
            chkInfoBackupDone.BackColor = System.Drawing.Color.Transparent;
            chkInfoBackupDone.Location = new System.Drawing.Point(32, 232);
            chkInfoBackupDone.Margin = new Padding(4);
            chkInfoBackupDone.Name = "chkInfoBackupDone";
            chkInfoBackupDone.Size = new System.Drawing.Size(564, 32);
            chkInfoBackupDone.TabIndex = 120;
            chkInfoBackupDone.Text = "Information anzeigen, dass eine Sicherung abgeschlossen ist.";
            chkInfoBackupDone.UseVisualStyleBackColor = false;
            // 
            // chkShowLocalized
            // 
            chkShowLocalized.AutoSize = true;
            chkShowLocalized.BackColor = System.Drawing.Color.Transparent;
            chkShowLocalized.Location = new System.Drawing.Point(32, 195);
            chkShowLocalized.Margin = new Padding(4);
            chkShowLocalized.Name = "chkShowLocalized";
            chkShowLocalized.Size = new System.Drawing.Size(315, 32);
            chkShowLocalized.TabIndex = 118;
            chkShowLocalized.Text = "Ordnerlokalisierungen anzeigen";
            chkShowLocalized.UseVisualStyleBackColor = false;
            // 
            // chkAbortWhenNotAvailable
            // 
            chkAbortWhenNotAvailable.AutoSize = true;
            chkAbortWhenNotAvailable.BackColor = System.Drawing.Color.Transparent;
            chkAbortWhenNotAvailable.Location = new System.Drawing.Point(32, 158);
            chkAbortWhenNotAvailable.Margin = new Padding(4);
            chkAbortWhenNotAvailable.Name = "chkAbortWhenNotAvailable";
            chkAbortWhenNotAvailable.Size = new System.Drawing.Size(604, 32);
            chkAbortWhenNotAvailable.TabIndex = 114;
            chkAbortWhenNotAvailable.Text = "Auf Medium warten, wenn Sicherungsmedium nicht verfügbar ist.";
            chkAbortWhenNotAvailable.UseVisualStyleBackColor = false;
            // 
            // chkRemindSpace
            // 
            chkRemindSpace.AutoSize = true;
            chkRemindSpace.BackColor = System.Drawing.Color.Transparent;
            chkRemindSpace.Location = new System.Drawing.Point(32, 22);
            chkRemindSpace.Margin = new Padding(4);
            chkRemindSpace.Name = "chkRemindSpace";
            chkRemindSpace.Size = new System.Drawing.Size(720, 32);
            chkRemindSpace.TabIndex = 108;
            chkRemindSpace.Text = "Warnen, wenn weniger Speicherplatz auf dem Backupmedium verfügbar ist als:";
            chkRemindSpace.UseVisualStyleBackColor = false;
            chkRemindSpace.CheckedChanged += chkRemindSpace_CheckedChanged;
            // 
            // chkDeactivateAutoBackupsWhenAkku
            // 
            chkDeactivateAutoBackupsWhenAkku.AutoSize = true;
            chkDeactivateAutoBackupsWhenAkku.BackColor = System.Drawing.Color.Transparent;
            chkDeactivateAutoBackupsWhenAkku.Location = new System.Drawing.Point(32, 120);
            chkDeactivateAutoBackupsWhenAkku.Margin = new Padding(4);
            chkDeactivateAutoBackupsWhenAkku.Name = "chkDeactivateAutoBackupsWhenAkku";
            chkDeactivateAutoBackupsWhenAkku.Size = new System.Drawing.Size(516, 32);
            chkDeactivateAutoBackupsWhenAkku.TabIndex = 112;
            chkDeactivateAutoBackupsWhenAkku.Text = "Im Akkubetrieb automatische Sicherungen ausschalten.";
            chkDeactivateAutoBackupsWhenAkku.UseVisualStyleBackColor = false;
            // 
            // Label14
            // 
            Label14.AutoSize = true;
            Label14.BackColor = System.Drawing.Color.Transparent;
            Label14.Location = new System.Drawing.Point(254, 63);
            Label14.Margin = new Padding(4, 0, 4, 0);
            Label14.Name = "Label14";
            Label14.Size = new System.Drawing.Size(41, 28);
            Label14.TabIndex = 110;
            Label14.Text = "MB";
            // 
            // txtRemindSpace
            // 
            txtRemindSpace.Enabled = false;
            txtRemindSpace.Location = new System.Drawing.Point(69, 60);
            txtRemindSpace.Margin = new Padding(4);
            txtRemindSpace.Maximum = new decimal(new int[] { 1410065407, 2, 0, 0 });
            txtRemindSpace.Name = "txtRemindSpace";
            txtRemindSpace.Size = new System.Drawing.Size(176, 33);
            txtRemindSpace.TabIndex = 111;
            // 
            // TabPage3
            // 
            TabPage3.BackColor = System.Drawing.Color.White;
            TabPage3.Controls.Add(cmdDeactivateEncrypt);
            TabPage3.Controls.Add(plCompressEncrypt);
            TabPage3.Controls.Add(cmdEncrypt);
            TabPage3.Location = new System.Drawing.Point(4, 34);
            TabPage3.Margin = new Padding(4);
            TabPage3.Name = "TabPage3";
            TabPage3.Padding = new Padding(4);
            TabPage3.Size = new System.Drawing.Size(936, 442);
            TabPage3.TabIndex = 2;
            TabPage3.Text = "Sicherungsoptionen";
            // 
            // cmdDeactivateEncrypt
            // 
            cmdDeactivateEncrypt.Location = new System.Drawing.Point(206, 369);
            cmdDeactivateEncrypt.Margin = new Padding(4);
            cmdDeactivateEncrypt.Name = "cmdDeactivateEncrypt";
            cmdDeactivateEncrypt.Size = new System.Drawing.Size(155, 48);
            cmdDeactivateEncrypt.TabIndex = 105;
            cmdDeactivateEncrypt.Tag = "";
            cmdDeactivateEncrypt.Text = "Ausschalten";
            cmdDeactivateEncrypt.UseVisualStyleBackColor = true;
            cmdDeactivateEncrypt.Click += cmdDeactivateEncrypt_Click;
            // 
            // plCompressEncrypt
            // 
            plCompressEncrypt.BackColor = System.Drawing.Color.Transparent;
            plCompressEncrypt.Controls.Add(cmdExcludeCompress);
            plCompressEncrypt.Controls.Add(lblCompressionLevel);
            plCompressEncrypt.Controls.Add(tbCompressionLevel);
            plCompressEncrypt.Controls.Add(rdEncrypt);
            plCompressEncrypt.Controls.Add(rdNoCompress);
            plCompressEncrypt.Controls.Add(rdCompress);
            plCompressEncrypt.Location = new System.Drawing.Point(6, 8);
            plCompressEncrypt.Margin = new Padding(4);
            plCompressEncrypt.Name = "plCompressEncrypt";
            plCompressEncrypt.Size = new System.Drawing.Size(916, 353);
            plCompressEncrypt.TabIndex = 107;
            // 
            // cmdExcludeCompress
            // 
            cmdExcludeCompress.Location = new System.Drawing.Point(732, 124);
            cmdExcludeCompress.Margin = new Padding(4);
            cmdExcludeCompress.Name = "cmdExcludeCompress";
            cmdExcludeCompress.Size = new System.Drawing.Size(152, 48);
            cmdExcludeCompress.TabIndex = 108;
            cmdExcludeCompress.Text = "Ausschließen";
            cmdExcludeCompress.UseVisualStyleBackColor = true;
            cmdExcludeCompress.Click += cmdExcludeCompress_Click;
            // 
            // lblCompressionLevel
            // 
            lblCompressionLevel.AutoSize = true;
            lblCompressionLevel.Location = new System.Drawing.Point(57, 192);
            lblCompressionLevel.Margin = new Padding(4, 0, 4, 0);
            lblCompressionLevel.Name = "lblCompressionLevel";
            lblCompressionLevel.Size = new System.Drawing.Size(0, 28);
            lblCompressionLevel.TabIndex = 107;
            // 
            // tbCompressionLevel
            // 
            tbCompressionLevel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            tbCompressionLevel.BackColor = System.Drawing.Color.White;
            tbCompressionLevel.Enabled = false;
            tbCompressionLevel.LargeChange = 1;
            tbCompressionLevel.Location = new System.Drawing.Point(50, 124);
            tbCompressionLevel.Margin = new Padding(4);
            tbCompressionLevel.Maximum = 9;
            tbCompressionLevel.Name = "tbCompressionLevel";
            tbCompressionLevel.Size = new System.Drawing.Size(674, 69);
            tbCompressionLevel.TabIndex = 106;
            tbCompressionLevel.TickStyle = TickStyle.TopLeft;
            tbCompressionLevel.ValueChanged += tbCompressionLevel_ValueChanged;
            // 
            // rdEncrypt
            // 
            rdEncrypt.AutoSize = true;
            rdEncrypt.CheckAlign = System.Drawing.ContentAlignment.TopLeft;
            rdEncrypt.Location = new System.Drawing.Point(26, 236);
            rdEncrypt.Margin = new Padding(4);
            rdEncrypt.Name = "rdEncrypt";
            rdEncrypt.Size = new System.Drawing.Size(881, 116);
            rdEncrypt.TabIndex = 102;
            rdEncrypt.Text = resources.GetString("rdEncrypt.Text");
            rdEncrypt.UseVisualStyleBackColor = true;
            rdEncrypt.CheckedChanged += rdEncrypt_CheckedChanged;
            // 
            // rdNoCompress
            // 
            rdNoCompress.AutoSize = true;
            rdNoCompress.Checked = true;
            rdNoCompress.Location = new System.Drawing.Point(26, 14);
            rdNoCompress.Margin = new Padding(4);
            rdNoCompress.Name = "rdNoCompress";
            rdNoCompress.Size = new System.Drawing.Size(392, 32);
            rdNoCompress.TabIndex = 104;
            rdNoCompress.TabStop = true;
            rdNoCompress.Text = "Keine Kompression oder Verschlüsselung";
            rdNoCompress.UseVisualStyleBackColor = true;
            // 
            // rdCompress
            // 
            rdCompress.AutoSize = true;
            rdCompress.Location = new System.Drawing.Point(26, 74);
            rdCompress.Margin = new Padding(4);
            rdCompress.Name = "rdCompress";
            rdCompress.Size = new System.Drawing.Size(582, 32);
            rdCompress.TabIndex = 103;
            rdCompress.Text = "Datensicherung komprimieren (erfordert mehr Rechenleistung)";
            rdCompress.UseVisualStyleBackColor = true;
            rdCompress.CheckedChanged += rdCompress_CheckedChanged;
            // 
            // cmdEncrypt
            // 
            cmdEncrypt.Enabled = false;
            cmdEncrypt.Location = new System.Drawing.Point(56, 369);
            cmdEncrypt.Margin = new Padding(4);
            cmdEncrypt.Name = "cmdEncrypt";
            cmdEncrypt.Size = new System.Drawing.Size(142, 48);
            cmdEncrypt.TabIndex = 97;
            cmdEncrypt.Tag = "";
            cmdEncrypt.Text = "Einrichten";
            cmdEncrypt.UseVisualStyleBackColor = true;
            cmdEncrypt.Click += cmdEncrypt_Click;
            // 
            // TabPage2
            // 
            TabPage2.BackColor = System.Drawing.Color.White;
            TabPage2.Controls.Add(chkDoPastBackups);
            TabPage2.Controls.Add(cmdEditSchedule);
            TabPage2.Controls.Add(rbMB);
            TabPage2.Controls.Add(rbFAB);
            TabPage2.Controls.Add(rbTSB);
            TabPage2.Location = new System.Drawing.Point(4, 34);
            TabPage2.Margin = new Padding(4);
            TabPage2.Name = "TabPage2";
            TabPage2.Padding = new Padding(4);
            TabPage2.Size = new System.Drawing.Size(936, 442);
            TabPage2.TabIndex = 1;
            TabPage2.Text = "Modus";
            // 
            // chkDoPastBackups
            // 
            chkDoPastBackups.AutoSize = true;
            chkDoPastBackups.BackColor = System.Drawing.Color.Transparent;
            chkDoPastBackups.Enabled = false;
            chkDoPastBackups.Location = new System.Drawing.Point(204, 257);
            chkDoPastBackups.Margin = new Padding(4);
            chkDoPastBackups.Name = "chkDoPastBackups";
            chkDoPastBackups.Size = new System.Drawing.Size(329, 32);
            chkDoPastBackups.TabIndex = 99;
            chkDoPastBackups.Text = "Verpasste Sicherungen nachholen";
            chkDoPastBackups.UseVisualStyleBackColor = false;
            // 
            // cmdEditSchedule
            // 
            cmdEditSchedule.Enabled = false;
            cmdEditSchedule.Location = new System.Drawing.Point(57, 248);
            cmdEditSchedule.Margin = new Padding(4);
            cmdEditSchedule.Name = "cmdEditSchedule";
            cmdEditSchedule.Size = new System.Drawing.Size(123, 48);
            cmdEditSchedule.TabIndex = 98;
            cmdEditSchedule.Tag = "";
            cmdEditSchedule.Text = "Zeitplan";
            cmdEditSchedule.UseVisualStyleBackColor = true;
            cmdEditSchedule.Click += cmdEditSchedule_Click;
            // 
            // rbMB
            // 
            rbMB.AutoSize = true;
            rbMB.BackColor = System.Drawing.Color.Transparent;
            rbMB.CheckAlign = System.Drawing.ContentAlignment.TopLeft;
            rbMB.Location = new System.Drawing.Point(32, 311);
            rbMB.Margin = new Padding(4);
            rbMB.Name = "rbMB";
            rbMB.Size = new System.Drawing.Size(364, 60);
            rbMB.TabIndex = 94;
            rbMB.Text = "Manuelle Sicherung\r\nSie erstellen die Sicherungen manuell.";
            rbMB.UseVisualStyleBackColor = false;
            // 
            // rbFAB
            // 
            rbFAB.AutoSize = true;
            rbFAB.BackColor = System.Drawing.Color.Transparent;
            rbFAB.CheckAlign = System.Drawing.ContentAlignment.TopLeft;
            rbFAB.Location = new System.Drawing.Point(32, 22);
            rbFAB.Margin = new Padding(4);
            rbFAB.Name = "rbFAB";
            rbFAB.Size = new System.Drawing.Size(878, 116);
            rbFAB.TabIndex = 92;
            rbFAB.Text = resources.GetString("rbFAB.Text");
            rbFAB.UseVisualStyleBackColor = false;
            // 
            // rbTSB
            // 
            rbTSB.AutoSize = true;
            rbTSB.BackColor = System.Drawing.Color.Transparent;
            rbTSB.CheckAlign = System.Drawing.ContentAlignment.TopLeft;
            rbTSB.Location = new System.Drawing.Point(32, 149);
            rbTSB.Margin = new Padding(4);
            rbTSB.Name = "rbTSB";
            rbTSB.Size = new System.Drawing.Size(822, 88);
            rbTSB.TabIndex = 93;
            rbTSB.Text = "Zeitplangesteuerte Sicherung\r\nSie legen fest, wann eine Datensicherung durchgeführt wird und wie lange diese verfügbar \r\nbleiben.";
            rbTSB.UseVisualStyleBackColor = false;
            rbTSB.CheckedChanged += rbTSB_CheckedChanged;
            // 
            // TabPage1
            // 
            TabPage1.BackColor = System.Drawing.Color.White;
            TabPage1.Controls.Add(cmdFilter);
            TabPage1.Controls.Add(lvSource);
            TabPage1.Controls.Add(cmdAddSource);
            TabPage1.Controls.Add(cmdDeleteSource);
            TabPage1.Location = new System.Drawing.Point(4, 37);
            TabPage1.Margin = new Padding(4);
            TabPage1.Name = "TabPage1";
            TabPage1.Padding = new Padding(4);
            TabPage1.Size = new System.Drawing.Size(936, 439);
            TabPage1.TabIndex = 0;
            TabPage1.Text = "Quellverzeichnisse";
            // 
            // cmdFilter
            // 
            cmdFilter.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            cmdFilter.Location = new System.Drawing.Point(676, 374);
            cmdFilter.Margin = new Padding(4);
            cmdFilter.Name = "cmdFilter";
            cmdFilter.Size = new System.Drawing.Size(231, 48);
            cmdFilter.TabIndex = 116;
            cmdFilter.Tag = "";
            cmdFilter.Text = "Dateien ausschließen";
            cmdFilter.UseVisualStyleBackColor = true;
            cmdFilter.Click += cmdFilter_Click;
            // 
            // lvSource
            // 
            lvSource.AllowDrop = true;
            lvSource.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            lvSource.Columns.AddRange(new ColumnHeader[] { ColumnHeader1 });
            lvSource.FullRowSelect = true;
            lvSource.Location = new System.Drawing.Point(22, 20);
            lvSource.Margin = new Padding(4);
            lvSource.Name = "lvSource";
            lvSource.Size = new System.Drawing.Size(885, 337);
            lvSource.SmallImageList = ImageList1;
            lvSource.Sorting = SortOrder.Ascending;
            lvSource.TabIndex = 73;
            lvSource.UseCompatibleStateImageBehavior = false;
            lvSource.View = View.Details;
            lvSource.SelectedIndexChanged += lvSource_SelectedIndexChanged;
            lvSource.DragDrop += lvSource_DragDrop;
            lvSource.DragEnter += lvSource_DragEnter;
            // 
            // ColumnHeader1
            // 
            ColumnHeader1.Text = "Verzeichnis";
            ColumnHeader1.Width = 522;
            // 
            // cmdAddSource
            // 
            cmdAddSource.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            cmdAddSource.Location = new System.Drawing.Point(22, 374);
            cmdAddSource.Margin = new Padding(4);
            cmdAddSource.Name = "cmdAddSource";
            cmdAddSource.Size = new System.Drawing.Size(147, 48);
            cmdAddSource.TabIndex = 74;
            cmdAddSource.Text = "Hinzufügen";
            cmdAddSource.UseVisualStyleBackColor = true;
            cmdAddSource.Click += cmdAddSource_Click;
            // 
            // cmdDeleteSource
            // 
            cmdDeleteSource.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            cmdDeleteSource.Enabled = false;
            cmdDeleteSource.Location = new System.Drawing.Point(177, 374);
            cmdDeleteSource.Margin = new Padding(4);
            cmdDeleteSource.Name = "cmdDeleteSource";
            cmdDeleteSource.Size = new System.Drawing.Size(125, 48);
            cmdDeleteSource.TabIndex = 75;
            cmdDeleteSource.Text = "Löschen";
            cmdDeleteSource.UseVisualStyleBackColor = true;
            cmdDeleteSource.Click += cmdDeleteSource_Click;
            // 
            // tcOptions
            // 
            tcOptions.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            tcOptions.Controls.Add(TabPage1);
            tcOptions.Controls.Add(TabPage5);
            tcOptions.Controls.Add(TabPage2);
            tcOptions.Controls.Add(TabPage3);
            tcOptions.Controls.Add(TabPage4);
            tcOptions.Location = new System.Drawing.Point(27, 33);
            tcOptions.Margin = new Padding(4);
            tcOptions.Name = "tcOptions";
            tcOptions.SelectedIndex = 0;
            tcOptions.Size = new System.Drawing.Size(944, 480);
            tcOptions.TabIndex = 85;
            // 
            // TabPage5
            // 
            TabPage5.BackColor = System.Drawing.Color.White;
            TabPage5.Controls.Add(cmdChangeBackupMedia);
            TabPage5.Controls.Add(Label1);
            TabPage5.Controls.Add(cboMedia);
            TabPage5.Controls.Add(Label2);
            TabPage5.Controls.Add(plDevice);
            TabPage5.Controls.Add(plFTP);
            TabPage5.Location = new System.Drawing.Point(4, 37);
            TabPage5.Margin = new Padding(4);
            TabPage5.Name = "TabPage5";
            TabPage5.Size = new System.Drawing.Size(936, 439);
            TabPage5.TabIndex = 4;
            TabPage5.Text = "Medium";
            // 
            // cmdChangeBackupMedia
            // 
            cmdChangeBackupMedia.Location = new System.Drawing.Point(32, 344);
            cmdChangeBackupMedia.Margin = new Padding(4);
            cmdChangeBackupMedia.Name = "cmdChangeBackupMedia";
            cmdChangeBackupMedia.Size = new System.Drawing.Size(144, 48);
            cmdChangeBackupMedia.TabIndex = 97;
            cmdChangeBackupMedia.Text = "Wechseln";
            cmdChangeBackupMedia.UseVisualStyleBackColor = true;
            cmdChangeBackupMedia.Click += Button1_Click;
            // 
            // Label1
            // 
            Label1.AutoSize = true;
            Label1.BackColor = System.Drawing.Color.Transparent;
            Label1.Location = new System.Drawing.Point(213, 348);
            Label1.Margin = new Padding(4, 0, 4, 0);
            Label1.Name = "Label1";
            Label1.Size = new System.Drawing.Size(518, 56);
            Label1.TabIndex = 96;
            Label1.Text = "Ermöglicht das Wechseln des Sicherungsmediums auf ein \r\nneues (leeres) Medium ohne die Einstellungen zu verlieren.";
            // 
            // cboMedia
            // 
            cboMedia.DropDownStyle = ComboBoxStyle.DropDownList;
            cboMedia.FormattingEnabled = true;
            cboMedia.Items.AddRange(new object[] { "Verzeichnis oder Laufwerk", "Server (FTP)" });
            cboMedia.Location = new System.Drawing.Point(218, 22);
            cboMedia.Margin = new Padding(4);
            cboMedia.Name = "cboMedia";
            cboMedia.Size = new System.Drawing.Size(478, 36);
            cboMedia.TabIndex = 95;
            cboMedia.SelectedIndexChanged += cboMedia_SelectedIndexChanged;
            // 
            // Label2
            // 
            Label2.AutoSize = true;
            Label2.BackColor = System.Drawing.Color.Transparent;
            Label2.Location = new System.Drawing.Point(27, 27);
            Label2.Margin = new Padding(4, 0, 4, 0);
            Label2.Name = "Label2";
            Label2.Size = new System.Drawing.Size(183, 28);
            Label2.TabIndex = 92;
            Label2.Text = "Sicherungsmedium:";
            // 
            // plDevice
            // 
            plDevice.BackColor = System.Drawing.Color.Transparent;
            plDevice.Controls.Add(plUNCAuthentication);
            plDevice.Controls.Add(Label15);
            plDevice.Controls.Add(cmdChange);
            plDevice.Controls.Add(txtBackupPath);
            plDevice.Location = new System.Drawing.Point(32, 69);
            plDevice.Margin = new Padding(4);
            plDevice.Name = "plDevice";
            plDevice.Size = new System.Drawing.Size(872, 188);
            plDevice.TabIndex = 94;
            // 
            // plUNCAuthentication
            // 
            plUNCAuthentication.Controls.Add(Label11);
            plUNCAuthentication.Controls.Add(txtUNCPassword);
            plUNCAuthentication.Controls.Add(Label10);
            plUNCAuthentication.Controls.Add(txtUNCUsername);
            plUNCAuthentication.Location = new System.Drawing.Point(40, 52);
            plUNCAuthentication.Margin = new Padding(4);
            plUNCAuthentication.Name = "plUNCAuthentication";
            plUNCAuthentication.Size = new System.Drawing.Size(696, 90);
            plUNCAuthentication.TabIndex = 85;
            // 
            // Label11
            // 
            Label11.AutoSize = true;
            Label11.Location = new System.Drawing.Point(4, 46);
            Label11.Margin = new Padding(4, 0, 4, 0);
            Label11.Name = "Label11";
            Label11.Size = new System.Drawing.Size(100, 28);
            Label11.TabIndex = 84;
            Label11.Text = "Kennwort:";
            // 
            // txtUNCPassword
            // 
            txtUNCPassword.BackColor = System.Drawing.Color.White;
            txtUNCPassword.Location = new System.Drawing.Point(146, 42);
            txtUNCPassword.Margin = new Padding(4);
            txtUNCPassword.Name = "txtUNCPassword";
            txtUNCPassword.Size = new System.Drawing.Size(548, 33);
            txtUNCPassword.TabIndex = 83;
            txtUNCPassword.UseSystemPasswordChar = true;
            // 
            // Label10
            // 
            Label10.AutoSize = true;
            Label10.Location = new System.Drawing.Point(4, 8);
            Label10.Margin = new Padding(4, 0, 4, 0);
            Label10.Name = "Label10";
            Label10.Size = new System.Drawing.Size(140, 28);
            Label10.TabIndex = 82;
            Label10.Text = "Benutzername:";
            // 
            // txtUNCUsername
            // 
            txtUNCUsername.BackColor = System.Drawing.Color.White;
            txtUNCUsername.Location = new System.Drawing.Point(146, 3);
            txtUNCUsername.Margin = new Padding(4);
            txtUNCUsername.Name = "txtUNCUsername";
            txtUNCUsername.Size = new System.Drawing.Size(548, 33);
            txtUNCUsername.TabIndex = 81;
            // 
            // Label15
            // 
            Label15.AutoSize = true;
            Label15.Location = new System.Drawing.Point(45, 10);
            Label15.Margin = new Padding(4, 0, 4, 0);
            Label15.Name = "Label15";
            Label15.Size = new System.Drawing.Size(117, 28);
            Label15.TabIndex = 80;
            Label15.Text = "Speicherort:";
            // 
            // cmdChange
            // 
            cmdChange.Location = new System.Drawing.Point(746, 6);
            cmdChange.Margin = new Padding(4);
            cmdChange.Name = "cmdChange";
            cmdChange.Size = new System.Drawing.Size(122, 48);
            cmdChange.TabIndex = 79;
            cmdChange.Tag = "";
            cmdChange.Text = "&Ändern";
            cmdChange.UseVisualStyleBackColor = true;
            cmdChange.Click += cmdChange_Click;
            // 
            // txtBackupPath
            // 
            txtBackupPath.BackColor = System.Drawing.Color.White;
            txtBackupPath.Location = new System.Drawing.Point(186, 6);
            txtBackupPath.Margin = new Padding(4);
            txtBackupPath.Name = "txtBackupPath";
            txtBackupPath.ReadOnly = true;
            txtBackupPath.Size = new System.Drawing.Size(548, 33);
            txtBackupPath.TabIndex = 78;
            txtBackupPath.TextChanged += txtBackupPath_TextChanged;
            // 
            // plFTP
            // 
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
            plFTP.Location = new System.Drawing.Point(32, 69);
            plFTP.Margin = new Padding(4);
            plFTP.Name = "plFTP";
            plFTP.Size = new System.Drawing.Size(872, 228);
            plFTP.TabIndex = 93;
            plFTP.Visible = false;
            // 
            // chkFtpEncryption
            // 
            chkFtpEncryption.Location = new System.Drawing.Point(457, 181);
            chkFtpEncryption.Name = "chkFtpEncryption";
            chkFtpEncryption.Size = new System.Drawing.Size(411, 40);
            chkFtpEncryption.TabIndex = 94;
            chkFtpEncryption.Text = "Unverschlüsselte Verbindung erzwingen";
            chkFtpEncryption.UseVisualStyleBackColor = true;
            // 
            // Label12
            // 
            Label12.AutoSize = true;
            Label12.Location = new System.Drawing.Point(45, 186);
            Label12.Margin = new Padding(4, 0, 4, 0);
            Label12.Name = "Label12";
            Label12.Size = new System.Drawing.Size(98, 28);
            Label12.TabIndex = 93;
            Label12.Text = "Encoding:";
            // 
            // cboFtpEncoding
            // 
            cboFtpEncoding.DropDownStyle = ComboBoxStyle.DropDownList;
            cboFtpEncoding.FormattingEnabled = true;
            cboFtpEncoding.Items.AddRange(new object[] { "ISO-8859-1", "UTF-8" });
            cboFtpEncoding.Location = new System.Drawing.Point(186, 178);
            cboFtpEncoding.Name = "cboFtpEncoding";
            cboFtpEncoding.Size = new System.Drawing.Size(248, 36);
            cboFtpEncoding.TabIndex = 92;
            // 
            // cmdFTPCheck
            // 
            cmdFTPCheck.Location = new System.Drawing.Point(675, 6);
            cmdFTPCheck.Margin = new Padding(4);
            cmdFTPCheck.Name = "cmdFTPCheck";
            cmdFTPCheck.Size = new System.Drawing.Size(122, 34);
            cmdFTPCheck.TabIndex = 91;
            cmdFTPCheck.Text = "&Testen";
            cmdFTPCheck.UseVisualStyleBackColor = true;
            cmdFTPCheck.Click += cmdFTPCheck_Click;
            // 
            // txtFTPPort
            // 
            txtFTPPort.Location = new System.Drawing.Point(592, 6);
            txtFTPPort.Margin = new Padding(4);
            txtFTPPort.Name = "txtFTPPort";
            txtFTPPort.Size = new System.Drawing.Size(72, 33);
            txtFTPPort.TabIndex = 90;
            // 
            // Label8
            // 
            Label8.AutoSize = true;
            Label8.Location = new System.Drawing.Point(536, 10);
            Label8.Margin = new Padding(4, 0, 4, 0);
            Label8.Name = "Label8";
            Label8.Size = new System.Drawing.Size(52, 28);
            Label8.TabIndex = 89;
            Label8.Text = "Port:";
            // 
            // txtFTPPath
            // 
            txtFTPPath.Location = new System.Drawing.Point(186, 138);
            txtFTPPath.Margin = new Padding(4);
            txtFTPPath.Name = "txtFTPPath";
            txtFTPPath.Size = new System.Drawing.Size(478, 33);
            txtFTPPath.TabIndex = 88;
            // 
            // Label7
            // 
            Label7.AutoSize = true;
            Label7.Location = new System.Drawing.Point(45, 142);
            Label7.Margin = new Padding(4, 0, 4, 0);
            Label7.Name = "Label7";
            Label7.Size = new System.Drawing.Size(112, 28);
            Label7.TabIndex = 87;
            Label7.Text = "Verzeichnis:";
            // 
            // txtFTPPassword
            // 
            txtFTPPassword.Location = new System.Drawing.Point(186, 94);
            txtFTPPassword.Margin = new Padding(4);
            txtFTPPassword.Name = "txtFTPPassword";
            txtFTPPassword.Size = new System.Drawing.Size(478, 33);
            txtFTPPassword.TabIndex = 86;
            txtFTPPassword.UseSystemPasswordChar = true;
            // 
            // Label5
            // 
            Label5.AutoSize = true;
            Label5.Location = new System.Drawing.Point(45, 99);
            Label5.Margin = new Padding(4, 0, 4, 0);
            Label5.Name = "Label5";
            Label5.Size = new System.Drawing.Size(100, 28);
            Label5.TabIndex = 85;
            Label5.Text = "Kennwort:";
            // 
            // txtFTPUsername
            // 
            txtFTPUsername.Location = new System.Drawing.Point(186, 51);
            txtFTPUsername.Margin = new Padding(4);
            txtFTPUsername.Name = "txtFTPUsername";
            txtFTPUsername.Size = new System.Drawing.Size(478, 33);
            txtFTPUsername.TabIndex = 84;
            // 
            // Label4
            // 
            Label4.AutoSize = true;
            Label4.Location = new System.Drawing.Point(45, 56);
            Label4.Margin = new Padding(4, 0, 4, 0);
            Label4.Name = "Label4";
            Label4.Size = new System.Drawing.Size(140, 28);
            Label4.TabIndex = 83;
            Label4.Text = "Benutzername:";
            // 
            // txtFTPServer
            // 
            txtFTPServer.Location = new System.Drawing.Point(186, 6);
            txtFTPServer.Margin = new Padding(4);
            txtFTPServer.Name = "txtFTPServer";
            txtFTPServer.Size = new System.Drawing.Size(338, 33);
            txtFTPServer.TabIndex = 82;
            // 
            // Label3
            // 
            Label3.AutoSize = true;
            Label3.Location = new System.Drawing.Point(45, 10);
            Label3.Margin = new Padding(4, 0, 4, 0);
            Label3.Name = "Label3";
            Label3.Size = new System.Drawing.Size(71, 28);
            Label3.TabIndex = 81;
            Label3.Text = "Server:";
            // 
            // cmdOK
            // 
            cmdOK.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            cmdOK.Location = new System.Drawing.Point(801, 512);
            cmdOK.Margin = new Padding(4);
            cmdOK.Name = "cmdOK";
            cmdOK.Size = new System.Drawing.Size(166, 48);
            cmdOK.TabIndex = 86;
            cmdOK.Text = "&Speichern";
            cmdOK.UseVisualStyleBackColor = true;
            cmdOK.Click += cmdOK_Click;
            // 
            // plExcludeCompress
            // 
            plExcludeCompress.BackColor = System.Drawing.Color.White;
            plExcludeCompress.BorderStyle = BorderStyle.FixedSingle;
            plExcludeCompress.Controls.Add(cmdCloseExcludeCompress);
            plExcludeCompress.Controls.Add(cmdDeleteExcludeCompress);
            plExcludeCompress.Controls.Add(cmdAddExcludeCompress);
            plExcludeCompress.Controls.Add(lstExcludeCompress);
            plExcludeCompress.Controls.Add(Label6);
            plExcludeCompress.Location = new System.Drawing.Point(98, 27);
            plExcludeCompress.Margin = new Padding(4);
            plExcludeCompress.Name = "plExcludeCompress";
            plExcludeCompress.Size = new System.Drawing.Size(800, 436);
            plExcludeCompress.TabIndex = 87;
            plExcludeCompress.Visible = false;
            // 
            // cmdCloseExcludeCompress
            // 
            cmdCloseExcludeCompress.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            cmdCloseExcludeCompress.Location = new System.Drawing.Point(650, 364);
            cmdCloseExcludeCompress.Margin = new Padding(4);
            cmdCloseExcludeCompress.Name = "cmdCloseExcludeCompress";
            cmdCloseExcludeCompress.Size = new System.Drawing.Size(112, 48);
            cmdCloseExcludeCompress.TabIndex = 5;
            cmdCloseExcludeCompress.Text = "&OK";
            cmdCloseExcludeCompress.UseVisualStyleBackColor = true;
            cmdCloseExcludeCompress.Click += cmdCloseExcludeCompress_Click;
            // 
            // cmdDeleteExcludeCompress
            // 
            cmdDeleteExcludeCompress.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            cmdDeleteExcludeCompress.Enabled = false;
            cmdDeleteExcludeCompress.Location = new System.Drawing.Point(191, 364);
            cmdDeleteExcludeCompress.Margin = new Padding(4);
            cmdDeleteExcludeCompress.Name = "cmdDeleteExcludeCompress";
            cmdDeleteExcludeCompress.Size = new System.Drawing.Size(150, 48);
            cmdDeleteExcludeCompress.TabIndex = 4;
            cmdDeleteExcludeCompress.Text = "Löschen";
            cmdDeleteExcludeCompress.UseVisualStyleBackColor = true;
            cmdDeleteExcludeCompress.Click += cmdDeleteExcludeCompress_Click;
            // 
            // cmdAddExcludeCompress
            // 
            cmdAddExcludeCompress.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            cmdAddExcludeCompress.Location = new System.Drawing.Point(33, 364);
            cmdAddExcludeCompress.Margin = new Padding(4);
            cmdAddExcludeCompress.Name = "cmdAddExcludeCompress";
            cmdAddExcludeCompress.Size = new System.Drawing.Size(150, 48);
            cmdAddExcludeCompress.TabIndex = 3;
            cmdAddExcludeCompress.Text = "Hinzufügen";
            cmdAddExcludeCompress.UseVisualStyleBackColor = true;
            cmdAddExcludeCompress.Click += cmdAddExcludeCompress_Click;
            // 
            // lstExcludeCompress
            // 
            lstExcludeCompress.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            lstExcludeCompress.FormattingEnabled = true;
            lstExcludeCompress.IntegralHeight = false;
            lstExcludeCompress.ItemHeight = 28;
            lstExcludeCompress.Location = new System.Drawing.Point(33, 76);
            lstExcludeCompress.Margin = new Padding(4);
            lstExcludeCompress.Name = "lstExcludeCompress";
            lstExcludeCompress.Size = new System.Drawing.Size(727, 262);
            lstExcludeCompress.Sorted = true;
            lstExcludeCompress.TabIndex = 2;
            lstExcludeCompress.SelectedIndexChanged += lstExcludeCompress_SelectedIndexChanged;
            // 
            // Label6
            // 
            Label6.AutoSize = true;
            Label6.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            Label6.ForeColor = System.Drawing.Color.FromArgb(0, 51, 153);
            Label6.Location = new System.Drawing.Point(27, 27);
            Label6.Margin = new Padding(4, 0, 4, 0);
            Label6.Name = "Label6";
            Label6.Size = new System.Drawing.Size(465, 32);
            Label6.TabIndex = 1;
            Label6.Text = "Dateitypen von Kompression ausschließen";
            // 
            // ucConfig
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(144F, 144F);
            AutoScaleMode = AutoScaleMode.Dpi;
            AutoScroll = true;
            Controls.Add(plExcludeCompress);
            Controls.Add(cmdOK);
            Controls.Add(tcOptions);
            DoubleBuffered = true;
            Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            Margin = new Padding(4);
            Name = "ucConfig";
            Size = new System.Drawing.Size(1004, 588);
            TabPage4.ResumeLayout(false);
            TabPage4.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)nudRemind).EndInit();
            ((System.ComponentModel.ISupportInitialize)txtRemindSpace).EndInit();
            TabPage3.ResumeLayout(false);
            plCompressEncrypt.ResumeLayout(false);
            plCompressEncrypt.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)tbCompressionLevel).EndInit();
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