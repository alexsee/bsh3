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
            components = new System.ComponentModel.Container();
            var resources = new System.ComponentModel.ComponentResourceManager(typeof(frmFileProperties));
            TabControl1 = new TabControl();
            TabPage1 = new TabPage();
            lblFileModified = new Label();
            lblFileCreated = new Label();
            lblFileSize = new Label();
            lblFilePath = new Label();
            lblFileType = new Label();
            Label7 = new Label();
            Label6 = new Label();
            Label5 = new Label();
            Label4 = new Label();
            Label3 = new Label();
            Label1 = new Label();
            lblFileName = new Label();
            PictureBox1 = new PictureBox();
            TabPage2 = new TabPage();
            cmdChange = new Button();
            cmdChange.Click += new EventHandler(cmdChange_Click);
            cmdPreview = new Button();
            cmdPreview.Click += new EventHandler(cmdPreview_Click);
            cmdRestore = new Button();
            cmdRestore.Click += new EventHandler(cmdRestore_Click);
            lvVersions = new ListView();
            lvVersions.SelectedIndexChanged += new EventHandler(lvVersions_SelectedIndexChanged);
            ColumnHeader1 = new ColumnHeader();
            ColumnHeader2 = new ColumnHeader();
            Label8 = new Label();
            PictureBox2 = new PictureBox();
            cmdCancel = new Button();
            toolTipCtl = new ToolTip(components);
            TabControl1.SuspendLayout();
            TabPage1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)PictureBox1).BeginInit();
            TabPage2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)PictureBox2).BeginInit();
            SuspendLayout();
            // 
            // TabControl1
            // 
            TabControl1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;

            TabControl1.Controls.Add(TabPage1);
            TabControl1.Controls.Add(TabPage2);
            TabControl1.Location = new Point(18, 18);
            TabControl1.Margin = new Padding(4);
            TabControl1.Name = "TabControl1";
            TabControl1.SelectedIndex = 0;
            TabControl1.Size = new Size(656, 555);
            TabControl1.TabIndex = 0;
            // 
            // TabPage1
            // 
            TabPage1.Controls.Add(lblFileModified);
            TabPage1.Controls.Add(lblFileCreated);
            TabPage1.Controls.Add(lblFileSize);
            TabPage1.Controls.Add(lblFilePath);
            TabPage1.Controls.Add(lblFileType);
            TabPage1.Controls.Add(Label7);
            TabPage1.Controls.Add(Label6);
            TabPage1.Controls.Add(Label5);
            TabPage1.Controls.Add(Label4);
            TabPage1.Controls.Add(Label3);
            TabPage1.Controls.Add(Label1);
            TabPage1.Controls.Add(lblFileName);
            TabPage1.Controls.Add(PictureBox1);
            TabPage1.Location = new Point(4, 32);
            TabPage1.Margin = new Padding(4);
            TabPage1.Name = "TabPage1";
            TabPage1.Padding = new Padding(4);
            TabPage1.Size = new Size(648, 519);
            TabPage1.TabIndex = 0;
            TabPage1.Text = "Allgemein";
            TabPage1.UseVisualStyleBackColor = true;
            // 
            // lblFileModified
            // 
            lblFileModified.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            lblFileModified.Location = new Point(118, 276);
            lblFileModified.Margin = new Padding(4, 0, 4, 0);
            lblFileModified.Name = "lblFileModified";
            lblFileModified.Size = new Size(497, 23);
            lblFileModified.TabIndex = 12;
            lblFileModified.Text = "Label12";
            // 
            // lblFileCreated
            // 
            lblFileCreated.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            lblFileCreated.Location = new Point(118, 237);
            lblFileCreated.Margin = new Padding(4, 0, 4, 0);
            lblFileCreated.Name = "lblFileCreated";
            lblFileCreated.Size = new Size(497, 23);
            lblFileCreated.TabIndex = 11;
            lblFileCreated.Text = "Label11";
            // 
            // lblFileSize
            // 
            lblFileSize.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            lblFileSize.Location = new Point(118, 182);
            lblFileSize.Margin = new Padding(4, 0, 4, 0);
            lblFileSize.Name = "lblFileSize";
            lblFileSize.Size = new Size(497, 23);
            lblFileSize.TabIndex = 10;
            lblFileSize.Text = "Label10";
            // 
            // lblFilePath
            // 
            lblFilePath.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            lblFilePath.Location = new Point(118, 141);
            lblFilePath.Margin = new Padding(4, 0, 4, 0);
            lblFilePath.Name = "lblFilePath";
            lblFilePath.Size = new Size(497, 23);
            lblFilePath.TabIndex = 9;
            lblFilePath.Text = "Label9";
            // 
            // lblFileType
            // 
            lblFileType.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            lblFileType.Location = new Point(118, 104);
            lblFileType.Margin = new Padding(4, 0, 4, 0);
            lblFileType.Name = "lblFileType";
            lblFileType.Size = new Size(497, 23);
            lblFileType.TabIndex = 8;
            lblFileType.Text = "Label8";
            // 
            // Label7
            // 
            Label7.AutoSize = true;
            Label7.Location = new Point(22, 276);
            Label7.Margin = new Padding(4, 0, 4, 0);
            Label7.Name = "Label7";
            Label7.Size = new Size(85, 23);
            Label7.TabIndex = 7;
            Label7.Text = "Geändert:";
            // 
            // Label6
            // 
            Label6.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            Label6.BackColor = Color.Gray;
            Label6.Location = new Point(27, 219);
            Label6.Margin = new Padding(4, 0, 4, 0);
            Label6.Name = "Label6";
            Label6.Size = new Size(588, 2);
            Label6.TabIndex = 6;
            // 
            // Label5
            // 
            Label5.AutoSize = true;
            Label5.Location = new Point(22, 237);
            Label5.Margin = new Padding(4, 0, 4, 0);
            Label5.Name = "Label5";
            Label5.Size = new Size(65, 23);
            Label5.TabIndex = 5;
            Label5.Text = "Erstellt:";
            // 
            // Label4
            // 
            Label4.AutoSize = true;
            Label4.Location = new Point(22, 182);
            Label4.Margin = new Padding(4, 0, 4, 0);
            Label4.Name = "Label4";
            Label4.Size = new Size(60, 23);
            Label4.TabIndex = 4;
            Label4.Text = "Größe:";
            // 
            // Label3
            // 
            Label3.AutoSize = true;
            Label3.Location = new Point(22, 141);
            Label3.Margin = new Padding(4, 0, 4, 0);
            Label3.Name = "Label3";
            Label3.Size = new Size(39, 23);
            Label3.TabIndex = 3;
            Label3.Text = "Ort:";
            // 
            // Label1
            // 
            Label1.AutoSize = true;
            Label1.Location = new Point(22, 104);
            Label1.Margin = new Padding(4, 0, 4, 0);
            Label1.Name = "Label1";
            Label1.Size = new Size(78, 23);
            Label1.TabIndex = 2;
            Label1.Text = "Dateityp:";
            // 
            // lblFileName
            // 
            lblFileName.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            lblFileName.Font = new Font("Segoe UI", 12.0f, FontStyle.Regular, GraphicsUnit.Point);
            lblFileName.ForeColor = Color.FromArgb(0, 51, 153);
            lblFileName.Location = new Point(117, 36);
            lblFileName.Margin = new Padding(4, 0, 4, 0);
            lblFileName.Name = "lblFileName";
            lblFileName.Size = new Size(498, 32);
            lblFileName.TabIndex = 1;
            lblFileName.Text = "Dateiname.txt";
            // 
            // PictureBox1
            // 
            PictureBox1.Location = new Point(27, 28);
            PictureBox1.Margin = new Padding(4);
            PictureBox1.Name = "PictureBox1";
            PictureBox1.Size = new Size(48, 48);
            PictureBox1.TabIndex = 0;
            PictureBox1.TabStop = false;
            // 
            // TabPage2
            // 
            TabPage2.Controls.Add(cmdChange);
            TabPage2.Controls.Add(cmdPreview);
            TabPage2.Controls.Add(cmdRestore);
            TabPage2.Controls.Add(lvVersions);
            TabPage2.Controls.Add(Label8);
            TabPage2.Controls.Add(PictureBox2);
            TabPage2.Location = new Point(4, 32);
            TabPage2.Margin = new Padding(4);
            TabPage2.Name = "TabPage2";
            TabPage2.Padding = new Padding(4);
            TabPage2.Size = new Size(648, 519);
            TabPage2.TabIndex = 1;
            TabPage2.Text = "Sicherungen";
            TabPage2.UseVisualStyleBackColor = true;
            // 
            // cmdChange
            // 
            cmdChange.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            cmdChange.Enabled = false;
            cmdChange.Font = new Font("Segoe UI", 10.0f, FontStyle.Regular, GraphicsUnit.Point);
            cmdChange.Location = new Point(102, 450);
            cmdChange.Margin = new Padding(4);
            cmdChange.Name = "cmdChange";
            cmdChange.Size = new Size(122, 48);
            cmdChange.TabIndex = 6;
            cmdChange.Text = "&Wechseln";
            cmdChange.UseVisualStyleBackColor = true;
            // 
            // cmdPreview
            // 
            cmdPreview.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            cmdPreview.Enabled = false;
            cmdPreview.Font = new Font("Segoe UI", 10.0f, FontStyle.Regular, GraphicsUnit.Point);
            cmdPreview.Location = new Point(232, 450);
            cmdPreview.Margin = new Padding(4);
            cmdPreview.Name = "cmdPreview";
            cmdPreview.Size = new Size(188, 48);
            cmdPreview.TabIndex = 5;
            cmdPreview.Text = "&Schnellvorschau";
            cmdPreview.UseVisualStyleBackColor = true;
            // 
            // cmdRestore
            // 
            cmdRestore.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            cmdRestore.Enabled = false;
            cmdRestore.Font = new Font("Segoe UI", 10.0f, FontStyle.Regular, GraphicsUnit.Point);
            cmdRestore.Location = new Point(428, 450);
            cmdRestore.Margin = new Padding(4);
            cmdRestore.Name = "cmdRestore";
            cmdRestore.Size = new Size(185, 48);
            cmdRestore.TabIndex = 4;
            cmdRestore.Text = "&Wiederherstellen";
            cmdRestore.UseVisualStyleBackColor = true;
            // 
            // lvVersions
            // 
            lvVersions.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;

            lvVersions.Columns.AddRange(new ColumnHeader[] { ColumnHeader1, ColumnHeader2 });
            lvVersions.FullRowSelect = true;
            lvVersions.HideSelection = false;
            lvVersions.Location = new Point(27, 86);
            lvVersions.Margin = new Padding(4);
            lvVersions.Name = "lvVersions";
            lvVersions.Size = new Size(586, 349);
            lvVersions.TabIndex = 3;
            lvVersions.UseCompatibleStateImageBehavior = false;
            lvVersions.View = View.Details;
            // 
            // ColumnHeader1
            // 
            ColumnHeader1.Text = "Sicherung";
            ColumnHeader1.Width = 150;
            // 
            // ColumnHeader2
            // 
            ColumnHeader2.Text = "Änderungsdatum der Datei";
            ColumnHeader2.Width = 160;
            // 
            // Label8
            // 
            Label8.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            Label8.Font = new Font("Segoe UI", 12.0f, FontStyle.Regular, GraphicsUnit.Point);
            Label8.ForeColor = Color.FromArgb(0, 51, 153);
            Label8.Location = new Point(117, 36);
            Label8.Margin = new Padding(4, 0, 4, 0);
            Label8.Name = "Label8";
            Label8.Size = new Size(496, 32);
            Label8.TabIndex = 2;
            Label8.Text = "Dateiversionen";
            // 
            // PictureBox2
            // 
            PictureBox2.Image = (Image)resources.GetObject("PictureBox2.Image");
            PictureBox2.Location = new Point(27, 28);
            PictureBox2.Margin = new Padding(4);
            PictureBox2.Name = "PictureBox2";
            PictureBox2.Size = new Size(48, 48);
            PictureBox2.SizeMode = PictureBoxSizeMode.Zoom;
            PictureBox2.TabIndex = 1;
            PictureBox2.TabStop = false;
            // 
            // cmdCancel
            // 
            cmdCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            cmdCancel.DialogResult = DialogResult.Cancel;
            cmdCancel.Font = new Font("Segoe UI", 10.0f, FontStyle.Regular, GraphicsUnit.Point);
            cmdCancel.Location = new Point(530, 581);
            cmdCancel.Margin = new Padding(4);
            cmdCancel.Name = "cmdCancel";
            cmdCancel.Size = new Size(140, 48);
            cmdCancel.TabIndex = 2;
            cmdCancel.Text = "&Schließen";
            cmdCancel.UseVisualStyleBackColor = true;
            // 
            // frmFileProperties
            // 
            AutoScaleDimensions = new SizeF(144.0f, 144.0f);
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(692, 645);
            Controls.Add(cmdCancel);
            Controls.Add(TabControl1);
            Font = new Font("Segoe UI", 8.25f, FontStyle.Regular, GraphicsUnit.Point);
            Margin = new Padding(4);
            MaximizeBox = false;
            MinimizeBox = false;
            MinimumSize = new Size(714, 701);
            Name = "frmFileProperties";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Dateieigenschaften";
            TabControl1.ResumeLayout(false);
            TabPage1.ResumeLayout(false);
            TabPage1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)PictureBox1).EndInit();
            TabPage2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)PictureBox2).EndInit();
            ResumeLayout(false);
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