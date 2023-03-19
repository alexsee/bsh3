using System.Diagnostics;
using System.Windows.Forms;

namespace Brightbits.BSH.Main
{
    public partial class frmBrowser : Form
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmBrowser));
            System.Windows.Forms.ListViewGroup listViewGroup3 = new System.Windows.Forms.ListViewGroup("Ordner", System.Windows.Forms.HorizontalAlignment.Left);
            System.Windows.Forms.ListViewGroup listViewGroup4 = new System.Windows.Forms.ListViewGroup("Dateien", System.Windows.Forms.HorizontalAlignment.Left);
            this.SplitContainer1 = new System.Windows.Forms.SplitContainer();
            this.plVersions = new System.Windows.Forms.Panel();
            this.cmdTakeMeLater = new System.Windows.Forms.Button();
            this.cmdTakeMeBack = new System.Windows.Forms.Button();
            this.AVersionList1 = new Brightbits.BSH.Main.aVersionList();
            this.lblExpand2 = new System.Windows.Forms.Label();
            this.plFavorites = new System.Windows.Forms.Panel();
            this.lvFavorite = new System.Windows.Forms.ListView();
            this.colName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.cmnuFavorits = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.LöschenToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.UmbenennenToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ilFolder = new System.Windows.Forms.ImageList(this.components);
            this.lblExpand1 = new System.Windows.Forms.Label();
            this.lvFiles = new System.Windows.Forms.ListView();
            this.ColumnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.ColumnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.ColumnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.ColumnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.ColumnHeader6 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.ColumnHeader5 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.cmnuListRight = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.WiederherstellenToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.EigenschaftenToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolStripMenuItem5 = new System.Windows.Forms.ToolStripSeparator();
            this.DateiOrdnerAusSicherungenLöschenToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolStripMenuItem3 = new System.Windows.Forms.ToolStripSeparator();
            this.SchnellansichtToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.ilBigIcons = new System.Windows.Forms.ImageList(this.components);
            this.ilSmallIcons = new System.Windows.Forms.ImageList(this.components);
            this.Panel3 = new System.Windows.Forms.Panel();
            this.chkFilesOfThisVersion = new System.Windows.Forms.CheckBox();
            this.lblBackupdate = new System.Windows.Forms.Label();
            this.ilBigFolder = new System.Windows.Forms.ImageList(this.components);
            this.StatusStrip1 = new System.Windows.Forms.StatusStrip();
            this.LCFiles = new MRG.Controls.UI.LoadingCircleToolStripMenuItem();
            this.tsslblStatus = new System.Windows.Forms.ToolStripStatusLabel();
            this.ttMain = new System.Windows.Forms.ToolTip(this.components);
            this.bgrWorkSearch = new System.ComponentModel.BackgroundWorker();
            this.tMain = new System.Windows.Forms.ToolStrip();
            this.ToolStripDropDownButton1 = new System.Windows.Forms.ToolStripDropDownButton();
            this.WiederherstellenToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.AllesWiederherstellenToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.EigenschaftenToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
            this.SchnellansichtToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ZuOrdnerfavoritenHinzufügenToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolStripMenuItem2 = new System.Windows.Forms.ToolStripSeparator();
            this.VersionBearbeitenToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.VersionLöschenToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.VersionAlsStabilMarkierenToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolStripMenuItem4 = new System.Windows.Forms.ToolStripSeparator();
            this.MehrereVersionenLöschenToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolStripSchnellansicht = new System.Windows.Forms.ToolStripButton();
            this.ToolStripDropDownButton2 = new System.Windows.Forms.ToolStripDropDownButton();
            this.GroßeSymboleToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ListenansichtToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.DetailsansichtToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.cmdRestore = new System.Windows.Forms.ToolStripButton();
            this.plGlass = new System.Windows.Forms.Panel();
            this.UcNav = new Brightbits.BSH.Main.ucNavigation();
            this.Panel2 = new System.Windows.Forms.Panel();
            this.panel4 = new System.Windows.Forms.Panel();
            this.PictureBox3 = new System.Windows.Forms.PictureBox();
            this.txtSearch = new System.Windows.Forms.TextBox();
            this.btnBack = new System.Windows.Forms.PictureBox();
            this.Panel1 = new System.Windows.Forms.Panel();
            this.flpDetails = new System.Windows.Forms.FlowLayoutPanel();
            this.flpColumn1 = new System.Windows.Forms.FlowLayoutPanel();
            this.lblFileName = new System.Windows.Forms.Label();
            this.lblFileType = new System.Windows.Forms.Label();
            this.flpColumn2 = new System.Windows.Forms.FlowLayoutPanel();
            this.flpDateLastWrite = new System.Windows.Forms.FlowLayoutPanel();
            this.lFileLastEdited = new System.Windows.Forms.Label();
            this.lblFileLastEdited = new System.Windows.Forms.Label();
            this.flpDateCreated = new System.Windows.Forms.FlowLayoutPanel();
            this.lFileCreated = new System.Windows.Forms.Label();
            this.lblFileCreated = new System.Windows.Forms.Label();
            this.flpColumn3 = new System.Windows.Forms.FlowLayoutPanel();
            this.flpSize = new System.Windows.Forms.FlowLayoutPanel();
            this.lFileSize = new System.Windows.Forms.Label();
            this.lblFileSize = new System.Windows.Forms.Label();
            this.flpVersion = new System.Windows.Forms.FlowLayoutPanel();
            this.lFileVersion = new System.Windows.Forms.Label();
            this.lblFileVersion = new System.Windows.Forms.Label();
            this.lblIntegrityCheck = new System.Windows.Forms.Label();
            this.Label4 = new System.Windows.Forms.Label();
            this.imgFileType = new System.Windows.Forms.PictureBox();
            this.AllesWiederherstellenToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)(this.SplitContainer1)).BeginInit();
            this.SplitContainer1.Panel1.SuspendLayout();
            this.SplitContainer1.Panel2.SuspendLayout();
            this.SplitContainer1.SuspendLayout();
            this.plVersions.SuspendLayout();
            this.plFavorites.SuspendLayout();
            this.cmnuFavorits.SuspendLayout();
            this.cmnuListRight.SuspendLayout();
            this.Panel3.SuspendLayout();
            this.StatusStrip1.SuspendLayout();
            this.tMain.SuspendLayout();
            this.plGlass.SuspendLayout();
            this.Panel2.SuspendLayout();
            this.panel4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.PictureBox3)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnBack)).BeginInit();
            this.Panel1.SuspendLayout();
            this.flpDetails.SuspendLayout();
            this.flpColumn1.SuspendLayout();
            this.flpColumn2.SuspendLayout();
            this.flpDateLastWrite.SuspendLayout();
            this.flpDateCreated.SuspendLayout();
            this.flpColumn3.SuspendLayout();
            this.flpSize.SuspendLayout();
            this.flpVersion.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.imgFileType)).BeginInit();
            this.SuspendLayout();
            // 
            // SplitContainer1
            // 
            this.SplitContainer1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(209)))), ((int)(((byte)(211)))));
            this.SplitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.SplitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.SplitContainer1.Location = new System.Drawing.Point(0, 113);
            this.SplitContainer1.Margin = new System.Windows.Forms.Padding(4);
            this.SplitContainer1.Name = "SplitContainer1";
            // 
            // SplitContainer1.Panel1
            // 
            this.SplitContainer1.Panel1.BackColor = System.Drawing.Color.White;
            this.SplitContainer1.Panel1.Controls.Add(this.plVersions);
            this.SplitContainer1.Panel1.Controls.Add(this.plFavorites);
            this.SplitContainer1.Panel1MinSize = 235;
            // 
            // SplitContainer1.Panel2
            // 
            this.SplitContainer1.Panel2.Controls.Add(this.lvFiles);
            this.SplitContainer1.Panel2.Controls.Add(this.Panel3);
            this.SplitContainer1.Size = new System.Drawing.Size(1188, 611);
            this.SplitContainer1.SplitterDistance = 235;
            this.SplitContainer1.SplitterWidth = 2;
            this.SplitContainer1.TabIndex = 1;
            // 
            // plVersions
            // 
            this.plVersions.AutoScroll = true;
            this.plVersions.BackColor = System.Drawing.Color.White;
            this.plVersions.Controls.Add(this.cmdTakeMeLater);
            this.plVersions.Controls.Add(this.cmdTakeMeBack);
            this.plVersions.Controls.Add(this.AVersionList1);
            this.plVersions.Controls.Add(this.lblExpand2);
            this.plVersions.Dock = System.Windows.Forms.DockStyle.Fill;
            this.plVersions.Location = new System.Drawing.Point(0, 198);
            this.plVersions.Margin = new System.Windows.Forms.Padding(4);
            this.plVersions.Name = "plVersions";
            this.plVersions.Size = new System.Drawing.Size(235, 413);
            this.plVersions.TabIndex = 1;
            // 
            // cmdTakeMeLater
            // 
            this.cmdTakeMeLater.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.cmdTakeMeLater.Cursor = System.Windows.Forms.Cursors.Hand;
            this.cmdTakeMeLater.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmdTakeMeLater.Image = ((System.Drawing.Image)(resources.GetObject("cmdTakeMeLater.Image")));
            this.cmdTakeMeLater.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.cmdTakeMeLater.Location = new System.Drawing.Point(19, 308);
            this.cmdTakeMeLater.Margin = new System.Windows.Forms.Padding(4);
            this.cmdTakeMeLater.Name = "cmdTakeMeLater";
            this.cmdTakeMeLater.Size = new System.Drawing.Size(195, 38);
            this.cmdTakeMeLater.TabIndex = 4;
            this.cmdTakeMeLater.TabStop = false;
            this.cmdTakeMeLater.Text = "Neuer";
            this.cmdTakeMeLater.TextImageRelation = System.Windows.Forms.TextImageRelation.TextBeforeImage;
            this.ttMain.SetToolTip(this.cmdTakeMeLater, "Eine ältere Version anzeigen.");
            this.cmdTakeMeLater.UseVisualStyleBackColor = true;
            this.cmdTakeMeLater.Click += new System.EventHandler(this.cmdTakeMeLater_Click);
            // 
            // cmdTakeMeBack
            // 
            this.cmdTakeMeBack.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.cmdTakeMeBack.Cursor = System.Windows.Forms.Cursors.Hand;
            this.cmdTakeMeBack.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmdTakeMeBack.Image = ((System.Drawing.Image)(resources.GetObject("cmdTakeMeBack.Image")));
            this.cmdTakeMeBack.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.cmdTakeMeBack.Location = new System.Drawing.Point(19, 356);
            this.cmdTakeMeBack.Margin = new System.Windows.Forms.Padding(4);
            this.cmdTakeMeBack.Name = "cmdTakeMeBack";
            this.cmdTakeMeBack.Size = new System.Drawing.Size(195, 38);
            this.cmdTakeMeBack.TabIndex = 3;
            this.cmdTakeMeBack.TabStop = false;
            this.cmdTakeMeBack.Text = "Älter";
            this.cmdTakeMeBack.TextImageRelation = System.Windows.Forms.TextImageRelation.TextBeforeImage;
            this.ttMain.SetToolTip(this.cmdTakeMeBack, "Eine frühere Version anzeigen.");
            this.cmdTakeMeBack.UseVisualStyleBackColor = true;
            this.cmdTakeMeBack.Click += new System.EventHandler(this.cmdTakeMeBack_Click);
            // 
            // AVersionList1
            // 
            this.AVersionList1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.AVersionList1.BackColor = System.Drawing.Color.White;
            this.AVersionList1.Location = new System.Drawing.Point(0, 66);
            this.AVersionList1.Margin = new System.Windows.Forms.Padding(4);
            this.AVersionList1.Name = "AVersionList1";
            this.AVersionList1.Size = new System.Drawing.Size(235, 226);
            this.AVersionList1.TabIndex = 2;
            this.AVersionList1.TabStop = false;
            this.AVersionList1.ItemClick += new Brightbits.BSH.Main.aVersionList.ItemClickEventHandler(this.AVersionList1_ItemClick);
            // 
            // lblExpand2
            // 
            this.lblExpand2.AutoSize = true;
            this.lblExpand2.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.lblExpand2.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblExpand2.ForeColor = System.Drawing.Color.Black;
            this.lblExpand2.Location = new System.Drawing.Point(21, 15);
            this.lblExpand2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblExpand2.Name = "lblExpand2";
            this.lblExpand2.Size = new System.Drawing.Size(148, 32);
            this.lblExpand2.TabIndex = 1;
            this.lblExpand2.Text = "Sicherungen";
            // 
            // plFavorites
            // 
            this.plFavorites.AutoScroll = true;
            this.plFavorites.BackColor = System.Drawing.Color.White;
            this.plFavorites.Controls.Add(this.lvFavorite);
            this.plFavorites.Controls.Add(this.lblExpand1);
            this.plFavorites.Dock = System.Windows.Forms.DockStyle.Top;
            this.plFavorites.Location = new System.Drawing.Point(0, 0);
            this.plFavorites.Margin = new System.Windows.Forms.Padding(4);
            this.plFavorites.Name = "plFavorites";
            this.plFavorites.Size = new System.Drawing.Size(235, 198);
            this.plFavorites.TabIndex = 0;
            // 
            // lvFavorite
            // 
            this.lvFavorite.Alignment = System.Windows.Forms.ListViewAlignment.Left;
            this.lvFavorite.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lvFavorite.BackColor = System.Drawing.Color.White;
            this.lvFavorite.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.lvFavorite.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colName});
            this.lvFavorite.ContextMenuStrip = this.cmnuFavorits;
            this.lvFavorite.FullRowSelect = true;
            this.lvFavorite.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.lvFavorite.HideSelection = false;
            this.lvFavorite.LabelEdit = true;
            this.lvFavorite.LargeImageList = this.ilFolder;
            this.lvFavorite.Location = new System.Drawing.Point(0, 63);
            this.lvFavorite.Margin = new System.Windows.Forms.Padding(4);
            this.lvFavorite.MultiSelect = false;
            this.lvFavorite.Name = "lvFavorite";
            this.lvFavorite.Scrollable = false;
            this.lvFavorite.ShowGroups = false;
            this.lvFavorite.ShowItemToolTips = true;
            this.lvFavorite.Size = new System.Drawing.Size(231, 126);
            this.lvFavorite.SmallImageList = this.ilFolder;
            this.lvFavorite.StateImageList = this.ilFolder;
            this.lvFavorite.TabIndex = 2;
            this.lvFavorite.TabStop = false;
            this.lvFavorite.TileSize = new System.Drawing.Size(16, 16);
            this.ttMain.SetToolTip(this.lvFavorite, "Ordnerfavoriten.\r\n\r\nHier können Sie Ordnerfavoriten festlegen, mit denen \r\nSie sc" +
        "hneller zu einem bestimmten Ziel navigieren können.");
            this.lvFavorite.UseCompatibleStateImageBehavior = false;
            this.lvFavorite.View = System.Windows.Forms.View.Details;
            this.lvFavorite.AfterLabelEdit += new System.Windows.Forms.LabelEditEventHandler(this.lvFavorite_AfterLabelEdit);
            this.lvFavorite.BeforeLabelEdit += new System.Windows.Forms.LabelEditEventHandler(this.lvFavorite_BeforeLabelEdit);
            this.lvFavorite.SizeChanged += new System.EventHandler(this.lvFavorite_SizeChanged);
            this.lvFavorite.MouseClick += new System.Windows.Forms.MouseEventHandler(this.lvFavorite_Click);
            // 
            // colName
            // 
            this.colName.Text = "Name";
            this.colName.Width = 100;
            // 
            // cmnuFavorits
            // 
            this.cmnuFavorits.Font = new System.Drawing.Font("Segoe UI", 9.75F);
            this.cmnuFavorits.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.cmnuFavorits.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.LöschenToolStripMenuItem,
            this.UmbenennenToolStripMenuItem});
            this.cmnuFavorits.Name = "cmnuFavorits";
            this.cmnuFavorits.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.cmnuFavorits.Size = new System.Drawing.Size(210, 72);
            this.cmnuFavorits.Opening += new System.ComponentModel.CancelEventHandler(this.cmnuFavorits_Opening);
            // 
            // LöschenToolStripMenuItem
            // 
            this.LöschenToolStripMenuItem.Image = global::BSH.Main.Properties.Resources.delete_icon_24;
            this.LöschenToolStripMenuItem.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.LöschenToolStripMenuItem.Name = "LöschenToolStripMenuItem";
            this.LöschenToolStripMenuItem.Size = new System.Drawing.Size(209, 34);
            this.LöschenToolStripMenuItem.Text = "Löschen";
            this.LöschenToolStripMenuItem.Click += new System.EventHandler(this.LöschenToolStripMenuItem_Click);
            // 
            // UmbenennenToolStripMenuItem
            // 
            this.UmbenennenToolStripMenuItem.Name = "UmbenennenToolStripMenuItem";
            this.UmbenennenToolStripMenuItem.Size = new System.Drawing.Size(209, 34);
            this.UmbenennenToolStripMenuItem.Text = "Umbenennen";
            this.UmbenennenToolStripMenuItem.Click += new System.EventHandler(this.UmbenennenToolStripMenuItem_Click);
            // 
            // ilFolder
            // 
            this.ilFolder.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("ilFolder.ImageStream")));
            this.ilFolder.TransparentColor = System.Drawing.Color.Transparent;
            this.ilFolder.Images.SetKeyName(0, "folder_icon_24.png");
            this.ilFolder.Images.SetKeyName(1, "folder_open_icon_24.png");
            this.ilFolder.Images.SetKeyName(2, "home_icon_24.png");
            this.ilFolder.Images.SetKeyName(3, "tab_icon_24.png");
            // 
            // lblExpand1
            // 
            this.lblExpand1.AutoSize = true;
            this.lblExpand1.Cursor = System.Windows.Forms.Cursors.Default;
            this.lblExpand1.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblExpand1.ForeColor = System.Drawing.Color.Black;
            this.lblExpand1.Location = new System.Drawing.Point(20, 15);
            this.lblExpand1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblExpand1.Name = "lblExpand1";
            this.lblExpand1.Size = new System.Drawing.Size(112, 32);
            this.lblExpand1.TabIndex = 1;
            this.lblExpand1.Text = "Favoriten";
            // 
            // lvFiles
            // 
            this.lvFiles.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.lvFiles.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.ColumnHeader1,
            this.ColumnHeader2,
            this.ColumnHeader3,
            this.ColumnHeader4,
            this.ColumnHeader6,
            this.ColumnHeader5});
            this.lvFiles.ContextMenuStrip = this.cmnuListRight;
            this.lvFiles.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lvFiles.FullRowSelect = true;
            listViewGroup3.Header = "Ordner";
            listViewGroup3.Name = "Ordner";
            listViewGroup4.Header = "Dateien";
            listViewGroup4.Name = "Dateien";
            this.lvFiles.Groups.AddRange(new System.Windows.Forms.ListViewGroup[] {
            listViewGroup3,
            listViewGroup4});
            this.lvFiles.HideSelection = false;
            this.lvFiles.LargeImageList = this.ilBigIcons;
            this.lvFiles.Location = new System.Drawing.Point(0, 58);
            this.lvFiles.Margin = new System.Windows.Forms.Padding(4);
            this.lvFiles.Name = "lvFiles";
            this.lvFiles.Size = new System.Drawing.Size(951, 553);
            this.lvFiles.SmallImageList = this.ilSmallIcons;
            this.lvFiles.Sorting = System.Windows.Forms.SortOrder.Ascending;
            this.lvFiles.TabIndex = 0;
            this.lvFiles.UseCompatibleStateImageBehavior = false;
            this.lvFiles.View = System.Windows.Forms.View.Details;
            this.lvFiles.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.lvFiles_ColumnClick);
            this.lvFiles.SelectedIndexChanged += new System.EventHandler(this.lvFiles_SelectedIndexChanged);
            this.lvFiles.KeyUp += new System.Windows.Forms.KeyEventHandler(this.lvFiles_KeyUp);
            this.lvFiles.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.lvFiles_MouseDoubleClick);
            // 
            // ColumnHeader1
            // 
            this.ColumnHeader1.Text = "Name";
            this.ColumnHeader1.Width = 244;
            // 
            // ColumnHeader2
            // 
            this.ColumnHeader2.DisplayIndex = 3;
            this.ColumnHeader2.Text = "Größe";
            this.ColumnHeader2.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.ColumnHeader2.Width = 93;
            // 
            // ColumnHeader3
            // 
            this.ColumnHeader3.Text = "Typ";
            this.ColumnHeader3.Width = 182;
            // 
            // ColumnHeader4
            // 
            this.ColumnHeader4.DisplayIndex = 1;
            this.ColumnHeader4.Text = "Änderungsdatum";
            this.ColumnHeader4.Width = 130;
            // 
            // ColumnHeader6
            // 
            this.ColumnHeader6.Text = "Erstelldatum";
            this.ColumnHeader6.Width = 158;
            // 
            // ColumnHeader5
            // 
            this.ColumnHeader5.Text = "Version";
            this.ColumnHeader5.Width = 140;
            // 
            // cmnuListRight
            // 
            this.cmnuListRight.Font = new System.Drawing.Font("Segoe UI", 9.75F);
            this.cmnuListRight.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.cmnuListRight.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.WiederherstellenToolStripMenuItem1,
            this.EigenschaftenToolStripMenuItem1,
            this.ToolStripMenuItem5,
            this.DateiOrdnerAusSicherungenLöschenToolStripMenuItem,
            this.ToolStripMenuItem3,
            this.SchnellansichtToolStripMenuItem1});
            this.cmnuListRight.Name = "cmnuListRight";
            this.cmnuListRight.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.cmnuListRight.Size = new System.Drawing.Size(437, 152);
            this.cmnuListRight.Opening += new System.ComponentModel.CancelEventHandler(this.cmnuListRight_Opening);
            // 
            // WiederherstellenToolStripMenuItem1
            // 
            this.WiederherstellenToolStripMenuItem1.Image = global::BSH.Main.Properties.Resources.settings_backup_restore_icon_24;
            this.WiederherstellenToolStripMenuItem1.Name = "WiederherstellenToolStripMenuItem1";
            this.WiederherstellenToolStripMenuItem1.Size = new System.Drawing.Size(436, 34);
            this.WiederherstellenToolStripMenuItem1.Text = "Wiederherstellen";
            this.WiederherstellenToolStripMenuItem1.Click += new System.EventHandler(this.WiederherstellenToolStripMenuItem1_Click);
            // 
            // EigenschaftenToolStripMenuItem1
            // 
            this.EigenschaftenToolStripMenuItem1.Name = "EigenschaftenToolStripMenuItem1";
            this.EigenschaftenToolStripMenuItem1.Size = new System.Drawing.Size(436, 34);
            this.EigenschaftenToolStripMenuItem1.Text = "Eigenschaften";
            this.EigenschaftenToolStripMenuItem1.Click += new System.EventHandler(this.EigenschaftenToolStripMenuItem1_Click);
            // 
            // ToolStripMenuItem5
            // 
            this.ToolStripMenuItem5.Name = "ToolStripMenuItem5";
            this.ToolStripMenuItem5.Size = new System.Drawing.Size(433, 6);
            // 
            // DateiOrdnerAusSicherungenLöschenToolStripMenuItem
            // 
            this.DateiOrdnerAusSicherungenLöschenToolStripMenuItem.Name = "DateiOrdnerAusSicherungenLöschenToolStripMenuItem";
            this.DateiOrdnerAusSicherungenLöschenToolStripMenuItem.Size = new System.Drawing.Size(436, 34);
            this.DateiOrdnerAusSicherungenLöschenToolStripMenuItem.Text = "Datei / Ordner aus Sicherungen löschen";
            this.DateiOrdnerAusSicherungenLöschenToolStripMenuItem.Click += new System.EventHandler(this.DateiOrdnerAusSicherungenLöschenToolStripMenuItem_Click);
            // 
            // ToolStripMenuItem3
            // 
            this.ToolStripMenuItem3.Name = "ToolStripMenuItem3";
            this.ToolStripMenuItem3.Size = new System.Drawing.Size(433, 6);
            // 
            // SchnellansichtToolStripMenuItem1
            // 
            this.SchnellansichtToolStripMenuItem1.Image = global::BSH.Main.Properties.Resources.visibility_icon_24;
            this.SchnellansichtToolStripMenuItem1.Name = "SchnellansichtToolStripMenuItem1";
            this.SchnellansichtToolStripMenuItem1.ShortcutKeyDisplayString = "Leertaste";
            this.SchnellansichtToolStripMenuItem1.Size = new System.Drawing.Size(436, 34);
            this.SchnellansichtToolStripMenuItem1.Text = "Schnellansicht";
            this.SchnellansichtToolStripMenuItem1.Click += new System.EventHandler(this.SchnellansichtToolStripMenuItem1_Click);
            // 
            // ilBigIcons
            // 
            this.ilBigIcons.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("ilBigIcons.ImageStream")));
            this.ilBigIcons.TransparentColor = System.Drawing.Color.White;
            this.ilBigIcons.Images.SetKeyName(0, "folder");
            // 
            // ilSmallIcons
            // 
            this.ilSmallIcons.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("ilSmallIcons.ImageStream")));
            this.ilSmallIcons.TransparentColor = System.Drawing.Color.White;
            this.ilSmallIcons.Images.SetKeyName(0, "folder");
            // 
            // Panel3
            // 
            this.Panel3.BackColor = System.Drawing.Color.White;
            this.Panel3.Controls.Add(this.chkFilesOfThisVersion);
            this.Panel3.Controls.Add(this.lblBackupdate);
            this.Panel3.Dock = System.Windows.Forms.DockStyle.Top;
            this.Panel3.Location = new System.Drawing.Point(0, 0);
            this.Panel3.Margin = new System.Windows.Forms.Padding(4);
            this.Panel3.Name = "Panel3";
            this.Panel3.Size = new System.Drawing.Size(951, 58);
            this.Panel3.TabIndex = 1;
            // 
            // chkFilesOfThisVersion
            // 
            this.chkFilesOfThisVersion.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.chkFilesOfThisVersion.AutoSize = true;
            this.chkFilesOfThisVersion.Location = new System.Drawing.Point(636, 15);
            this.chkFilesOfThisVersion.Margin = new System.Windows.Forms.Padding(4);
            this.chkFilesOfThisVersion.Name = "chkFilesOfThisVersion";
            this.chkFilesOfThisVersion.Size = new System.Drawing.Size(292, 32);
            this.chkFilesOfThisVersion.TabIndex = 1;
            this.chkFilesOfThisVersion.TabStop = false;
            this.chkFilesOfThisVersion.Text = "Nur Dateien dieser Sicherung";
            this.ttMain.SetToolTip(this.chkFilesOfThisVersion, "Zeigt nur die Dateien an, die in der ausgewählten Sicherung gesichert wurden.");
            this.chkFilesOfThisVersion.UseVisualStyleBackColor = true;
            this.chkFilesOfThisVersion.CheckedChanged += new System.EventHandler(this.chkFilesOfThisVersion_CheckedChanged);
            // 
            // lblBackupdate
            // 
            this.lblBackupdate.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblBackupdate.AutoEllipsis = true;
            this.lblBackupdate.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblBackupdate.ForeColor = System.Drawing.Color.Black;
            this.lblBackupdate.Location = new System.Drawing.Point(16, 13);
            this.lblBackupdate.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblBackupdate.Name = "lblBackupdate";
            this.lblBackupdate.Size = new System.Drawing.Size(611, 33);
            this.lblBackupdate.TabIndex = 0;
            // 
            // ilBigFolder
            // 
            this.ilBigFolder.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("ilBigFolder.ImageStream")));
            this.ilBigFolder.TransparentColor = System.Drawing.Color.Transparent;
            this.ilBigFolder.Images.SetKeyName(0, "folder_open_icon_48.png");
            this.ilBigFolder.Images.SetKeyName(1, "folder_icon_48.png");
            // 
            // StatusStrip1
            // 
            this.StatusStrip1.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.StatusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.LCFiles,
            this.tsslblStatus});
            this.StatusStrip1.Location = new System.Drawing.Point(0, 817);
            this.StatusStrip1.Name = "StatusStrip1";
            this.StatusStrip1.Padding = new System.Windows.Forms.Padding(2, 0, 21, 0);
            this.StatusStrip1.Size = new System.Drawing.Size(1188, 32);
            this.StatusStrip1.TabIndex = 8;
            this.StatusStrip1.Text = "StatusStrip1";
            // 
            // LCFiles
            // 
            this.LCFiles.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            // 
            // LCFiles
            // 
            this.LCFiles.LoadingCircleControl.AccessibleName = "LCFiles";
            this.LCFiles.LoadingCircleControl.Active = false;
            this.LCFiles.LoadingCircleControl.Color = System.Drawing.Color.DarkGray;
            this.LCFiles.LoadingCircleControl.InnerCircleRadius = 8;
            this.LCFiles.LoadingCircleControl.Location = new System.Drawing.Point(2, 3);
            this.LCFiles.LoadingCircleControl.Name = "LCFiles";
            this.LCFiles.LoadingCircleControl.NumberSpoke = 24;
            this.LCFiles.LoadingCircleControl.OuterCircleRadius = 9;
            this.LCFiles.LoadingCircleControl.RotationSpeed = 100;
            this.LCFiles.LoadingCircleControl.Size = new System.Drawing.Size(26, 29);
            this.LCFiles.LoadingCircleControl.SpokeThickness = 4;
            this.LCFiles.LoadingCircleControl.StylePreset = MRG.Controls.UI.LoadingCircle.StylePresets.IE7;
            this.LCFiles.LoadingCircleControl.TabIndex = 1;
            this.LCFiles.Name = "LCFiles";
            this.LCFiles.Overflow = System.Windows.Forms.ToolStripItemOverflow.Never;
            this.LCFiles.Size = new System.Drawing.Size(26, 29);
            // 
            // tsslblStatus
            // 
            this.tsslblStatus.Name = "tsslblStatus";
            this.tsslblStatus.Size = new System.Drawing.Size(56, 25);
            this.tsslblStatus.Text = "Bereit";
            // 
            // ttMain
            // 
            this.ttMain.ToolTipIcon = System.Windows.Forms.ToolTipIcon.Info;
            this.ttMain.ToolTipTitle = "Quickhilfe";
            // 
            // bgrWorkSearch
            // 
            this.bgrWorkSearch.WorkerReportsProgress = true;
            this.bgrWorkSearch.WorkerSupportsCancellation = true;
            this.bgrWorkSearch.DoWork += new System.ComponentModel.DoWorkEventHandler(this.bgrWorkSearch_DoWork);
            this.bgrWorkSearch.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.bgrWorkSearch_RunWorkerCompleted);
            // 
            // tMain
            // 
            this.tMain.AllowMerge = false;
            this.tMain.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(243)))), ((int)(((byte)(243)))), ((int)(((byte)(245)))));
            this.tMain.CanOverflow = false;
            this.tMain.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tMain.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.tMain.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.tMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ToolStripDropDownButton1,
            this.ToolStripSchnellansicht,
            this.ToolStripDropDownButton2,
            this.cmdRestore});
            this.tMain.Location = new System.Drawing.Point(0, 0);
            this.tMain.Name = "tMain";
            this.tMain.Padding = new System.Windows.Forms.Padding(15, 2, 15, 2);
            this.tMain.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.tMain.Size = new System.Drawing.Size(1188, 50);
            this.tMain.TabIndex = 0;
            // 
            // ToolStripDropDownButton1
            // 
            this.ToolStripDropDownButton1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.WiederherstellenToolStripMenuItem,
            this.AllesWiederherstellenToolStripMenuItem1,
            this.EigenschaftenToolStripMenuItem,
            this.ToolStripMenuItem1,
            this.SchnellansichtToolStripMenuItem,
            this.ZuOrdnerfavoritenHinzufügenToolStripMenuItem,
            this.ToolStripMenuItem2,
            this.VersionBearbeitenToolStripMenuItem,
            this.VersionLöschenToolStripMenuItem,
            this.VersionAlsStabilMarkierenToolStripMenuItem,
            this.ToolStripMenuItem4,
            this.MehrereVersionenLöschenToolStripMenuItem});
            this.ToolStripDropDownButton1.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ToolStripDropDownButton1.ForeColor = System.Drawing.Color.Black;
            this.ToolStripDropDownButton1.Image = global::BSH.Main.Properties.Resources.edit_square_icon_24;
            this.ToolStripDropDownButton1.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.ToolStripDropDownButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.ToolStripDropDownButton1.Margin = new System.Windows.Forms.Padding(3, 5, 3, 5);
            this.ToolStripDropDownButton1.Name = "ToolStripDropDownButton1";
            this.ToolStripDropDownButton1.Padding = new System.Windows.Forms.Padding(2);
            this.ToolStripDropDownButton1.Size = new System.Drawing.Size(179, 36);
            this.ToolStripDropDownButton1.Text = " Organisieren ";
            this.ToolStripDropDownButton1.ToolTipText = "Organisieren der ausgewählten Elemente";
            // 
            // WiederherstellenToolStripMenuItem
            // 
            this.WiederherstellenToolStripMenuItem.Image = global::BSH.Main.Properties.Resources.settings_backup_restore_icon_24;
            this.WiederherstellenToolStripMenuItem.Name = "WiederherstellenToolStripMenuItem";
            this.WiederherstellenToolStripMenuItem.Size = new System.Drawing.Size(383, 36);
            this.WiederherstellenToolStripMenuItem.Text = "Wiederherstellen";
            this.WiederherstellenToolStripMenuItem.Click += new System.EventHandler(this.WiederherstellenToolStripMenuItem_MouseClick);
            // 
            // AllesWiederherstellenToolStripMenuItem1
            // 
            this.AllesWiederherstellenToolStripMenuItem1.Name = "AllesWiederherstellenToolStripMenuItem1";
            this.AllesWiederherstellenToolStripMenuItem1.Size = new System.Drawing.Size(383, 36);
            this.AllesWiederherstellenToolStripMenuItem1.Text = "Alles wiederherstellen";
            this.AllesWiederherstellenToolStripMenuItem1.Click += new System.EventHandler(this.AllesWiederherstellenToolStripMenuItem_Click);
            // 
            // EigenschaftenToolStripMenuItem
            // 
            this.EigenschaftenToolStripMenuItem.Enabled = false;
            this.EigenschaftenToolStripMenuItem.Name = "EigenschaftenToolStripMenuItem";
            this.EigenschaftenToolStripMenuItem.Size = new System.Drawing.Size(383, 36);
            this.EigenschaftenToolStripMenuItem.Text = "Eigenschaften";
            this.EigenschaftenToolStripMenuItem.Click += new System.EventHandler(this.EigenschaftenToolStripMenuItem_Click);
            // 
            // ToolStripMenuItem1
            // 
            this.ToolStripMenuItem1.Name = "ToolStripMenuItem1";
            this.ToolStripMenuItem1.Size = new System.Drawing.Size(380, 6);
            // 
            // SchnellansichtToolStripMenuItem
            // 
            this.SchnellansichtToolStripMenuItem.Enabled = false;
            this.SchnellansichtToolStripMenuItem.Image = global::BSH.Main.Properties.Resources.visibility_icon_24;
            this.SchnellansichtToolStripMenuItem.Name = "SchnellansichtToolStripMenuItem";
            this.SchnellansichtToolStripMenuItem.ShortcutKeyDisplayString = "Leertaste";
            this.SchnellansichtToolStripMenuItem.Size = new System.Drawing.Size(383, 36);
            this.SchnellansichtToolStripMenuItem.Text = "Schnellansicht";
            this.SchnellansichtToolStripMenuItem.Click += new System.EventHandler(this.SchnellansichtToolStripMenuItem_Click);
            // 
            // ZuOrdnerfavoritenHinzufügenToolStripMenuItem
            // 
            this.ZuOrdnerfavoritenHinzufügenToolStripMenuItem.Name = "ZuOrdnerfavoritenHinzufügenToolStripMenuItem";
            this.ZuOrdnerfavoritenHinzufügenToolStripMenuItem.Size = new System.Drawing.Size(383, 36);
            this.ZuOrdnerfavoritenHinzufügenToolStripMenuItem.Text = "Zu Ordnerfavoriten hinzufügen";
            this.ZuOrdnerfavoritenHinzufügenToolStripMenuItem.Click += new System.EventHandler(this.ZuOrdnerfavoritenHinzufügenToolStripMenuItem_Click);
            // 
            // ToolStripMenuItem2
            // 
            this.ToolStripMenuItem2.Name = "ToolStripMenuItem2";
            this.ToolStripMenuItem2.Size = new System.Drawing.Size(380, 6);
            // 
            // VersionBearbeitenToolStripMenuItem
            // 
            this.VersionBearbeitenToolStripMenuItem.Name = "VersionBearbeitenToolStripMenuItem";
            this.VersionBearbeitenToolStripMenuItem.Size = new System.Drawing.Size(383, 36);
            this.VersionBearbeitenToolStripMenuItem.Text = "Sicherung bearbeiten";
            this.VersionBearbeitenToolStripMenuItem.Click += new System.EventHandler(this.VersionBearbeitenToolStripMenuItem_Click);
            // 
            // VersionLöschenToolStripMenuItem
            // 
            this.VersionLöschenToolStripMenuItem.Image = global::BSH.Main.Properties.Resources.delete_icon_24;
            this.VersionLöschenToolStripMenuItem.Name = "VersionLöschenToolStripMenuItem";
            this.VersionLöschenToolStripMenuItem.Size = new System.Drawing.Size(383, 36);
            this.VersionLöschenToolStripMenuItem.Text = "Sicherung löschen";
            this.VersionLöschenToolStripMenuItem.Click += new System.EventHandler(this.VersionLöschenToolStripMenuItem_Click);
            // 
            // VersionAlsStabilMarkierenToolStripMenuItem
            // 
            this.VersionAlsStabilMarkierenToolStripMenuItem.Name = "VersionAlsStabilMarkierenToolStripMenuItem";
            this.VersionAlsStabilMarkierenToolStripMenuItem.Size = new System.Drawing.Size(383, 36);
            this.VersionAlsStabilMarkierenToolStripMenuItem.Text = "Sicherung fixieren";
            this.VersionAlsStabilMarkierenToolStripMenuItem.ToolTipText = "Eine fixierte Version wird nie automatisch gelöscht, sondern immer behalten.";
            this.VersionAlsStabilMarkierenToolStripMenuItem.Click += new System.EventHandler(this.VersionAlsStabilMarkierenToolStripMenuItem_Click);
            // 
            // ToolStripMenuItem4
            // 
            this.ToolStripMenuItem4.Name = "ToolStripMenuItem4";
            this.ToolStripMenuItem4.Size = new System.Drawing.Size(380, 6);
            // 
            // MehrereVersionenLöschenToolStripMenuItem
            // 
            this.MehrereVersionenLöschenToolStripMenuItem.Name = "MehrereVersionenLöschenToolStripMenuItem";
            this.MehrereVersionenLöschenToolStripMenuItem.Size = new System.Drawing.Size(383, 36);
            this.MehrereVersionenLöschenToolStripMenuItem.Text = "Mehrere Sicherungen löschen";
            this.MehrereVersionenLöschenToolStripMenuItem.Click += new System.EventHandler(this.MehrereVersionenLöschenToolStripMenuItem_Click);
            // 
            // ToolStripSchnellansicht
            // 
            this.ToolStripSchnellansicht.Enabled = false;
            this.ToolStripSchnellansicht.ForeColor = System.Drawing.Color.Black;
            this.ToolStripSchnellansicht.Image = global::BSH.Main.Properties.Resources.visibility_icon_24;
            this.ToolStripSchnellansicht.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.ToolStripSchnellansicht.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.ToolStripSchnellansicht.Margin = new System.Windows.Forms.Padding(3, 5, 3, 5);
            this.ToolStripSchnellansicht.Name = "ToolStripSchnellansicht";
            this.ToolStripSchnellansicht.Padding = new System.Windows.Forms.Padding(2);
            this.ToolStripSchnellansicht.Size = new System.Drawing.Size(167, 36);
            this.ToolStripSchnellansicht.Text = "Schnellansicht";
            this.ToolStripSchnellansicht.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.ToolStripSchnellansicht.Click += new System.EventHandler(this.ToolStripSchnellansicht_Click);
            // 
            // ToolStripDropDownButton2
            // 
            this.ToolStripDropDownButton2.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.ToolStripDropDownButton2.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.GroßeSymboleToolStripMenuItem,
            this.ListenansichtToolStripMenuItem,
            this.DetailsansichtToolStripMenuItem});
            this.ToolStripDropDownButton2.ForeColor = System.Drawing.Color.Black;
            this.ToolStripDropDownButton2.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.ToolStripDropDownButton2.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.ToolStripDropDownButton2.Margin = new System.Windows.Forms.Padding(3, 5, 3, 5);
            this.ToolStripDropDownButton2.Name = "ToolStripDropDownButton2";
            this.ToolStripDropDownButton2.Padding = new System.Windows.Forms.Padding(2);
            this.ToolStripDropDownButton2.Size = new System.Drawing.Size(129, 36);
            this.ToolStripDropDownButton2.Text = " Ansichten ";
            this.ToolStripDropDownButton2.ToolTipText = "Ansichten wechseln";
            // 
            // GroßeSymboleToolStripMenuItem
            // 
            this.GroßeSymboleToolStripMenuItem.Image = global::BSH.Main.Properties.Resources.grid_view_icon_24;
            this.GroßeSymboleToolStripMenuItem.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.GroßeSymboleToolStripMenuItem.Name = "GroßeSymboleToolStripMenuItem";
            this.GroßeSymboleToolStripMenuItem.Size = new System.Drawing.Size(249, 36);
            this.GroßeSymboleToolStripMenuItem.Text = "Große Symbole";
            this.GroßeSymboleToolStripMenuItem.Click += new System.EventHandler(this.GroßeSymboleToolStripMenuItem_Click);
            // 
            // ListenansichtToolStripMenuItem
            // 
            this.ListenansichtToolStripMenuItem.Image = global::BSH.Main.Properties.Resources.list_icon_24;
            this.ListenansichtToolStripMenuItem.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.ListenansichtToolStripMenuItem.Name = "ListenansichtToolStripMenuItem";
            this.ListenansichtToolStripMenuItem.Size = new System.Drawing.Size(249, 36);
            this.ListenansichtToolStripMenuItem.Text = "Listenansicht";
            this.ListenansichtToolStripMenuItem.Click += new System.EventHandler(this.ListenansichtToolStripMenuItem_Click);
            // 
            // DetailsansichtToolStripMenuItem
            // 
            this.DetailsansichtToolStripMenuItem.Image = global::BSH.Main.Properties.Resources.view_list_icon_24;
            this.DetailsansichtToolStripMenuItem.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.DetailsansichtToolStripMenuItem.Name = "DetailsansichtToolStripMenuItem";
            this.DetailsansichtToolStripMenuItem.Size = new System.Drawing.Size(249, 36);
            this.DetailsansichtToolStripMenuItem.Text = "Detailsansicht";
            this.DetailsansichtToolStripMenuItem.Click += new System.EventHandler(this.DetailansichtToolStripMenuItem_Click);
            // 
            // cmdRestore
            // 
            this.cmdRestore.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.cmdRestore.ForeColor = System.Drawing.Color.Black;
            this.cmdRestore.Image = global::BSH.Main.Properties.Resources.settings_backup_restore_icon_24;
            this.cmdRestore.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.cmdRestore.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.cmdRestore.Margin = new System.Windows.Forms.Padding(3, 5, 3, 5);
            this.cmdRestore.Name = "cmdRestore";
            this.cmdRestore.Padding = new System.Windows.Forms.Padding(2);
            this.cmdRestore.Size = new System.Drawing.Size(191, 36);
            this.cmdRestore.Text = "Wiederherstellen";
            this.cmdRestore.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.cmdRestore.ToolTipText = "Eine Datei oder Ordner wiederherstellen.\r\nHalten Sie Strg-Taste gedrückt, um zu b" +
    "estimmen wohin die Datei\r\nwiederhergestellt werden soll.";
            this.cmdRestore.MouseUp += new System.Windows.Forms.MouseEventHandler(this.cmdRestore_MouseClick);
            // 
            // plGlass
            // 
            this.plGlass.BackColor = System.Drawing.Color.White;
            this.plGlass.Controls.Add(this.UcNav);
            this.plGlass.Controls.Add(this.Panel2);
            this.plGlass.Controls.Add(this.btnBack);
            this.plGlass.Dock = System.Windows.Forms.DockStyle.Top;
            this.plGlass.Location = new System.Drawing.Point(0, 50);
            this.plGlass.Margin = new System.Windows.Forms.Padding(4);
            this.plGlass.Name = "plGlass";
            this.plGlass.Size = new System.Drawing.Size(1188, 63);
            this.plGlass.TabIndex = 9;
            // 
            // UcNav
            // 
            this.UcNav.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.UcNav.BackColor = System.Drawing.Color.White;
            this.UcNav.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.UcNav.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.UcNav.ForeColor = System.Drawing.Color.White;
            this.UcNav.Location = new System.Drawing.Point(66, 12);
            this.UcNav.Margin = new System.Windows.Forms.Padding(6);
            this.UcNav.Name = "UcNav";
            this.UcNav.Path = "";
            this.UcNav.PathLocalized = null;
            this.UcNav.Size = new System.Drawing.Size(749, 38);
            this.UcNav.TabIndex = 1;
            this.UcNav.TabStop = false;
            this.UcNav.ItemClick += new Brightbits.BSH.Main.ucNavigation.ItemClickEventHandler(this.UcNav_ItemClick);
            // 
            // Panel2
            // 
            this.Panel2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.Panel2.BackColor = System.Drawing.Color.White;
            this.Panel2.Controls.Add(this.panel4);
            this.Panel2.Location = new System.Drawing.Point(825, 0);
            this.Panel2.Margin = new System.Windows.Forms.Padding(4);
            this.Panel2.Name = "Panel2";
            this.Panel2.Size = new System.Drawing.Size(362, 62);
            this.Panel2.TabIndex = 9;
            // 
            // panel4
            // 
            this.panel4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.panel4.BackColor = System.Drawing.Color.White;
            this.panel4.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel4.Controls.Add(this.PictureBox3);
            this.panel4.Controls.Add(this.txtSearch);
            this.panel4.Location = new System.Drawing.Point(4, 12);
            this.panel4.Margin = new System.Windows.Forms.Padding(4);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(346, 38);
            this.panel4.TabIndex = 10;
            // 
            // PictureBox3
            // 
            this.PictureBox3.Image = global::BSH.Main.Properties.Resources.search_icon_48;
            this.PictureBox3.Location = new System.Drawing.Point(2, 2);
            this.PictureBox3.Margin = new System.Windows.Forms.Padding(4);
            this.PictureBox3.Name = "PictureBox3";
            this.PictureBox3.Size = new System.Drawing.Size(32, 32);
            this.PictureBox3.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.PictureBox3.TabIndex = 7;
            this.PictureBox3.TabStop = false;
            // 
            // txtSearch
            // 
            this.txtSearch.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtSearch.BackColor = System.Drawing.Color.White;
            this.txtSearch.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtSearch.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtSearch.ForeColor = System.Drawing.Color.Black;
            this.txtSearch.Location = new System.Drawing.Point(45, 3);
            this.txtSearch.Margin = new System.Windows.Forms.Padding(4);
            this.txtSearch.Name = "txtSearch";
            this.txtSearch.Size = new System.Drawing.Size(295, 26);
            this.txtSearch.TabIndex = 4;
            this.txtSearch.Tag = "search";
            this.txtSearch.Text = "Suche";
            this.txtSearch.TextChanged += new System.EventHandler(this.txtSearch_TextChanged);
            this.txtSearch.Enter += new System.EventHandler(this.txtSearch_Enter);
            this.txtSearch.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtSearch_KeyDown);
            this.txtSearch.Leave += new System.EventHandler(this.txtSearch_Leave);
            // 
            // btnBack
            // 
            this.btnBack.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnBack.Image = global::BSH.Main.Properties.Resources.arrow_upward_icon_48;
            this.btnBack.Location = new System.Drawing.Point(19, 12);
            this.btnBack.Margin = new System.Windows.Forms.Padding(4);
            this.btnBack.Name = "btnBack";
            this.btnBack.Size = new System.Drawing.Size(37, 37);
            this.btnBack.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.btnBack.TabIndex = 8;
            this.btnBack.TabStop = false;
            this.btnBack.Click += new System.EventHandler(this.btnBack_Click);
            // 
            // Panel1
            // 
            this.Panel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(243)))), ((int)(((byte)(243)))), ((int)(((byte)(245)))));
            this.Panel1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.Panel1.Controls.Add(this.flpDetails);
            this.Panel1.Controls.Add(this.Label4);
            this.Panel1.Controls.Add(this.imgFileType);
            this.Panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.Panel1.Location = new System.Drawing.Point(0, 724);
            this.Panel1.Margin = new System.Windows.Forms.Padding(4);
            this.Panel1.Name = "Panel1";
            this.Panel1.Size = new System.Drawing.Size(1188, 93);
            this.Panel1.TabIndex = 2;
            // 
            // flpDetails
            // 
            this.flpDetails.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.flpDetails.BackColor = System.Drawing.Color.Transparent;
            this.flpDetails.Controls.Add(this.flpColumn1);
            this.flpDetails.Controls.Add(this.flpColumn2);
            this.flpDetails.Controls.Add(this.flpColumn3);
            this.flpDetails.Controls.Add(this.lblIntegrityCheck);
            this.flpDetails.Location = new System.Drawing.Point(78, 12);
            this.flpDetails.Margin = new System.Windows.Forms.Padding(0);
            this.flpDetails.Name = "flpDetails";
            this.flpDetails.Size = new System.Drawing.Size(1106, 88);
            this.flpDetails.TabIndex = 49;
            // 
            // flpColumn1
            // 
            this.flpColumn1.AutoSize = true;
            this.flpColumn1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flpColumn1.Controls.Add(this.lblFileName);
            this.flpColumn1.Controls.Add(this.lblFileType);
            this.flpColumn1.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flpColumn1.Location = new System.Drawing.Point(0, 8);
            this.flpColumn1.Margin = new System.Windows.Forms.Padding(0, 8, 0, 0);
            this.flpColumn1.Name = "flpColumn1";
            this.flpColumn1.Size = new System.Drawing.Size(32, 53);
            this.flpColumn1.TabIndex = 47;
            // 
            // lblFileName
            // 
            this.lblFileName.AutoSize = true;
            this.lblFileName.BackColor = System.Drawing.Color.Transparent;
            this.lblFileName.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblFileName.Location = new System.Drawing.Point(4, 0);
            this.lblFileName.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblFileName.Name = "lblFileName";
            this.lblFileName.Size = new System.Drawing.Size(23, 25);
            this.lblFileName.TabIndex = 1;
            this.lblFileName.Text = "#";
            // 
            // lblFileType
            // 
            this.lblFileType.AutoSize = true;
            this.lblFileType.BackColor = System.Drawing.Color.Transparent;
            this.lblFileType.ForeColor = System.Drawing.Color.Black;
            this.lblFileType.Location = new System.Drawing.Point(4, 25);
            this.lblFileType.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblFileType.Name = "lblFileType";
            this.lblFileType.Size = new System.Drawing.Size(24, 28);
            this.lblFileType.TabIndex = 2;
            this.lblFileType.Text = "#";
            // 
            // flpColumn2
            // 
            this.flpColumn2.AutoSize = true;
            this.flpColumn2.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flpColumn2.Controls.Add(this.flpDateLastWrite);
            this.flpColumn2.Controls.Add(this.flpDateCreated);
            this.flpColumn2.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flpColumn2.Location = new System.Drawing.Point(32, 0);
            this.flpColumn2.Margin = new System.Windows.Forms.Padding(0);
            this.flpColumn2.Name = "flpColumn2";
            this.flpColumn2.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.flpColumn2.Size = new System.Drawing.Size(230, 64);
            this.flpColumn2.TabIndex = 46;
            // 
            // flpDateLastWrite
            // 
            this.flpDateLastWrite.AutoSize = true;
            this.flpDateLastWrite.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flpDateLastWrite.BackColor = System.Drawing.Color.Transparent;
            this.flpDateLastWrite.Controls.Add(this.lFileLastEdited);
            this.flpDateLastWrite.Controls.Add(this.lblFileLastEdited);
            this.flpDateLastWrite.Location = new System.Drawing.Point(4, 4);
            this.flpDateLastWrite.Margin = new System.Windows.Forms.Padding(4);
            this.flpDateLastWrite.Name = "flpDateLastWrite";
            this.flpDateLastWrite.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.flpDateLastWrite.Size = new System.Drawing.Size(222, 28);
            this.flpDateLastWrite.TabIndex = 43;
            // 
            // lFileLastEdited
            // 
            this.lFileLastEdited.AutoSize = true;
            this.lFileLastEdited.BackColor = System.Drawing.Color.Transparent;
            this.lFileLastEdited.ForeColor = System.Drawing.Color.Gray;
            this.lFileLastEdited.Location = new System.Drawing.Point(4, 0);
            this.lFileLastEdited.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lFileLastEdited.Name = "lFileLastEdited";
            this.lFileLastEdited.Size = new System.Drawing.Size(160, 28);
            this.lFileLastEdited.TabIndex = 0;
            this.lFileLastEdited.Text = "Zuletzt geändert:";
            // 
            // lblFileLastEdited
            // 
            this.lblFileLastEdited.AutoSize = true;
            this.lblFileLastEdited.BackColor = System.Drawing.Color.Transparent;
            this.lblFileLastEdited.ForeColor = System.Drawing.Color.Black;
            this.lblFileLastEdited.Location = new System.Drawing.Point(172, 0);
            this.lblFileLastEdited.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblFileLastEdited.Name = "lblFileLastEdited";
            this.lblFileLastEdited.Padding = new System.Windows.Forms.Padding(0, 0, 22, 0);
            this.lblFileLastEdited.Size = new System.Drawing.Size(46, 28);
            this.lblFileLastEdited.TabIndex = 1;
            this.lblFileLastEdited.Text = "#";
            // 
            // flpDateCreated
            // 
            this.flpDateCreated.AutoSize = true;
            this.flpDateCreated.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flpDateCreated.BackColor = System.Drawing.Color.Transparent;
            this.flpDateCreated.Controls.Add(this.lFileCreated);
            this.flpDateCreated.Controls.Add(this.lblFileCreated);
            this.flpDateCreated.Location = new System.Drawing.Point(57, 36);
            this.flpDateCreated.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.flpDateCreated.Name = "flpDateCreated";
            this.flpDateCreated.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.flpDateCreated.Size = new System.Drawing.Size(169, 28);
            this.flpDateCreated.TabIndex = 42;
            // 
            // lFileCreated
            // 
            this.lFileCreated.AutoSize = true;
            this.lFileCreated.BackColor = System.Drawing.Color.Transparent;
            this.lFileCreated.ForeColor = System.Drawing.Color.Gray;
            this.lFileCreated.Location = new System.Drawing.Point(4, 0);
            this.lFileCreated.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lFileCreated.Name = "lFileCreated";
            this.lFileCreated.Size = new System.Drawing.Size(107, 28);
            this.lFileCreated.TabIndex = 4;
            this.lFileCreated.Text = "Erstellt am:";
            // 
            // lblFileCreated
            // 
            this.lblFileCreated.AutoSize = true;
            this.lblFileCreated.BackColor = System.Drawing.Color.Transparent;
            this.lblFileCreated.ForeColor = System.Drawing.Color.Black;
            this.lblFileCreated.Location = new System.Drawing.Point(119, 0);
            this.lblFileCreated.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblFileCreated.Name = "lblFileCreated";
            this.lblFileCreated.Padding = new System.Windows.Forms.Padding(0, 0, 22, 0);
            this.lblFileCreated.Size = new System.Drawing.Size(46, 28);
            this.lblFileCreated.TabIndex = 5;
            this.lblFileCreated.Text = "#";
            // 
            // flpColumn3
            // 
            this.flpColumn3.AutoSize = true;
            this.flpColumn3.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flpColumn3.Controls.Add(this.flpSize);
            this.flpColumn3.Controls.Add(this.flpVersion);
            this.flpColumn3.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flpColumn3.Location = new System.Drawing.Point(262, 0);
            this.flpColumn3.Margin = new System.Windows.Forms.Padding(0);
            this.flpColumn3.Name = "flpColumn3";
            this.flpColumn3.Size = new System.Drawing.Size(151, 64);
            this.flpColumn3.TabIndex = 48;
            // 
            // flpSize
            // 
            this.flpSize.AutoSize = true;
            this.flpSize.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flpSize.BackColor = System.Drawing.Color.Transparent;
            this.flpSize.Controls.Add(this.lFileSize);
            this.flpSize.Controls.Add(this.lblFileSize);
            this.flpSize.Location = new System.Drawing.Point(4, 4);
            this.flpSize.Margin = new System.Windows.Forms.Padding(4);
            this.flpSize.Name = "flpSize";
            this.flpSize.Size = new System.Drawing.Size(143, 28);
            this.flpSize.TabIndex = 45;
            // 
            // lFileSize
            // 
            this.lFileSize.AutoSize = true;
            this.lFileSize.BackColor = System.Drawing.Color.Transparent;
            this.lFileSize.ForeColor = System.Drawing.Color.Gray;
            this.lFileSize.Location = new System.Drawing.Point(15, 0);
            this.lFileSize.Margin = new System.Windows.Forms.Padding(15, 0, 4, 0);
            this.lFileSize.Name = "lFileSize";
            this.lFileSize.Size = new System.Drawing.Size(70, 28);
            this.lFileSize.TabIndex = 2;
            this.lFileSize.Text = "Größe:";
            // 
            // lblFileSize
            // 
            this.lblFileSize.AutoSize = true;
            this.lblFileSize.BackColor = System.Drawing.Color.Transparent;
            this.lblFileSize.ForeColor = System.Drawing.Color.Black;
            this.lblFileSize.Location = new System.Drawing.Point(93, 0);
            this.lblFileSize.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblFileSize.Name = "lblFileSize";
            this.lblFileSize.Padding = new System.Windows.Forms.Padding(0, 0, 22, 0);
            this.lblFileSize.Size = new System.Drawing.Size(46, 28);
            this.lblFileSize.TabIndex = 3;
            this.lblFileSize.Text = "#";
            // 
            // flpVersion
            // 
            this.flpVersion.AutoSize = true;
            this.flpVersion.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flpVersion.BackColor = System.Drawing.Color.Transparent;
            this.flpVersion.Controls.Add(this.lFileVersion);
            this.flpVersion.Controls.Add(this.lblFileVersion);
            this.flpVersion.Location = new System.Drawing.Point(4, 36);
            this.flpVersion.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.flpVersion.Name = "flpVersion";
            this.flpVersion.Size = new System.Drawing.Size(142, 28);
            this.flpVersion.TabIndex = 44;
            // 
            // lFileVersion
            // 
            this.lFileVersion.AutoSize = true;
            this.lFileVersion.BackColor = System.Drawing.Color.Transparent;
            this.lFileVersion.ForeColor = System.Drawing.Color.Gray;
            this.lFileVersion.Location = new System.Drawing.Point(4, 0);
            this.lFileVersion.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lFileVersion.Name = "lFileVersion";
            this.lFileVersion.Size = new System.Drawing.Size(80, 28);
            this.lFileVersion.TabIndex = 6;
            this.lFileVersion.Text = "Version:";
            // 
            // lblFileVersion
            // 
            this.lblFileVersion.AutoSize = true;
            this.lblFileVersion.BackColor = System.Drawing.Color.Transparent;
            this.lblFileVersion.ForeColor = System.Drawing.Color.Black;
            this.lblFileVersion.Location = new System.Drawing.Point(92, 0);
            this.lblFileVersion.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblFileVersion.Name = "lblFileVersion";
            this.lblFileVersion.Padding = new System.Windows.Forms.Padding(0, 0, 22, 0);
            this.lblFileVersion.Size = new System.Drawing.Size(46, 28);
            this.lblFileVersion.TabIndex = 7;
            this.lblFileVersion.Text = "#";
            // 
            // lblIntegrityCheck
            // 
            this.lblIntegrityCheck.AutoSize = true;
            this.lblIntegrityCheck.ForeColor = System.Drawing.Color.Red;
            this.lblIntegrityCheck.Location = new System.Drawing.Point(417, 0);
            this.lblIntegrityCheck.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblIntegrityCheck.Name = "lblIntegrityCheck";
            this.lblIntegrityCheck.Size = new System.Drawing.Size(0, 28);
            this.lblIntegrityCheck.TabIndex = 49;
            // 
            // Label4
            // 
            this.Label4.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.Label4.Dock = System.Windows.Forms.DockStyle.Top;
            this.Label4.Location = new System.Drawing.Point(0, 0);
            this.Label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.Label4.Name = "Label4";
            this.Label4.Size = new System.Drawing.Size(1188, 2);
            this.Label4.TabIndex = 0;
            // 
            // imgFileType
            // 
            this.imgFileType.BackColor = System.Drawing.Color.Transparent;
            this.imgFileType.Location = new System.Drawing.Point(22, 22);
            this.imgFileType.Margin = new System.Windows.Forms.Padding(4);
            this.imgFileType.Name = "imgFileType";
            this.imgFileType.Size = new System.Drawing.Size(48, 48);
            this.imgFileType.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.imgFileType.TabIndex = 41;
            this.imgFileType.TabStop = false;
            // 
            // AllesWiederherstellenToolStripMenuItem
            // 
            this.AllesWiederherstellenToolStripMenuItem.Name = "AllesWiederherstellenToolStripMenuItem";
            this.AllesWiederherstellenToolStripMenuItem.Size = new System.Drawing.Size(366, 32);
            this.AllesWiederherstellenToolStripMenuItem.Text = "Alles Wiederherstellen";
            // 
            // frmBrowser
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(144F, 144F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(1188, 849);
            this.Controls.Add(this.SplitContainer1);
            this.Controls.Add(this.plGlass);
            this.Controls.Add(this.Panel1);
            this.Controls.Add(this.tMain);
            this.Controls.Add(this.StatusStrip1);
            this.DoubleBuffered = true;
            this.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MinimumSize = new System.Drawing.Size(1183, 854);
            this.Name = "frmBrowser";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Backupbrowser";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmBrowser_FormClosing);
            this.Load += new System.EventHandler(this.frmBrowser_Load);
            this.SplitContainer1.Panel1.ResumeLayout(false);
            this.SplitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.SplitContainer1)).EndInit();
            this.SplitContainer1.ResumeLayout(false);
            this.plVersions.ResumeLayout(false);
            this.plVersions.PerformLayout();
            this.plFavorites.ResumeLayout(false);
            this.plFavorites.PerformLayout();
            this.cmnuFavorits.ResumeLayout(false);
            this.cmnuListRight.ResumeLayout(false);
            this.Panel3.ResumeLayout(false);
            this.Panel3.PerformLayout();
            this.StatusStrip1.ResumeLayout(false);
            this.StatusStrip1.PerformLayout();
            this.tMain.ResumeLayout(false);
            this.tMain.PerformLayout();
            this.plGlass.ResumeLayout(false);
            this.Panel2.ResumeLayout(false);
            this.panel4.ResumeLayout(false);
            this.panel4.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.PictureBox3)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnBack)).EndInit();
            this.Panel1.ResumeLayout(false);
            this.Panel1.PerformLayout();
            this.flpDetails.ResumeLayout(false);
            this.flpDetails.PerformLayout();
            this.flpColumn1.ResumeLayout(false);
            this.flpColumn1.PerformLayout();
            this.flpColumn2.ResumeLayout(false);
            this.flpColumn2.PerformLayout();
            this.flpDateLastWrite.ResumeLayout(false);
            this.flpDateLastWrite.PerformLayout();
            this.flpDateCreated.ResumeLayout(false);
            this.flpDateCreated.PerformLayout();
            this.flpColumn3.ResumeLayout(false);
            this.flpColumn3.PerformLayout();
            this.flpSize.ResumeLayout(false);
            this.flpSize.PerformLayout();
            this.flpVersion.ResumeLayout(false);
            this.flpVersion.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.imgFileType)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        internal SplitContainer SplitContainer1;
        internal Panel plFavorites;
        internal Label lblExpand1;
        internal ListView lvFiles;
        internal Panel Panel1;
        internal ColumnHeader ColumnHeader1;
        internal ColumnHeader ColumnHeader2;
        internal ColumnHeader ColumnHeader3;
        internal ColumnHeader ColumnHeader4;
        internal ImageList ilFolder;
        internal ImageList ilBigIcons;
        internal ImageList ilSmallIcons;
        internal ColumnHeader ColumnHeader5;
        internal Label lblFileName;
        internal Label lblFileCreated;
        internal PictureBox imgFileType;
        internal Label lblFileVersion;
        internal Label lblFileLastEdited;
        internal Label lblFileType;
        internal Label lblFileSize;
        internal ColumnHeader ColumnHeader6;
        internal ImageList ilBigFolder;
        internal ListView lvFavorite;
        internal aVersionList AVersionList1;
        internal StatusStrip StatusStrip1;
        internal ToolStripStatusLabel tsslblStatus;
        internal Label lFileSize;
        internal Label lFileLastEdited;
        internal Label lFileVersion;
        internal Label lFileCreated;
        internal Panel plVersions;
        internal Label lblExpand2;
        internal ToolStrip tMain;
        internal ToolStripButton ToolStripSchnellansicht;
        internal ToolStripDropDownButton ToolStripDropDownButton1;
        internal ToolStripMenuItem WiederherstellenToolStripMenuItem;
        internal ToolStripMenuItem EigenschaftenToolStripMenuItem;
        internal ToolStripSeparator ToolStripMenuItem1;
        internal ToolStripMenuItem SchnellansichtToolStripMenuItem;
        internal ToolStripSeparator ToolStripMenuItem2;
        internal ToolStripMenuItem VersionBearbeitenToolStripMenuItem;
        internal ToolStripMenuItem VersionLöschenToolStripMenuItem;
        internal ToolStripDropDownButton ToolStripDropDownButton2;
        internal ToolStripMenuItem GroßeSymboleToolStripMenuItem;
        internal ToolStripMenuItem ListenansichtToolStripMenuItem;
        internal ToolStripMenuItem DetailsansichtToolStripMenuItem;
        internal ToolTip ttMain;
        internal ToolStripMenuItem ZuOrdnerfavoritenHinzufügenToolStripMenuItem;
        internal ContextMenuStrip cmnuFavorits;
        internal ToolStripMenuItem LöschenToolStripMenuItem;
        internal System.ComponentModel.BackgroundWorker bgrWorkSearch;
        internal Panel plGlass;
        internal ToolStripMenuItem VersionAlsStabilMarkierenToolStripMenuItem;
        internal Button cmdTakeMeLater;
        internal Button cmdTakeMeBack;
        internal Label Label4;
        internal FlowLayoutPanel flpVersion;
        internal FlowLayoutPanel flpDateLastWrite;
        internal FlowLayoutPanel flpDateCreated;
        internal FlowLayoutPanel flpColumn1;
        internal FlowLayoutPanel flpColumn2;
        internal FlowLayoutPanel flpSize;
        internal FlowLayoutPanel flpDetails;
        internal FlowLayoutPanel flpColumn3;
        internal ToolStripButton cmdRestore;
        internal Label lblIntegrityCheck;
        internal TextBox txtSearch;
        internal Panel Panel3;
        internal Label lblBackupdate;
        internal CheckBox chkFilesOfThisVersion;
        internal ToolStripMenuItem UmbenennenToolStripMenuItem;
        internal ContextMenuStrip cmnuListRight;
        internal ToolStripMenuItem WiederherstellenToolStripMenuItem1;
        internal ToolStripMenuItem SchnellansichtToolStripMenuItem1;
        internal ToolStripMenuItem EigenschaftenToolStripMenuItem1;
        internal ToolStripSeparator ToolStripMenuItem3;
        internal ToolStripSeparator ToolStripMenuItem4;
        internal ToolStripMenuItem MehrereVersionenLöschenToolStripMenuItem;
        internal ColumnHeader colName;
        internal PictureBox PictureBox3;
        internal PictureBox btnBack;
        internal Panel Panel2;
        internal ToolStripSeparator ToolStripMenuItem5;
        internal ToolStripMenuItem DateiOrdnerAusSicherungenLöschenToolStripMenuItem;
        internal ToolStripMenuItem AllesWiederherstellenToolStripMenuItem;
        internal ToolStripMenuItem AllesWiederherstellenToolStripMenuItem1;
        internal ucNavigation UcNav;
        internal Panel panel4;
        private MRG.Controls.UI.LoadingCircleToolStripMenuItem LCFiles;
    }
}