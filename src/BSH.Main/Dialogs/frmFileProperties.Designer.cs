using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace Brightbits.BSH.Main
{
    public partial class frmFileProperties : Form
    {

        // Das Formular überschreibt den Löschvorgang, um die Komponentenliste zu bereinigen.
        [DebuggerNonUserCode()]
        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing && components is object)
                {
                    components.Dispose();
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmFileProperties));
            this.TabControl1 = new System.Windows.Forms.TabControl();
            this.TabPage1 = new System.Windows.Forms.TabPage();
            this.lblFileModified = new System.Windows.Forms.Label();
            this.lblFileCreated = new System.Windows.Forms.Label();
            this.lblFileSize = new System.Windows.Forms.Label();
            this.lblFilePath = new System.Windows.Forms.Label();
            this.lblFileType = new System.Windows.Forms.Label();
            this.Label7 = new System.Windows.Forms.Label();
            this.Label6 = new System.Windows.Forms.Label();
            this.Label5 = new System.Windows.Forms.Label();
            this.Label4 = new System.Windows.Forms.Label();
            this.Label3 = new System.Windows.Forms.Label();
            this.Label1 = new System.Windows.Forms.Label();
            this.lblFileName = new System.Windows.Forms.Label();
            this.PictureBox1 = new System.Windows.Forms.PictureBox();
            this.TabPage2 = new System.Windows.Forms.TabPage();
            this.cmdChange = new System.Windows.Forms.Button();
            this.cmdPreview = new System.Windows.Forms.Button();
            this.cmdRestore = new System.Windows.Forms.Button();
            this.lvVersions = new System.Windows.Forms.ListView();
            this.ColumnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.ColumnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.Label8 = new System.Windows.Forms.Label();
            this.PictureBox2 = new System.Windows.Forms.PictureBox();
            this.cmdCancel = new System.Windows.Forms.Button();
            this.toolTipCtl = new System.Windows.Forms.ToolTip(this.components);
            this.TabControl1.SuspendLayout();
            this.TabPage1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.PictureBox1)).BeginInit();
            this.TabPage2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.PictureBox2)).BeginInit();
            this.SuspendLayout();
            // 
            // TabControl1
            // 
            this.TabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.TabControl1.Controls.Add(this.TabPage1);
            this.TabControl1.Controls.Add(this.TabPage2);
            this.TabControl1.Location = new System.Drawing.Point(18, 18);
            this.TabControl1.Margin = new System.Windows.Forms.Padding(4);
            this.TabControl1.Name = "TabControl1";
            this.TabControl1.SelectedIndex = 0;
            this.TabControl1.Size = new System.Drawing.Size(656, 555);
            this.TabControl1.TabIndex = 0;
            // 
            // TabPage1
            // 
            this.TabPage1.Controls.Add(this.lblFileModified);
            this.TabPage1.Controls.Add(this.lblFileCreated);
            this.TabPage1.Controls.Add(this.lblFileSize);
            this.TabPage1.Controls.Add(this.lblFilePath);
            this.TabPage1.Controls.Add(this.lblFileType);
            this.TabPage1.Controls.Add(this.Label7);
            this.TabPage1.Controls.Add(this.Label6);
            this.TabPage1.Controls.Add(this.Label5);
            this.TabPage1.Controls.Add(this.Label4);
            this.TabPage1.Controls.Add(this.Label3);
            this.TabPage1.Controls.Add(this.Label1);
            this.TabPage1.Controls.Add(this.lblFileName);
            this.TabPage1.Controls.Add(this.PictureBox1);
            this.TabPage1.Location = new System.Drawing.Point(4, 32);
            this.TabPage1.Margin = new System.Windows.Forms.Padding(4);
            this.TabPage1.Name = "TabPage1";
            this.TabPage1.Padding = new System.Windows.Forms.Padding(4);
            this.TabPage1.Size = new System.Drawing.Size(648, 519);
            this.TabPage1.TabIndex = 0;
            this.TabPage1.Text = "Allgemein";
            this.TabPage1.UseVisualStyleBackColor = true;
            // 
            // lblFileModified
            // 
            this.lblFileModified.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblFileModified.Location = new System.Drawing.Point(118, 276);
            this.lblFileModified.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblFileModified.Name = "lblFileModified";
            this.lblFileModified.Size = new System.Drawing.Size(497, 23);
            this.lblFileModified.TabIndex = 12;
            this.lblFileModified.Text = "Label12";
            // 
            // lblFileCreated
            // 
            this.lblFileCreated.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblFileCreated.Location = new System.Drawing.Point(118, 237);
            this.lblFileCreated.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblFileCreated.Name = "lblFileCreated";
            this.lblFileCreated.Size = new System.Drawing.Size(497, 23);
            this.lblFileCreated.TabIndex = 11;
            this.lblFileCreated.Text = "Label11";
            // 
            // lblFileSize
            // 
            this.lblFileSize.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblFileSize.Location = new System.Drawing.Point(118, 182);
            this.lblFileSize.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblFileSize.Name = "lblFileSize";
            this.lblFileSize.Size = new System.Drawing.Size(497, 23);
            this.lblFileSize.TabIndex = 10;
            this.lblFileSize.Text = "Label10";
            // 
            // lblFilePath
            // 
            this.lblFilePath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblFilePath.Location = new System.Drawing.Point(118, 141);
            this.lblFilePath.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblFilePath.Name = "lblFilePath";
            this.lblFilePath.Size = new System.Drawing.Size(497, 23);
            this.lblFilePath.TabIndex = 9;
            this.lblFilePath.Text = "Label9";
            // 
            // lblFileType
            // 
            this.lblFileType.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblFileType.Location = new System.Drawing.Point(118, 104);
            this.lblFileType.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblFileType.Name = "lblFileType";
            this.lblFileType.Size = new System.Drawing.Size(497, 23);
            this.lblFileType.TabIndex = 8;
            this.lblFileType.Text = "Label8";
            // 
            // Label7
            // 
            this.Label7.AutoSize = true;
            this.Label7.Location = new System.Drawing.Point(22, 276);
            this.Label7.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.Label7.Name = "Label7";
            this.Label7.Size = new System.Drawing.Size(85, 23);
            this.Label7.TabIndex = 7;
            this.Label7.Text = "Geändert:";
            // 
            // Label6
            // 
            this.Label6.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.Label6.BackColor = System.Drawing.Color.Gray;
            this.Label6.Location = new System.Drawing.Point(27, 219);
            this.Label6.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.Label6.Name = "Label6";
            this.Label6.Size = new System.Drawing.Size(588, 2);
            this.Label6.TabIndex = 6;
            // 
            // Label5
            // 
            this.Label5.AutoSize = true;
            this.Label5.Location = new System.Drawing.Point(22, 237);
            this.Label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.Label5.Name = "Label5";
            this.Label5.Size = new System.Drawing.Size(65, 23);
            this.Label5.TabIndex = 5;
            this.Label5.Text = "Erstellt:";
            // 
            // Label4
            // 
            this.Label4.AutoSize = true;
            this.Label4.Location = new System.Drawing.Point(22, 182);
            this.Label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.Label4.Name = "Label4";
            this.Label4.Size = new System.Drawing.Size(60, 23);
            this.Label4.TabIndex = 4;
            this.Label4.Text = "Größe:";
            // 
            // Label3
            // 
            this.Label3.AutoSize = true;
            this.Label3.Location = new System.Drawing.Point(22, 141);
            this.Label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.Label3.Name = "Label3";
            this.Label3.Size = new System.Drawing.Size(39, 23);
            this.Label3.TabIndex = 3;
            this.Label3.Text = "Ort:";
            // 
            // Label1
            // 
            this.Label1.AutoSize = true;
            this.Label1.Location = new System.Drawing.Point(22, 104);
            this.Label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.Label1.Name = "Label1";
            this.Label1.Size = new System.Drawing.Size(78, 23);
            this.Label1.TabIndex = 2;
            this.Label1.Text = "Dateityp:";
            // 
            // lblFileName
            // 
            this.lblFileName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblFileName.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.lblFileName.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(153)))));
            this.lblFileName.Location = new System.Drawing.Point(117, 36);
            this.lblFileName.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblFileName.Name = "lblFileName";
            this.lblFileName.Size = new System.Drawing.Size(498, 32);
            this.lblFileName.TabIndex = 1;
            this.lblFileName.Text = "Dateiname.txt";
            // 
            // PictureBox1
            // 
            this.PictureBox1.Location = new System.Drawing.Point(27, 28);
            this.PictureBox1.Margin = new System.Windows.Forms.Padding(4);
            this.PictureBox1.Name = "PictureBox1";
            this.PictureBox1.Size = new System.Drawing.Size(48, 48);
            this.PictureBox1.TabIndex = 0;
            this.PictureBox1.TabStop = false;
            // 
            // TabPage2
            // 
            this.TabPage2.Controls.Add(this.cmdChange);
            this.TabPage2.Controls.Add(this.cmdPreview);
            this.TabPage2.Controls.Add(this.cmdRestore);
            this.TabPage2.Controls.Add(this.lvVersions);
            this.TabPage2.Controls.Add(this.Label8);
            this.TabPage2.Controls.Add(this.PictureBox2);
            this.TabPage2.Location = new System.Drawing.Point(4, 32);
            this.TabPage2.Margin = new System.Windows.Forms.Padding(4);
            this.TabPage2.Name = "TabPage2";
            this.TabPage2.Padding = new System.Windows.Forms.Padding(4);
            this.TabPage2.Size = new System.Drawing.Size(648, 519);
            this.TabPage2.TabIndex = 1;
            this.TabPage2.Text = "Sicherungen";
            this.TabPage2.UseVisualStyleBackColor = true;
            // 
            // cmdChange
            // 
            this.cmdChange.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdChange.Enabled = false;
            this.cmdChange.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.cmdChange.Location = new System.Drawing.Point(102, 450);
            this.cmdChange.Margin = new System.Windows.Forms.Padding(4);
            this.cmdChange.Name = "cmdChange";
            this.cmdChange.Size = new System.Drawing.Size(122, 48);
            this.cmdChange.TabIndex = 6;
            this.cmdChange.Text = "&Wechseln";
            this.cmdChange.UseVisualStyleBackColor = true;
            this.cmdChange.Click += new System.EventHandler(this.cmdChange_Click);
            // 
            // cmdPreview
            // 
            this.cmdPreview.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdPreview.Enabled = false;
            this.cmdPreview.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.cmdPreview.Location = new System.Drawing.Point(232, 450);
            this.cmdPreview.Margin = new System.Windows.Forms.Padding(4);
            this.cmdPreview.Name = "cmdPreview";
            this.cmdPreview.Size = new System.Drawing.Size(188, 48);
            this.cmdPreview.TabIndex = 5;
            this.cmdPreview.Text = "&Schnellvorschau";
            this.cmdPreview.UseVisualStyleBackColor = true;
            this.cmdPreview.Click += new System.EventHandler(this.cmdPreview_Click);
            // 
            // cmdRestore
            // 
            this.cmdRestore.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdRestore.Enabled = false;
            this.cmdRestore.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.cmdRestore.Location = new System.Drawing.Point(428, 450);
            this.cmdRestore.Margin = new System.Windows.Forms.Padding(4);
            this.cmdRestore.Name = "cmdRestore";
            this.cmdRestore.Size = new System.Drawing.Size(185, 48);
            this.cmdRestore.TabIndex = 4;
            this.cmdRestore.Text = "&Wiederherstellen";
            this.cmdRestore.UseVisualStyleBackColor = true;
            this.cmdRestore.Click += new System.EventHandler(this.cmdRestore_Click);
            // 
            // lvVersions
            // 
            this.lvVersions.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lvVersions.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.ColumnHeader1,
            this.ColumnHeader2});
            this.lvVersions.FullRowSelect = true;
            this.lvVersions.HideSelection = false;
            this.lvVersions.Location = new System.Drawing.Point(27, 86);
            this.lvVersions.Margin = new System.Windows.Forms.Padding(4);
            this.lvVersions.Name = "lvVersions";
            this.lvVersions.Size = new System.Drawing.Size(586, 349);
            this.lvVersions.TabIndex = 3;
            this.lvVersions.UseCompatibleStateImageBehavior = false;
            this.lvVersions.View = System.Windows.Forms.View.Details;
            this.lvVersions.SelectedIndexChanged += new System.EventHandler(this.lvVersions_SelectedIndexChanged);
            // 
            // ColumnHeader1
            // 
            this.ColumnHeader1.Text = "Sicherung";
            this.ColumnHeader1.Width = 150;
            // 
            // ColumnHeader2
            // 
            this.ColumnHeader2.Text = "Änderungsdatum der Datei";
            this.ColumnHeader2.Width = 160;
            // 
            // Label8
            // 
            this.Label8.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.Label8.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.Label8.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(153)))));
            this.Label8.Location = new System.Drawing.Point(117, 36);
            this.Label8.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.Label8.Name = "Label8";
            this.Label8.Size = new System.Drawing.Size(496, 32);
            this.Label8.TabIndex = 2;
            this.Label8.Text = "Dateiversionen";
            // 
            // PictureBox2
            // 
            this.PictureBox2.Image = global::BSH.Main.Properties.Resources.settings_backup_restore_icon_48;
            this.PictureBox2.Location = new System.Drawing.Point(27, 28);
            this.PictureBox2.Margin = new System.Windows.Forms.Padding(4);
            this.PictureBox2.Name = "PictureBox2";
            this.PictureBox2.Size = new System.Drawing.Size(48, 48);
            this.PictureBox2.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.PictureBox2.TabIndex = 1;
            this.PictureBox2.TabStop = false;
            // 
            // cmdCancel
            // 
            this.cmdCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cmdCancel.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.cmdCancel.Location = new System.Drawing.Point(530, 581);
            this.cmdCancel.Margin = new System.Windows.Forms.Padding(4);
            this.cmdCancel.Name = "cmdCancel";
            this.cmdCancel.Size = new System.Drawing.Size(140, 48);
            this.cmdCancel.TabIndex = 2;
            this.cmdCancel.Text = "&Schließen";
            this.cmdCancel.UseVisualStyleBackColor = true;
            // 
            // frmFileProperties
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(144F, 144F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(692, 645);
            this.Controls.Add(this.cmdCancel);
            this.Controls.Add(this.TabControl1);
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(714, 701);
            this.Name = "frmFileProperties";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Dateieigenschaften";
            this.TabControl1.ResumeLayout(false);
            this.TabPage1.ResumeLayout(false);
            this.TabPage1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.PictureBox1)).EndInit();
            this.TabPage2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.PictureBox2)).EndInit();
            this.ResumeLayout(false);

        }

        internal TabControl TabControl1;
        internal TabPage TabPage1;
        internal PictureBox PictureBox1;
        internal TabPage TabPage2;
        internal Label Label1;
        internal Label lblFileName;
        internal Label Label7;
        internal Label Label6;
        internal Label Label5;
        internal Label Label4;
        internal Label Label3;
        internal Button cmdCancel;
        internal Label lblFileModified;
        internal Label lblFileCreated;
        internal Label lblFileSize;
        internal Label lblFilePath;
        internal Label lblFileType;
        internal ListView lvVersions;
        internal ColumnHeader ColumnHeader1;
        internal ColumnHeader ColumnHeader2;
        internal Label Label8;
        internal PictureBox PictureBox2;
        internal Button cmdChange;
        internal Button cmdPreview;
        internal Button cmdRestore;
        internal ToolTip toolTipCtl;
    }
}