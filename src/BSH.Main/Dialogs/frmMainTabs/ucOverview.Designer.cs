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
            this.components = new System.ComponentModel.Container();
            this.lblBdStatus = new System.Windows.Forms.Label();
            this.Label4 = new System.Windows.Forms.Label();
            this.lblBdOldestBackup = new System.Windows.Forms.Label();
            this.Label2 = new System.Windows.Forms.Label();
            this.lblBdNewestBackup = new System.Windows.Forms.Label();
            this.lblOldBackup = new System.Windows.Forms.Label();
            this.lblBdSpaceAvailable = new System.Windows.Forms.Label();
            this.pbStatus = new System.Windows.Forms.ProgressBar();
            this.plStatus = new System.Windows.Forms.Panel();
            this.cmdBackupCancel = new System.Windows.Forms.PictureBox();
            this.Label7 = new System.Windows.Forms.Label();
            this.lblNextBackup = new System.Windows.Forms.Label();
            this.plUpdates = new System.Windows.Forms.Panel();
            this.btnUpdates = new System.Windows.Forms.Button();
            this.Label3 = new System.Windows.Forms.Label();
            this.plBottom = new System.Windows.Forms.Panel();
            this.Label5 = new System.Windows.Forms.Label();
            this.lblInfo = new System.Windows.Forms.Label();
            this.Label1 = new System.Windows.Forms.Label();
            this.lblBackupMode = new System.Windows.Forms.Label();
            this.ttMain = new System.Windows.Forms.ToolTip(this.components);
            this.btnOnOff = new System.Windows.Forms.PictureBox();
            this.cmdBackupNow = new System.Windows.Forms.PictureBox();
            this.btnSettings = new System.Windows.Forms.PictureBox();
            this.FlowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.Label6 = new System.Windows.Forms.Label();
            this.llOptions = new System.Windows.Forms.LinkLabel();
            this.llBackup = new System.Windows.Forms.LinkLabel();
            this.picDataType = new System.Windows.Forms.PictureBox();
            this.llShowExceptionDialog = new System.Windows.Forms.LinkLabel();
            this.Panel1 = new System.Windows.Forms.Panel();
            this.loadingCircle = new MRG.Controls.UI.LoadingCircle();
            this.plStatus.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.cmdBackupCancel)).BeginInit();
            this.plUpdates.SuspendLayout();
            this.plBottom.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.btnOnOff)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmdBackupNow)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnSettings)).BeginInit();
            this.FlowLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picDataType)).BeginInit();
            this.Panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblBdStatus
            // 
            this.lblBdStatus.BackColor = System.Drawing.Color.Transparent;
            this.lblBdStatus.Font = new System.Drawing.Font("Segoe UI Semilight", 14.25F);
            this.lblBdStatus.ForeColor = System.Drawing.Color.Black;
            this.lblBdStatus.Location = new System.Drawing.Point(141, 36);
            this.lblBdStatus.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblBdStatus.Name = "lblBdStatus";
            this.lblBdStatus.Size = new System.Drawing.Size(850, 38);
            this.lblBdStatus.TabIndex = 70;
            this.lblBdStatus.Text = "Backup Service Home wird ordnungsgemäß ausgeführt.";
            // 
            // Label4
            // 
            this.Label4.AutoSize = true;
            this.Label4.BackColor = System.Drawing.Color.Transparent;
            this.Label4.Font = new System.Drawing.Font("Segoe UI", 9.75F);
            this.Label4.ForeColor = System.Drawing.Color.Black;
            this.Label4.Location = new System.Drawing.Point(144, 183);
            this.Label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.Label4.Name = "Label4";
            this.Label4.Size = new System.Drawing.Size(144, 28);
            this.Label4.TabIndex = 61;
            this.Label4.Text = "Letztes Backup:";
            // 
            // lblBdOldestBackup
            // 
            this.lblBdOldestBackup.BackColor = System.Drawing.Color.Transparent;
            this.lblBdOldestBackup.Font = new System.Drawing.Font("Segoe UI", 9.75F);
            this.lblBdOldestBackup.ForeColor = System.Drawing.Color.Black;
            this.lblBdOldestBackup.Location = new System.Drawing.Point(368, 138);
            this.lblBdOldestBackup.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblBdOldestBackup.Name = "lblBdOldestBackup";
            this.lblBdOldestBackup.Size = new System.Drawing.Size(624, 27);
            this.lblBdOldestBackup.TabIndex = 68;
            this.lblBdOldestBackup.Text = "--";
            // 
            // Label2
            // 
            this.Label2.AutoSize = true;
            this.Label2.BackColor = System.Drawing.Color.Transparent;
            this.Label2.Font = new System.Drawing.Font("Segoe UI", 9.75F);
            this.Label2.ForeColor = System.Drawing.Color.Black;
            this.Label2.Location = new System.Drawing.Point(144, 93);
            this.Label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.Label2.Name = "Label2";
            this.Label2.Size = new System.Drawing.Size(102, 28);
            this.Label2.TabIndex = 59;
            this.Label2.Text = "Verfügbar:";
            // 
            // lblBdNewestBackup
            // 
            this.lblBdNewestBackup.AutoSize = true;
            this.lblBdNewestBackup.BackColor = System.Drawing.Color.Transparent;
            this.lblBdNewestBackup.Dock = System.Windows.Forms.DockStyle.Left;
            this.lblBdNewestBackup.Font = new System.Drawing.Font("Segoe UI", 9.75F);
            this.lblBdNewestBackup.ForeColor = System.Drawing.Color.Black;
            this.lblBdNewestBackup.Location = new System.Drawing.Point(0, 0);
            this.lblBdNewestBackup.Margin = new System.Windows.Forms.Padding(0);
            this.lblBdNewestBackup.Name = "lblBdNewestBackup";
            this.lblBdNewestBackup.Size = new System.Drawing.Size(28, 28);
            this.lblBdNewestBackup.TabIndex = 69;
            this.lblBdNewestBackup.Text = "--";
            // 
            // lblOldBackup
            // 
            this.lblOldBackup.AutoSize = true;
            this.lblOldBackup.BackColor = System.Drawing.Color.Transparent;
            this.lblOldBackup.Font = new System.Drawing.Font("Segoe UI", 9.75F);
            this.lblOldBackup.ForeColor = System.Drawing.Color.Black;
            this.lblOldBackup.Location = new System.Drawing.Point(144, 138);
            this.lblOldBackup.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblOldBackup.Name = "lblOldBackup";
            this.lblOldBackup.Size = new System.Drawing.Size(193, 28);
            this.lblOldBackup.TabIndex = 60;
            this.lblOldBackup.Text = "Voraussichtl. voll am:";
            // 
            // lblBdSpaceAvailable
            // 
            this.lblBdSpaceAvailable.BackColor = System.Drawing.Color.Transparent;
            this.lblBdSpaceAvailable.Font = new System.Drawing.Font("Segoe UI", 9.75F);
            this.lblBdSpaceAvailable.ForeColor = System.Drawing.Color.Black;
            this.lblBdSpaceAvailable.Location = new System.Drawing.Point(368, 93);
            this.lblBdSpaceAvailable.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblBdSpaceAvailable.Name = "lblBdSpaceAvailable";
            this.lblBdSpaceAvailable.Size = new System.Drawing.Size(624, 27);
            this.lblBdSpaceAvailable.TabIndex = 67;
            this.lblBdSpaceAvailable.Text = "--";
            // 
            // pbStatus
            // 
            this.pbStatus.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pbStatus.Location = new System.Drawing.Point(0, 0);
            this.pbStatus.Margin = new System.Windows.Forms.Padding(4);
            this.pbStatus.Name = "pbStatus";
            this.pbStatus.Size = new System.Drawing.Size(810, 30);
            this.pbStatus.TabIndex = 76;
            // 
            // plStatus
            // 
            this.plStatus.Controls.Add(this.pbStatus);
            this.plStatus.Controls.Add(this.cmdBackupCancel);
            this.plStatus.Location = new System.Drawing.Point(148, 93);
            this.plStatus.Margin = new System.Windows.Forms.Padding(4);
            this.plStatus.Name = "plStatus";
            this.plStatus.Size = new System.Drawing.Size(843, 30);
            this.plStatus.TabIndex = 78;
            this.plStatus.Visible = false;
            // 
            // cmdBackupCancel
            // 
            this.cmdBackupCancel.Cursor = System.Windows.Forms.Cursors.Hand;
            this.cmdBackupCancel.Dock = System.Windows.Forms.DockStyle.Right;
            this.cmdBackupCancel.Image = global::BSH.Main.Properties.Resources.cancel_icon_48;
            this.cmdBackupCancel.Location = new System.Drawing.Point(819, 0);
            this.cmdBackupCancel.Margin = new System.Windows.Forms.Padding(4);
            this.cmdBackupCancel.Name = "cmdBackupCancel";
            this.cmdBackupCancel.Size = new System.Drawing.Size(24, 30);
            this.cmdBackupCancel.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.cmdBackupCancel.TabIndex = 77;
            this.cmdBackupCancel.TabStop = false;
            this.cmdBackupCancel.Tag = "false";
            this.ttMain.SetToolTip(this.cmdBackupCancel, "Bricht die aktuelle Datensicherung ab. Alle Änderungen auf dem Backupmedium\r\nwerd" +
        "en rückgängig gemacht.");
            this.cmdBackupCancel.Click += new System.EventHandler(this.cmdBackupCancel_Click);
            // 
            // Label7
            // 
            this.Label7.AutoSize = true;
            this.Label7.BackColor = System.Drawing.Color.Transparent;
            this.Label7.Font = new System.Drawing.Font("Segoe UI", 9.75F);
            this.Label7.ForeColor = System.Drawing.Color.Black;
            this.Label7.Location = new System.Drawing.Point(144, 228);
            this.Label7.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.Label7.Name = "Label7";
            this.Label7.Size = new System.Drawing.Size(162, 28);
            this.Label7.TabIndex = 97;
            this.Label7.Text = "Nächstes Backup:";
            // 
            // lblNextBackup
            // 
            this.lblNextBackup.BackColor = System.Drawing.Color.Transparent;
            this.lblNextBackup.Font = new System.Drawing.Font("Segoe UI", 9.75F);
            this.lblNextBackup.ForeColor = System.Drawing.Color.Black;
            this.lblNextBackup.Location = new System.Drawing.Point(368, 228);
            this.lblNextBackup.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblNextBackup.Name = "lblNextBackup";
            this.lblNextBackup.Size = new System.Drawing.Size(624, 27);
            this.lblNextBackup.TabIndex = 98;
            this.lblNextBackup.Text = "--";
            // 
            // plUpdates
            // 
            this.plUpdates.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(192)))));
            this.plUpdates.Controls.Add(this.btnUpdates);
            this.plUpdates.Controls.Add(this.Label3);
            this.plUpdates.Location = new System.Drawing.Point(0, 153);
            this.plUpdates.Margin = new System.Windows.Forms.Padding(0);
            this.plUpdates.Name = "plUpdates";
            this.plUpdates.Size = new System.Drawing.Size(1050, 51);
            this.plUpdates.TabIndex = 96;
            this.plUpdates.Visible = false;
            // 
            // btnUpdates
            // 
            this.btnUpdates.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnUpdates.BackColor = System.Drawing.Color.Transparent;
            this.btnUpdates.Location = new System.Drawing.Point(884, 6);
            this.btnUpdates.Margin = new System.Windows.Forms.Padding(4);
            this.btnUpdates.Name = "btnUpdates";
            this.btnUpdates.Size = new System.Drawing.Size(134, 39);
            this.btnUpdates.TabIndex = 75;
            this.btnUpdates.Text = "Installieren";
            this.ttMain.SetToolTip(this.btnUpdates, "Installiert wichtige Aktualisierungen.");
            this.btnUpdates.UseVisualStyleBackColor = false;
            this.btnUpdates.Click += new System.EventHandler(this.btnUpdates_Click);
            // 
            // Label3
            // 
            this.Label3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.Label3.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold);
            this.Label3.Location = new System.Drawing.Point(38, 14);
            this.Label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.Label3.Name = "Label3";
            this.Label3.Size = new System.Drawing.Size(792, 26);
            this.Label3.TabIndex = 0;
            this.Label3.Text = "Es sind Aktualisierungen verfügbar.";
            this.Label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // plBottom
            // 
            this.plBottom.BackColor = System.Drawing.Color.White;
            this.plBottom.Controls.Add(this.Label5);
            this.plBottom.Controls.Add(this.lblInfo);
            this.plBottom.Location = new System.Drawing.Point(0, 0);
            this.plBottom.Margin = new System.Windows.Forms.Padding(0);
            this.plBottom.Name = "plBottom";
            this.plBottom.Size = new System.Drawing.Size(1050, 153);
            this.plBottom.TabIndex = 95;
            // 
            // Label5
            // 
            this.Label5.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold);
            this.Label5.Location = new System.Drawing.Point(42, 24);
            this.Label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.Label5.Name = "Label5";
            this.Label5.Size = new System.Drawing.Size(327, 38);
            this.Label5.TabIndex = 1;
            this.Label5.Text = "Sicherungseinstellungen:";
            // 
            // lblInfo
            // 
            this.lblInfo.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblInfo.Font = new System.Drawing.Font("Segoe UI", 9.75F);
            this.lblInfo.Location = new System.Drawing.Point(42, 66);
            this.lblInfo.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblInfo.Name = "lblInfo";
            this.lblInfo.Size = new System.Drawing.Size(950, 62);
            this.lblInfo.TabIndex = 0;
            // 
            // Label1
            // 
            this.Label1.AutoSize = true;
            this.Label1.BackColor = System.Drawing.Color.Transparent;
            this.Label1.Font = new System.Drawing.Font("Segoe UI", 9.75F);
            this.Label1.ForeColor = System.Drawing.Color.Black;
            this.Label1.Location = new System.Drawing.Point(144, 273);
            this.Label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.Label1.Name = "Label1";
            this.Label1.Size = new System.Drawing.Size(139, 28);
            this.Label1.TabIndex = 83;
            this.Label1.Text = "Backupmodus:";
            // 
            // lblBackupMode
            // 
            this.lblBackupMode.BackColor = System.Drawing.Color.Transparent;
            this.lblBackupMode.Font = new System.Drawing.Font("Segoe UI", 9.75F);
            this.lblBackupMode.ForeColor = System.Drawing.Color.Black;
            this.lblBackupMode.Location = new System.Drawing.Point(368, 273);
            this.lblBackupMode.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblBackupMode.Name = "lblBackupMode";
            this.lblBackupMode.Size = new System.Drawing.Size(624, 27);
            this.lblBackupMode.TabIndex = 84;
            this.lblBackupMode.Text = "--";
            // 
            // ttMain
            // 
            this.ttMain.ToolTipIcon = System.Windows.Forms.ToolTipIcon.Info;
            this.ttMain.ToolTipTitle = "Quickinfo";
            // 
            // btnOnOff
            // 
            this.btnOnOff.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnOnOff.Image = global::BSH.Main.Properties.Resources.toggle_off_icon_48;
            this.btnOnOff.Location = new System.Drawing.Point(42, 149);
            this.btnOnOff.Margin = new System.Windows.Forms.Padding(4);
            this.btnOnOff.Name = "btnOnOff";
            this.btnOnOff.Size = new System.Drawing.Size(90, 50);
            this.btnOnOff.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.btnOnOff.TabIndex = 101;
            this.btnOnOff.TabStop = false;
            this.btnOnOff.Tag = "OFF";
            this.ttMain.SetToolTip(this.btnOnOff, "Datensicherung ein- / ausschalten");
            this.btnOnOff.Click += new System.EventHandler(this.btnOnOff_Click);
            // 
            // cmdBackupNow
            // 
            this.cmdBackupNow.Cursor = System.Windows.Forms.Cursors.Hand;
            this.cmdBackupNow.Image = global::BSH.Main.Properties.Resources.backup_icon_48;
            this.cmdBackupNow.Location = new System.Drawing.Point(470, 321);
            this.cmdBackupNow.Margin = new System.Windows.Forms.Padding(4);
            this.cmdBackupNow.Name = "cmdBackupNow";
            this.cmdBackupNow.Size = new System.Drawing.Size(38, 38);
            this.cmdBackupNow.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.cmdBackupNow.TabIndex = 100;
            this.cmdBackupNow.TabStop = false;
            this.ttMain.SetToolTip(this.cmdBackupNow, "Datensicherung starten");
            this.cmdBackupNow.EnabledChanged += new System.EventHandler(this.cmdBackupNow_EnabledChanged);
            this.cmdBackupNow.MouseClick += new System.Windows.Forms.MouseEventHandler(this.cmdBackupNow_MouseClick);
            this.cmdBackupNow.MouseEnter += new System.EventHandler(this.cmdBackupNow_MouseEnter);
            this.cmdBackupNow.MouseLeave += new System.EventHandler(this.cmdBackupNow_MouseLeave);
            // 
            // btnSettings
            // 
            this.btnSettings.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnSettings.Image = global::BSH.Main.Properties.Resources.settings_icon_48;
            this.btnSettings.Location = new System.Drawing.Point(148, 321);
            this.btnSettings.Margin = new System.Windows.Forms.Padding(4);
            this.btnSettings.Name = "btnSettings";
            this.btnSettings.Size = new System.Drawing.Size(38, 38);
            this.btnSettings.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.btnSettings.TabIndex = 99;
            this.btnSettings.TabStop = false;
            this.ttMain.SetToolTip(this.btnSettings, "Einstellungen");
            this.btnSettings.EnabledChanged += new System.EventHandler(this.btnSettings_EnabledChanged);
            this.btnSettings.Click += new System.EventHandler(this.btnSettings_Click);
            this.btnSettings.MouseEnter += new System.EventHandler(this.btnSettings_MouseEnter);
            this.btnSettings.MouseLeave += new System.EventHandler(this.btnSettings_MouseLeave);
            // 
            // FlowLayoutPanel1
            // 
            this.FlowLayoutPanel1.BackColor = System.Drawing.Color.White;
            this.FlowLayoutPanel1.Controls.Add(this.plUpdates);
            this.FlowLayoutPanel1.Controls.Add(this.plBottom);
            this.FlowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.FlowLayoutPanel1.FlowDirection = System.Windows.Forms.FlowDirection.BottomUp;
            this.FlowLayoutPanel1.Location = new System.Drawing.Point(0, 356);
            this.FlowLayoutPanel1.Margin = new System.Windows.Forms.Padding(0);
            this.FlowLayoutPanel1.Name = "FlowLayoutPanel1";
            this.FlowLayoutPanel1.Size = new System.Drawing.Size(1050, 204);
            this.FlowLayoutPanel1.TabIndex = 103;
            this.FlowLayoutPanel1.WrapContents = false;
            // 
            // Label6
            // 
            this.Label6.BackColor = System.Drawing.Color.Transparent;
            this.Label6.Font = new System.Drawing.Font("Segoe UI", 9.75F);
            this.Label6.ForeColor = System.Drawing.Color.Black;
            this.Label6.Location = new System.Drawing.Point(42, 118);
            this.Label6.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.Label6.Name = "Label6";
            this.Label6.Size = new System.Drawing.Size(90, 27);
            this.Label6.TabIndex = 104;
            this.Label6.Text = "Aus / Ein";
            this.Label6.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // llOptions
            // 
            this.llOptions.AutoSize = true;
            this.llOptions.Location = new System.Drawing.Point(195, 326);
            this.llOptions.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.llOptions.Name = "llOptions";
            this.llOptions.Size = new System.Drawing.Size(188, 28);
            this.llOptions.TabIndex = 105;
            this.llOptions.TabStop = true;
            this.llOptions.Text = "Einstellungen ändern";
            this.llOptions.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.llOptions_LinkClicked);
            this.llOptions.MouseEnter += new System.EventHandler(this.btnSettings_MouseEnter);
            this.llOptions.MouseLeave += new System.EventHandler(this.btnSettings_MouseLeave);
            // 
            // llBackup
            // 
            this.llBackup.AutoSize = true;
            this.llBackup.Location = new System.Drawing.Point(516, 326);
            this.llBackup.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.llBackup.Name = "llBackup";
            this.llBackup.Size = new System.Drawing.Size(242, 28);
            this.llBackup.TabIndex = 106;
            this.llBackup.TabStop = true;
            this.llBackup.Text = "Manuelle Sicherung starten";
            this.llBackup.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.llBackup_LinkClicked);
            this.llBackup.MouseEnter += new System.EventHandler(this.cmdBackupNow_MouseEnter);
            this.llBackup.MouseLeave += new System.EventHandler(this.cmdBackupNow_MouseLeave);
            // 
            // picDataType
            // 
            this.picDataType.Image = global::BSH.Main.Properties.Resources.status_ok;
            this.picDataType.Location = new System.Drawing.Point(42, 26);
            this.picDataType.Margin = new System.Windows.Forms.Padding(4);
            this.picDataType.Name = "picDataType";
            this.picDataType.Size = new System.Drawing.Size(90, 68);
            this.picDataType.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picDataType.TabIndex = 63;
            this.picDataType.TabStop = false;
            // 
            // llShowExceptionDialog
            // 
            this.llShowExceptionDialog.AutoSize = true;
            this.llShowExceptionDialog.Dock = System.Windows.Forms.DockStyle.Left;
            this.llShowExceptionDialog.Location = new System.Drawing.Point(28, 0);
            this.llShowExceptionDialog.Name = "llShowExceptionDialog";
            this.llShowExceptionDialog.Size = new System.Drawing.Size(156, 28);
            this.llShowExceptionDialog.TabIndex = 107;
            this.llShowExceptionDialog.TabStop = true;
            this.llShowExceptionDialog.Text = "(Fehler anzeigen)";
            this.llShowExceptionDialog.Visible = false;
            this.llShowExceptionDialog.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.llShowExceptionDialog_LinkClicked);
            // 
            // Panel1
            // 
            this.Panel1.Controls.Add(this.llShowExceptionDialog);
            this.Panel1.Controls.Add(this.lblBdNewestBackup);
            this.Panel1.Location = new System.Drawing.Point(368, 183);
            this.Panel1.Margin = new System.Windows.Forms.Padding(0);
            this.Panel1.Name = "Panel1";
            this.Panel1.Size = new System.Drawing.Size(619, 28);
            this.Panel1.TabIndex = 108;
            // 
            // loadingCircle
            // 
            this.loadingCircle.Active = true;
            this.loadingCircle.Color = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(95)))), ((int)(((byte)(169)))));
            this.loadingCircle.InnerCircleRadius = 8;
            this.loadingCircle.Location = new System.Drawing.Point(42, 20);
            this.loadingCircle.Margin = new System.Windows.Forms.Padding(4);
            this.loadingCircle.Name = "loadingCircle";
            this.loadingCircle.NumberSpoke = 24;
            this.loadingCircle.OuterCircleRadius = 9;
            this.loadingCircle.RotationSpeed = 20;
            this.loadingCircle.Size = new System.Drawing.Size(90, 78);
            this.loadingCircle.SpokeThickness = 4;
            this.loadingCircle.StylePreset = MRG.Controls.UI.LoadingCircle.StylePresets.IE7;
            this.loadingCircle.TabIndex = 102;
            this.loadingCircle.Visible = false;
            // 
            // ucOverview
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(144F, 144F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.BackColor = System.Drawing.Color.White;
            this.Controls.Add(this.Panel1);
            this.Controls.Add(this.llBackup);
            this.Controls.Add(this.llOptions);
            this.Controls.Add(this.Label6);
            this.Controls.Add(this.FlowLayoutPanel1);
            this.Controls.Add(this.plStatus);
            this.Controls.Add(this.loadingCircle);
            this.Controls.Add(this.btnOnOff);
            this.Controls.Add(this.cmdBackupNow);
            this.Controls.Add(this.btnSettings);
            this.Controls.Add(this.Label7);
            this.Controls.Add(this.lblNextBackup);
            this.Controls.Add(this.picDataType);
            this.Controls.Add(this.lblBdSpaceAvailable);
            this.Controls.Add(this.lblOldBackup);
            this.Controls.Add(this.Label1);
            this.Controls.Add(this.Label2);
            this.Controls.Add(this.lblBackupMode);
            this.Controls.Add(this.lblBdOldestBackup);
            this.Controls.Add(this.Label4);
            this.Controls.Add(this.lblBdStatus);
            this.DoubleBuffered = true;
            this.Font = new System.Drawing.Font("Segoe UI Light", 9.75F);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "ucOverview";
            this.Size = new System.Drawing.Size(1050, 560);
            this.plStatus.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.cmdBackupCancel)).EndInit();
            this.plUpdates.ResumeLayout(false);
            this.plBottom.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.btnOnOff)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmdBackupNow)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnSettings)).EndInit();
            this.FlowLayoutPanel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.picDataType)).EndInit();
            this.Panel1.ResumeLayout(false);
            this.Panel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

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