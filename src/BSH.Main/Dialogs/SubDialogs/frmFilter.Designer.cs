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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmFilter));
            this.Panel1 = new System.Windows.Forms.Panel();
            this.cmdOK = new System.Windows.Forms.Button();
            this.Label1 = new System.Windows.Forms.Label();
            this.cmdCancel = new System.Windows.Forms.Button();
            this.Label2 = new System.Windows.Forms.Label();
            this.lstExcludeFolders = new System.Windows.Forms.ListBox();
            this.cmdDeleteFolders = new System.Windows.Forms.Button();
            this.cmdAddFolders = new System.Windows.Forms.Button();
            this.cmdDeleteFile = new System.Windows.Forms.Button();
            this.cmdAddFile = new System.Windows.Forms.Button();
            this.lstExcludeFiles = new System.Windows.Forms.ListBox();
            this.Panel3 = new System.Windows.Forms.Panel();
            this.TabControl1 = new System.Windows.Forms.TabControl();
            this.TabPage1 = new System.Windows.Forms.TabPage();
            this.TableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.FlowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.TabPage2 = new System.Windows.Forms.TabPage();
            this.TableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.FlowLayoutPanel2 = new System.Windows.Forms.FlowLayoutPanel();
            this.TabPage3 = new System.Windows.Forms.TabPage();
            this.TableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.lstExcludeSingleFile = new System.Windows.Forms.ListBox();
            this.FlowLayoutPanel3 = new System.Windows.Forms.FlowLayoutPanel();
            this.cmdAddSingleFile = new System.Windows.Forms.Button();
            this.cmdDeleteSingleFile = new System.Windows.Forms.Button();
            this.TabPage4 = new System.Windows.Forms.TabPage();
            this.FlowLayoutPanel4 = new System.Windows.Forms.FlowLayoutPanel();
            this.chkFilesBigger = new System.Windows.Forms.CheckBox();
            this.txtFilesBigger = new System.Windows.Forms.NumericUpDown();
            this.Label4 = new System.Windows.Forms.Label();
            this.LinkLabel1 = new System.Windows.Forms.LinkLabel();
            this.txtRegEx = new System.Windows.Forms.TextBox();
            this.Panel1.SuspendLayout();
            this.Panel3.SuspendLayout();
            this.TabControl1.SuspendLayout();
            this.TabPage1.SuspendLayout();
            this.TableLayoutPanel1.SuspendLayout();
            this.FlowLayoutPanel1.SuspendLayout();
            this.TabPage2.SuspendLayout();
            this.TableLayoutPanel2.SuspendLayout();
            this.FlowLayoutPanel2.SuspendLayout();
            this.TabPage3.SuspendLayout();
            this.TableLayoutPanel3.SuspendLayout();
            this.FlowLayoutPanel3.SuspendLayout();
            this.TabPage4.SuspendLayout();
            this.FlowLayoutPanel4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txtFilesBigger)).BeginInit();
            this.SuspendLayout();
            // 
            // Panel1
            // 
            this.Panel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.Panel1.BackColor = System.Drawing.SystemColors.Control;
            this.Panel1.Controls.Add(this.cmdOK);
            this.Panel1.Controls.Add(this.Label1);
            this.Panel1.Controls.Add(this.cmdCancel);
            this.Panel1.Location = new System.Drawing.Point(0, 736);
            this.Panel1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Panel1.Name = "Panel1";
            this.Panel1.Size = new System.Drawing.Size(904, 68);
            this.Panel1.TabIndex = 6;
            // 
            // cmdOK
            // 
            this.cmdOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdOK.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmdOK.Location = new System.Drawing.Point(588, 15);
            this.cmdOK.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.cmdOK.Name = "cmdOK";
            this.cmdOK.Size = new System.Drawing.Size(140, 39);
            this.cmdOK.TabIndex = 0;
            this.cmdOK.Text = "&Übernehmen";
            this.cmdOK.UseVisualStyleBackColor = true;
            this.cmdOK.Click += new System.EventHandler(this.cmdOK_Click);
            // 
            // Label1
            // 
            this.Label1.BackColor = System.Drawing.Color.DarkGray;
            this.Label1.Dock = System.Windows.Forms.DockStyle.Top;
            this.Label1.Location = new System.Drawing.Point(0, 0);
            this.Label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.Label1.Name = "Label1";
            this.Label1.Size = new System.Drawing.Size(904, 2);
            this.Label1.TabIndex = 5;
            // 
            // cmdCancel
            // 
            this.cmdCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cmdCancel.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmdCancel.Location = new System.Drawing.Point(736, 15);
            this.cmdCancel.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.cmdCancel.Name = "cmdCancel";
            this.cmdCancel.Size = new System.Drawing.Size(140, 39);
            this.cmdCancel.TabIndex = 1;
            this.cmdCancel.Text = "&Abbrechen";
            this.cmdCancel.UseVisualStyleBackColor = true;
            // 
            // Label2
            // 
            this.Label2.AutoSize = true;
            this.Label2.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Label2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(153)))));
            this.Label2.Location = new System.Drawing.Point(27, 27);
            this.Label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.Label2.Name = "Label2";
            this.Label2.Size = new System.Drawing.Size(222, 32);
            this.Label2.TabIndex = 0;
            this.Label2.Text = "Ausschließen von ...";
            // 
            // lstExcludeFolders
            // 
            this.lstExcludeFolders.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lstExcludeFolders.FormattingEnabled = true;
            this.lstExcludeFolders.HorizontalScrollbar = true;
            this.lstExcludeFolders.ItemHeight = 28;
            this.lstExcludeFolders.Location = new System.Drawing.Point(4, 4);
            this.lstExcludeFolders.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.lstExcludeFolders.Name = "lstExcludeFolders";
            this.lstExcludeFolders.ScrollAlwaysVisible = true;
            this.lstExcludeFolders.Size = new System.Drawing.Size(824, 516);
            this.lstExcludeFolders.Sorted = true;
            this.lstExcludeFolders.TabIndex = 8;
            this.lstExcludeFolders.SelectedIndexChanged += new System.EventHandler(this.lstExcludeFolders_SelectedIndexChanged);
            // 
            // cmdDeleteFolders
            // 
            this.cmdDeleteFolders.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.cmdDeleteFolders.Enabled = false;
            this.cmdDeleteFolders.Location = new System.Drawing.Point(152, 4);
            this.cmdDeleteFolders.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.cmdDeleteFolders.Name = "cmdDeleteFolders";
            this.cmdDeleteFolders.Size = new System.Drawing.Size(140, 39);
            this.cmdDeleteFolders.TabIndex = 77;
            this.cmdDeleteFolders.Text = "Löschen";
            this.cmdDeleteFolders.UseVisualStyleBackColor = true;
            this.cmdDeleteFolders.Click += new System.EventHandler(this.cmdDeleteFolders_Click);
            // 
            // cmdAddFolders
            // 
            this.cmdAddFolders.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.cmdAddFolders.Location = new System.Drawing.Point(4, 4);
            this.cmdAddFolders.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.cmdAddFolders.Name = "cmdAddFolders";
            this.cmdAddFolders.Size = new System.Drawing.Size(140, 39);
            this.cmdAddFolders.TabIndex = 76;
            this.cmdAddFolders.Text = "Hinzufügen";
            this.cmdAddFolders.UseVisualStyleBackColor = true;
            this.cmdAddFolders.Click += new System.EventHandler(this.cmdAddFolders_Click);
            // 
            // cmdDeleteFile
            // 
            this.cmdDeleteFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.cmdDeleteFile.Enabled = false;
            this.cmdDeleteFile.Location = new System.Drawing.Point(152, 4);
            this.cmdDeleteFile.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.cmdDeleteFile.Name = "cmdDeleteFile";
            this.cmdDeleteFile.Size = new System.Drawing.Size(140, 39);
            this.cmdDeleteFile.TabIndex = 77;
            this.cmdDeleteFile.Text = "Löschen";
            this.cmdDeleteFile.UseVisualStyleBackColor = true;
            this.cmdDeleteFile.Click += new System.EventHandler(this.cmdDeleteFile_Click);
            // 
            // cmdAddFile
            // 
            this.cmdAddFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.cmdAddFile.Location = new System.Drawing.Point(4, 4);
            this.cmdAddFile.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.cmdAddFile.Name = "cmdAddFile";
            this.cmdAddFile.Size = new System.Drawing.Size(140, 39);
            this.cmdAddFile.TabIndex = 76;
            this.cmdAddFile.Text = "Hinzufügen";
            this.cmdAddFile.UseVisualStyleBackColor = true;
            this.cmdAddFile.Click += new System.EventHandler(this.cmdAddFile_Click);
            // 
            // lstExcludeFiles
            // 
            this.lstExcludeFiles.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lstExcludeFiles.FormattingEnabled = true;
            this.lstExcludeFiles.HorizontalScrollbar = true;
            this.lstExcludeFiles.ItemHeight = 28;
            this.lstExcludeFiles.Location = new System.Drawing.Point(4, 4);
            this.lstExcludeFiles.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.lstExcludeFiles.Name = "lstExcludeFiles";
            this.lstExcludeFiles.ScrollAlwaysVisible = true;
            this.lstExcludeFiles.Size = new System.Drawing.Size(824, 516);
            this.lstExcludeFiles.Sorted = true;
            this.lstExcludeFiles.TabIndex = 8;
            this.lstExcludeFiles.SelectedIndexChanged += new System.EventHandler(this.lstExcludeFiles_SelectedIndexChanged);
            // 
            // Panel3
            // 
            this.Panel3.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.Panel3.Controls.Add(this.TabControl1);
            this.Panel3.Controls.Add(this.Label2);
            this.Panel3.Location = new System.Drawing.Point(0, 0);
            this.Panel3.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Panel3.Name = "Panel3";
            this.Panel3.Size = new System.Drawing.Size(906, 738);
            this.Panel3.TabIndex = 81;
            // 
            // TabControl1
            // 
            this.TabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.TabControl1.Controls.Add(this.TabPage1);
            this.TabControl1.Controls.Add(this.TabPage2);
            this.TabControl1.Controls.Add(this.TabPage3);
            this.TabControl1.Controls.Add(this.TabPage4);
            this.TabControl1.Location = new System.Drawing.Point(33, 84);
            this.TabControl1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.TabControl1.Name = "TabControl1";
            this.TabControl1.SelectedIndex = 0;
            this.TabControl1.Size = new System.Drawing.Size(848, 633);
            this.TabControl1.TabIndex = 91;
            // 
            // TabPage1
            // 
            this.TabPage1.Controls.Add(this.TableLayoutPanel1);
            this.TabPage1.Location = new System.Drawing.Point(4, 37);
            this.TabPage1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.TabPage1.Name = "TabPage1";
            this.TabPage1.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.TabPage1.Size = new System.Drawing.Size(840, 592);
            this.TabPage1.TabIndex = 0;
            this.TabPage1.Text = "Ordner";
            this.TabPage1.UseVisualStyleBackColor = true;
            // 
            // TableLayoutPanel1
            // 
            this.TableLayoutPanel1.ColumnCount = 1;
            this.TableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.TableLayoutPanel1.Controls.Add(this.lstExcludeFolders, 0, 0);
            this.TableLayoutPanel1.Controls.Add(this.FlowLayoutPanel1, 0, 1);
            this.TableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.TableLayoutPanel1.Location = new System.Drawing.Point(4, 4);
            this.TableLayoutPanel1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.TableLayoutPanel1.Name = "TableLayoutPanel1";
            this.TableLayoutPanel1.RowCount = 2;
            this.TableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.TableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 60F));
            this.TableLayoutPanel1.Size = new System.Drawing.Size(832, 584);
            this.TableLayoutPanel1.TabIndex = 78;
            // 
            // FlowLayoutPanel1
            // 
            this.FlowLayoutPanel1.Controls.Add(this.cmdAddFolders);
            this.FlowLayoutPanel1.Controls.Add(this.cmdDeleteFolders);
            this.FlowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.FlowLayoutPanel1.Location = new System.Drawing.Point(4, 528);
            this.FlowLayoutPanel1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.FlowLayoutPanel1.Name = "FlowLayoutPanel1";
            this.FlowLayoutPanel1.Size = new System.Drawing.Size(824, 52);
            this.FlowLayoutPanel1.TabIndex = 9;
            // 
            // TabPage2
            // 
            this.TabPage2.Controls.Add(this.TableLayoutPanel2);
            this.TabPage2.Location = new System.Drawing.Point(4, 37);
            this.TabPage2.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.TabPage2.Name = "TabPage2";
            this.TabPage2.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.TabPage2.Size = new System.Drawing.Size(840, 592);
            this.TabPage2.TabIndex = 1;
            this.TabPage2.Text = "Dateityp";
            this.TabPage2.UseVisualStyleBackColor = true;
            // 
            // TableLayoutPanel2
            // 
            this.TableLayoutPanel2.ColumnCount = 1;
            this.TableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.TableLayoutPanel2.Controls.Add(this.lstExcludeFiles, 0, 0);
            this.TableLayoutPanel2.Controls.Add(this.FlowLayoutPanel2, 0, 1);
            this.TableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.TableLayoutPanel2.Location = new System.Drawing.Point(4, 4);
            this.TableLayoutPanel2.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.TableLayoutPanel2.Name = "TableLayoutPanel2";
            this.TableLayoutPanel2.RowCount = 2;
            this.TableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.TableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 60F));
            this.TableLayoutPanel2.Size = new System.Drawing.Size(832, 584);
            this.TableLayoutPanel2.TabIndex = 78;
            // 
            // FlowLayoutPanel2
            // 
            this.FlowLayoutPanel2.Controls.Add(this.cmdAddFile);
            this.FlowLayoutPanel2.Controls.Add(this.cmdDeleteFile);
            this.FlowLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.FlowLayoutPanel2.Location = new System.Drawing.Point(4, 528);
            this.FlowLayoutPanel2.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.FlowLayoutPanel2.Name = "FlowLayoutPanel2";
            this.FlowLayoutPanel2.Size = new System.Drawing.Size(824, 52);
            this.FlowLayoutPanel2.TabIndex = 9;
            // 
            // TabPage3
            // 
            this.TabPage3.Controls.Add(this.TableLayoutPanel3);
            this.TabPage3.Location = new System.Drawing.Point(4, 37);
            this.TabPage3.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.TabPage3.Name = "TabPage3";
            this.TabPage3.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.TabPage3.Size = new System.Drawing.Size(840, 592);
            this.TabPage3.TabIndex = 2;
            this.TabPage3.Text = "Datei";
            this.TabPage3.UseVisualStyleBackColor = true;
            // 
            // TableLayoutPanel3
            // 
            this.TableLayoutPanel3.ColumnCount = 1;
            this.TableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.TableLayoutPanel3.Controls.Add(this.lstExcludeSingleFile, 0, 0);
            this.TableLayoutPanel3.Controls.Add(this.FlowLayoutPanel3, 0, 1);
            this.TableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.TableLayoutPanel3.Location = new System.Drawing.Point(4, 4);
            this.TableLayoutPanel3.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.TableLayoutPanel3.Name = "TableLayoutPanel3";
            this.TableLayoutPanel3.RowCount = 2;
            this.TableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.TableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 60F));
            this.TableLayoutPanel3.Size = new System.Drawing.Size(832, 584);
            this.TableLayoutPanel3.TabIndex = 81;
            // 
            // lstExcludeSingleFile
            // 
            this.lstExcludeSingleFile.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lstExcludeSingleFile.FormattingEnabled = true;
            this.lstExcludeSingleFile.HorizontalScrollbar = true;
            this.lstExcludeSingleFile.ItemHeight = 28;
            this.lstExcludeSingleFile.Location = new System.Drawing.Point(4, 4);
            this.lstExcludeSingleFile.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.lstExcludeSingleFile.Name = "lstExcludeSingleFile";
            this.lstExcludeSingleFile.ScrollAlwaysVisible = true;
            this.lstExcludeSingleFile.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.lstExcludeSingleFile.Size = new System.Drawing.Size(824, 516);
            this.lstExcludeSingleFile.Sorted = true;
            this.lstExcludeSingleFile.TabIndex = 78;
            this.lstExcludeSingleFile.SelectedIndexChanged += new System.EventHandler(this.lstExcludeSingleFile_SelectedIndexChanged);
            // 
            // FlowLayoutPanel3
            // 
            this.FlowLayoutPanel3.Controls.Add(this.cmdAddSingleFile);
            this.FlowLayoutPanel3.Controls.Add(this.cmdDeleteSingleFile);
            this.FlowLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.FlowLayoutPanel3.Location = new System.Drawing.Point(4, 528);
            this.FlowLayoutPanel3.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.FlowLayoutPanel3.Name = "FlowLayoutPanel3";
            this.FlowLayoutPanel3.Size = new System.Drawing.Size(824, 52);
            this.FlowLayoutPanel3.TabIndex = 79;
            // 
            // cmdAddSingleFile
            // 
            this.cmdAddSingleFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.cmdAddSingleFile.Location = new System.Drawing.Point(4, 4);
            this.cmdAddSingleFile.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.cmdAddSingleFile.Name = "cmdAddSingleFile";
            this.cmdAddSingleFile.Size = new System.Drawing.Size(140, 39);
            this.cmdAddSingleFile.TabIndex = 79;
            this.cmdAddSingleFile.Text = "Hinzufügen";
            this.cmdAddSingleFile.UseVisualStyleBackColor = true;
            this.cmdAddSingleFile.Click += new System.EventHandler(this.cmdAddSingleFile_Click);
            // 
            // cmdDeleteSingleFile
            // 
            this.cmdDeleteSingleFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.cmdDeleteSingleFile.Enabled = false;
            this.cmdDeleteSingleFile.Location = new System.Drawing.Point(152, 4);
            this.cmdDeleteSingleFile.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.cmdDeleteSingleFile.Name = "cmdDeleteSingleFile";
            this.cmdDeleteSingleFile.Size = new System.Drawing.Size(140, 39);
            this.cmdDeleteSingleFile.TabIndex = 80;
            this.cmdDeleteSingleFile.Text = "Löschen";
            this.cmdDeleteSingleFile.UseVisualStyleBackColor = true;
            this.cmdDeleteSingleFile.Click += new System.EventHandler(this.cmdDeleteSingleFile_Click);
            // 
            // TabPage4
            // 
            this.TabPage4.Controls.Add(this.FlowLayoutPanel4);
            this.TabPage4.Controls.Add(this.LinkLabel1);
            this.TabPage4.Controls.Add(this.txtRegEx);
            this.TabPage4.Location = new System.Drawing.Point(4, 37);
            this.TabPage4.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.TabPage4.Name = "TabPage4";
            this.TabPage4.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.TabPage4.Size = new System.Drawing.Size(840, 592);
            this.TabPage4.TabIndex = 3;
            this.TabPage4.Text = "Sonstige";
            this.TabPage4.UseVisualStyleBackColor = true;
            // 
            // FlowLayoutPanel4
            // 
            this.FlowLayoutPanel4.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.FlowLayoutPanel4.Controls.Add(this.chkFilesBigger);
            this.FlowLayoutPanel4.Controls.Add(this.txtFilesBigger);
            this.FlowLayoutPanel4.Controls.Add(this.Label4);
            this.FlowLayoutPanel4.Location = new System.Drawing.Point(27, 446);
            this.FlowLayoutPanel4.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.FlowLayoutPanel4.Name = "FlowLayoutPanel4";
            this.FlowLayoutPanel4.Size = new System.Drawing.Size(784, 56);
            this.FlowLayoutPanel4.TabIndex = 91;
            // 
            // chkFilesBigger
            // 
            this.chkFilesBigger.AutoSize = true;
            this.chkFilesBigger.Location = new System.Drawing.Point(4, 4);
            this.chkFilesBigger.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.chkFilesBigger.Name = "chkFilesBigger";
            this.chkFilesBigger.Size = new System.Drawing.Size(392, 32);
            this.chkFilesBigger.TabIndex = 82;
            this.chkFilesBigger.Text = "Dateien ausschließen, die größer sind als:";
            this.chkFilesBigger.UseVisualStyleBackColor = true;
            this.chkFilesBigger.CheckedChanged += new System.EventHandler(this.chkFilesBigger_CheckedChanged);
            // 
            // txtFilesBigger
            // 
            this.txtFilesBigger.Location = new System.Drawing.Point(404, 4);
            this.txtFilesBigger.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.txtFilesBigger.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.txtFilesBigger.Minimum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.txtFilesBigger.Name = "txtFilesBigger";
            this.txtFilesBigger.Size = new System.Drawing.Size(94, 33);
            this.txtFilesBigger.TabIndex = 85;
            this.txtFilesBigger.Value = new decimal(new int[] {
            10,
            0,
            0,
            0});
            // 
            // Label4
            // 
            this.Label4.AutoSize = true;
            this.Label4.Location = new System.Drawing.Point(506, 8);
            this.Label4.Margin = new System.Windows.Forms.Padding(4, 8, 4, 0);
            this.Label4.Name = "Label4";
            this.Label4.Size = new System.Drawing.Size(41, 28);
            this.Label4.TabIndex = 84;
            this.Label4.Text = "MB";
            // 
            // LinkLabel1
            // 
            this.LinkLabel1.AutoSize = true;
            this.LinkLabel1.Location = new System.Drawing.Point(22, 30);
            this.LinkLabel1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.LinkLabel1.Name = "LinkLabel1";
            this.LinkLabel1.Size = new System.Drawing.Size(141, 28);
            this.LinkLabel1.TabIndex = 90;
            this.LinkLabel1.TabStop = true;
            this.LinkLabel1.Text = "RegEx (Maske):";
            this.LinkLabel1.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LinkLabel1_LinkClicked);
            // 
            // txtRegEx
            // 
            this.txtRegEx.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtRegEx.Location = new System.Drawing.Point(27, 62);
            this.txtRegEx.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.txtRegEx.Multiline = true;
            this.txtRegEx.Name = "txtRegEx";
            this.txtRegEx.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtRegEx.Size = new System.Drawing.Size(782, 360);
            this.txtRegEx.TabIndex = 89;
            // 
            // frmFilter
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(144F, 144F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(904, 804);
            this.Controls.Add(this.Panel3);
            this.Controls.Add(this.Panel1);
            this.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Name = "frmFilter";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Filter einrichten";
            this.Load += new System.EventHandler(this.frmFilter_Load);
            this.Panel1.ResumeLayout(false);
            this.Panel3.ResumeLayout(false);
            this.Panel3.PerformLayout();
            this.TabControl1.ResumeLayout(false);
            this.TabPage1.ResumeLayout(false);
            this.TableLayoutPanel1.ResumeLayout(false);
            this.FlowLayoutPanel1.ResumeLayout(false);
            this.TabPage2.ResumeLayout(false);
            this.TableLayoutPanel2.ResumeLayout(false);
            this.FlowLayoutPanel2.ResumeLayout(false);
            this.TabPage3.ResumeLayout(false);
            this.TableLayoutPanel3.ResumeLayout(false);
            this.FlowLayoutPanel3.ResumeLayout(false);
            this.TabPage4.ResumeLayout(false);
            this.TabPage4.PerformLayout();
            this.FlowLayoutPanel4.ResumeLayout(false);
            this.FlowLayoutPanel4.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txtFilesBigger)).EndInit();
            this.ResumeLayout(false);

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