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
            picHome.BackColor = Color.Transparent;
            picHome.Image = global::BSH.Main.Properties.Resources.home_icon_48;
            picHome.Location = new Point(22, 14);
            picHome.Margin = new Padding(4);
            picHome.Name = "picHome";
            picHome.Size = new Size(32, 32);
            picHome.SizeMode = PictureBoxSizeMode.Zoom;
            picHome.TabIndex = 21;
            picHome.TabStop = false;
            ttMain.SetToolTip(picHome, "Zur Startseite wechseln.\r\n(Nur aktiviert, wenn nicht die Startseite angezeigt wird.)");
            picHome.Click += picHome_Click;
            // 
            // picHelp
            // 
            picHelp.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            picHelp.BackColor = Color.Transparent;
            picHelp.Cursor = Cursors.Hand;
            picHelp.Image = global::BSH.Main.Properties.Resources.help_icon_48;
            picHelp.Location = new Point(986, 14);
            picHelp.Margin = new Padding(4);
            picHelp.Name = "picHelp";
            picHelp.Size = new Size(32, 32);
            picHelp.SizeMode = PictureBoxSizeMode.Zoom;
            picHelp.TabIndex = 35;
            picHelp.TabStop = false;
            ttMain.SetToolTip(picHelp, "Extras und Support.\r\n\r\nHier haben Sie die Möglichkeit, sich Hilfe zu holen oder Aktualisierungen\r\nvon Backup Service Home herunterzuladen.");
            picHelp.Click += picHelp_Click;
            // 
            // cmsHelp
            // 
            cmsHelp.ImageScalingSize = new Size(20, 20);
            cmsHelp.Items.AddRange(new ToolStripItem[] { HilfeUndSupportToolStripMenuItem, AufAktualisierungenPrüfenToolStripMenuItem, AutomatischNachAktualisierungenSuchenToolStripMenuItem, BetaversionenÜberAktualiserungenHerunterladenToolStripMenuItem, ÜberBackupServiceHome3ToolStripMenuItem, ToolStripMenuItem1, MitWindowsStartenToolStripMenuItem, ToolStripMenuItem4, EreignisprotokollAnzeigenToolStripMenuItem, AlteDatensicherungImportierenToolStripMenuItem, ZurücksetzenToolStripMenuItem, GespeichertesKennwortLöschenToolStripMenuItem, btnResetUserId, ToolStripMenuItem2, BackupServiceHome3BeendenToolStripMenuItem });
            cmsHelp.Name = "cmsHelp";
            cmsHelp.RenderMode = ToolStripRenderMode.System;
            cmsHelp.Size = new Size(423, 439);
            // 
            // HilfeUndSupportToolStripMenuItem
            // 
            HilfeUndSupportToolStripMenuItem.Name = "HilfeUndSupportToolStripMenuItem";
            HilfeUndSupportToolStripMenuItem.ShortcutKeys = Keys.F1;
            HilfeUndSupportToolStripMenuItem.Size = new Size(422, 32);
            HilfeUndSupportToolStripMenuItem.Text = "Hilfe und Support";
            HilfeUndSupportToolStripMenuItem.Click += HilfeUndSupportToolStripMenuItem_Click;
            // 
            // AufAktualisierungenPrüfenToolStripMenuItem
            // 
            AufAktualisierungenPrüfenToolStripMenuItem.Name = "AufAktualisierungenPrüfenToolStripMenuItem";
            AufAktualisierungenPrüfenToolStripMenuItem.Size = new Size(422, 32);
            AufAktualisierungenPrüfenToolStripMenuItem.Text = "Auf Aktualisierungen prüfen";
            AufAktualisierungenPrüfenToolStripMenuItem.Click += AufAktualisierungenPrüfenToolStripMenuItem_Click;
            // 
            // AutomatischNachAktualisierungenSuchenToolStripMenuItem
            // 
            AutomatischNachAktualisierungenSuchenToolStripMenuItem.Name = "AutomatischNachAktualisierungenSuchenToolStripMenuItem";
            AutomatischNachAktualisierungenSuchenToolStripMenuItem.Size = new Size(422, 32);
            AutomatischNachAktualisierungenSuchenToolStripMenuItem.Text = "Automatisch nach Aktualisierungen suchen";
            AutomatischNachAktualisierungenSuchenToolStripMenuItem.Click += AutomatischNachAktualisierungenSuchenToolStripMenuItem_Click;
            // 
            // BetaversionenÜberAktualiserungenHerunterladenToolStripMenuItem
            // 
            BetaversionenÜberAktualiserungenHerunterladenToolStripMenuItem.CheckOnClick = true;
            BetaversionenÜberAktualiserungenHerunterladenToolStripMenuItem.Name = "BetaversionenÜberAktualiserungenHerunterladenToolStripMenuItem";
            BetaversionenÜberAktualiserungenHerunterladenToolStripMenuItem.Size = new Size(422, 32);
            BetaversionenÜberAktualiserungenHerunterladenToolStripMenuItem.Text = "Betaversionen herunterladen";
            BetaversionenÜberAktualiserungenHerunterladenToolStripMenuItem.Visible = false;
            BetaversionenÜberAktualiserungenHerunterladenToolStripMenuItem.Click += BetaversionenÜberAktualiserungenHerunterladenToolStripMenuItem_Click;
            // 
            // ÜberBackupServiceHome3ToolStripMenuItem
            // 
            ÜberBackupServiceHome3ToolStripMenuItem.Name = "ÜberBackupServiceHome3ToolStripMenuItem";
            ÜberBackupServiceHome3ToolStripMenuItem.Size = new Size(422, 32);
            ÜberBackupServiceHome3ToolStripMenuItem.Text = "Über Backup Service Home 3";
            ÜberBackupServiceHome3ToolStripMenuItem.Click += ÜberBackupServiceHome3ToolStripMenuItem_Click;
            // 
            // ToolStripMenuItem1
            // 
            ToolStripMenuItem1.Name = "ToolStripMenuItem1";
            ToolStripMenuItem1.Size = new Size(419, 6);
            // 
            // MitWindowsStartenToolStripMenuItem
            // 
            MitWindowsStartenToolStripMenuItem.Name = "MitWindowsStartenToolStripMenuItem";
            MitWindowsStartenToolStripMenuItem.Size = new Size(422, 32);
            MitWindowsStartenToolStripMenuItem.Text = "Mit Windows automatisch starten";
            MitWindowsStartenToolStripMenuItem.Click += MitWindowsStartenToolStripMenuItem_Click;
            // 
            // ToolStripMenuItem4
            // 
            ToolStripMenuItem4.Name = "ToolStripMenuItem4";
            ToolStripMenuItem4.Size = new Size(419, 6);
            // 
            // EreignisprotokollAnzeigenToolStripMenuItem
            // 
            EreignisprotokollAnzeigenToolStripMenuItem.Name = "EreignisprotokollAnzeigenToolStripMenuItem";
            EreignisprotokollAnzeigenToolStripMenuItem.Size = new Size(422, 32);
            EreignisprotokollAnzeigenToolStripMenuItem.Text = "Ereignisprotokoll anzeigen";
            EreignisprotokollAnzeigenToolStripMenuItem.Click += EreignisprotokollAnzeigenToolStripMenuItem_Click;
            // 
            // AlteDatensicherungImportierenToolStripMenuItem
            // 
            AlteDatensicherungImportierenToolStripMenuItem.Name = "AlteDatensicherungImportierenToolStripMenuItem";
            AlteDatensicherungImportierenToolStripMenuItem.Size = new Size(422, 32);
            AlteDatensicherungImportierenToolStripMenuItem.Text = "Alte Datensicherung importieren";
            AlteDatensicherungImportierenToolStripMenuItem.Visible = false;
            // 
            // ZurücksetzenToolStripMenuItem
            // 
            ZurücksetzenToolStripMenuItem.Name = "ZurücksetzenToolStripMenuItem";
            ZurücksetzenToolStripMenuItem.Size = new Size(422, 32);
            ZurücksetzenToolStripMenuItem.Text = "Zurücksetzen";
            ZurücksetzenToolStripMenuItem.Click += ZurücksetzenToolStripMenuItem_Click;
            // 
            // GespeichertesKennwortLöschenToolStripMenuItem
            // 
            GespeichertesKennwortLöschenToolStripMenuItem.Name = "GespeichertesKennwortLöschenToolStripMenuItem";
            GespeichertesKennwortLöschenToolStripMenuItem.Size = new Size(422, 32);
            GespeichertesKennwortLöschenToolStripMenuItem.Text = "Gespeichertes Kennwort löschen";
            GespeichertesKennwortLöschenToolStripMenuItem.Click += GespeichertesKennwortLöschenToolStripMenuItem_Click;
            // 
            // btnResetUserId
            // 
            btnResetUserId.Name = "btnResetUserId";
            btnResetUserId.Size = new Size(422, 32);
            btnResetUserId.Text = "Eindeutige Benutzerkennung zurücksetzen";
            btnResetUserId.ToolTipText = "Setzt die eindeutige Benutzerkennung für die Aktualisierungsfunktion zurück.";
            btnResetUserId.Click += btnResetUserId_Click;
            // 
            // ToolStripMenuItem2
            // 
            ToolStripMenuItem2.Name = "ToolStripMenuItem2";
            ToolStripMenuItem2.Size = new Size(419, 6);
            // 
            // BackupServiceHome3BeendenToolStripMenuItem
            // 
            BackupServiceHome3BeendenToolStripMenuItem.Name = "BackupServiceHome3BeendenToolStripMenuItem";
            BackupServiceHome3BeendenToolStripMenuItem.Size = new Size(422, 32);
            BackupServiceHome3BeendenToolStripMenuItem.Text = "Backup Service Home 3 beenden";
            BackupServiceHome3BeendenToolStripMenuItem.Click += BackupServiceHome3BeendenToolStripMenuItem_Click;
            // 
            // plMain
            // 
            plMain.BackColor = Color.White;
            plMain.Dock = DockStyle.Bottom;
            plMain.Location = new Point(0, 58);
            plMain.Margin = new Padding(4);
            plMain.Name = "plMain";
            plMain.Size = new Size(1036, 570);
            plMain.TabIndex = 1;
            // 
            // lblHeadTitle
            // 
            lblHeadTitle.BackColor = Color.Transparent;
            lblHeadTitle.Font = new Font("Segoe UI", 9.75F, FontStyle.Regular, GraphicsUnit.Point);
            lblHeadTitle.Location = new Point(64, 14);
            lblHeadTitle.Margin = new Padding(4, 0, 4, 0);
            lblHeadTitle.Name = "lblHeadTitle";
            lblHeadTitle.Size = new Size(606, 33);
            lblHeadTitle.TabIndex = 0;
            lblHeadTitle.Text = "Backup Service Home";
            lblHeadTitle.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblExtras
            // 
            lblExtras.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lblExtras.BackColor = Color.Transparent;
            lblExtras.Cursor = Cursors.Hand;
            lblExtras.Font = new Font("Segoe UI", 9.75F, FontStyle.Regular, GraphicsUnit.Point);
            lblExtras.Location = new Point(768, 14);
            lblExtras.Margin = new Padding(4, 0, 4, 0);
            lblExtras.Name = "lblExtras";
            lblExtras.Size = new Size(208, 33);
            lblExtras.TabIndex = 36;
            lblExtras.Text = "Extras und Support";
            lblExtras.TextAlign = ContentAlignment.MiddleRight;
            lblExtras.Click += lblExtras_Click;
            // 
            // frmMain
            // 
            AutoScaleDimensions = new SizeF(144F, 144F);
            AutoScaleMode = AutoScaleMode.Dpi;
            BackColor = Color.FromArgb(243, 243, 245);
            ClientSize = new Size(1036, 628);
            Controls.Add(lblExtras);
            Controls.Add(picHome);
            Controls.Add(picHelp);
            Controls.Add(plMain);
            Controls.Add(lblHeadTitle);
            Font = new Font("Segoe UI", 9.75F, FontStyle.Regular, GraphicsUnit.Point);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Icon = (Icon)resources.GetObject("$this.Icon");
            Margin = new Padding(4);
            MaximizeBox = false;
            Name = "frmMain";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Backup Service Home";
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