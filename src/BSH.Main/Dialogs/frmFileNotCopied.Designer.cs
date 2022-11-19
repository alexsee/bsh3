using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace Brightbits.BSH.Main
{
    public partial class frmFileNotCopied : Form
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
            var resources = new System.ComponentModel.ComponentResourceManager(typeof(frmFileNotCopied));
            Panel1 = new Panel();
            Label1 = new Label();
            cmdCancel = new Button();
            Label2 = new Label();
            lblText = new Label();
            picIcon = new PictureBox();
            lvFiles = new ListView();
            ColumnHeader1 = new ColumnHeader();
            ColumnHeader2 = new ColumnHeader();
            cmdPopup = new ContextMenuStrip(components);
            KopierenToolStripMenuItem = new ToolStripMenuItem();
            KopierenToolStripMenuItem.Click += new EventHandler(KopierenToolStripMenuItem_Click);
            llCopyToClipboard = new LinkLabel();
            llCopyToClipboard.LinkClicked += new LinkLabelLinkClickedEventHandler(llCopyToClipboard_LinkClicked);
            Panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)picIcon).BeginInit();
            cmdPopup.SuspendLayout();
            SuspendLayout();
            // 
            // Panel1
            // 
            Panel1.BackColor = SystemColors.Control;
            Panel1.Controls.Add(Label1);
            Panel1.Controls.Add(cmdCancel);
            Panel1.Dock = DockStyle.Bottom;
            Panel1.Location = new Point(0, 597);
            Panel1.Name = "Panel1";
            Panel1.Size = new Size(864, 68);
            Panel1.TabIndex = 82;
            // 
            // Label1
            // 
            Label1.BackColor = Color.DarkGray;
            Label1.Dock = DockStyle.Top;
            Label1.Location = new Point(0, 0);
            Label1.Name = "Label1";
            Label1.Size = new Size(864, 2);
            Label1.TabIndex = 5;
            // 
            // cmdCancel
            // 
            cmdCancel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            cmdCancel.DialogResult = DialogResult.OK;
            cmdCancel.Font = new Font("Segoe UI", 9.75f, FontStyle.Regular, GraphicsUnit.Point);
            cmdCancel.Location = new Point(705, 12);
            cmdCancel.Name = "cmdCancel";
            cmdCancel.Size = new Size(140, 39);
            cmdCancel.TabIndex = 1;
            cmdCancel.Text = "&OK";
            cmdCancel.UseVisualStyleBackColor = true;
            // 
            // Label2
            // 
            Label2.AutoSize = true;
            Label2.Font = new Font("Segoe UI", 12.0f, FontStyle.Regular, GraphicsUnit.Point);
            Label2.ForeColor = Color.FromArgb(0, 51, 153);
            Label2.Location = new Point(15, 18);
            Label2.Name = "Label2";
            Label2.Size = new Size(421, 32);
            Label2.TabIndex = 84;
            Label2.Text = "Dateien konnten nicht kopiert werden";
            // 
            // lblText
            // 
            lblText.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            lblText.Location = new Point(87, 68);
            lblText.Name = "lblText";
            lblText.Size = new Size(758, 87);
            lblText.TabIndex = 85;
            lblText.Text = "Es konnten nicht alle Dateien korrekt gesichert werden. Beheben Sie die Probleme " + "und starten Sie eine erneute Sicherung. Die betroffenen Dateien wurden dem Proto" + "koll hinzugefügt.";
            // 
            // picIcon
            // 
            picIcon.Image = (Image)resources.GetObject("picIcon.Image");
            picIcon.Location = new Point(21, 68);
            picIcon.Name = "picIcon";
            picIcon.Size = new Size(48, 48);
            picIcon.SizeMode = PictureBoxSizeMode.Zoom;
            picIcon.TabIndex = 86;
            picIcon.TabStop = false;
            // 
            // lvFiles
            // 
            lvFiles.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;

            lvFiles.Columns.AddRange(new ColumnHeader[] { ColumnHeader1, ColumnHeader2 });
            lvFiles.ContextMenuStrip = cmdPopup;
            lvFiles.FullRowSelect = true;
            lvFiles.HideSelection = false;
            lvFiles.Location = new Point(20, 158);
            lvFiles.Name = "lvFiles";
            lvFiles.Size = new Size(823, 385);
            lvFiles.TabIndex = 87;
            lvFiles.UseCompatibleStateImageBehavior = false;
            lvFiles.View = View.Details;
            // 
            // ColumnHeader1
            // 
            ColumnHeader1.Text = "Datei";
            ColumnHeader1.Width = 359;
            // 
            // ColumnHeader2
            // 
            ColumnHeader2.Text = "Problem";
            ColumnHeader2.Width = 319;
            // 
            // cmdPopup
            // 
            cmdPopup.ImageScalingSize = new Size(24, 24);
            cmdPopup.Items.AddRange(new ToolStripItem[] { KopierenToolStripMenuItem });
            cmdPopup.Name = "cmdPopup";
            cmdPopup.Size = new Size(155, 36);
            // 
            // KopierenToolStripMenuItem
            // 
            KopierenToolStripMenuItem.Name = "KopierenToolStripMenuItem";
            KopierenToolStripMenuItem.Size = new Size(154, 32);
            KopierenToolStripMenuItem.Text = "&Kopieren";
            // 
            // llCopyToClipboard
            // 
            llCopyToClipboard.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            llCopyToClipboard.AutoSize = true;
            llCopyToClipboard.Location = new Point(17, 559);
            llCopyToClipboard.Name = "llCopyToClipboard";
            llCopyToClipboard.Size = new Size(374, 24);
            llCopyToClipboard.TabIndex = 88;
            llCopyToClipboard.TabStop = true;
            llCopyToClipboard.Text = "Problembericht in Zwischenablage kopieren";
            // 
            // frmFileNotCopied
            // 
            AutoScaleDimensions = new SizeF(144.0f, 144.0f);
            AutoScaleMode = AutoScaleMode.Dpi;
            BackColor = Color.White;
            ClientSize = new Size(864, 665);
            Controls.Add(llCopyToClipboard);
            Controls.Add(lvFiles);
            Controls.Add(picIcon);
            Controls.Add(lblText);
            Controls.Add(Label2);
            Controls.Add(Panel1);
            Font = new Font("Calibri", 9.75f, FontStyle.Regular, GraphicsUnit.Point);
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            MinimizeBox = false;
            MinimumSize = new Size(602, 474);
            Name = "frmFileNotCopied";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Problembericht";
            Panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)picIcon).EndInit();
            cmdPopup.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        internal Panel Panel1;
        internal Label Label1;
        internal Button cmdCancel;
        internal Label Label2;
        internal Label lblText;
        internal PictureBox picIcon;
        internal ListView lvFiles;
        internal ColumnHeader ColumnHeader1;
        internal ColumnHeader ColumnHeader2;
        internal ContextMenuStrip cmdPopup;
        internal ToolStripMenuItem KopierenToolStripMenuItem;
        internal LinkLabel llCopyToClipboard;
    }
}