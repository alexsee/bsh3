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
            picHome.Click += new EventHandler(picHome_Click);
            picHelp = new PictureBox();
            picHelp.Click += new EventHandler(picHelp_Click);
            cmsHelp = new ContextMenuStrip(components);
            HilfeUndSupportToolStripMenuItem = new ToolStripMenuItem();
            HilfeUndSupportToolStripMenuItem.Click += new EventHandler(HilfeUndSupportToolStripMenuItem_Click);
            AufAktualisierungenPrüfenToolStripMenuItem = new ToolStripMenuItem();
            AufAktualisierungenPrüfenToolStripMenuItem.Click += new EventHandler(AufAktualisierungenPrüfenToolStripMenuItem_Click);
            AutomatischNachAktualisierungenSuchenToolStripMenuItem = new ToolStripMenuItem();
            AutomatischNachAktualisierungenSuchenToolStripMenuItem.Click += new EventHandler(AutomatischNachAktualisierungenSuchenToolStripMenuItem_Click);
            BetaversionenÜberAktualiserungenHerunterladenToolStripMenuItem = new ToolStripMenuItem();
            BetaversionenÜberAktualiserungenHerunterladenToolStripMenuItem.Click += new EventHandler(BetaversionenÜberAktualiserungenHerunterladenToolStripMenuItem_Click);
            ÜberBackupServiceHome3ToolStripMenuItem = new ToolStripMenuItem();
            ÜberBackupServiceHome3ToolStripMenuItem.Click += new EventHandler(ÜberBackupServiceHome3ToolStripMenuItem_Click);
            ToolStripMenuItem1 = new ToolStripSeparator();
            MitWindowsStartenToolStripMenuItem = new ToolStripMenuItem();
            MitWindowsStartenToolStripMenuItem.Click += new EventHandler(MitWindowsStartenToolStripMenuItem_Click);
            ToolStripMenuItem4 = new ToolStripSeparator();
            AlteDatensicherungImportierenToolStripMenuItem = new ToolStripMenuItem();
            ZurücksetzenToolStripMenuItem = new ToolStripMenuItem();
            ZurücksetzenToolStripMenuItem.Click += new EventHandler(ZurücksetzenToolStripMenuItem_Click);
            GespeichertesKennwortLöschenToolStripMenuItem = new ToolStripMenuItem();
            GespeichertesKennwortLöschenToolStripMenuItem.Click += new EventHandler(GespeichertesKennwortLöschenToolStripMenuItem_Click);
            ToolStripMenuItem2 = new ToolStripSeparator();
            BackupServiceHome3BeendenToolStripMenuItem = new ToolStripMenuItem();
            BackupServiceHome3BeendenToolStripMenuItem.Click += new EventHandler(BackupServiceHome3BeendenToolStripMenuItem_Click);
            plMain = new Panel();
            lblHeadTitle = new Label();
            lblExtras = new Label();
            lblExtras.Click += new EventHandler(lblExtras_Click);
            EreignisprotokollAnzeigenToolStripMenuItem = new ToolStripMenuItem();
            EreignisprotokollAnzeigenToolStripMenuItem.Click += new EventHandler(EreignisprotokollAnzeigenToolStripMenuItem_Click);
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
            picHome.Image = global::BSH.Main.Properties.Resources.home_4_line;
            picHome.Location = new Point(22, 14);
            picHome.Margin = new Padding(4);
            picHome.Name = "picHome";
            picHome.Size = new Size(33, 33);
            picHome.SizeMode = PictureBoxSizeMode.Zoom;
            picHome.TabIndex = 21;
            picHome.TabStop = false;
            ttMain.SetToolTip(picHome, "Zur Startseite wechseln." + '\r' + '\n' + "(Nur aktiviert, wenn nicht die Startseite angezeigt wir" + "d.)");
            // 
            // picHelp
            // 
            picHelp.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            picHelp.BackColor = Color.Transparent;
            picHelp.Cursor = Cursors.Hand;
            picHelp.Image = global::BSH.Main.Properties.Resources.question_line;
            picHelp.Location = new Point(986, 14);
            picHelp.Margin = new Padding(4);
            picHelp.Name = "picHelp";
            picHelp.Size = new Size(33, 33);
            picHelp.SizeMode = PictureBoxSizeMode.Zoom;
            picHelp.TabIndex = 35;
            picHelp.TabStop = false;
            ttMain.SetToolTip(picHelp, "Extras und Support." + '\r' + '\n' + '\r' + '\n' + "Hier haben Sie die Möglichkeit, sich Hilfe zu holen oder A" + "ktualisierungen" + '\r' + '\n' + "von Backup Service Home herunterzuladen.");
            // 
            // cmsHelp
            // 
            cmsHelp.ImageScalingSize = new Size(20, 20);
            cmsHelp.Items.AddRange(new ToolStripItem[] { HilfeUndSupportToolStripMenuItem, AufAktualisierungenPrüfenToolStripMenuItem, AutomatischNachAktualisierungenSuchenToolStripMenuItem, BetaversionenÜberAktualiserungenHerunterladenToolStripMenuItem, ÜberBackupServiceHome3ToolStripMenuItem, ToolStripMenuItem1, MitWindowsStartenToolStripMenuItem, ToolStripMenuItem4, EreignisprotokollAnzeigenToolStripMenuItem, AlteDatensicherungImportierenToolStripMenuItem, ZurücksetzenToolStripMenuItem, GespeichertesKennwortLöschenToolStripMenuItem, ToolStripMenuItem2, BackupServiceHome3BeendenToolStripMenuItem });
            cmsHelp.Name = "cmsHelp";
            cmsHelp.RenderMode = ToolStripRenderMode.System;
            cmsHelp.Size = new Size(423, 407);
            // 
            // HilfeUndSupportToolStripMenuItem
            // 
            HilfeUndSupportToolStripMenuItem.Name = "HilfeUndSupportToolStripMenuItem";
            HilfeUndSupportToolStripMenuItem.ShortcutKeys = Keys.F1;
            HilfeUndSupportToolStripMenuItem.Size = new Size(422, 32);
            HilfeUndSupportToolStripMenuItem.Text = "Hilfe und Support";
            // 
            // AufAktualisierungenPrüfenToolStripMenuItem
            // 
            AufAktualisierungenPrüfenToolStripMenuItem.Name = "AufAktualisierungenPrüfenToolStripMenuItem";
            AufAktualisierungenPrüfenToolStripMenuItem.Size = new Size(422, 32);
            AufAktualisierungenPrüfenToolStripMenuItem.Text = "Auf Aktualisierungen prüfen";
            // 
            // AutomatischNachAktualisierungenSuchenToolStripMenuItem
            // 
            AutomatischNachAktualisierungenSuchenToolStripMenuItem.Name = "AutomatischNachAktualisierungenSuchenToolStripMenuItem";
            AutomatischNachAktualisierungenSuchenToolStripMenuItem.Size = new Size(422, 32);
            AutomatischNachAktualisierungenSuchenToolStripMenuItem.Text = "Automatisch nach Aktualisierungen suchen";
            // 
            // BetaversionenÜberAktualiserungenHerunterladenToolStripMenuItem
            // 
            BetaversionenÜberAktualiserungenHerunterladenToolStripMenuItem.CheckOnClick = true;
            BetaversionenÜberAktualiserungenHerunterladenToolStripMenuItem.Name = "BetaversionenÜberAktualiserungenHerunterladenToolStripMenuItem";
            BetaversionenÜberAktualiserungenHerunterladenToolStripMenuItem.Size = new Size(422, 32);
            BetaversionenÜberAktualiserungenHerunterladenToolStripMenuItem.Text = "Betaversionen herunterladen";
            BetaversionenÜberAktualiserungenHerunterladenToolStripMenuItem.Visible = false;
            // 
            // ÜberBackupServiceHome3ToolStripMenuItem
            // 
            ÜberBackupServiceHome3ToolStripMenuItem.Name = "ÜberBackupServiceHome3ToolStripMenuItem";
            ÜberBackupServiceHome3ToolStripMenuItem.Size = new Size(422, 32);
            ÜberBackupServiceHome3ToolStripMenuItem.Text = "Über Backup Service Home 3";
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
            // 
            // ToolStripMenuItem4
            // 
            ToolStripMenuItem4.Name = "ToolStripMenuItem4";
            ToolStripMenuItem4.Size = new Size(419, 6);
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
            // 
            // GespeichertesKennwortLöschenToolStripMenuItem
            // 
            GespeichertesKennwortLöschenToolStripMenuItem.Name = "GespeichertesKennwortLöschenToolStripMenuItem";
            GespeichertesKennwortLöschenToolStripMenuItem.Size = new Size(422, 32);
            GespeichertesKennwortLöschenToolStripMenuItem.Text = "Gespeichertes Kennwort löschen";
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
            lblHeadTitle.Font = new Font("Segoe UI", 9.75f, FontStyle.Regular, GraphicsUnit.Point);
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
            lblExtras.Font = new Font("Segoe UI", 9.75f, FontStyle.Regular, GraphicsUnit.Point);
            lblExtras.Location = new Point(768, 14);
            lblExtras.Margin = new Padding(4, 0, 4, 0);
            lblExtras.Name = "lblExtras";
            lblExtras.Size = new Size(208, 33);
            lblExtras.TabIndex = 36;
            lblExtras.Text = "Extras und Support";
            lblExtras.TextAlign = ContentAlignment.MiddleRight;
            // 
            // EreignisprotokollAnzeigenToolStripMenuItem
            // 
            EreignisprotokollAnzeigenToolStripMenuItem.Name = "EreignisprotokollAnzeigenToolStripMenuItem";
            EreignisprotokollAnzeigenToolStripMenuItem.Size = new Size(422, 32);
            EreignisprotokollAnzeigenToolStripMenuItem.Text = "Ereignisprotokoll anzeigen";
            // 
            // frmMain
            // 
            AutoScaleDimensions = new SizeF(144.0f, 144.0f);
            AutoScaleMode = AutoScaleMode.Dpi;
            BackColor = Color.FromArgb(230, 230, 230);
            ClientSize = new Size(1036, 628);
            Controls.Add(lblExtras);
            Controls.Add(picHome);
            Controls.Add(picHelp);
            Controls.Add(plMain);
            Controls.Add(lblHeadTitle);
            Font = new Font("Segoe UI", 9.75f, FontStyle.Regular, GraphicsUnit.Point);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Icon = (Icon)resources.GetObject("$this.Icon");
            Margin = new Padding(4);
            MaximizeBox = false;
            Name = "frmMain";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Backup Service Home";
            ((System.ComponentModel.ISupportInitialize)picHome).EndInit();
            ((System.ComponentModel.ISupportInitialize)picHelp).EndInit();
            cmsHelp.ResumeLayout(false);
            FormClosing += new FormClosingEventHandler(frmMain_FormClosing);
            Load += new EventHandler(frmMain_Load);
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
    }
}