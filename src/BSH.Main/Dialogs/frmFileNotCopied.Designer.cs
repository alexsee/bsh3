using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using BSH.Main.Properties;

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
            llCopyToClipboard = new LinkLabel();
            Panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)picIcon).BeginInit();
            cmdPopup.SuspendLayout();
            SuspendLayout();
            // 
            // Panel1
            // 
            resources.ApplyResources(Panel1, "Panel1");
            Panel1.BackColor = SystemColors.Control;
            Panel1.Controls.Add(Label1);
            Panel1.Controls.Add(cmdCancel);
            Panel1.Name = "Panel1";
            // 
            // Label1
            // 
            resources.ApplyResources(Label1, "Label1");
            Label1.BackColor = Color.DarkGray;
            Label1.Name = "Label1";
            // 
            // cmdCancel
            // 
            resources.ApplyResources(cmdCancel, "cmdCancel");
            cmdCancel.DialogResult = DialogResult.OK;
            cmdCancel.Name = "cmdCancel";
            cmdCancel.UseVisualStyleBackColor = true;
            // 
            // Label2
            // 
            resources.ApplyResources(Label2, "Label2");
            Label2.ForeColor = Color.FromArgb(0, 51, 153);
            Label2.Name = "Label2";
            // 
            // lblText
            // 
            resources.ApplyResources(lblText, "lblText");
            lblText.Name = "lblText";
            // 
            // picIcon
            // 
            resources.ApplyResources(picIcon, "picIcon");
            picIcon.Image = Resources.error_icon_48;
            picIcon.Name = "picIcon";
            picIcon.TabStop = false;
            // 
            // lvFiles
            // 
            resources.ApplyResources(lvFiles, "lvFiles");
            lvFiles.Columns.AddRange(new ColumnHeader[] { ColumnHeader1, ColumnHeader2 });
            lvFiles.ContextMenuStrip = cmdPopup;
            lvFiles.FullRowSelect = true;
            lvFiles.Name = "lvFiles";
            lvFiles.UseCompatibleStateImageBehavior = false;
            lvFiles.View = View.Details;
            // 
            // ColumnHeader1
            // 
            resources.ApplyResources(ColumnHeader1, "ColumnHeader1");
            // 
            // ColumnHeader2
            // 
            resources.ApplyResources(ColumnHeader2, "ColumnHeader2");
            // 
            // cmdPopup
            // 
            resources.ApplyResources(cmdPopup, "cmdPopup");
            cmdPopup.ImageScalingSize = new Size(24, 24);
            cmdPopup.Items.AddRange(new ToolStripItem[] { KopierenToolStripMenuItem });
            cmdPopup.Name = "cmdPopup";
            // 
            // KopierenToolStripMenuItem
            // 
            resources.ApplyResources(KopierenToolStripMenuItem, "KopierenToolStripMenuItem");
            KopierenToolStripMenuItem.Name = "KopierenToolStripMenuItem";
            KopierenToolStripMenuItem.Click += KopierenToolStripMenuItem_Click;
            // 
            // llCopyToClipboard
            // 
            resources.ApplyResources(llCopyToClipboard, "llCopyToClipboard");
            llCopyToClipboard.Name = "llCopyToClipboard";
            llCopyToClipboard.TabStop = true;
            llCopyToClipboard.LinkClicked += llCopyToClipboard_LinkClicked;
            // 
            // frmFileNotCopied
            // 
            resources.ApplyResources(this, "$this");
            AutoScaleMode = AutoScaleMode.Dpi;
            BackColor = Color.White;
            Controls.Add(llCopyToClipboard);
            Controls.Add(lvFiles);
            Controls.Add(picIcon);
            Controls.Add(lblText);
            Controls.Add(Label2);
            Controls.Add(Panel1);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "frmFileNotCopied";
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