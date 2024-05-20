using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using BSH.Main.Properties;

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
            cmdPreview = new Button();
            cmdRestore = new Button();
            lvVersions = new ListView();
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
            resources.ApplyResources(TabControl1, "TabControl1");
            TabControl1.Controls.Add(TabPage1);
            TabControl1.Controls.Add(TabPage2);
            TabControl1.Name = "TabControl1";
            TabControl1.SelectedIndex = 0;
            toolTipCtl.SetToolTip(TabControl1, resources.GetString("TabControl1.ToolTip"));
            // 
            // TabPage1
            // 
            resources.ApplyResources(TabPage1, "TabPage1");
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
            TabPage1.Name = "TabPage1";
            toolTipCtl.SetToolTip(TabPage1, resources.GetString("TabPage1.ToolTip"));
            TabPage1.UseVisualStyleBackColor = true;
            // 
            // lblFileModified
            // 
            resources.ApplyResources(lblFileModified, "lblFileModified");
            lblFileModified.Name = "lblFileModified";
            toolTipCtl.SetToolTip(lblFileModified, resources.GetString("lblFileModified.ToolTip"));
            // 
            // lblFileCreated
            // 
            resources.ApplyResources(lblFileCreated, "lblFileCreated");
            lblFileCreated.Name = "lblFileCreated";
            toolTipCtl.SetToolTip(lblFileCreated, resources.GetString("lblFileCreated.ToolTip"));
            // 
            // lblFileSize
            // 
            resources.ApplyResources(lblFileSize, "lblFileSize");
            lblFileSize.Name = "lblFileSize";
            toolTipCtl.SetToolTip(lblFileSize, resources.GetString("lblFileSize.ToolTip"));
            // 
            // lblFilePath
            // 
            resources.ApplyResources(lblFilePath, "lblFilePath");
            lblFilePath.Name = "lblFilePath";
            toolTipCtl.SetToolTip(lblFilePath, resources.GetString("lblFilePath.ToolTip"));
            // 
            // lblFileType
            // 
            resources.ApplyResources(lblFileType, "lblFileType");
            lblFileType.Name = "lblFileType";
            toolTipCtl.SetToolTip(lblFileType, resources.GetString("lblFileType.ToolTip"));
            // 
            // Label7
            // 
            resources.ApplyResources(Label7, "Label7");
            Label7.Name = "Label7";
            toolTipCtl.SetToolTip(Label7, resources.GetString("Label7.ToolTip"));
            // 
            // Label6
            // 
            resources.ApplyResources(Label6, "Label6");
            Label6.BackColor = Color.Gray;
            Label6.Name = "Label6";
            toolTipCtl.SetToolTip(Label6, resources.GetString("Label6.ToolTip"));
            // 
            // Label5
            // 
            resources.ApplyResources(Label5, "Label5");
            Label5.Name = "Label5";
            toolTipCtl.SetToolTip(Label5, resources.GetString("Label5.ToolTip"));
            // 
            // Label4
            // 
            resources.ApplyResources(Label4, "Label4");
            Label4.Name = "Label4";
            toolTipCtl.SetToolTip(Label4, resources.GetString("Label4.ToolTip"));
            // 
            // Label3
            // 
            resources.ApplyResources(Label3, "Label3");
            Label3.Name = "Label3";
            toolTipCtl.SetToolTip(Label3, resources.GetString("Label3.ToolTip"));
            // 
            // Label1
            // 
            resources.ApplyResources(Label1, "Label1");
            Label1.Name = "Label1";
            toolTipCtl.SetToolTip(Label1, resources.GetString("Label1.ToolTip"));
            // 
            // lblFileName
            // 
            resources.ApplyResources(lblFileName, "lblFileName");
            lblFileName.ForeColor = Color.FromArgb(0, 51, 153);
            lblFileName.Name = "lblFileName";
            toolTipCtl.SetToolTip(lblFileName, resources.GetString("lblFileName.ToolTip"));
            // 
            // PictureBox1
            // 
            resources.ApplyResources(PictureBox1, "PictureBox1");
            PictureBox1.Name = "PictureBox1";
            PictureBox1.TabStop = false;
            toolTipCtl.SetToolTip(PictureBox1, resources.GetString("PictureBox1.ToolTip"));
            // 
            // TabPage2
            // 
            resources.ApplyResources(TabPage2, "TabPage2");
            TabPage2.Controls.Add(cmdChange);
            TabPage2.Controls.Add(cmdPreview);
            TabPage2.Controls.Add(cmdRestore);
            TabPage2.Controls.Add(lvVersions);
            TabPage2.Controls.Add(Label8);
            TabPage2.Controls.Add(PictureBox2);
            TabPage2.Name = "TabPage2";
            toolTipCtl.SetToolTip(TabPage2, resources.GetString("TabPage2.ToolTip"));
            TabPage2.UseVisualStyleBackColor = true;
            // 
            // cmdChange
            // 
            resources.ApplyResources(cmdChange, "cmdChange");
            cmdChange.Name = "cmdChange";
            toolTipCtl.SetToolTip(cmdChange, resources.GetString("cmdChange.ToolTip"));
            cmdChange.UseVisualStyleBackColor = true;
            cmdChange.Click += cmdChange_Click;
            // 
            // cmdPreview
            // 
            resources.ApplyResources(cmdPreview, "cmdPreview");
            cmdPreview.Name = "cmdPreview";
            toolTipCtl.SetToolTip(cmdPreview, resources.GetString("cmdPreview.ToolTip"));
            cmdPreview.UseVisualStyleBackColor = true;
            cmdPreview.Click += cmdPreview_Click;
            // 
            // cmdRestore
            // 
            resources.ApplyResources(cmdRestore, "cmdRestore");
            cmdRestore.Name = "cmdRestore";
            toolTipCtl.SetToolTip(cmdRestore, resources.GetString("cmdRestore.ToolTip"));
            cmdRestore.UseVisualStyleBackColor = true;
            cmdRestore.Click += cmdRestore_Click;
            // 
            // lvVersions
            // 
            resources.ApplyResources(lvVersions, "lvVersions");
            lvVersions.Columns.AddRange(new ColumnHeader[] { ColumnHeader1, ColumnHeader2 });
            lvVersions.FullRowSelect = true;
            lvVersions.Name = "lvVersions";
            toolTipCtl.SetToolTip(lvVersions, resources.GetString("lvVersions.ToolTip"));
            lvVersions.UseCompatibleStateImageBehavior = false;
            lvVersions.View = View.Details;
            lvVersions.SelectedIndexChanged += lvVersions_SelectedIndexChanged;
            // 
            // ColumnHeader1
            // 
            resources.ApplyResources(ColumnHeader1, "ColumnHeader1");
            // 
            // ColumnHeader2
            // 
            resources.ApplyResources(ColumnHeader2, "ColumnHeader2");
            // 
            // Label8
            // 
            resources.ApplyResources(Label8, "Label8");
            Label8.ForeColor = Color.FromArgb(0, 51, 153);
            Label8.Name = "Label8";
            toolTipCtl.SetToolTip(Label8, resources.GetString("Label8.ToolTip"));
            // 
            // PictureBox2
            // 
            resources.ApplyResources(PictureBox2, "PictureBox2");
            PictureBox2.Image = Resources.settings_backup_restore_icon_48;
            PictureBox2.Name = "PictureBox2";
            PictureBox2.TabStop = false;
            toolTipCtl.SetToolTip(PictureBox2, resources.GetString("PictureBox2.ToolTip"));
            // 
            // cmdCancel
            // 
            resources.ApplyResources(cmdCancel, "cmdCancel");
            cmdCancel.DialogResult = DialogResult.Cancel;
            cmdCancel.Name = "cmdCancel";
            toolTipCtl.SetToolTip(cmdCancel, resources.GetString("cmdCancel.ToolTip"));
            cmdCancel.UseVisualStyleBackColor = true;
            // 
            // frmFileProperties
            // 
            resources.ApplyResources(this, "$this");
            AutoScaleMode = AutoScaleMode.Dpi;
            Controls.Add(cmdCancel);
            Controls.Add(TabControl1);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "frmFileProperties";
            toolTipCtl.SetToolTip(this, resources.GetString("$this.ToolTip"));
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