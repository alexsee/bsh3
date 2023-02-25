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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmFileNotCopied));
            this.Panel1 = new System.Windows.Forms.Panel();
            this.Label1 = new System.Windows.Forms.Label();
            this.cmdCancel = new System.Windows.Forms.Button();
            this.Label2 = new System.Windows.Forms.Label();
            this.lblText = new System.Windows.Forms.Label();
            this.picIcon = new System.Windows.Forms.PictureBox();
            this.lvFiles = new System.Windows.Forms.ListView();
            this.ColumnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.ColumnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.cmdPopup = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.KopierenToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.llCopyToClipboard = new System.Windows.Forms.LinkLabel();
            this.Panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picIcon)).BeginInit();
            this.cmdPopup.SuspendLayout();
            this.SuspendLayout();
            // 
            // Panel1
            // 
            this.Panel1.BackColor = System.Drawing.SystemColors.Control;
            this.Panel1.Controls.Add(this.Label1);
            this.Panel1.Controls.Add(this.cmdCancel);
            this.Panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.Panel1.Location = new System.Drawing.Point(0, 597);
            this.Panel1.Name = "Panel1";
            this.Panel1.Size = new System.Drawing.Size(864, 68);
            this.Panel1.TabIndex = 82;
            // 
            // Label1
            // 
            this.Label1.BackColor = System.Drawing.Color.DarkGray;
            this.Label1.Dock = System.Windows.Forms.DockStyle.Top;
            this.Label1.Location = new System.Drawing.Point(0, 0);
            this.Label1.Name = "Label1";
            this.Label1.Size = new System.Drawing.Size(864, 2);
            this.Label1.TabIndex = 5;
            // 
            // cmdCancel
            // 
            this.cmdCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdCancel.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.cmdCancel.Font = new System.Drawing.Font("Segoe UI", 9.75F);
            this.cmdCancel.Location = new System.Drawing.Point(705, 12);
            this.cmdCancel.Name = "cmdCancel";
            this.cmdCancel.Size = new System.Drawing.Size(140, 39);
            this.cmdCancel.TabIndex = 1;
            this.cmdCancel.Text = "&OK";
            this.cmdCancel.UseVisualStyleBackColor = true;
            // 
            // Label2
            // 
            this.Label2.AutoSize = true;
            this.Label2.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.Label2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(153)))));
            this.Label2.Location = new System.Drawing.Point(15, 18);
            this.Label2.Name = "Label2";
            this.Label2.Size = new System.Drawing.Size(421, 32);
            this.Label2.TabIndex = 84;
            this.Label2.Text = "Dateien konnten nicht kopiert werden";
            // 
            // lblText
            // 
            this.lblText.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblText.Location = new System.Drawing.Point(87, 68);
            this.lblText.Name = "lblText";
            this.lblText.Size = new System.Drawing.Size(758, 87);
            this.lblText.TabIndex = 85;
            this.lblText.Text = "Es konnten nicht alle Dateien korrekt gesichert werden. Beheben Sie die Probleme " +
    "und starten Sie eine erneute Sicherung. Die betroffenen Dateien wurden dem Proto" +
    "koll hinzugefügt.";
            // 
            // picIcon
            // 
            this.picIcon.Image = global::BSH.Main.Properties.Resources.error_icon_48;
            this.picIcon.Location = new System.Drawing.Point(21, 68);
            this.picIcon.Name = "picIcon";
            this.picIcon.Size = new System.Drawing.Size(48, 48);
            this.picIcon.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picIcon.TabIndex = 86;
            this.picIcon.TabStop = false;
            // 
            // lvFiles
            // 
            this.lvFiles.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lvFiles.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.ColumnHeader1,
            this.ColumnHeader2});
            this.lvFiles.ContextMenuStrip = this.cmdPopup;
            this.lvFiles.FullRowSelect = true;
            this.lvFiles.HideSelection = false;
            this.lvFiles.Location = new System.Drawing.Point(20, 158);
            this.lvFiles.Name = "lvFiles";
            this.lvFiles.Size = new System.Drawing.Size(823, 385);
            this.lvFiles.TabIndex = 87;
            this.lvFiles.UseCompatibleStateImageBehavior = false;
            this.lvFiles.View = System.Windows.Forms.View.Details;
            // 
            // ColumnHeader1
            // 
            this.ColumnHeader1.Text = "Datei";
            this.ColumnHeader1.Width = 359;
            // 
            // ColumnHeader2
            // 
            this.ColumnHeader2.Text = "Problem";
            this.ColumnHeader2.Width = 319;
            // 
            // cmdPopup
            // 
            this.cmdPopup.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.cmdPopup.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.KopierenToolStripMenuItem});
            this.cmdPopup.Name = "cmdPopup";
            this.cmdPopup.Size = new System.Drawing.Size(155, 36);
            // 
            // KopierenToolStripMenuItem
            // 
            this.KopierenToolStripMenuItem.Name = "KopierenToolStripMenuItem";
            this.KopierenToolStripMenuItem.Size = new System.Drawing.Size(154, 32);
            this.KopierenToolStripMenuItem.Text = "&Kopieren";
            this.KopierenToolStripMenuItem.Click += new System.EventHandler(this.KopierenToolStripMenuItem_Click);
            // 
            // llCopyToClipboard
            // 
            this.llCopyToClipboard.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.llCopyToClipboard.AutoSize = true;
            this.llCopyToClipboard.Location = new System.Drawing.Point(17, 559);
            this.llCopyToClipboard.Name = "llCopyToClipboard";
            this.llCopyToClipboard.Size = new System.Drawing.Size(374, 24);
            this.llCopyToClipboard.TabIndex = 88;
            this.llCopyToClipboard.TabStop = true;
            this.llCopyToClipboard.Text = "Problembericht in Zwischenablage kopieren";
            this.llCopyToClipboard.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.llCopyToClipboard_LinkClicked);
            // 
            // frmFileNotCopied
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(144F, 144F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(864, 665);
            this.Controls.Add(this.llCopyToClipboard);
            this.Controls.Add(this.lvFiles);
            this.Controls.Add(this.picIcon);
            this.Controls.Add(this.lblText);
            this.Controls.Add(this.Label2);
            this.Controls.Add(this.Panel1);
            this.Font = new System.Drawing.Font("Calibri", 9.75F);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(602, 474);
            this.Name = "frmFileNotCopied";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Problembericht";
            this.Panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.picIcon)).EndInit();
            this.cmdPopup.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

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