using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmMain));
            this.ttMain = new System.Windows.Forms.ToolTip(this.components);
            this.picHome = new System.Windows.Forms.PictureBox();
            this.picHelp = new System.Windows.Forms.PictureBox();
            this.cmsHelp = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.HilfeUndSupportToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.AufAktualisierungenPrüfenToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.AutomatischNachAktualisierungenSuchenToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.BetaversionenÜberAktualiserungenHerunterladenToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ÜberBackupServiceHome3ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
            this.MitWindowsStartenToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolStripMenuItem4 = new System.Windows.Forms.ToolStripSeparator();
            this.EreignisprotokollAnzeigenToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.AlteDatensicherungImportierenToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ZurücksetzenToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.GespeichertesKennwortLöschenToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolStripMenuItem2 = new System.Windows.Forms.ToolStripSeparator();
            this.BackupServiceHome3BeendenToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.plMain = new System.Windows.Forms.Panel();
            this.lblHeadTitle = new System.Windows.Forms.Label();
            this.lblExtras = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.picHome)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picHelp)).BeginInit();
            this.cmsHelp.SuspendLayout();
            this.SuspendLayout();
            // 
            // ttMain
            // 
            this.ttMain.ToolTipIcon = System.Windows.Forms.ToolTipIcon.Info;
            this.ttMain.ToolTipTitle = "Quickhilfe";
            // 
            // picHome
            // 
            this.picHome.BackColor = System.Drawing.Color.Transparent;
            this.picHome.Image = global::BSH.Main.Properties.Resources.home_icon_48;
            this.picHome.Location = new System.Drawing.Point(22, 14);
            this.picHome.Margin = new System.Windows.Forms.Padding(4);
            this.picHome.Name = "picHome";
            this.picHome.Size = new System.Drawing.Size(33, 33);
            this.picHome.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picHome.TabIndex = 21;
            this.picHome.TabStop = false;
            this.ttMain.SetToolTip(this.picHome, "Zur Startseite wechseln.\r\n(Nur aktiviert, wenn nicht die Startseite angezeigt wir" +
        "d.)");
            this.picHome.Click += new System.EventHandler(this.picHome_Click);
            // 
            // picHelp
            // 
            this.picHelp.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.picHelp.BackColor = System.Drawing.Color.Transparent;
            this.picHelp.Cursor = System.Windows.Forms.Cursors.Hand;
            this.picHelp.Image = global::BSH.Main.Properties.Resources.help_icon_48;
            this.picHelp.Location = new System.Drawing.Point(986, 14);
            this.picHelp.Margin = new System.Windows.Forms.Padding(4);
            this.picHelp.Name = "picHelp";
            this.picHelp.Size = new System.Drawing.Size(33, 33);
            this.picHelp.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picHelp.TabIndex = 35;
            this.picHelp.TabStop = false;
            this.ttMain.SetToolTip(this.picHelp, "Extras und Support.\r\n\r\nHier haben Sie die Möglichkeit, sich Hilfe zu holen oder A" +
        "ktualisierungen\r\nvon Backup Service Home herunterzuladen.");
            this.picHelp.Click += new System.EventHandler(this.picHelp_Click);
            // 
            // cmsHelp
            // 
            this.cmsHelp.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.cmsHelp.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.HilfeUndSupportToolStripMenuItem,
            this.AufAktualisierungenPrüfenToolStripMenuItem,
            this.AutomatischNachAktualisierungenSuchenToolStripMenuItem,
            this.BetaversionenÜberAktualiserungenHerunterladenToolStripMenuItem,
            this.ÜberBackupServiceHome3ToolStripMenuItem,
            this.ToolStripMenuItem1,
            this.MitWindowsStartenToolStripMenuItem,
            this.ToolStripMenuItem4,
            this.EreignisprotokollAnzeigenToolStripMenuItem,
            this.AlteDatensicherungImportierenToolStripMenuItem,
            this.ZurücksetzenToolStripMenuItem,
            this.GespeichertesKennwortLöschenToolStripMenuItem,
            this.ToolStripMenuItem2,
            this.BackupServiceHome3BeendenToolStripMenuItem});
            this.cmsHelp.Name = "cmsHelp";
            this.cmsHelp.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.cmsHelp.Size = new System.Drawing.Size(423, 374);
            // 
            // HilfeUndSupportToolStripMenuItem
            // 
            this.HilfeUndSupportToolStripMenuItem.Name = "HilfeUndSupportToolStripMenuItem";
            this.HilfeUndSupportToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F1;
            this.HilfeUndSupportToolStripMenuItem.Size = new System.Drawing.Size(422, 32);
            this.HilfeUndSupportToolStripMenuItem.Text = "Hilfe und Support";
            this.HilfeUndSupportToolStripMenuItem.Click += new System.EventHandler(this.HilfeUndSupportToolStripMenuItem_Click);
            // 
            // AufAktualisierungenPrüfenToolStripMenuItem
            // 
            this.AufAktualisierungenPrüfenToolStripMenuItem.Name = "AufAktualisierungenPrüfenToolStripMenuItem";
            this.AufAktualisierungenPrüfenToolStripMenuItem.Size = new System.Drawing.Size(422, 32);
            this.AufAktualisierungenPrüfenToolStripMenuItem.Text = "Auf Aktualisierungen prüfen";
            this.AufAktualisierungenPrüfenToolStripMenuItem.Click += new System.EventHandler(this.AufAktualisierungenPrüfenToolStripMenuItem_Click);
            // 
            // AutomatischNachAktualisierungenSuchenToolStripMenuItem
            // 
            this.AutomatischNachAktualisierungenSuchenToolStripMenuItem.Name = "AutomatischNachAktualisierungenSuchenToolStripMenuItem";
            this.AutomatischNachAktualisierungenSuchenToolStripMenuItem.Size = new System.Drawing.Size(422, 32);
            this.AutomatischNachAktualisierungenSuchenToolStripMenuItem.Text = "Automatisch nach Aktualisierungen suchen";
            this.AutomatischNachAktualisierungenSuchenToolStripMenuItem.Click += new System.EventHandler(this.AutomatischNachAktualisierungenSuchenToolStripMenuItem_Click);
            // 
            // BetaversionenÜberAktualiserungenHerunterladenToolStripMenuItem
            // 
            this.BetaversionenÜberAktualiserungenHerunterladenToolStripMenuItem.CheckOnClick = true;
            this.BetaversionenÜberAktualiserungenHerunterladenToolStripMenuItem.Name = "BetaversionenÜberAktualiserungenHerunterladenToolStripMenuItem";
            this.BetaversionenÜberAktualiserungenHerunterladenToolStripMenuItem.Size = new System.Drawing.Size(422, 32);
            this.BetaversionenÜberAktualiserungenHerunterladenToolStripMenuItem.Text = "Betaversionen herunterladen";
            this.BetaversionenÜberAktualiserungenHerunterladenToolStripMenuItem.Visible = false;
            this.BetaversionenÜberAktualiserungenHerunterladenToolStripMenuItem.Click += new System.EventHandler(this.BetaversionenÜberAktualiserungenHerunterladenToolStripMenuItem_Click);
            // 
            // ÜberBackupServiceHome3ToolStripMenuItem
            // 
            this.ÜberBackupServiceHome3ToolStripMenuItem.Name = "ÜberBackupServiceHome3ToolStripMenuItem";
            this.ÜberBackupServiceHome3ToolStripMenuItem.Size = new System.Drawing.Size(422, 32);
            this.ÜberBackupServiceHome3ToolStripMenuItem.Text = "Über Backup Service Home 3";
            this.ÜberBackupServiceHome3ToolStripMenuItem.Click += new System.EventHandler(this.ÜberBackupServiceHome3ToolStripMenuItem_Click);
            // 
            // ToolStripMenuItem1
            // 
            this.ToolStripMenuItem1.Name = "ToolStripMenuItem1";
            this.ToolStripMenuItem1.Size = new System.Drawing.Size(419, 6);
            // 
            // MitWindowsStartenToolStripMenuItem
            // 
            this.MitWindowsStartenToolStripMenuItem.Name = "MitWindowsStartenToolStripMenuItem";
            this.MitWindowsStartenToolStripMenuItem.Size = new System.Drawing.Size(422, 32);
            this.MitWindowsStartenToolStripMenuItem.Text = "Mit Windows automatisch starten";
            this.MitWindowsStartenToolStripMenuItem.Click += new System.EventHandler(this.MitWindowsStartenToolStripMenuItem_Click);
            // 
            // ToolStripMenuItem4
            // 
            this.ToolStripMenuItem4.Name = "ToolStripMenuItem4";
            this.ToolStripMenuItem4.Size = new System.Drawing.Size(419, 6);
            // 
            // EreignisprotokollAnzeigenToolStripMenuItem
            // 
            this.EreignisprotokollAnzeigenToolStripMenuItem.Name = "EreignisprotokollAnzeigenToolStripMenuItem";
            this.EreignisprotokollAnzeigenToolStripMenuItem.Size = new System.Drawing.Size(422, 32);
            this.EreignisprotokollAnzeigenToolStripMenuItem.Text = "Ereignisprotokoll anzeigen";
            this.EreignisprotokollAnzeigenToolStripMenuItem.Click += new System.EventHandler(this.EreignisprotokollAnzeigenToolStripMenuItem_Click);
            // 
            // AlteDatensicherungImportierenToolStripMenuItem
            // 
            this.AlteDatensicherungImportierenToolStripMenuItem.Name = "AlteDatensicherungImportierenToolStripMenuItem";
            this.AlteDatensicherungImportierenToolStripMenuItem.Size = new System.Drawing.Size(422, 32);
            this.AlteDatensicherungImportierenToolStripMenuItem.Text = "Alte Datensicherung importieren";
            this.AlteDatensicherungImportierenToolStripMenuItem.Visible = false;
            // 
            // ZurücksetzenToolStripMenuItem
            // 
            this.ZurücksetzenToolStripMenuItem.Name = "ZurücksetzenToolStripMenuItem";
            this.ZurücksetzenToolStripMenuItem.Size = new System.Drawing.Size(422, 32);
            this.ZurücksetzenToolStripMenuItem.Text = "Zurücksetzen";
            this.ZurücksetzenToolStripMenuItem.Click += new System.EventHandler(this.ZurücksetzenToolStripMenuItem_Click);
            // 
            // GespeichertesKennwortLöschenToolStripMenuItem
            // 
            this.GespeichertesKennwortLöschenToolStripMenuItem.Name = "GespeichertesKennwortLöschenToolStripMenuItem";
            this.GespeichertesKennwortLöschenToolStripMenuItem.Size = new System.Drawing.Size(422, 32);
            this.GespeichertesKennwortLöschenToolStripMenuItem.Text = "Gespeichertes Kennwort löschen";
            this.GespeichertesKennwortLöschenToolStripMenuItem.Click += new System.EventHandler(this.GespeichertesKennwortLöschenToolStripMenuItem_Click);
            // 
            // ToolStripMenuItem2
            // 
            this.ToolStripMenuItem2.Name = "ToolStripMenuItem2";
            this.ToolStripMenuItem2.Size = new System.Drawing.Size(419, 6);
            // 
            // BackupServiceHome3BeendenToolStripMenuItem
            // 
            this.BackupServiceHome3BeendenToolStripMenuItem.Name = "BackupServiceHome3BeendenToolStripMenuItem";
            this.BackupServiceHome3BeendenToolStripMenuItem.Size = new System.Drawing.Size(422, 32);
            this.BackupServiceHome3BeendenToolStripMenuItem.Text = "Backup Service Home 3 beenden";
            this.BackupServiceHome3BeendenToolStripMenuItem.Click += new System.EventHandler(this.BackupServiceHome3BeendenToolStripMenuItem_Click);
            // 
            // plMain
            // 
            this.plMain.BackColor = System.Drawing.Color.White;
            this.plMain.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.plMain.Location = new System.Drawing.Point(0, 58);
            this.plMain.Margin = new System.Windows.Forms.Padding(4);
            this.plMain.Name = "plMain";
            this.plMain.Size = new System.Drawing.Size(1036, 570);
            this.plMain.TabIndex = 1;
            // 
            // lblHeadTitle
            // 
            this.lblHeadTitle.BackColor = System.Drawing.Color.Transparent;
            this.lblHeadTitle.Font = new System.Drawing.Font("Segoe UI", 9.75F);
            this.lblHeadTitle.Location = new System.Drawing.Point(64, 14);
            this.lblHeadTitle.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblHeadTitle.Name = "lblHeadTitle";
            this.lblHeadTitle.Size = new System.Drawing.Size(606, 33);
            this.lblHeadTitle.TabIndex = 0;
            this.lblHeadTitle.Text = "Backup Service Home";
            this.lblHeadTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblExtras
            // 
            this.lblExtras.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblExtras.BackColor = System.Drawing.Color.Transparent;
            this.lblExtras.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lblExtras.Font = new System.Drawing.Font("Segoe UI", 9.75F);
            this.lblExtras.Location = new System.Drawing.Point(768, 14);
            this.lblExtras.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblExtras.Name = "lblExtras";
            this.lblExtras.Size = new System.Drawing.Size(208, 33);
            this.lblExtras.TabIndex = 36;
            this.lblExtras.Text = "Extras und Support";
            this.lblExtras.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lblExtras.Click += new System.EventHandler(this.lblExtras_Click);
            // 
            // frmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(144F, 144F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(243)))), ((int)(((byte)(243)))), ((int)(((byte)(245)))));
            this.ClientSize = new System.Drawing.Size(1036, 628);
            this.Controls.Add(this.lblExtras);
            this.Controls.Add(this.picHome);
            this.Controls.Add(this.picHelp);
            this.Controls.Add(this.plMain);
            this.Controls.Add(this.lblHeadTitle);
            this.Font = new System.Drawing.Font("Segoe UI", 9.75F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MaximizeBox = false;
            this.Name = "frmMain";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Backup Service Home";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmMain_FormClosing);
            this.Load += new System.EventHandler(this.frmMain_Load);
            ((System.ComponentModel.ISupportInitialize)(this.picHome)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picHelp)).EndInit();
            this.cmsHelp.ResumeLayout(false);
            this.ResumeLayout(false);

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
    }
}