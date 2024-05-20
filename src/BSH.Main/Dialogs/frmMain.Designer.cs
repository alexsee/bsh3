using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using BSH.Main.Properties;

namespace Brightbits.BSH.Main
{
    public partial class frmMain : Form
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
            var resources = new System.ComponentModel.ComponentResourceManager(typeof(frmMain));
            ttMain = new ToolTip(components);
            picHome = new PictureBox();
            picHelp = new PictureBox();
            cmsHelp = new ContextMenuStrip(components);
            HilfeUndSupportToolStripMenuItem = new ToolStripMenuItem();
            AufAktualisierungenPrüfenToolStripMenuItem = new ToolStripMenuItem();
            AutomatischNachAktualisierungenSuchenToolStripMenuItem = new ToolStripMenuItem();
            BetaversionenÜberAktualiserungenHerunterladenToolStripMenuItem = new ToolStripMenuItem();
            ÜberBackupServiceHome3ToolStripMenuItem = new ToolStripMenuItem();
            ToolStripMenuItem1 = new ToolStripSeparator();
            MitWindowsStartenToolStripMenuItem = new ToolStripMenuItem();
            ToolStripMenuItem4 = new ToolStripSeparator();
            EreignisprotokollAnzeigenToolStripMenuItem = new ToolStripMenuItem();
            AlteDatensicherungImportierenToolStripMenuItem = new ToolStripMenuItem();
            ZurücksetzenToolStripMenuItem = new ToolStripMenuItem();
            GespeichertesKennwortLöschenToolStripMenuItem = new ToolStripMenuItem();
            btnResetUserId = new ToolStripMenuItem();
            ToolStripMenuItem2 = new ToolStripSeparator();
            BackupServiceHome3BeendenToolStripMenuItem = new ToolStripMenuItem();
            plMain = new Panel();
            lblHeadTitle = new Label();
            lblExtras = new Label();
            ((System.ComponentModel.ISupportInitialize)picHome).BeginInit();
            ((System.ComponentModel.ISupportInitialize)picHelp).BeginInit();
            cmsHelp.SuspendLayout();
            SuspendLayout();
            // 
            // ttMain
            // 
            ttMain.ToolTipIcon = ToolTipIcon.Info;
            ttMain.ToolTipTitle = "Quickhilfe";
            // 
            // picHome
            // 
            resources.ApplyResources(picHome, "picHome");
            picHome.BackColor = Color.Transparent;
            picHome.Image = Resources.home_icon_48;
            picHome.Name = "picHome";
            picHome.TabStop = false;
            ttMain.SetToolTip(picHome, resources.GetString("picHome.ToolTip"));
            picHome.Click += picHome_Click;
            // 
            // picHelp
            // 
            resources.ApplyResources(picHelp, "picHelp");
            picHelp.BackColor = Color.Transparent;
            picHelp.Cursor = Cursors.Hand;
            picHelp.Image = Resources.help_icon_48;
            picHelp.Name = "picHelp";
            picHelp.TabStop = false;
            ttMain.SetToolTip(picHelp, resources.GetString("picHelp.ToolTip"));
            picHelp.Click += picHelp_Click;
            // 
            // cmsHelp
            // 
            resources.ApplyResources(cmsHelp, "cmsHelp");
            cmsHelp.ImageScalingSize = new Size(20, 20);
            cmsHelp.Items.AddRange(new ToolStripItem[] { HilfeUndSupportToolStripMenuItem, AufAktualisierungenPrüfenToolStripMenuItem, AutomatischNachAktualisierungenSuchenToolStripMenuItem, BetaversionenÜberAktualiserungenHerunterladenToolStripMenuItem, ÜberBackupServiceHome3ToolStripMenuItem, ToolStripMenuItem1, MitWindowsStartenToolStripMenuItem, ToolStripMenuItem4, EreignisprotokollAnzeigenToolStripMenuItem, AlteDatensicherungImportierenToolStripMenuItem, ZurücksetzenToolStripMenuItem, GespeichertesKennwortLöschenToolStripMenuItem, btnResetUserId, ToolStripMenuItem2, BackupServiceHome3BeendenToolStripMenuItem });
            cmsHelp.Name = "cmsHelp";
            cmsHelp.RenderMode = ToolStripRenderMode.System;
            ttMain.SetToolTip(cmsHelp, resources.GetString("cmsHelp.ToolTip"));
            // 
            // HilfeUndSupportToolStripMenuItem
            // 
            resources.ApplyResources(HilfeUndSupportToolStripMenuItem, "HilfeUndSupportToolStripMenuItem");
            HilfeUndSupportToolStripMenuItem.Name = "HilfeUndSupportToolStripMenuItem";
            HilfeUndSupportToolStripMenuItem.Click += HilfeUndSupportToolStripMenuItem_Click;
            // 
            // AufAktualisierungenPrüfenToolStripMenuItem
            // 
            resources.ApplyResources(AufAktualisierungenPrüfenToolStripMenuItem, "AufAktualisierungenPrüfenToolStripMenuItem");
            AufAktualisierungenPrüfenToolStripMenuItem.Name = "AufAktualisierungenPrüfenToolStripMenuItem";
            AufAktualisierungenPrüfenToolStripMenuItem.Click += AufAktualisierungenPrüfenToolStripMenuItem_Click;
            // 
            // AutomatischNachAktualisierungenSuchenToolStripMenuItem
            // 
            resources.ApplyResources(AutomatischNachAktualisierungenSuchenToolStripMenuItem, "AutomatischNachAktualisierungenSuchenToolStripMenuItem");
            AutomatischNachAktualisierungenSuchenToolStripMenuItem.Name = "AutomatischNachAktualisierungenSuchenToolStripMenuItem";
            AutomatischNachAktualisierungenSuchenToolStripMenuItem.Click += AutomatischNachAktualisierungenSuchenToolStripMenuItem_Click;
            // 
            // BetaversionenÜberAktualiserungenHerunterladenToolStripMenuItem
            // 
            resources.ApplyResources(BetaversionenÜberAktualiserungenHerunterladenToolStripMenuItem, "BetaversionenÜberAktualiserungenHerunterladenToolStripMenuItem");
            BetaversionenÜberAktualiserungenHerunterladenToolStripMenuItem.CheckOnClick = true;
            BetaversionenÜberAktualiserungenHerunterladenToolStripMenuItem.Name = "BetaversionenÜberAktualiserungenHerunterladenToolStripMenuItem";
            BetaversionenÜberAktualiserungenHerunterladenToolStripMenuItem.Click += BetaversionenÜberAktualiserungenHerunterladenToolStripMenuItem_Click;
            // 
            // ÜberBackupServiceHome3ToolStripMenuItem
            // 
            resources.ApplyResources(ÜberBackupServiceHome3ToolStripMenuItem, "ÜberBackupServiceHome3ToolStripMenuItem");
            ÜberBackupServiceHome3ToolStripMenuItem.Name = "ÜberBackupServiceHome3ToolStripMenuItem";
            ÜberBackupServiceHome3ToolStripMenuItem.Click += ÜberBackupServiceHome3ToolStripMenuItem_Click;
            // 
            // ToolStripMenuItem1
            // 
            resources.ApplyResources(ToolStripMenuItem1, "ToolStripMenuItem1");
            ToolStripMenuItem1.Name = "ToolStripMenuItem1";
            // 
            // MitWindowsStartenToolStripMenuItem
            // 
            resources.ApplyResources(MitWindowsStartenToolStripMenuItem, "MitWindowsStartenToolStripMenuItem");
            MitWindowsStartenToolStripMenuItem.Name = "MitWindowsStartenToolStripMenuItem";
            MitWindowsStartenToolStripMenuItem.Click += MitWindowsStartenToolStripMenuItem_Click;
            // 
            // ToolStripMenuItem4
            // 
            resources.ApplyResources(ToolStripMenuItem4, "ToolStripMenuItem4");
            ToolStripMenuItem4.Name = "ToolStripMenuItem4";
            // 
            // EreignisprotokollAnzeigenToolStripMenuItem
            // 
            resources.ApplyResources(EreignisprotokollAnzeigenToolStripMenuItem, "EreignisprotokollAnzeigenToolStripMenuItem");
            EreignisprotokollAnzeigenToolStripMenuItem.Name = "EreignisprotokollAnzeigenToolStripMenuItem";
            EreignisprotokollAnzeigenToolStripMenuItem.Click += EreignisprotokollAnzeigenToolStripMenuItem_Click;
            // 
            // AlteDatensicherungImportierenToolStripMenuItem
            // 
            resources.ApplyResources(AlteDatensicherungImportierenToolStripMenuItem, "AlteDatensicherungImportierenToolStripMenuItem");
            AlteDatensicherungImportierenToolStripMenuItem.Name = "AlteDatensicherungImportierenToolStripMenuItem";
            // 
            // ZurücksetzenToolStripMenuItem
            // 
            resources.ApplyResources(ZurücksetzenToolStripMenuItem, "ZurücksetzenToolStripMenuItem");
            ZurücksetzenToolStripMenuItem.Name = "ZurücksetzenToolStripMenuItem";
            ZurücksetzenToolStripMenuItem.Click += ZurücksetzenToolStripMenuItem_Click;
            // 
            // GespeichertesKennwortLöschenToolStripMenuItem
            // 
            resources.ApplyResources(GespeichertesKennwortLöschenToolStripMenuItem, "GespeichertesKennwortLöschenToolStripMenuItem");
            GespeichertesKennwortLöschenToolStripMenuItem.Name = "GespeichertesKennwortLöschenToolStripMenuItem";
            GespeichertesKennwortLöschenToolStripMenuItem.Click += GespeichertesKennwortLöschenToolStripMenuItem_Click;
            // 
            // btnResetUserId
            // 
            resources.ApplyResources(btnResetUserId, "btnResetUserId");
            btnResetUserId.Name = "btnResetUserId";
            btnResetUserId.Click += btnResetUserId_Click;
            // 
            // ToolStripMenuItem2
            // 
            resources.ApplyResources(ToolStripMenuItem2, "ToolStripMenuItem2");
            ToolStripMenuItem2.Name = "ToolStripMenuItem2";
            // 
            // BackupServiceHome3BeendenToolStripMenuItem
            // 
            resources.ApplyResources(BackupServiceHome3BeendenToolStripMenuItem, "BackupServiceHome3BeendenToolStripMenuItem");
            BackupServiceHome3BeendenToolStripMenuItem.Name = "BackupServiceHome3BeendenToolStripMenuItem";
            BackupServiceHome3BeendenToolStripMenuItem.Click += BackupServiceHome3BeendenToolStripMenuItem_Click;
            // 
            // plMain
            // 
            resources.ApplyResources(plMain, "plMain");
            plMain.BackColor = Color.White;
            plMain.Name = "plMain";
            ttMain.SetToolTip(plMain, resources.GetString("plMain.ToolTip"));
            // 
            // lblHeadTitle
            // 
            resources.ApplyResources(lblHeadTitle, "lblHeadTitle");
            lblHeadTitle.BackColor = Color.Transparent;
            lblHeadTitle.Name = "lblHeadTitle";
            ttMain.SetToolTip(lblHeadTitle, resources.GetString("lblHeadTitle.ToolTip"));
            // 
            // lblExtras
            // 
            resources.ApplyResources(lblExtras, "lblExtras");
            lblExtras.BackColor = Color.Transparent;
            lblExtras.Cursor = Cursors.Hand;
            lblExtras.Name = "lblExtras";
            ttMain.SetToolTip(lblExtras, resources.GetString("lblExtras.ToolTip"));
            lblExtras.Click += lblExtras_Click;
            // 
            // frmMain
            // 
            resources.ApplyResources(this, "$this");
            AutoScaleMode = AutoScaleMode.Dpi;
            BackColor = Color.FromArgb(243, 243, 245);
            Controls.Add(lblExtras);
            Controls.Add(picHome);
            Controls.Add(picHelp);
            Controls.Add(plMain);
            Controls.Add(lblHeadTitle);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            Name = "frmMain";
            ttMain.SetToolTip(this, resources.GetString("$this.ToolTip"));
            FormClosing += frmMain_FormClosing;
            Load += frmMain_Load;
            ((System.ComponentModel.ISupportInitialize)picHome).EndInit();
            ((System.ComponentModel.ISupportInitialize)picHelp).EndInit();
            cmsHelp.ResumeLayout(false);
            ResumeLayout(false);
        }

        internal ToolTip ttMain;
        internal ContextMenuStrip cmsHelp;
        internal ToolStripMenuItem BackupServiceHome3BeendenToolStripMenuItem;
        internal ToolStripMenuItem HilfeUndSupportToolStripMenuItem;
        internal ToolStripMenuItem AufAktualisierungenPrüfenToolStripMenuItem;
        internal ToolStripSeparator ToolStripMenuItem1;
        internal ToolStripMenuItem ZurücksetzenToolStripMenuItem;
        internal ToolStripSeparator ToolStripMenuItem2;
        internal PictureBox picHome;
        internal Panel plMain;
        internal Label lblHeadTitle;
        internal PictureBox picHelp;
        internal ToolStripMenuItem MitWindowsStartenToolStripMenuItem;
        internal ToolStripMenuItem ÜberBackupServiceHome3ToolStripMenuItem;
        internal ToolStripMenuItem BetaversionenÜberAktualiserungenHerunterladenToolStripMenuItem;
        internal ToolStripMenuItem GespeichertesKennwortLöschenToolStripMenuItem;
        internal ToolStripMenuItem AutomatischNachAktualisierungenSuchenToolStripMenuItem;
        internal Label lblExtras;
        internal ToolStripSeparator ToolStripMenuItem4;
        internal ToolStripMenuItem AlteDatensicherungImportierenToolStripMenuItem;
        internal ToolStripMenuItem EreignisprotokollAnzeigenToolStripMenuItem;
        private ToolStripMenuItem btnResetUserId;
    }
}