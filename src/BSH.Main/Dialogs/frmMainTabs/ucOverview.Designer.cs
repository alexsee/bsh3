using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace Brightbits.BSH.Main
{
    public partial class ucOverview : UserControl
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
            var resources = new System.ComponentModel.ComponentResourceManager(typeof(ucOverview));
            lblBdStatus = new Label();
            Label4 = new Label();
            lblBdOldestBackup = new Label();
            Label2 = new Label();
            lblBdNewestBackup = new Label();
            lblOldBackup = new Label();
            lblBdSpaceAvailable = new Label();
            pbStatus = new ProgressBar();
            plStatus = new Panel();
            cmdBackupCancel = new PictureBox();
            cmdBackupCancel.Click += new EventHandler(cmdBackupCancel_Click);
            Label7 = new Label();
            lblNextBackup = new Label();
            plUpdates = new Panel();
            btnUpdates = new Button();
#if !WIN_UWP
            btnUpdates.Click += new EventHandler(btnUpdates_Click);
#endif
            Label3 = new Label();
            plBottom = new Panel();
            Label5 = new Label();
            lblInfo = new Label();
            Label1 = new Label();
            lblBackupMode = new Label();
            ttMain = new ToolTip(components);
            btnOnOff = new PictureBox();
            btnOnOff.Click += new EventHandler(btnOnOff_Click);
            cmdBackupNow = new PictureBox();
            cmdBackupNow.MouseClick += new MouseEventHandler(cmdBackupNow_MouseClick);
            cmdBackupNow.MouseEnter += new EventHandler(cmdBackupNow_MouseEnter);
            cmdBackupNow.MouseLeave += new EventHandler(cmdBackupNow_MouseLeave);
            cmdBackupNow.EnabledChanged += new EventHandler(cmdBackupNow_EnabledChanged);
            btnSettings = new PictureBox();
            btnSettings.Click += new EventHandler(btnSettings_Click);
            btnSettings.MouseLeave += new EventHandler(btnSettings_MouseLeave);
            btnSettings.MouseEnter += new EventHandler(btnSettings_MouseEnter);
            btnSettings.EnabledChanged += new EventHandler(btnSettings_EnabledChanged);
            loadingCircle = new MRG.Controls.UI.LoadingCircle();
            FlowLayoutPanel1 = new FlowLayoutPanel();
            Label6 = new Label();
            llOptions = new LinkLabel();
            llOptions.MouseLeave += new EventHandler(btnSettings_MouseLeave);
            llOptions.MouseEnter += new EventHandler(btnSettings_MouseEnter);
            llOptions.LinkClicked += new LinkLabelLinkClickedEventHandler(llOptions_LinkClicked);
            llBackup = new LinkLabel();
            llBackup.MouseEnter += new EventHandler(cmdBackupNow_MouseEnter);
            llBackup.MouseLeave += new EventHandler(cmdBackupNow_MouseLeave);
            llBackup.LinkClicked += new LinkLabelLinkClickedEventHandler(llBackup_LinkClicked);
            picDataType = new PictureBox();
            llShowExceptionDialog = new LinkLabel();
            llShowExceptionDialog.LinkClicked += new LinkLabelLinkClickedEventHandler(llShowExceptionDialog_LinkClicked);
            Panel1 = new Panel();
            plStatus.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)cmdBackupCancel).BeginInit();
            plUpdates.SuspendLayout();
            plBottom.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)btnOnOff).BeginInit();
            ((System.ComponentModel.ISupportInitialize)cmdBackupNow).BeginInit();
            ((System.ComponentModel.ISupportInitialize)btnSettings).BeginInit();
            FlowLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)picDataType).BeginInit();
            Panel1.SuspendLayout();
            SuspendLayout();
            // 
            // lblBdStatus
            // 
            lblBdStatus.BackColor = Color.Transparent;
            lblBdStatus.Font = new Font("Segoe UI Semilight", 14.25f, FontStyle.Regular, GraphicsUnit.Point);
            lblBdStatus.ForeColor = Color.Black;
            lblBdStatus.Location = new Point(141, 36);
            lblBdStatus.Margin = new Padding(4, 0, 4, 0);
            lblBdStatus.Name = "lblBdStatus";
            lblBdStatus.Size = new Size(850, 38);
            lblBdStatus.TabIndex = 70;
            lblBdStatus.Text = "Backup Service Home wird ordnungsgemäß ausgeführt.";
            // 
            // Label4
            // 
            Label4.AutoSize = true;
            Label4.BackColor = Color.Transparent;
            Label4.Font = new Font("Segoe UI", 9.75f, FontStyle.Regular, GraphicsUnit.Point);
            Label4.ForeColor = Color.Black;
            Label4.Location = new Point(144, 183);
            Label4.Margin = new Padding(4, 0, 4, 0);
            Label4.Name = "Label4";
            Label4.Size = new Size(144, 28);
            Label4.TabIndex = 61;
            Label4.Text = "Letztes Backup:";
            // 
            // lblBdOldestBackup
            // 
            lblBdOldestBackup.BackColor = Color.Transparent;
            lblBdOldestBackup.Font = new Font("Segoe UI", 9.75f, FontStyle.Regular, GraphicsUnit.Point);
            lblBdOldestBackup.ForeColor = Color.Black;
            lblBdOldestBackup.Location = new Point(368, 138);
            lblBdOldestBackup.Margin = new Padding(4, 0, 4, 0);
            lblBdOldestBackup.Name = "lblBdOldestBackup";
            lblBdOldestBackup.Size = new Size(624, 27);
            lblBdOldestBackup.TabIndex = 68;
            lblBdOldestBackup.Text = "--";
            // 
            // Label2
            // 
            Label2.AutoSize = true;
            Label2.BackColor = Color.Transparent;
            Label2.Font = new Font("Segoe UI", 9.75f, FontStyle.Regular, GraphicsUnit.Point);
            Label2.ForeColor = Color.Black;
            Label2.Location = new Point(144, 93);
            Label2.Margin = new Padding(4, 0, 4, 0);
            Label2.Name = "Label2";
            Label2.Size = new Size(102, 28);
            Label2.TabIndex = 59;
            Label2.Text = "Verfügbar:";
            // 
            // lblBdNewestBackup
            // 
            lblBdNewestBackup.AutoSize = true;
            lblBdNewestBackup.BackColor = Color.Transparent;
            lblBdNewestBackup.Dock = DockStyle.Left;
            lblBdNewestBackup.Font = new Font("Segoe UI", 9.75f, FontStyle.Regular, GraphicsUnit.Point);
            lblBdNewestBackup.ForeColor = Color.Black;
            lblBdNewestBackup.Location = new Point(0, 0);
            lblBdNewestBackup.Margin = new Padding(0);
            lblBdNewestBackup.Name = "lblBdNewestBackup";
            lblBdNewestBackup.Size = new Size(28, 28);
            lblBdNewestBackup.TabIndex = 69;
            lblBdNewestBackup.Text = "--";
            // 
            // lblOldBackup
            // 
            lblOldBackup.AutoSize = true;
            lblOldBackup.BackColor = Color.Transparent;
            lblOldBackup.Font = new Font("Segoe UI", 9.75f, FontStyle.Regular, GraphicsUnit.Point);
            lblOldBackup.ForeColor = Color.Black;
            lblOldBackup.Location = new Point(144, 138);
            lblOldBackup.Margin = new Padding(4, 0, 4, 0);
            lblOldBackup.Name = "lblOldBackup";
            lblOldBackup.Size = new Size(193, 28);
            lblOldBackup.TabIndex = 60;
            lblOldBackup.Text = "Voraussichtl. voll am:";
            // 
            // lblBdSpaceAvailable
            // 
            lblBdSpaceAvailable.BackColor = Color.Transparent;
            lblBdSpaceAvailable.Font = new Font("Segoe UI", 9.75f, FontStyle.Regular, GraphicsUnit.Point);
            lblBdSpaceAvailable.ForeColor = Color.Black;
            lblBdSpaceAvailable.Location = new Point(368, 93);
            lblBdSpaceAvailable.Margin = new Padding(4, 0, 4, 0);
            lblBdSpaceAvailable.Name = "lblBdSpaceAvailable";
            lblBdSpaceAvailable.Size = new Size(624, 27);
            lblBdSpaceAvailable.TabIndex = 67;
            lblBdSpaceAvailable.Text = "--";
            // 
            // pbStatus
            // 
            pbStatus.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;

            pbStatus.Location = new Point(0, 0);
            pbStatus.Margin = new Padding(4);
            pbStatus.Name = "pbStatus";
            pbStatus.Size = new Size(810, 30);
            pbStatus.TabIndex = 76;
            // 
            // plStatus
            // 
            plStatus.Controls.Add(pbStatus);
            plStatus.Controls.Add(cmdBackupCancel);
            plStatus.Location = new Point(148, 93);
            plStatus.Margin = new Padding(4);
            plStatus.Name = "plStatus";
            plStatus.Size = new Size(843, 30);
            plStatus.TabIndex = 78;
            plStatus.Visible = false;
            // 
            // cmdBackupCancel
            // 
            cmdBackupCancel.Cursor = Cursors.Hand;
            cmdBackupCancel.Dock = DockStyle.Right;
            cmdBackupCancel.Image = global::BSH.Main.Properties.Resources.close_circle_line;
            cmdBackupCancel.Location = new Point(819, 0);
            cmdBackupCancel.Margin = new Padding(4);
            cmdBackupCancel.Name = "cmdBackupCancel";
            cmdBackupCancel.Size = new Size(24, 30);
            cmdBackupCancel.SizeMode = PictureBoxSizeMode.Zoom;
            cmdBackupCancel.TabIndex = 77;
            cmdBackupCancel.TabStop = false;
            cmdBackupCancel.Tag = "false";
            ttMain.SetToolTip(cmdBackupCancel, "Bricht die aktuelle Datensicherung ab. Alle Änderungen auf dem Backupmedium" + '\r' + '\n' + "werd" + "en rückgängig gemacht.");
            // 
            // Label7
            // 
            Label7.AutoSize = true;
            Label7.BackColor = Color.Transparent;
            Label7.Font = new Font("Segoe UI", 9.75f, FontStyle.Regular, GraphicsUnit.Point);
            Label7.ForeColor = Color.Black;
            Label7.Location = new Point(144, 228);
            Label7.Margin = new Padding(4, 0, 4, 0);
            Label7.Name = "Label7";
            Label7.Size = new Size(162, 28);
            Label7.TabIndex = 97;
            Label7.Text = "Nächstes Backup:";
            // 
            // lblNextBackup
            // 
            lblNextBackup.BackColor = Color.Transparent;
            lblNextBackup.Font = new Font("Segoe UI", 9.75f, FontStyle.Regular, GraphicsUnit.Point);
            lblNextBackup.ForeColor = Color.Black;
            lblNextBackup.Location = new Point(368, 228);
            lblNextBackup.Margin = new Padding(4, 0, 4, 0);
            lblNextBackup.Name = "lblNextBackup";
            lblNextBackup.Size = new Size(624, 27);
            lblNextBackup.TabIndex = 98;
            lblNextBackup.Text = "--";
            // 
            // plUpdates
            // 
            plUpdates.BackColor = Color.FromArgb(255, 192, 192);
            plUpdates.Controls.Add(btnUpdates);
            plUpdates.Controls.Add(Label3);
            plUpdates.Location = new Point(0, 153);
            plUpdates.Margin = new Padding(0);
            plUpdates.Name = "plUpdates";
            plUpdates.Size = new Size(1050, 51);
            plUpdates.TabIndex = 96;
            plUpdates.Visible = false;
            // 
            // btnUpdates
            // 
            btnUpdates.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnUpdates.BackColor = Color.Transparent;
            btnUpdates.Location = new Point(884, 6);
            btnUpdates.Margin = new Padding(4);
            btnUpdates.Name = "btnUpdates";
            btnUpdates.Size = new Size(134, 39);
            btnUpdates.TabIndex = 75;
            btnUpdates.Text = "Installieren";
            ttMain.SetToolTip(btnUpdates, "Installiert wichtige Aktualisierungen.");
            btnUpdates.UseVisualStyleBackColor = false;
            // 
            // Label3
            // 
            Label3.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            Label3.Font = new Font("Segoe UI", 9.75f, FontStyle.Bold, GraphicsUnit.Point);
            Label3.Location = new Point(38, 14);
            Label3.Margin = new Padding(4, 0, 4, 0);
            Label3.Name = "Label3";
            Label3.Size = new Size(792, 26);
            Label3.TabIndex = 0;
            Label3.Text = "Es sind Aktualisierungen verfügbar.";
            Label3.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // plBottom
            // 
            plBottom.BackColor = Color.White;
            plBottom.Controls.Add(Label5);
            plBottom.Controls.Add(lblInfo);
            plBottom.Location = new Point(0, 0);
            plBottom.Margin = new Padding(0);
            plBottom.Name = "plBottom";
            plBottom.Size = new Size(1050, 153);
            plBottom.TabIndex = 95;
            // 
            // Label5
            // 
            Label5.Font = new Font("Segoe UI", 9.75f, FontStyle.Bold, GraphicsUnit.Point);
            Label5.Location = new Point(42, 24);
            Label5.Margin = new Padding(4, 0, 4, 0);
            Label5.Name = "Label5";
            Label5.Size = new Size(327, 38);
            Label5.TabIndex = 1;
            Label5.Text = "Sicherungseinstellungen:";
            // 
            // lblInfo
            // 
            lblInfo.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            lblInfo.Font = new Font("Segoe UI", 9.75f, FontStyle.Regular, GraphicsUnit.Point);
            lblInfo.Location = new Point(42, 66);
            lblInfo.Margin = new Padding(4, 0, 4, 0);
            lblInfo.Name = "lblInfo";
            lblInfo.Size = new Size(950, 62);
            lblInfo.TabIndex = 0;
            // 
            // Label1
            // 
            Label1.AutoSize = true;
            Label1.BackColor = Color.Transparent;
            Label1.Font = new Font("Segoe UI", 9.75f, FontStyle.Regular, GraphicsUnit.Point);
            Label1.ForeColor = Color.Black;
            Label1.Location = new Point(144, 273);
            Label1.Margin = new Padding(4, 0, 4, 0);
            Label1.Name = "Label1";
            Label1.Size = new Size(139, 28);
            Label1.TabIndex = 83;
            Label1.Text = "Backupmodus:";
            // 
            // lblBackupMode
            // 
            lblBackupMode.BackColor = Color.Transparent;
            lblBackupMode.Font = new Font("Segoe UI", 9.75f, FontStyle.Regular, GraphicsUnit.Point);
            lblBackupMode.ForeColor = Color.Black;
            lblBackupMode.Location = new Point(368, 273);
            lblBackupMode.Margin = new Padding(4, 0, 4, 0);
            lblBackupMode.Name = "lblBackupMode";
            lblBackupMode.Size = new Size(624, 27);
            lblBackupMode.TabIndex = 84;
            lblBackupMode.Text = "--";
            // 
            // ttMain
            // 
            ttMain.ToolTipIcon = ToolTipIcon.Info;
            ttMain.ToolTipTitle = "Quickinfo";
            // 
            // btnOnOff
            // 
            btnOnOff.Cursor = Cursors.Hand;
            btnOnOff.Image = (Image)resources.GetObject("btnOnOff.Image");
            btnOnOff.Location = new Point(42, 150);
            btnOnOff.Margin = new Padding(4);
            btnOnOff.Name = "btnOnOff";
            btnOnOff.Size = new Size(90, 33);
            btnOnOff.SizeMode = PictureBoxSizeMode.Zoom;
            btnOnOff.TabIndex = 101;
            btnOnOff.TabStop = false;
            btnOnOff.Tag = "OFF";
            ttMain.SetToolTip(btnOnOff, "Datensicherung ein- / ausschalten");
            // 
            // cmdBackupNow
            // 
            cmdBackupNow.Cursor = Cursors.Hand;
            cmdBackupNow.Image = global::BSH.Main.Properties.Resources.file_copy_2_line;
            cmdBackupNow.Location = new Point(470, 321);
            cmdBackupNow.Margin = new Padding(4);
            cmdBackupNow.Name = "cmdBackupNow";
            cmdBackupNow.Size = new Size(38, 38);
            cmdBackupNow.SizeMode = PictureBoxSizeMode.Zoom;
            cmdBackupNow.TabIndex = 100;
            cmdBackupNow.TabStop = false;
            ttMain.SetToolTip(cmdBackupNow, "Datensicherung starten");
            // 
            // btnSettings
            // 
            btnSettings.Cursor = Cursors.Hand;
            btnSettings.Image = (Image)resources.GetObject("btnSettings.Image");
            btnSettings.Location = new Point(148, 321);
            btnSettings.Margin = new Padding(4);
            btnSettings.Name = "btnSettings";
            btnSettings.Size = new Size(38, 38);
            btnSettings.SizeMode = PictureBoxSizeMode.Zoom;
            btnSettings.TabIndex = 99;
            btnSettings.TabStop = false;
            ttMain.SetToolTip(btnSettings, "Einstellungen");
            // 
            // loadingCircle
            // 
            loadingCircle.Active = true;
            loadingCircle.Color = Color.FromArgb(48, 95, 169);
            loadingCircle.InnerCircleRadius = 8;
            loadingCircle.Location = new Point(42, 20);
            loadingCircle.Margin = new Padding(4);
            loadingCircle.Name = "loadingCircle";
            loadingCircle.NumberSpoke = 24;
            loadingCircle.OuterCircleRadius = 9;
            loadingCircle.RotationSpeed = 20;
            loadingCircle.Size = new Size(90, 78);
            loadingCircle.SpokeThickness = 4;
            loadingCircle.StylePreset = MRG.Controls.UI.LoadingCircle.StylePresets.IE7;
            loadingCircle.TabIndex = 102;
            loadingCircle.Visible = false;
            // 
            // FlowLayoutPanel1
            // 
            FlowLayoutPanel1.BackColor = Color.White;
            FlowLayoutPanel1.Controls.Add(plUpdates);
            FlowLayoutPanel1.Controls.Add(plBottom);
            FlowLayoutPanel1.Dock = DockStyle.Bottom;
            FlowLayoutPanel1.FlowDirection = FlowDirection.BottomUp;
            FlowLayoutPanel1.Location = new Point(0, 356);
            FlowLayoutPanel1.Margin = new Padding(0);
            FlowLayoutPanel1.Name = "FlowLayoutPanel1";
            FlowLayoutPanel1.Size = new Size(1050, 204);
            FlowLayoutPanel1.TabIndex = 103;
            FlowLayoutPanel1.WrapContents = false;
            // 
            // Label6
            // 
            Label6.BackColor = Color.Transparent;
            Label6.Font = new Font("Segoe UI", 9.75f, FontStyle.Regular, GraphicsUnit.Point);
            Label6.ForeColor = Color.Black;
            Label6.Location = new Point(42, 118);
            Label6.Margin = new Padding(4, 0, 4, 0);
            Label6.Name = "Label6";
            Label6.Size = new Size(90, 27);
            Label6.TabIndex = 104;
            Label6.Text = "Aus / Ein";
            Label6.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // llOptions
            // 
            llOptions.AutoSize = true;
            llOptions.Location = new Point(195, 326);
            llOptions.Margin = new Padding(4, 0, 4, 0);
            llOptions.Name = "llOptions";
            llOptions.Size = new Size(188, 28);
            llOptions.TabIndex = 105;
            llOptions.TabStop = true;
            llOptions.Text = "Einstellungen ändern";
            // 
            // llBackup
            // 
            llBackup.AutoSize = true;
            llBackup.Location = new Point(516, 326);
            llBackup.Margin = new Padding(4, 0, 4, 0);
            llBackup.Name = "llBackup";
            llBackup.Size = new Size(242, 28);
            llBackup.TabIndex = 106;
            llBackup.TabStop = true;
            llBackup.Text = "Manuelle Sicherung starten";
            // 
            // picDataType
            // 
            picDataType.Image = global::BSH.Main.Properties.Resources.status_ok;
            picDataType.Location = new Point(42, 26);
            picDataType.Margin = new Padding(4);
            picDataType.Name = "picDataType";
            picDataType.Size = new Size(90, 68);
            picDataType.SizeMode = PictureBoxSizeMode.Zoom;
            picDataType.TabIndex = 63;
            picDataType.TabStop = false;
            // 
            // llShowExceptionDialog
            // 
            llShowExceptionDialog.AutoSize = true;
            llShowExceptionDialog.Dock = DockStyle.Left;
            llShowExceptionDialog.Location = new Point(28, 0);
            llShowExceptionDialog.Name = "llShowExceptionDialog";
            llShowExceptionDialog.Size = new Size(156, 28);
            llShowExceptionDialog.TabIndex = 107;
            llShowExceptionDialog.TabStop = true;
            llShowExceptionDialog.Text = "(Fehler anzeigen)";
            llShowExceptionDialog.Visible = false;
            // 
            // Panel1
            // 
            Panel1.Controls.Add(llShowExceptionDialog);
            Panel1.Controls.Add(lblBdNewestBackup);
            Panel1.Location = new Point(368, 183);
            Panel1.Margin = new Padding(0);
            Panel1.Name = "Panel1";
            Panel1.Size = new Size(619, 28);
            Panel1.TabIndex = 108;
            // 
            // ucOverview
            // 
            AutoScaleDimensions = new SizeF(144.0f, 144.0f);
            AutoScaleMode = AutoScaleMode.Dpi;
            BackColor = Color.White;
            Controls.Add(Panel1);
            Controls.Add(llBackup);
            Controls.Add(llOptions);
            Controls.Add(Label6);
            Controls.Add(FlowLayoutPanel1);
            Controls.Add(plStatus);
            Controls.Add(loadingCircle);
            Controls.Add(btnOnOff);
            Controls.Add(cmdBackupNow);
            Controls.Add(btnSettings);
            Controls.Add(Label7);
            Controls.Add(lblNextBackup);
            Controls.Add(picDataType);
            Controls.Add(lblBdSpaceAvailable);
            Controls.Add(lblOldBackup);
            Controls.Add(Label1);
            Controls.Add(Label2);
            Controls.Add(lblBackupMode);
            Controls.Add(lblBdOldestBackup);
            Controls.Add(Label4);
            Controls.Add(lblBdStatus);
            DoubleBuffered = true;
            Font = new Font("Segoe UI Light", 9.75f, FontStyle.Regular, GraphicsUnit.Point);
            Margin = new Padding(4);
            Name = "ucOverview";
            Size = new Size(1050, 560);
            plStatus.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)cmdBackupCancel).EndInit();
            plUpdates.ResumeLayout(false);
            plBottom.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)btnOnOff).EndInit();
            ((System.ComponentModel.ISupportInitialize)cmdBackupNow).EndInit();
            ((System.ComponentModel.ISupportInitialize)btnSettings).EndInit();
            FlowLayoutPanel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)picDataType).EndInit();
            Panel1.ResumeLayout(false);
            Panel1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        internal PictureBox picDataType;
        internal Label lblBdStatus;
        internal Label Label4;
        internal Label lblBdOldestBackup;
        internal Label Label2;
        internal Label lblBdNewestBackup;
        internal Label lblOldBackup;
        internal Label lblBdSpaceAvailable;
        internal ProgressBar pbStatus;
        internal PictureBox cmdBackupCancel;
        internal Panel plStatus;
        internal Label Label1;
        internal Label lblBackupMode;
        internal ToolTip ttMain;
        internal Panel plBottom;
        internal Label lblInfo;
        internal Panel plUpdates;
        internal Label Label3;
        internal Button btnUpdates;
        internal Label Label7;
        internal Label lblNextBackup;
        internal PictureBox btnSettings;
        internal PictureBox cmdBackupNow;
        internal PictureBox btnOnOff;
        internal MRG.Controls.UI.LoadingCircle loadingCircle;
        internal Label Label5;
        internal FlowLayoutPanel FlowLayoutPanel1;
        internal Label Label6;
        internal LinkLabel llOptions;
        internal LinkLabel llBackup;
        internal LinkLabel llShowExceptionDialog;
        internal Panel Panel1;
    }
}