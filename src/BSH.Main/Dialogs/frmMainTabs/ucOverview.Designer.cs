using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using BSH.Main.Properties;

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
            Label7 = new Label();
            lblNextBackup = new Label();
            plUpdates = new Panel();
            btnUpdates = new Button();
            Label3 = new Label();
            plBottom = new Panel();
            Label5 = new Label();
            lblInfo = new Label();
            Label1 = new Label();
            lblBackupMode = new Label();
            ttMain = new ToolTip(components);
            btnOnOff = new PictureBox();
            cmdBackupNow = new PictureBox();
            btnSettings = new PictureBox();
            FlowLayoutPanel1 = new FlowLayoutPanel();
            Label6 = new Label();
            llOptions = new LinkLabel();
            llBackup = new LinkLabel();
            picDataType = new PictureBox();
            llShowExceptionDialog = new LinkLabel();
            Panel1 = new Panel();
            loadingCircle = new MRG.Controls.UI.LoadingCircle();
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
            resources.ApplyResources(lblBdStatus, "lblBdStatus");
            lblBdStatus.BackColor = Color.Transparent;
            lblBdStatus.ForeColor = Color.Black;
            lblBdStatus.Name = "lblBdStatus";
            ttMain.SetToolTip(lblBdStatus, resources.GetString("lblBdStatus.ToolTip"));
            // 
            // Label4
            // 
            resources.ApplyResources(Label4, "Label4");
            Label4.BackColor = Color.Transparent;
            Label4.ForeColor = Color.Black;
            Label4.Name = "Label4";
            ttMain.SetToolTip(Label4, resources.GetString("Label4.ToolTip"));
            // 
            // lblBdOldestBackup
            // 
            resources.ApplyResources(lblBdOldestBackup, "lblBdOldestBackup");
            lblBdOldestBackup.BackColor = Color.Transparent;
            lblBdOldestBackup.ForeColor = Color.Black;
            lblBdOldestBackup.Name = "lblBdOldestBackup";
            ttMain.SetToolTip(lblBdOldestBackup, resources.GetString("lblBdOldestBackup.ToolTip"));
            // 
            // Label2
            // 
            resources.ApplyResources(Label2, "Label2");
            Label2.BackColor = Color.Transparent;
            Label2.ForeColor = Color.Black;
            Label2.Name = "Label2";
            ttMain.SetToolTip(Label2, resources.GetString("Label2.ToolTip"));
            // 
            // lblBdNewestBackup
            // 
            resources.ApplyResources(lblBdNewestBackup, "lblBdNewestBackup");
            lblBdNewestBackup.BackColor = Color.Transparent;
            lblBdNewestBackup.ForeColor = Color.Black;
            lblBdNewestBackup.Name = "lblBdNewestBackup";
            ttMain.SetToolTip(lblBdNewestBackup, resources.GetString("lblBdNewestBackup.ToolTip"));
            // 
            // lblOldBackup
            // 
            resources.ApplyResources(lblOldBackup, "lblOldBackup");
            lblOldBackup.BackColor = Color.Transparent;
            lblOldBackup.ForeColor = Color.Black;
            lblOldBackup.Name = "lblOldBackup";
            ttMain.SetToolTip(lblOldBackup, resources.GetString("lblOldBackup.ToolTip"));
            // 
            // lblBdSpaceAvailable
            // 
            resources.ApplyResources(lblBdSpaceAvailable, "lblBdSpaceAvailable");
            lblBdSpaceAvailable.BackColor = Color.Transparent;
            lblBdSpaceAvailable.ForeColor = Color.Black;
            lblBdSpaceAvailable.Name = "lblBdSpaceAvailable";
            ttMain.SetToolTip(lblBdSpaceAvailable, resources.GetString("lblBdSpaceAvailable.ToolTip"));
            // 
            // pbStatus
            // 
            resources.ApplyResources(pbStatus, "pbStatus");
            pbStatus.Name = "pbStatus";
            ttMain.SetToolTip(pbStatus, resources.GetString("pbStatus.ToolTip"));
            // 
            // plStatus
            // 
            resources.ApplyResources(plStatus, "plStatus");
            plStatus.Controls.Add(pbStatus);
            plStatus.Controls.Add(cmdBackupCancel);
            plStatus.Name = "plStatus";
            ttMain.SetToolTip(plStatus, resources.GetString("plStatus.ToolTip"));
            // 
            // cmdBackupCancel
            // 
            resources.ApplyResources(cmdBackupCancel, "cmdBackupCancel");
            cmdBackupCancel.Cursor = Cursors.Hand;
            cmdBackupCancel.Image = Resources.cancel_icon_48;
            cmdBackupCancel.Name = "cmdBackupCancel";
            cmdBackupCancel.TabStop = false;
            cmdBackupCancel.Tag = "false";
            ttMain.SetToolTip(cmdBackupCancel, resources.GetString("cmdBackupCancel.ToolTip"));
            cmdBackupCancel.Click += cmdBackupCancel_Click;
            // 
            // Label7
            // 
            resources.ApplyResources(Label7, "Label7");
            Label7.BackColor = Color.Transparent;
            Label7.ForeColor = Color.Black;
            Label7.Name = "Label7";
            ttMain.SetToolTip(Label7, resources.GetString("Label7.ToolTip"));
            // 
            // lblNextBackup
            // 
            resources.ApplyResources(lblNextBackup, "lblNextBackup");
            lblNextBackup.BackColor = Color.Transparent;
            lblNextBackup.ForeColor = Color.Black;
            lblNextBackup.Name = "lblNextBackup";
            ttMain.SetToolTip(lblNextBackup, resources.GetString("lblNextBackup.ToolTip"));
            // 
            // plUpdates
            // 
            resources.ApplyResources(plUpdates, "plUpdates");
            plUpdates.BackColor = Color.FromArgb(255, 192, 192);
            plUpdates.Controls.Add(btnUpdates);
            plUpdates.Controls.Add(Label3);
            plUpdates.Name = "plUpdates";
            ttMain.SetToolTip(plUpdates, resources.GetString("plUpdates.ToolTip"));
            // 
            // btnUpdates
            // 
            resources.ApplyResources(btnUpdates, "btnUpdates");
            btnUpdates.BackColor = Color.Transparent;
            btnUpdates.Name = "btnUpdates";
            ttMain.SetToolTip(btnUpdates, resources.GetString("btnUpdates.ToolTip"));
            btnUpdates.UseVisualStyleBackColor = false;
            btnUpdates.Click += btnUpdates_Click;
            // 
            // Label3
            // 
            resources.ApplyResources(Label3, "Label3");
            Label3.Name = "Label3";
            ttMain.SetToolTip(Label3, resources.GetString("Label3.ToolTip"));
            // 
            // plBottom
            // 
            resources.ApplyResources(plBottom, "plBottom");
            plBottom.BackColor = Color.White;
            plBottom.Controls.Add(Label5);
            plBottom.Controls.Add(lblInfo);
            plBottom.Name = "plBottom";
            ttMain.SetToolTip(plBottom, resources.GetString("plBottom.ToolTip"));
            // 
            // Label5
            // 
            resources.ApplyResources(Label5, "Label5");
            Label5.Name = "Label5";
            ttMain.SetToolTip(Label5, resources.GetString("Label5.ToolTip"));
            // 
            // lblInfo
            // 
            resources.ApplyResources(lblInfo, "lblInfo");
            lblInfo.Name = "lblInfo";
            ttMain.SetToolTip(lblInfo, resources.GetString("lblInfo.ToolTip"));
            // 
            // Label1
            // 
            resources.ApplyResources(Label1, "Label1");
            Label1.BackColor = Color.Transparent;
            Label1.ForeColor = Color.Black;
            Label1.Name = "Label1";
            ttMain.SetToolTip(Label1, resources.GetString("Label1.ToolTip"));
            // 
            // lblBackupMode
            // 
            resources.ApplyResources(lblBackupMode, "lblBackupMode");
            lblBackupMode.BackColor = Color.Transparent;
            lblBackupMode.ForeColor = Color.Black;
            lblBackupMode.Name = "lblBackupMode";
            ttMain.SetToolTip(lblBackupMode, resources.GetString("lblBackupMode.ToolTip"));
            // 
            // ttMain
            // 
            ttMain.ToolTipIcon = ToolTipIcon.Info;
            ttMain.ToolTipTitle = "Quickinfo";
            // 
            // btnOnOff
            // 
            resources.ApplyResources(btnOnOff, "btnOnOff");
            btnOnOff.Cursor = Cursors.Hand;
            btnOnOff.Image = Resources.toggle_off_icon_48;
            btnOnOff.Name = "btnOnOff";
            btnOnOff.TabStop = false;
            btnOnOff.Tag = "OFF";
            ttMain.SetToolTip(btnOnOff, resources.GetString("btnOnOff.ToolTip"));
            btnOnOff.Click += btnOnOff_Click;
            // 
            // cmdBackupNow
            // 
            resources.ApplyResources(cmdBackupNow, "cmdBackupNow");
            cmdBackupNow.Cursor = Cursors.Hand;
            cmdBackupNow.Image = Resources.backup_icon_48;
            cmdBackupNow.Name = "cmdBackupNow";
            cmdBackupNow.TabStop = false;
            ttMain.SetToolTip(cmdBackupNow, resources.GetString("cmdBackupNow.ToolTip"));
            cmdBackupNow.EnabledChanged += cmdBackupNow_EnabledChanged;
            cmdBackupNow.MouseClick += cmdBackupNow_MouseClick;
            cmdBackupNow.MouseEnter += cmdBackupNow_MouseEnter;
            cmdBackupNow.MouseLeave += cmdBackupNow_MouseLeave;
            // 
            // btnSettings
            // 
            resources.ApplyResources(btnSettings, "btnSettings");
            btnSettings.Cursor = Cursors.Hand;
            btnSettings.Image = Resources.settings_icon_48;
            btnSettings.Name = "btnSettings";
            btnSettings.TabStop = false;
            ttMain.SetToolTip(btnSettings, resources.GetString("btnSettings.ToolTip"));
            btnSettings.EnabledChanged += btnSettings_EnabledChanged;
            btnSettings.Click += btnSettings_Click;
            btnSettings.MouseEnter += btnSettings_MouseEnter;
            btnSettings.MouseLeave += btnSettings_MouseLeave;
            // 
            // FlowLayoutPanel1
            // 
            resources.ApplyResources(FlowLayoutPanel1, "FlowLayoutPanel1");
            FlowLayoutPanel1.BackColor = Color.White;
            FlowLayoutPanel1.Controls.Add(plUpdates);
            FlowLayoutPanel1.Controls.Add(plBottom);
            FlowLayoutPanel1.Name = "FlowLayoutPanel1";
            ttMain.SetToolTip(FlowLayoutPanel1, resources.GetString("FlowLayoutPanel1.ToolTip"));
            // 
            // Label6
            // 
            resources.ApplyResources(Label6, "Label6");
            Label6.BackColor = Color.Transparent;
            Label6.ForeColor = Color.Black;
            Label6.Name = "Label6";
            ttMain.SetToolTip(Label6, resources.GetString("Label6.ToolTip"));
            // 
            // llOptions
            // 
            resources.ApplyResources(llOptions, "llOptions");
            llOptions.Name = "llOptions";
            llOptions.TabStop = true;
            ttMain.SetToolTip(llOptions, resources.GetString("llOptions.ToolTip"));
            llOptions.LinkClicked += llOptions_LinkClicked;
            llOptions.MouseEnter += btnSettings_MouseEnter;
            llOptions.MouseLeave += btnSettings_MouseLeave;
            // 
            // llBackup
            // 
            resources.ApplyResources(llBackup, "llBackup");
            llBackup.Name = "llBackup";
            llBackup.TabStop = true;
            ttMain.SetToolTip(llBackup, resources.GetString("llBackup.ToolTip"));
            llBackup.LinkClicked += llBackup_LinkClicked;
            llBackup.MouseEnter += cmdBackupNow_MouseEnter;
            llBackup.MouseLeave += cmdBackupNow_MouseLeave;
            // 
            // picDataType
            // 
            resources.ApplyResources(picDataType, "picDataType");
            picDataType.Image = Resources.status_ok;
            picDataType.Name = "picDataType";
            picDataType.TabStop = false;
            ttMain.SetToolTip(picDataType, resources.GetString("picDataType.ToolTip"));
            // 
            // llShowExceptionDialog
            // 
            resources.ApplyResources(llShowExceptionDialog, "llShowExceptionDialog");
            llShowExceptionDialog.Name = "llShowExceptionDialog";
            llShowExceptionDialog.TabStop = true;
            ttMain.SetToolTip(llShowExceptionDialog, resources.GetString("llShowExceptionDialog.ToolTip"));
            llShowExceptionDialog.LinkClicked += llShowExceptionDialog_LinkClicked;
            // 
            // Panel1
            // 
            resources.ApplyResources(Panel1, "Panel1");
            Panel1.Controls.Add(llShowExceptionDialog);
            Panel1.Controls.Add(lblBdNewestBackup);
            Panel1.Name = "Panel1";
            ttMain.SetToolTip(Panel1, resources.GetString("Panel1.ToolTip"));
            // 
            // loadingCircle
            // 
            resources.ApplyResources(loadingCircle, "loadingCircle");
            loadingCircle.Active = true;
            loadingCircle.Color = Color.FromArgb(48, 95, 169);
            loadingCircle.InnerCircleRadius = 8;
            loadingCircle.Name = "loadingCircle";
            loadingCircle.NumberSpoke = 24;
            loadingCircle.OuterCircleRadius = 9;
            loadingCircle.RotationSpeed = 20;
            loadingCircle.SpokeThickness = 4;
            loadingCircle.StylePreset = MRG.Controls.UI.LoadingCircle.StylePresets.IE7;
            ttMain.SetToolTip(loadingCircle, resources.GetString("loadingCircle.ToolTip"));
            // 
            // ucOverview
            // 
            resources.ApplyResources(this, "$this");
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
            Name = "ucOverview";
            ttMain.SetToolTip(this, resources.GetString("$this.ToolTip"));
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