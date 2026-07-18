using System.Diagnostics;
using System.Windows.Forms;

namespace Brightbits.BSH.Main
{
    public partial class frmFilter : Form
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
            var resources = new System.ComponentModel.ComponentResourceManager(typeof(frmFilter));
            Panel1 = new Panel();
            cmdOK = new Button();
            Label1 = new Label();
            cmdCancel = new Button();
            Label2 = new Label();
            lstExcludeFolders = new ListBox();
            cmdDeleteFolders = new Button();
            cmdAddFolders = new Button();
            cmdDeleteFile = new Button();
            cmdAddFile = new Button();
            lstExcludeFiles = new ListBox();
            Panel3 = new Panel();
            TabControl1 = new TabControl();
            TabPage1 = new TabPage();
            TableLayoutPanel1 = new TableLayoutPanel();
            FlowLayoutPanel1 = new FlowLayoutPanel();
            TabPage2 = new TabPage();
            TableLayoutPanel2 = new TableLayoutPanel();
            FlowLayoutPanel2 = new FlowLayoutPanel();
            TabPage3 = new TabPage();
            TableLayoutPanel3 = new TableLayoutPanel();
            lstExcludeSingleFile = new ListBox();
            FlowLayoutPanel3 = new FlowLayoutPanel();
            cmdAddSingleFile = new Button();
            cmdDeleteSingleFile = new Button();
            TabPage4 = new TabPage();
            FlowLayoutPanel4 = new FlowLayoutPanel();
            chkFilesBigger = new CheckBox();
            txtFilesBigger = new NumericUpDown();
            Label4 = new Label();
            LinkLabel1 = new LinkLabel();
            txtRegEx = new TextBox();
            Panel1.SuspendLayout();
            Panel3.SuspendLayout();
            TabControl1.SuspendLayout();
            TabPage1.SuspendLayout();
            TableLayoutPanel1.SuspendLayout();
            FlowLayoutPanel1.SuspendLayout();
            TabPage2.SuspendLayout();
            TableLayoutPanel2.SuspendLayout();
            FlowLayoutPanel2.SuspendLayout();
            TabPage3.SuspendLayout();
            TableLayoutPanel3.SuspendLayout();
            FlowLayoutPanel3.SuspendLayout();
            TabPage4.SuspendLayout();
            FlowLayoutPanel4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)txtFilesBigger).BeginInit();
            SuspendLayout();
            // 
            // Panel1
            // 
            resources.ApplyResources(Panel1, "Panel1");
            Panel1.BackColor = System.Drawing.SystemColors.Control;
            Panel1.Controls.Add(cmdOK);
            Panel1.Controls.Add(Label1);
            Panel1.Controls.Add(cmdCancel);
            Panel1.Name = "Panel1";
            // 
            // cmdOK
            // 
            resources.ApplyResources(cmdOK, "cmdOK");
            cmdOK.Name = "cmdOK";
            cmdOK.UseVisualStyleBackColor = true;
            cmdOK.Click += cmdOK_Click;
            // 
            // Label1
            // 
            resources.ApplyResources(Label1, "Label1");
            Label1.BackColor = System.Drawing.Color.DarkGray;
            Label1.Name = "Label1";
            // 
            // cmdCancel
            // 
            resources.ApplyResources(cmdCancel, "cmdCancel");
            cmdCancel.DialogResult = DialogResult.Cancel;
            cmdCancel.Name = "cmdCancel";
            cmdCancel.UseVisualStyleBackColor = true;
            // 
            // Label2
            // 
            resources.ApplyResources(Label2, "Label2");
            Label2.ForeColor = System.Drawing.Color.FromArgb(0, 51, 153);
            Label2.Name = "Label2";
            // 
            // lstExcludeFolders
            // 
            resources.ApplyResources(lstExcludeFolders, "lstExcludeFolders");
            lstExcludeFolders.FormattingEnabled = true;
            lstExcludeFolders.Name = "lstExcludeFolders";
            lstExcludeFolders.Sorted = true;
            lstExcludeFolders.SelectedIndexChanged += lstExcludeFolders_SelectedIndexChanged;
            // 
            // cmdDeleteFolders
            // 
            resources.ApplyResources(cmdDeleteFolders, "cmdDeleteFolders");
            cmdDeleteFolders.Name = "cmdDeleteFolders";
            cmdDeleteFolders.UseVisualStyleBackColor = true;
            cmdDeleteFolders.Click += cmdDeleteFolders_Click;
            // 
            // cmdAddFolders
            // 
            resources.ApplyResources(cmdAddFolders, "cmdAddFolders");
            cmdAddFolders.Name = "cmdAddFolders";
            cmdAddFolders.UseVisualStyleBackColor = true;
            cmdAddFolders.Click += cmdAddFolders_Click;
            // 
            // cmdDeleteFile
            // 
            resources.ApplyResources(cmdDeleteFile, "cmdDeleteFile");
            cmdDeleteFile.Name = "cmdDeleteFile";
            cmdDeleteFile.UseVisualStyleBackColor = true;
            cmdDeleteFile.Click += cmdDeleteFile_Click;
            // 
            // cmdAddFile
            // 
            resources.ApplyResources(cmdAddFile, "cmdAddFile");
            cmdAddFile.Name = "cmdAddFile";
            cmdAddFile.UseVisualStyleBackColor = true;
            cmdAddFile.Click += cmdAddFile_Click;
            // 
            // lstExcludeFiles
            // 
            resources.ApplyResources(lstExcludeFiles, "lstExcludeFiles");
            lstExcludeFiles.FormattingEnabled = true;
            lstExcludeFiles.Name = "lstExcludeFiles";
            lstExcludeFiles.Sorted = true;
            lstExcludeFiles.SelectedIndexChanged += lstExcludeFiles_SelectedIndexChanged;
            // 
            // Panel3
            // 
            resources.ApplyResources(Panel3, "Panel3");
            Panel3.Controls.Add(TabControl1);
            Panel3.Controls.Add(Label2);
            Panel3.Name = "Panel3";
            // 
            // TabControl1
            // 
            resources.ApplyResources(TabControl1, "TabControl1");
            TabControl1.Controls.Add(TabPage1);
            TabControl1.Controls.Add(TabPage2);
            TabControl1.Controls.Add(TabPage3);
            TabControl1.Controls.Add(TabPage4);
            TabControl1.Name = "TabControl1";
            TabControl1.SelectedIndex = 0;
            // 
            // TabPage1
            // 
            resources.ApplyResources(TabPage1, "TabPage1");
            TabPage1.Controls.Add(TableLayoutPanel1);
            TabPage1.Name = "TabPage1";
            TabPage1.UseVisualStyleBackColor = true;
            // 
            // TableLayoutPanel1
            // 
            resources.ApplyResources(TableLayoutPanel1, "TableLayoutPanel1");
            TableLayoutPanel1.Controls.Add(lstExcludeFolders, 0, 0);
            TableLayoutPanel1.Controls.Add(FlowLayoutPanel1, 0, 1);
            TableLayoutPanel1.Name = "TableLayoutPanel1";
            // 
            // FlowLayoutPanel1
            // 
            resources.ApplyResources(FlowLayoutPanel1, "FlowLayoutPanel1");
            FlowLayoutPanel1.Controls.Add(cmdAddFolders);
            FlowLayoutPanel1.Controls.Add(cmdDeleteFolders);
            FlowLayoutPanel1.Name = "FlowLayoutPanel1";
            // 
            // TabPage2
            // 
            resources.ApplyResources(TabPage2, "TabPage2");
            TabPage2.Controls.Add(TableLayoutPanel2);
            TabPage2.Name = "TabPage2";
            TabPage2.UseVisualStyleBackColor = true;
            // 
            // TableLayoutPanel2
            // 
            resources.ApplyResources(TableLayoutPanel2, "TableLayoutPanel2");
            TableLayoutPanel2.Controls.Add(lstExcludeFiles, 0, 0);
            TableLayoutPanel2.Controls.Add(FlowLayoutPanel2, 0, 1);
            TableLayoutPanel2.Name = "TableLayoutPanel2";
            // 
            // FlowLayoutPanel2
            // 
            resources.ApplyResources(FlowLayoutPanel2, "FlowLayoutPanel2");
            FlowLayoutPanel2.Controls.Add(cmdAddFile);
            FlowLayoutPanel2.Controls.Add(cmdDeleteFile);
            FlowLayoutPanel2.Name = "FlowLayoutPanel2";
            // 
            // TabPage3
            // 
            resources.ApplyResources(TabPage3, "TabPage3");
            TabPage3.Controls.Add(TableLayoutPanel3);
            TabPage3.Name = "TabPage3";
            TabPage3.UseVisualStyleBackColor = true;
            // 
            // TableLayoutPanel3
            // 
            resources.ApplyResources(TableLayoutPanel3, "TableLayoutPanel3");
            TableLayoutPanel3.Controls.Add(lstExcludeSingleFile, 0, 0);
            TableLayoutPanel3.Controls.Add(FlowLayoutPanel3, 0, 1);
            TableLayoutPanel3.Name = "TableLayoutPanel3";
            // 
            // lstExcludeSingleFile
            // 
            resources.ApplyResources(lstExcludeSingleFile, "lstExcludeSingleFile");
            lstExcludeSingleFile.FormattingEnabled = true;
            lstExcludeSingleFile.Name = "lstExcludeSingleFile";
            lstExcludeSingleFile.SelectionMode = SelectionMode.MultiExtended;
            lstExcludeSingleFile.Sorted = true;
            lstExcludeSingleFile.SelectedIndexChanged += lstExcludeSingleFile_SelectedIndexChanged;
            // 
            // FlowLayoutPanel3
            // 
            resources.ApplyResources(FlowLayoutPanel3, "FlowLayoutPanel3");
            FlowLayoutPanel3.Controls.Add(cmdAddSingleFile);
            FlowLayoutPanel3.Controls.Add(cmdDeleteSingleFile);
            FlowLayoutPanel3.Name = "FlowLayoutPanel3";
            // 
            // cmdAddSingleFile
            // 
            resources.ApplyResources(cmdAddSingleFile, "cmdAddSingleFile");
            cmdAddSingleFile.Name = "cmdAddSingleFile";
            cmdAddSingleFile.UseVisualStyleBackColor = true;
            cmdAddSingleFile.Click += cmdAddSingleFile_Click;
            // 
            // cmdDeleteSingleFile
            // 
            resources.ApplyResources(cmdDeleteSingleFile, "cmdDeleteSingleFile");
            cmdDeleteSingleFile.Name = "cmdDeleteSingleFile";
            cmdDeleteSingleFile.UseVisualStyleBackColor = true;
            cmdDeleteSingleFile.Click += cmdDeleteSingleFile_Click;
            // 
            // TabPage4
            // 
            resources.ApplyResources(TabPage4, "TabPage4");
            TabPage4.Controls.Add(FlowLayoutPanel4);
            TabPage4.Controls.Add(LinkLabel1);
            TabPage4.Controls.Add(txtRegEx);
            TabPage4.Name = "TabPage4";
            TabPage4.UseVisualStyleBackColor = true;
            // 
            // FlowLayoutPanel4
            // 
            resources.ApplyResources(FlowLayoutPanel4, "FlowLayoutPanel4");
            FlowLayoutPanel4.Controls.Add(chkFilesBigger);
            FlowLayoutPanel4.Controls.Add(txtFilesBigger);
            FlowLayoutPanel4.Controls.Add(Label4);
            FlowLayoutPanel4.Name = "FlowLayoutPanel4";
            // 
            // chkFilesBigger
            // 
            resources.ApplyResources(chkFilesBigger, "chkFilesBigger");
            chkFilesBigger.Name = "chkFilesBigger";
            chkFilesBigger.UseVisualStyleBackColor = true;
            chkFilesBigger.CheckedChanged += chkFilesBigger_CheckedChanged;
            // 
            // txtFilesBigger
            // 
            resources.ApplyResources(txtFilesBigger, "txtFilesBigger");
            txtFilesBigger.Maximum = new decimal(new int[] { 10000, 0, 0, 0 });
            txtFilesBigger.Minimum = new decimal(new int[] { 10, 0, 0, 0 });
            txtFilesBigger.Name = "txtFilesBigger";
            txtFilesBigger.Value = new decimal(new int[] { 10, 0, 0, 0 });
            // 
            // Label4
            // 
            resources.ApplyResources(Label4, "Label4");
            Label4.Name = "Label4";
            // 
            // LinkLabel1
            // 
            resources.ApplyResources(LinkLabel1, "LinkLabel1");
            LinkLabel1.Name = "LinkLabel1";
            LinkLabel1.TabStop = true;
            LinkLabel1.LinkClicked += LinkLabel1_LinkClicked;
            // 
            // txtRegEx
            // 
            resources.ApplyResources(txtRegEx, "txtRegEx");
            txtRegEx.Name = "txtRegEx";
            // 
            // frmFilter
            // 
            resources.ApplyResources(this, "$this");
            AutoScaleMode = AutoScaleMode.Dpi;
            BackColor = System.Drawing.Color.White;
            Controls.Add(Panel3);
            Controls.Add(Panel1);
            Name = "frmFilter";
            ShowInTaskbar = false;
            Load += frmFilter_Load;
            Panel1.ResumeLayout(false);
            Panel3.ResumeLayout(false);
            Panel3.PerformLayout();
            TabControl1.ResumeLayout(false);
            TabPage1.ResumeLayout(false);
            TableLayoutPanel1.ResumeLayout(false);
            FlowLayoutPanel1.ResumeLayout(false);
            TabPage2.ResumeLayout(false);
            TableLayoutPanel2.ResumeLayout(false);
            FlowLayoutPanel2.ResumeLayout(false);
            TabPage3.ResumeLayout(false);
            TableLayoutPanel3.ResumeLayout(false);
            FlowLayoutPanel3.ResumeLayout(false);
            TabPage4.ResumeLayout(false);
            TabPage4.PerformLayout();
            FlowLayoutPanel4.ResumeLayout(false);
            FlowLayoutPanel4.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)txtFilesBigger).EndInit();
            ResumeLayout(false);
        }

        internal Panel Panel1;
        internal Label Label1;
        internal Button cmdCancel;
        internal Label Label2;
        internal Button cmdOK;
        internal ListBox lstExcludeFolders;
        internal Button cmdDeleteFolders;
        internal Button cmdAddFolders;
        internal Button cmdDeleteFile;
        internal Button cmdAddFile;
        internal ListBox lstExcludeFiles;
        internal Panel Panel3;
        internal Label Label4;
        internal CheckBox chkFilesBigger;
        internal NumericUpDown txtFilesBigger;
        internal TextBox txtRegEx;
        internal LinkLabel LinkLabel1;
        internal TabControl TabControl1;
        internal TabPage TabPage1;
        internal TabPage TabPage2;
        internal TabPage TabPage3;
        internal TabPage TabPage4;
        internal ListBox lstExcludeSingleFile;
        internal Button cmdAddSingleFile;
        internal Button cmdDeleteSingleFile;
        internal TableLayoutPanel TableLayoutPanel1;
        internal FlowLayoutPanel FlowLayoutPanel1;
        internal TableLayoutPanel TableLayoutPanel2;
        internal FlowLayoutPanel FlowLayoutPanel2;
        internal TableLayoutPanel TableLayoutPanel3;
        internal FlowLayoutPanel FlowLayoutPanel3;
        internal FlowLayoutPanel FlowLayoutPanel4;
    }
}