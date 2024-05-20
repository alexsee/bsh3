using System.Diagnostics;
using System.Windows.Forms;
using BSH.Main.Properties;

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
            components = new System.ComponentModel.Container();
            var resources = new System.ComponentModel.ComponentResourceManager(typeof(frmBrowser));
            LCFiles = new MRG.Controls.UI.LoadingCircleToolStripMenuItem();
            SplitContainer1 = new SplitContainer();
            plVersions = new Panel();
            cmdTakeMeLater = new Button();
            cmdTakeMeBack = new Button();
            AVersionList1 = new aVersionList();
            lblExpand2 = new Label();
            plFavorites = new Panel();
            lvFavorite = new ListView();
            colName = new ColumnHeader();
            cmnuFavorits = new ContextMenuStrip(components);
            LöschenToolStripMenuItem = new ToolStripMenuItem();
            UmbenennenToolStripMenuItem = new ToolStripMenuItem();
            ilFolder = new ImageList(components);
            lblExpand1 = new Label();
            lvFiles = new ListView();
            ColumnHeader1 = new ColumnHeader();
            ColumnHeader2 = new ColumnHeader();
            ColumnHeader3 = new ColumnHeader();
            ColumnHeader4 = new ColumnHeader();
            ColumnHeader6 = new ColumnHeader();
            ColumnHeader5 = new ColumnHeader();
            cmnuListRight = new ContextMenuStrip(components);
            WiederherstellenToolStripMenuItem1 = new ToolStripMenuItem();
            EigenschaftenToolStripMenuItem1 = new ToolStripMenuItem();
            ToolStripMenuItem5 = new ToolStripSeparator();
            DateiOrdnerAusSicherungenLöschenToolStripMenuItem = new ToolStripMenuItem();
            ToolStripMenuItem3 = new ToolStripSeparator();
            SchnellansichtToolStripMenuItem1 = new ToolStripMenuItem();
            ilBigIcons = new ImageList(components);
            ilSmallIcons = new ImageList(components);
            Panel3 = new Panel();
            chkFilesOfThisVersion = new CheckBox();
            lblBackupdate = new Label();
            ilBigFolder = new ImageList(components);
            StatusStrip1 = new StatusStrip();
            tsslblStatus = new ToolStripStatusLabel();
            ttMain = new ToolTip(components);
            tMain = new ToolStrip();
            ToolStripDropDownButton1 = new ToolStripDropDownButton();
            WiederherstellenToolStripMenuItem = new ToolStripMenuItem();
            AllesWiederherstellenToolStripMenuItem1 = new ToolStripMenuItem();
            EigenschaftenToolStripMenuItem = new ToolStripMenuItem();
            ToolStripMenuItem1 = new ToolStripSeparator();
            SchnellansichtToolStripMenuItem = new ToolStripMenuItem();
            ZuOrdnerfavoritenHinzufügenToolStripMenuItem = new ToolStripMenuItem();
            ToolStripMenuItem2 = new ToolStripSeparator();
            VersionBearbeitenToolStripMenuItem = new ToolStripMenuItem();
            VersionLöschenToolStripMenuItem = new ToolStripMenuItem();
            VersionAlsStabilMarkierenToolStripMenuItem = new ToolStripMenuItem();
            ToolStripMenuItem4 = new ToolStripSeparator();
            MehrereVersionenLöschenToolStripMenuItem = new ToolStripMenuItem();
            ToolStripSchnellansicht = new ToolStripButton();
            ToolStripDropDownButton2 = new ToolStripDropDownButton();
            GroßeSymboleToolStripMenuItem = new ToolStripMenuItem();
            ListenansichtToolStripMenuItem = new ToolStripMenuItem();
            DetailsansichtToolStripMenuItem = new ToolStripMenuItem();
            cmdRestore = new ToolStripButton();
            plGlass = new Panel();
            UcNav = new ucNavigation();
            Panel2 = new Panel();
            panel4 = new Panel();
            PictureBox3 = new PictureBox();
            txtSearch = new TextBox();
            btnBack = new PictureBox();
            Panel1 = new Panel();
            flpDetails = new FlowLayoutPanel();
            flpColumn1 = new FlowLayoutPanel();
            lblFileName = new Label();
            lblFileType = new Label();
            flpColumn2 = new FlowLayoutPanel();
            flpDateLastWrite = new FlowLayoutPanel();
            lFileLastEdited = new Label();
            lblFileLastEdited = new Label();
            flpDateCreated = new FlowLayoutPanel();
            lFileCreated = new Label();
            lblFileCreated = new Label();
            flpColumn3 = new FlowLayoutPanel();
            flpSize = new FlowLayoutPanel();
            lFileSize = new Label();
            lblFileSize = new Label();
            flpVersion = new FlowLayoutPanel();
            lFileVersion = new Label();
            lblFileVersion = new Label();
            lblIntegrityCheck = new Label();
            Label4 = new Label();
            imgFileType = new PictureBox();
            bgrWorkSearch = new System.ComponentModel.BackgroundWorker();
            AllesWiederherstellenToolStripMenuItem = new ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)SplitContainer1).BeginInit();
            SplitContainer1.Panel1.SuspendLayout();
            SplitContainer1.Panel2.SuspendLayout();
            SplitContainer1.SuspendLayout();
            plVersions.SuspendLayout();
            plFavorites.SuspendLayout();
            cmnuFavorits.SuspendLayout();
            cmnuListRight.SuspendLayout();
            Panel3.SuspendLayout();
            StatusStrip1.SuspendLayout();
            tMain.SuspendLayout();
            plGlass.SuspendLayout();
            Panel2.SuspendLayout();
            panel4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)PictureBox3).BeginInit();
            ((System.ComponentModel.ISupportInitialize)btnBack).BeginInit();
            Panel1.SuspendLayout();
            flpDetails.SuspendLayout();
            flpColumn1.SuspendLayout();
            flpColumn2.SuspendLayout();
            flpDateLastWrite.SuspendLayout();
            flpDateCreated.SuspendLayout();
            flpColumn3.SuspendLayout();
            flpSize.SuspendLayout();
            flpVersion.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)imgFileType).BeginInit();
            SuspendLayout();
            // 
            // LCFiles
            // 
            resources.ApplyResources(LCFiles, "LCFiles");
            LCFiles.Alignment = ToolStripItemAlignment.Right;
            // 
            // LCFiles
            // 
            resources.ApplyResources(LCFiles.LoadingCircleControl, "LCFiles");
            LCFiles.LoadingCircleControl.Active = false;
            LCFiles.LoadingCircleControl.Color = System.Drawing.Color.DarkGray;
            LCFiles.LoadingCircleControl.InnerCircleRadius = 8;
            LCFiles.LoadingCircleControl.Name = "LCFiles";
            LCFiles.LoadingCircleControl.NumberSpoke = 24;
            LCFiles.LoadingCircleControl.OuterCircleRadius = 9;
            LCFiles.LoadingCircleControl.RotationSpeed = 100;
            LCFiles.LoadingCircleControl.SpokeThickness = 4;
            LCFiles.LoadingCircleControl.StylePreset = MRG.Controls.UI.LoadingCircle.StylePresets.IE7;
            ttMain.SetToolTip(LCFiles.LoadingCircleControl, resources.GetString("LCFiles.ToolTip"));
            LCFiles.Name = "LCFiles";
            LCFiles.Overflow = ToolStripItemOverflow.Never;
            // 
            // SplitContainer1
            // 
            resources.ApplyResources(SplitContainer1, "SplitContainer1");
            SplitContainer1.BackColor = System.Drawing.Color.FromArgb(209, 209, 211);
            SplitContainer1.FixedPanel = FixedPanel.Panel1;
            SplitContainer1.Name = "SplitContainer1";
            // 
            // SplitContainer1.Panel1
            // 
            resources.ApplyResources(SplitContainer1.Panel1, "SplitContainer1.Panel1");
            SplitContainer1.Panel1.BackColor = System.Drawing.Color.White;
            SplitContainer1.Panel1.Controls.Add(plVersions);
            SplitContainer1.Panel1.Controls.Add(plFavorites);
            ttMain.SetToolTip(SplitContainer1.Panel1, resources.GetString("SplitContainer1.Panel1.ToolTip"));
            // 
            // SplitContainer1.Panel2
            // 
            resources.ApplyResources(SplitContainer1.Panel2, "SplitContainer1.Panel2");
            SplitContainer1.Panel2.Controls.Add(lvFiles);
            SplitContainer1.Panel2.Controls.Add(Panel3);
            ttMain.SetToolTip(SplitContainer1.Panel2, resources.GetString("SplitContainer1.Panel2.ToolTip"));
            ttMain.SetToolTip(SplitContainer1, resources.GetString("SplitContainer1.ToolTip"));
            // 
            // plVersions
            // 
            resources.ApplyResources(plVersions, "plVersions");
            plVersions.BackColor = System.Drawing.Color.White;
            plVersions.Controls.Add(cmdTakeMeLater);
            plVersions.Controls.Add(cmdTakeMeBack);
            plVersions.Controls.Add(AVersionList1);
            plVersions.Controls.Add(lblExpand2);
            plVersions.Name = "plVersions";
            ttMain.SetToolTip(plVersions, resources.GetString("plVersions.ToolTip"));
            // 
            // cmdTakeMeLater
            // 
            resources.ApplyResources(cmdTakeMeLater, "cmdTakeMeLater");
            cmdTakeMeLater.Cursor = Cursors.Hand;
            cmdTakeMeLater.Name = "cmdTakeMeLater";
            cmdTakeMeLater.TabStop = false;
            ttMain.SetToolTip(cmdTakeMeLater, resources.GetString("cmdTakeMeLater.ToolTip"));
            cmdTakeMeLater.UseVisualStyleBackColor = true;
            cmdTakeMeLater.Click += cmdTakeMeLater_Click;
            // 
            // cmdTakeMeBack
            // 
            resources.ApplyResources(cmdTakeMeBack, "cmdTakeMeBack");
            cmdTakeMeBack.Cursor = Cursors.Hand;
            cmdTakeMeBack.Name = "cmdTakeMeBack";
            cmdTakeMeBack.TabStop = false;
            ttMain.SetToolTip(cmdTakeMeBack, resources.GetString("cmdTakeMeBack.ToolTip"));
            cmdTakeMeBack.UseVisualStyleBackColor = true;
            cmdTakeMeBack.Click += cmdTakeMeBack_Click;
            // 
            // AVersionList1
            // 
            resources.ApplyResources(AVersionList1, "AVersionList1");
            AVersionList1.BackColor = System.Drawing.Color.White;
            AVersionList1.Name = "AVersionList1";
            AVersionList1.TabStop = false;
            ttMain.SetToolTip(AVersionList1, resources.GetString("AVersionList1.ToolTip"));
            AVersionList1.ItemClick += AVersionList1_ItemClick;
            // 
            // lblExpand2
            // 
            resources.ApplyResources(lblExpand2, "lblExpand2");
            lblExpand2.ForeColor = System.Drawing.Color.Black;
            lblExpand2.Name = "lblExpand2";
            ttMain.SetToolTip(lblExpand2, resources.GetString("lblExpand2.ToolTip"));
            // 
            // plFavorites
            // 
            resources.ApplyResources(plFavorites, "plFavorites");
            plFavorites.BackColor = System.Drawing.Color.White;
            plFavorites.Controls.Add(lvFavorite);
            plFavorites.Controls.Add(lblExpand1);
            plFavorites.Name = "plFavorites";
            ttMain.SetToolTip(plFavorites, resources.GetString("plFavorites.ToolTip"));
            // 
            // lvFavorite
            // 
            resources.ApplyResources(lvFavorite, "lvFavorite");
            lvFavorite.BackColor = System.Drawing.Color.White;
            lvFavorite.BorderStyle = BorderStyle.None;
            lvFavorite.Columns.AddRange(new ColumnHeader[] { colName });
            lvFavorite.ContextMenuStrip = cmnuFavorits;
            lvFavorite.FullRowSelect = true;
            lvFavorite.HeaderStyle = ColumnHeaderStyle.None;
            lvFavorite.LabelEdit = true;
            lvFavorite.LargeImageList = ilFolder;
            lvFavorite.MultiSelect = false;
            lvFavorite.Name = "lvFavorite";
            lvFavorite.Scrollable = false;
            lvFavorite.ShowGroups = false;
            lvFavorite.ShowItemToolTips = true;
            lvFavorite.SmallImageList = ilFolder;
            lvFavorite.StateImageList = ilFolder;
            lvFavorite.TabStop = false;
            lvFavorite.TileSize = new System.Drawing.Size(16, 16);
            ttMain.SetToolTip(lvFavorite, resources.GetString("lvFavorite.ToolTip"));
            lvFavorite.UseCompatibleStateImageBehavior = false;
            lvFavorite.View = View.Details;
            lvFavorite.AfterLabelEdit += lvFavorite_AfterLabelEdit;
            lvFavorite.BeforeLabelEdit += lvFavorite_BeforeLabelEdit;
            lvFavorite.SizeChanged += lvFavorite_SizeChanged;
            lvFavorite.MouseClick += lvFavorite_Click;
            // 
            // colName
            // 
            resources.ApplyResources(colName, "colName");
            // 
            // cmnuFavorits
            // 
            resources.ApplyResources(cmnuFavorits, "cmnuFavorits");
            cmnuFavorits.ImageScalingSize = new System.Drawing.Size(24, 24);
            cmnuFavorits.Items.AddRange(new ToolStripItem[] { LöschenToolStripMenuItem, UmbenennenToolStripMenuItem });
            cmnuFavorits.Name = "cmnuFavorits";
            cmnuFavorits.RenderMode = ToolStripRenderMode.System;
            ttMain.SetToolTip(cmnuFavorits, resources.GetString("cmnuFavorits.ToolTip"));
            cmnuFavorits.Opening += cmnuFavorits_Opening;
            // 
            // LöschenToolStripMenuItem
            // 
            resources.ApplyResources(LöschenToolStripMenuItem, "LöschenToolStripMenuItem");
            LöschenToolStripMenuItem.Image = Resources.delete_icon_24;
            LöschenToolStripMenuItem.Name = "LöschenToolStripMenuItem";
            LöschenToolStripMenuItem.Click += LöschenToolStripMenuItem_Click;
            // 
            // UmbenennenToolStripMenuItem
            // 
            resources.ApplyResources(UmbenennenToolStripMenuItem, "UmbenennenToolStripMenuItem");
            UmbenennenToolStripMenuItem.Name = "UmbenennenToolStripMenuItem";
            UmbenennenToolStripMenuItem.Click += UmbenennenToolStripMenuItem_Click;
            // 
            // ilFolder
            // 
            ilFolder.ColorDepth = ColorDepth.Depth32Bit;
            ilFolder.ImageStream = (ImageListStreamer)resources.GetObject("ilFolder.ImageStream");
            ilFolder.TransparentColor = System.Drawing.Color.Transparent;
            ilFolder.Images.SetKeyName(0, "folder_icon_24.png");
            ilFolder.Images.SetKeyName(1, "folder_open_icon_24.png");
            ilFolder.Images.SetKeyName(2, "home_icon_24.png");
            ilFolder.Images.SetKeyName(3, "tab_icon_24.png");
            // 
            // lblExpand1
            // 
            resources.ApplyResources(lblExpand1, "lblExpand1");
            lblExpand1.ForeColor = System.Drawing.Color.Black;
            lblExpand1.Name = "lblExpand1";
            ttMain.SetToolTip(lblExpand1, resources.GetString("lblExpand1.ToolTip"));
            // 
            // lvFiles
            // 
            resources.ApplyResources(lvFiles, "lvFiles");
            lvFiles.BorderStyle = BorderStyle.None;
            lvFiles.Columns.AddRange(new ColumnHeader[] { ColumnHeader1, ColumnHeader2, ColumnHeader3, ColumnHeader4, ColumnHeader6, ColumnHeader5 });
            lvFiles.ContextMenuStrip = cmnuListRight;
            lvFiles.FullRowSelect = true;
            lvFiles.Groups.AddRange(new ListViewGroup[] { (ListViewGroup)resources.GetObject("lvFiles.Groups"), (ListViewGroup)resources.GetObject("lvFiles.Groups1") });
            lvFiles.LargeImageList = ilBigIcons;
            lvFiles.Name = "lvFiles";
            lvFiles.SmallImageList = ilSmallIcons;
            lvFiles.Sorting = SortOrder.Ascending;
            ttMain.SetToolTip(lvFiles, resources.GetString("lvFiles.ToolTip"));
            lvFiles.UseCompatibleStateImageBehavior = false;
            lvFiles.View = View.Details;
            lvFiles.ColumnClick += lvFiles_ColumnClick;
            lvFiles.SelectedIndexChanged += lvFiles_SelectedIndexChanged;
            lvFiles.KeyUp += lvFiles_KeyUp;
            lvFiles.MouseDoubleClick += lvFiles_MouseDoubleClick;
            // 
            // ColumnHeader1
            // 
            resources.ApplyResources(ColumnHeader1, "ColumnHeader1");
            // 
            // ColumnHeader2
            // 
            resources.ApplyResources(ColumnHeader2, "ColumnHeader2");
            // 
            // ColumnHeader3
            // 
            resources.ApplyResources(ColumnHeader3, "ColumnHeader3");
            // 
            // ColumnHeader4
            // 
            resources.ApplyResources(ColumnHeader4, "ColumnHeader4");
            // 
            // ColumnHeader6
            // 
            resources.ApplyResources(ColumnHeader6, "ColumnHeader6");
            // 
            // ColumnHeader5
            // 
            resources.ApplyResources(ColumnHeader5, "ColumnHeader5");
            // 
            // cmnuListRight
            // 
            resources.ApplyResources(cmnuListRight, "cmnuListRight");
            cmnuListRight.ImageScalingSize = new System.Drawing.Size(24, 24);
            cmnuListRight.Items.AddRange(new ToolStripItem[] { WiederherstellenToolStripMenuItem1, EigenschaftenToolStripMenuItem1, ToolStripMenuItem5, DateiOrdnerAusSicherungenLöschenToolStripMenuItem, ToolStripMenuItem3, SchnellansichtToolStripMenuItem1 });
            cmnuListRight.Name = "cmnuListRight";
            cmnuListRight.RenderMode = ToolStripRenderMode.System;
            ttMain.SetToolTip(cmnuListRight, resources.GetString("cmnuListRight.ToolTip"));
            cmnuListRight.Opening += cmnuListRight_Opening;
            // 
            // WiederherstellenToolStripMenuItem1
            // 
            resources.ApplyResources(WiederherstellenToolStripMenuItem1, "WiederherstellenToolStripMenuItem1");
            WiederherstellenToolStripMenuItem1.Image = Resources.settings_backup_restore_icon_24;
            WiederherstellenToolStripMenuItem1.Name = "WiederherstellenToolStripMenuItem1";
            WiederherstellenToolStripMenuItem1.Click += WiederherstellenToolStripMenuItem1_Click;
            // 
            // EigenschaftenToolStripMenuItem1
            // 
            resources.ApplyResources(EigenschaftenToolStripMenuItem1, "EigenschaftenToolStripMenuItem1");
            EigenschaftenToolStripMenuItem1.Name = "EigenschaftenToolStripMenuItem1";
            EigenschaftenToolStripMenuItem1.Click += EigenschaftenToolStripMenuItem1_Click;
            // 
            // ToolStripMenuItem5
            // 
            resources.ApplyResources(ToolStripMenuItem5, "ToolStripMenuItem5");
            ToolStripMenuItem5.Name = "ToolStripMenuItem5";
            // 
            // DateiOrdnerAusSicherungenLöschenToolStripMenuItem
            // 
            resources.ApplyResources(DateiOrdnerAusSicherungenLöschenToolStripMenuItem, "DateiOrdnerAusSicherungenLöschenToolStripMenuItem");
            DateiOrdnerAusSicherungenLöschenToolStripMenuItem.Name = "DateiOrdnerAusSicherungenLöschenToolStripMenuItem";
            DateiOrdnerAusSicherungenLöschenToolStripMenuItem.Click += DateiOrdnerAusSicherungenLöschenToolStripMenuItem_Click;
            // 
            // ToolStripMenuItem3
            // 
            resources.ApplyResources(ToolStripMenuItem3, "ToolStripMenuItem3");
            ToolStripMenuItem3.Name = "ToolStripMenuItem3";
            // 
            // SchnellansichtToolStripMenuItem1
            // 
            resources.ApplyResources(SchnellansichtToolStripMenuItem1, "SchnellansichtToolStripMenuItem1");
            SchnellansichtToolStripMenuItem1.Image = Resources.visibility_icon_24;
            SchnellansichtToolStripMenuItem1.Name = "SchnellansichtToolStripMenuItem1";
            SchnellansichtToolStripMenuItem1.Click += SchnellansichtToolStripMenuItem1_Click;
            // 
            // ilBigIcons
            // 
            ilBigIcons.ColorDepth = ColorDepth.Depth32Bit;
            ilBigIcons.ImageStream = (ImageListStreamer)resources.GetObject("ilBigIcons.ImageStream");
            ilBigIcons.TransparentColor = System.Drawing.Color.White;
            ilBigIcons.Images.SetKeyName(0, "folder");
            // 
            // ilSmallIcons
            // 
            ilSmallIcons.ColorDepth = ColorDepth.Depth32Bit;
            ilSmallIcons.ImageStream = (ImageListStreamer)resources.GetObject("ilSmallIcons.ImageStream");
            ilSmallIcons.TransparentColor = System.Drawing.Color.White;
            ilSmallIcons.Images.SetKeyName(0, "folder");
            // 
            // Panel3
            // 
            resources.ApplyResources(Panel3, "Panel3");
            Panel3.BackColor = System.Drawing.Color.White;
            Panel3.Controls.Add(chkFilesOfThisVersion);
            Panel3.Controls.Add(lblBackupdate);
            Panel3.Name = "Panel3";
            ttMain.SetToolTip(Panel3, resources.GetString("Panel3.ToolTip"));
            // 
            // chkFilesOfThisVersion
            // 
            resources.ApplyResources(chkFilesOfThisVersion, "chkFilesOfThisVersion");
            chkFilesOfThisVersion.Name = "chkFilesOfThisVersion";
            chkFilesOfThisVersion.TabStop = false;
            ttMain.SetToolTip(chkFilesOfThisVersion, resources.GetString("chkFilesOfThisVersion.ToolTip"));
            chkFilesOfThisVersion.UseVisualStyleBackColor = true;
            chkFilesOfThisVersion.CheckedChanged += chkFilesOfThisVersion_CheckedChanged;
            // 
            // lblBackupdate
            // 
            resources.ApplyResources(lblBackupdate, "lblBackupdate");
            lblBackupdate.AutoEllipsis = true;
            lblBackupdate.ForeColor = System.Drawing.Color.Black;
            lblBackupdate.Name = "lblBackupdate";
            ttMain.SetToolTip(lblBackupdate, resources.GetString("lblBackupdate.ToolTip"));
            // 
            // ilBigFolder
            // 
            ilBigFolder.ColorDepth = ColorDepth.Depth32Bit;
            ilBigFolder.ImageStream = (ImageListStreamer)resources.GetObject("ilBigFolder.ImageStream");
            ilBigFolder.TransparentColor = System.Drawing.Color.Transparent;
            ilBigFolder.Images.SetKeyName(0, "folder_open_icon_48.png");
            ilBigFolder.Images.SetKeyName(1, "folder_icon_48.png");
            // 
            // StatusStrip1
            // 
            resources.ApplyResources(StatusStrip1, "StatusStrip1");
            StatusStrip1.ImageScalingSize = new System.Drawing.Size(24, 24);
            StatusStrip1.Items.AddRange(new ToolStripItem[] { LCFiles, tsslblStatus });
            StatusStrip1.Name = "StatusStrip1";
            ttMain.SetToolTip(StatusStrip1, resources.GetString("StatusStrip1.ToolTip"));
            // 
            // tsslblStatus
            // 
            resources.ApplyResources(tsslblStatus, "tsslblStatus");
            tsslblStatus.Name = "tsslblStatus";
            // 
            // ttMain
            // 
            ttMain.ToolTipIcon = ToolTipIcon.Info;
            ttMain.ToolTipTitle = "Quickhilfe";
            // 
            // tMain
            // 
            resources.ApplyResources(tMain, "tMain");
            tMain.AllowMerge = false;
            tMain.BackColor = System.Drawing.Color.FromArgb(243, 243, 245);
            tMain.CanOverflow = false;
            tMain.GripStyle = ToolStripGripStyle.Hidden;
            tMain.ImageScalingSize = new System.Drawing.Size(24, 24);
            tMain.Items.AddRange(new ToolStripItem[] { ToolStripDropDownButton1, ToolStripSchnellansicht, ToolStripDropDownButton2, cmdRestore });
            tMain.Name = "tMain";
            tMain.RenderMode = ToolStripRenderMode.System;
            ttMain.SetToolTip(tMain, resources.GetString("tMain.ToolTip"));
            // 
            // ToolStripDropDownButton1
            // 
            resources.ApplyResources(ToolStripDropDownButton1, "ToolStripDropDownButton1");
            ToolStripDropDownButton1.DropDownItems.AddRange(new ToolStripItem[] { WiederherstellenToolStripMenuItem, AllesWiederherstellenToolStripMenuItem1, EigenschaftenToolStripMenuItem, ToolStripMenuItem1, SchnellansichtToolStripMenuItem, ZuOrdnerfavoritenHinzufügenToolStripMenuItem, ToolStripMenuItem2, VersionBearbeitenToolStripMenuItem, VersionLöschenToolStripMenuItem, VersionAlsStabilMarkierenToolStripMenuItem, ToolStripMenuItem4, MehrereVersionenLöschenToolStripMenuItem });
            ToolStripDropDownButton1.ForeColor = System.Drawing.Color.Black;
            ToolStripDropDownButton1.Image = Resources.edit_square_icon_24;
            ToolStripDropDownButton1.Margin = new Padding(3, 5, 3, 5);
            ToolStripDropDownButton1.Name = "ToolStripDropDownButton1";
            ToolStripDropDownButton1.Padding = new Padding(2);
            // 
            // WiederherstellenToolStripMenuItem
            // 
            resources.ApplyResources(WiederherstellenToolStripMenuItem, "WiederherstellenToolStripMenuItem");
            WiederherstellenToolStripMenuItem.Image = Resources.settings_backup_restore_icon_24;
            WiederherstellenToolStripMenuItem.Name = "WiederherstellenToolStripMenuItem";
            WiederherstellenToolStripMenuItem.Click += WiederherstellenToolStripMenuItem_MouseClick;
            // 
            // AllesWiederherstellenToolStripMenuItem1
            // 
            resources.ApplyResources(AllesWiederherstellenToolStripMenuItem1, "AllesWiederherstellenToolStripMenuItem1");
            AllesWiederherstellenToolStripMenuItem1.Name = "AllesWiederherstellenToolStripMenuItem1";
            AllesWiederherstellenToolStripMenuItem1.Click += AllesWiederherstellenToolStripMenuItem_Click;
            // 
            // EigenschaftenToolStripMenuItem
            // 
            resources.ApplyResources(EigenschaftenToolStripMenuItem, "EigenschaftenToolStripMenuItem");
            EigenschaftenToolStripMenuItem.Name = "EigenschaftenToolStripMenuItem";
            EigenschaftenToolStripMenuItem.Click += EigenschaftenToolStripMenuItem_Click;
            // 
            // ToolStripMenuItem1
            // 
            resources.ApplyResources(ToolStripMenuItem1, "ToolStripMenuItem1");
            ToolStripMenuItem1.Name = "ToolStripMenuItem1";
            // 
            // SchnellansichtToolStripMenuItem
            // 
            resources.ApplyResources(SchnellansichtToolStripMenuItem, "SchnellansichtToolStripMenuItem");
            SchnellansichtToolStripMenuItem.Image = Resources.visibility_icon_24;
            SchnellansichtToolStripMenuItem.Name = "SchnellansichtToolStripMenuItem";
            SchnellansichtToolStripMenuItem.Click += SchnellansichtToolStripMenuItem_Click;
            // 
            // ZuOrdnerfavoritenHinzufügenToolStripMenuItem
            // 
            resources.ApplyResources(ZuOrdnerfavoritenHinzufügenToolStripMenuItem, "ZuOrdnerfavoritenHinzufügenToolStripMenuItem");
            ZuOrdnerfavoritenHinzufügenToolStripMenuItem.Name = "ZuOrdnerfavoritenHinzufügenToolStripMenuItem";
            ZuOrdnerfavoritenHinzufügenToolStripMenuItem.Click += ZuOrdnerfavoritenHinzufügenToolStripMenuItem_Click;
            // 
            // ToolStripMenuItem2
            // 
            resources.ApplyResources(ToolStripMenuItem2, "ToolStripMenuItem2");
            ToolStripMenuItem2.Name = "ToolStripMenuItem2";
            // 
            // VersionBearbeitenToolStripMenuItem
            // 
            resources.ApplyResources(VersionBearbeitenToolStripMenuItem, "VersionBearbeitenToolStripMenuItem");
            VersionBearbeitenToolStripMenuItem.Name = "VersionBearbeitenToolStripMenuItem";
            VersionBearbeitenToolStripMenuItem.Click += VersionBearbeitenToolStripMenuItem_Click;
            // 
            // VersionLöschenToolStripMenuItem
            // 
            resources.ApplyResources(VersionLöschenToolStripMenuItem, "VersionLöschenToolStripMenuItem");
            VersionLöschenToolStripMenuItem.Image = Resources.delete_icon_24;
            VersionLöschenToolStripMenuItem.Name = "VersionLöschenToolStripMenuItem";
            VersionLöschenToolStripMenuItem.Click += VersionLöschenToolStripMenuItem_Click;
            // 
            // VersionAlsStabilMarkierenToolStripMenuItem
            // 
            resources.ApplyResources(VersionAlsStabilMarkierenToolStripMenuItem, "VersionAlsStabilMarkierenToolStripMenuItem");
            VersionAlsStabilMarkierenToolStripMenuItem.Name = "VersionAlsStabilMarkierenToolStripMenuItem";
            VersionAlsStabilMarkierenToolStripMenuItem.Click += VersionAlsStabilMarkierenToolStripMenuItem_Click;
            // 
            // ToolStripMenuItem4
            // 
            resources.ApplyResources(ToolStripMenuItem4, "ToolStripMenuItem4");
            ToolStripMenuItem4.Name = "ToolStripMenuItem4";
            // 
            // MehrereVersionenLöschenToolStripMenuItem
            // 
            resources.ApplyResources(MehrereVersionenLöschenToolStripMenuItem, "MehrereVersionenLöschenToolStripMenuItem");
            MehrereVersionenLöschenToolStripMenuItem.Name = "MehrereVersionenLöschenToolStripMenuItem";
            MehrereVersionenLöschenToolStripMenuItem.Click += MehrereVersionenLöschenToolStripMenuItem_Click;
            // 
            // ToolStripSchnellansicht
            // 
            resources.ApplyResources(ToolStripSchnellansicht, "ToolStripSchnellansicht");
            ToolStripSchnellansicht.ForeColor = System.Drawing.Color.Black;
            ToolStripSchnellansicht.Image = Resources.visibility_icon_24;
            ToolStripSchnellansicht.Margin = new Padding(3, 5, 3, 5);
            ToolStripSchnellansicht.Name = "ToolStripSchnellansicht";
            ToolStripSchnellansicht.Padding = new Padding(2);
            ToolStripSchnellansicht.Click += ToolStripSchnellansicht_Click;
            // 
            // ToolStripDropDownButton2
            // 
            resources.ApplyResources(ToolStripDropDownButton2, "ToolStripDropDownButton2");
            ToolStripDropDownButton2.Alignment = ToolStripItemAlignment.Right;
            ToolStripDropDownButton2.DropDownItems.AddRange(new ToolStripItem[] { GroßeSymboleToolStripMenuItem, ListenansichtToolStripMenuItem, DetailsansichtToolStripMenuItem });
            ToolStripDropDownButton2.ForeColor = System.Drawing.Color.Black;
            ToolStripDropDownButton2.Margin = new Padding(3, 5, 3, 5);
            ToolStripDropDownButton2.Name = "ToolStripDropDownButton2";
            ToolStripDropDownButton2.Padding = new Padding(2);
            // 
            // GroßeSymboleToolStripMenuItem
            // 
            resources.ApplyResources(GroßeSymboleToolStripMenuItem, "GroßeSymboleToolStripMenuItem");
            GroßeSymboleToolStripMenuItem.Image = Resources.grid_view_icon_24;
            GroßeSymboleToolStripMenuItem.Name = "GroßeSymboleToolStripMenuItem";
            GroßeSymboleToolStripMenuItem.Click += GroßeSymboleToolStripMenuItem_Click;
            // 
            // ListenansichtToolStripMenuItem
            // 
            resources.ApplyResources(ListenansichtToolStripMenuItem, "ListenansichtToolStripMenuItem");
            ListenansichtToolStripMenuItem.Image = Resources.list_icon_24;
            ListenansichtToolStripMenuItem.Name = "ListenansichtToolStripMenuItem";
            ListenansichtToolStripMenuItem.Click += ListenansichtToolStripMenuItem_Click;
            // 
            // DetailsansichtToolStripMenuItem
            // 
            resources.ApplyResources(DetailsansichtToolStripMenuItem, "DetailsansichtToolStripMenuItem");
            DetailsansichtToolStripMenuItem.Image = Resources.view_list_icon_24;
            DetailsansichtToolStripMenuItem.Name = "DetailsansichtToolStripMenuItem";
            DetailsansichtToolStripMenuItem.Click += DetailansichtToolStripMenuItem_Click;
            // 
            // cmdRestore
            // 
            resources.ApplyResources(cmdRestore, "cmdRestore");
            cmdRestore.Alignment = ToolStripItemAlignment.Right;
            cmdRestore.ForeColor = System.Drawing.Color.Black;
            cmdRestore.Image = Resources.settings_backup_restore_icon_24;
            cmdRestore.Margin = new Padding(3, 5, 3, 5);
            cmdRestore.Name = "cmdRestore";
            cmdRestore.Padding = new Padding(2);
            cmdRestore.MouseUp += cmdRestore_MouseClick;
            // 
            // plGlass
            // 
            resources.ApplyResources(plGlass, "plGlass");
            plGlass.BackColor = System.Drawing.Color.White;
            plGlass.Controls.Add(UcNav);
            plGlass.Controls.Add(Panel2);
            plGlass.Controls.Add(btnBack);
            plGlass.Name = "plGlass";
            ttMain.SetToolTip(plGlass, resources.GetString("plGlass.ToolTip"));
            // 
            // UcNav
            // 
            resources.ApplyResources(UcNav, "UcNav");
            UcNav.BackColor = System.Drawing.Color.White;
            UcNav.BorderStyle = BorderStyle.FixedSingle;
            UcNav.ForeColor = System.Drawing.Color.White;
            UcNav.Name = "UcNav";
            UcNav.Path = "";
            UcNav.PathLocalized = null;
            UcNav.TabStop = false;
            ttMain.SetToolTip(UcNav, resources.GetString("UcNav.ToolTip"));
            UcNav.ItemClick += UcNav_ItemClick;
            // 
            // Panel2
            // 
            resources.ApplyResources(Panel2, "Panel2");
            Panel2.BackColor = System.Drawing.Color.White;
            Panel2.Controls.Add(panel4);
            Panel2.Name = "Panel2";
            ttMain.SetToolTip(Panel2, resources.GetString("Panel2.ToolTip"));
            // 
            // panel4
            // 
            resources.ApplyResources(panel4, "panel4");
            panel4.BackColor = System.Drawing.Color.White;
            panel4.BorderStyle = BorderStyle.FixedSingle;
            panel4.Controls.Add(PictureBox3);
            panel4.Controls.Add(txtSearch);
            panel4.Name = "panel4";
            ttMain.SetToolTip(panel4, resources.GetString("panel4.ToolTip"));
            // 
            // PictureBox3
            // 
            resources.ApplyResources(PictureBox3, "PictureBox3");
            PictureBox3.Image = Resources.search_icon_48;
            PictureBox3.Name = "PictureBox3";
            PictureBox3.TabStop = false;
            ttMain.SetToolTip(PictureBox3, resources.GetString("PictureBox3.ToolTip"));
            // 
            // txtSearch
            // 
            resources.ApplyResources(txtSearch, "txtSearch");
            txtSearch.BackColor = System.Drawing.Color.White;
            txtSearch.BorderStyle = BorderStyle.None;
            txtSearch.ForeColor = System.Drawing.Color.Black;
            txtSearch.Name = "txtSearch";
            txtSearch.Tag = "search";
            ttMain.SetToolTip(txtSearch, resources.GetString("txtSearch.ToolTip"));
            txtSearch.TextChanged += txtSearch_TextChanged;
            txtSearch.Enter += txtSearch_Enter;
            txtSearch.KeyDown += txtSearch_KeyDown;
            txtSearch.Leave += txtSearch_Leave;
            // 
            // btnBack
            // 
            resources.ApplyResources(btnBack, "btnBack");
            btnBack.Cursor = Cursors.Hand;
            btnBack.Image = Resources.arrow_upward_icon_48;
            btnBack.Name = "btnBack";
            btnBack.TabStop = false;
            ttMain.SetToolTip(btnBack, resources.GetString("btnBack.ToolTip"));
            btnBack.Click += btnBack_Click;
            // 
            // Panel1
            // 
            resources.ApplyResources(Panel1, "Panel1");
            Panel1.BackColor = System.Drawing.Color.FromArgb(243, 243, 245);
            Panel1.Controls.Add(flpDetails);
            Panel1.Controls.Add(Label4);
            Panel1.Controls.Add(imgFileType);
            Panel1.Name = "Panel1";
            ttMain.SetToolTip(Panel1, resources.GetString("Panel1.ToolTip"));
            // 
            // flpDetails
            // 
            resources.ApplyResources(flpDetails, "flpDetails");
            flpDetails.BackColor = System.Drawing.Color.Transparent;
            flpDetails.Controls.Add(flpColumn1);
            flpDetails.Controls.Add(flpColumn2);
            flpDetails.Controls.Add(flpColumn3);
            flpDetails.Controls.Add(lblIntegrityCheck);
            flpDetails.Name = "flpDetails";
            ttMain.SetToolTip(flpDetails, resources.GetString("flpDetails.ToolTip"));
            // 
            // flpColumn1
            // 
            resources.ApplyResources(flpColumn1, "flpColumn1");
            flpColumn1.Controls.Add(lblFileName);
            flpColumn1.Controls.Add(lblFileType);
            flpColumn1.Name = "flpColumn1";
            ttMain.SetToolTip(flpColumn1, resources.GetString("flpColumn1.ToolTip"));
            // 
            // lblFileName
            // 
            resources.ApplyResources(lblFileName, "lblFileName");
            lblFileName.BackColor = System.Drawing.Color.Transparent;
            lblFileName.Name = "lblFileName";
            ttMain.SetToolTip(lblFileName, resources.GetString("lblFileName.ToolTip"));
            // 
            // lblFileType
            // 
            resources.ApplyResources(lblFileType, "lblFileType");
            lblFileType.BackColor = System.Drawing.Color.Transparent;
            lblFileType.ForeColor = System.Drawing.Color.Black;
            lblFileType.Name = "lblFileType";
            ttMain.SetToolTip(lblFileType, resources.GetString("lblFileType.ToolTip"));
            // 
            // flpColumn2
            // 
            resources.ApplyResources(flpColumn2, "flpColumn2");
            flpColumn2.Controls.Add(flpDateLastWrite);
            flpColumn2.Controls.Add(flpDateCreated);
            flpColumn2.Name = "flpColumn2";
            ttMain.SetToolTip(flpColumn2, resources.GetString("flpColumn2.ToolTip"));
            // 
            // flpDateLastWrite
            // 
            resources.ApplyResources(flpDateLastWrite, "flpDateLastWrite");
            flpDateLastWrite.BackColor = System.Drawing.Color.Transparent;
            flpDateLastWrite.Controls.Add(lFileLastEdited);
            flpDateLastWrite.Controls.Add(lblFileLastEdited);
            flpDateLastWrite.Name = "flpDateLastWrite";
            ttMain.SetToolTip(flpDateLastWrite, resources.GetString("flpDateLastWrite.ToolTip"));
            // 
            // lFileLastEdited
            // 
            resources.ApplyResources(lFileLastEdited, "lFileLastEdited");
            lFileLastEdited.BackColor = System.Drawing.Color.Transparent;
            lFileLastEdited.ForeColor = System.Drawing.Color.Gray;
            lFileLastEdited.Name = "lFileLastEdited";
            ttMain.SetToolTip(lFileLastEdited, resources.GetString("lFileLastEdited.ToolTip"));
            // 
            // lblFileLastEdited
            // 
            resources.ApplyResources(lblFileLastEdited, "lblFileLastEdited");
            lblFileLastEdited.BackColor = System.Drawing.Color.Transparent;
            lblFileLastEdited.ForeColor = System.Drawing.Color.Black;
            lblFileLastEdited.Name = "lblFileLastEdited";
            ttMain.SetToolTip(lblFileLastEdited, resources.GetString("lblFileLastEdited.ToolTip"));
            // 
            // flpDateCreated
            // 
            resources.ApplyResources(flpDateCreated, "flpDateCreated");
            flpDateCreated.BackColor = System.Drawing.Color.Transparent;
            flpDateCreated.Controls.Add(lFileCreated);
            flpDateCreated.Controls.Add(lblFileCreated);
            flpDateCreated.Name = "flpDateCreated";
            ttMain.SetToolTip(flpDateCreated, resources.GetString("flpDateCreated.ToolTip"));
            // 
            // lFileCreated
            // 
            resources.ApplyResources(lFileCreated, "lFileCreated");
            lFileCreated.BackColor = System.Drawing.Color.Transparent;
            lFileCreated.ForeColor = System.Drawing.Color.Gray;
            lFileCreated.Name = "lFileCreated";
            ttMain.SetToolTip(lFileCreated, resources.GetString("lFileCreated.ToolTip"));
            // 
            // lblFileCreated
            // 
            resources.ApplyResources(lblFileCreated, "lblFileCreated");
            lblFileCreated.BackColor = System.Drawing.Color.Transparent;
            lblFileCreated.ForeColor = System.Drawing.Color.Black;
            lblFileCreated.Name = "lblFileCreated";
            ttMain.SetToolTip(lblFileCreated, resources.GetString("lblFileCreated.ToolTip"));
            // 
            // flpColumn3
            // 
            resources.ApplyResources(flpColumn3, "flpColumn3");
            flpColumn3.Controls.Add(flpSize);
            flpColumn3.Controls.Add(flpVersion);
            flpColumn3.Name = "flpColumn3";
            ttMain.SetToolTip(flpColumn3, resources.GetString("flpColumn3.ToolTip"));
            // 
            // flpSize
            // 
            resources.ApplyResources(flpSize, "flpSize");
            flpSize.BackColor = System.Drawing.Color.Transparent;
            flpSize.Controls.Add(lFileSize);
            flpSize.Controls.Add(lblFileSize);
            flpSize.Name = "flpSize";
            ttMain.SetToolTip(flpSize, resources.GetString("flpSize.ToolTip"));
            // 
            // lFileSize
            // 
            resources.ApplyResources(lFileSize, "lFileSize");
            lFileSize.BackColor = System.Drawing.Color.Transparent;
            lFileSize.ForeColor = System.Drawing.Color.Gray;
            lFileSize.Name = "lFileSize";
            ttMain.SetToolTip(lFileSize, resources.GetString("lFileSize.ToolTip"));
            // 
            // lblFileSize
            // 
            resources.ApplyResources(lblFileSize, "lblFileSize");
            lblFileSize.BackColor = System.Drawing.Color.Transparent;
            lblFileSize.ForeColor = System.Drawing.Color.Black;
            lblFileSize.Name = "lblFileSize";
            ttMain.SetToolTip(lblFileSize, resources.GetString("lblFileSize.ToolTip"));
            // 
            // flpVersion
            // 
            resources.ApplyResources(flpVersion, "flpVersion");
            flpVersion.BackColor = System.Drawing.Color.Transparent;
            flpVersion.Controls.Add(lFileVersion);
            flpVersion.Controls.Add(lblFileVersion);
            flpVersion.Name = "flpVersion";
            ttMain.SetToolTip(flpVersion, resources.GetString("flpVersion.ToolTip"));
            // 
            // lFileVersion
            // 
            resources.ApplyResources(lFileVersion, "lFileVersion");
            lFileVersion.BackColor = System.Drawing.Color.Transparent;
            lFileVersion.ForeColor = System.Drawing.Color.Gray;
            lFileVersion.Name = "lFileVersion";
            ttMain.SetToolTip(lFileVersion, resources.GetString("lFileVersion.ToolTip"));
            // 
            // lblFileVersion
            // 
            resources.ApplyResources(lblFileVersion, "lblFileVersion");
            lblFileVersion.BackColor = System.Drawing.Color.Transparent;
            lblFileVersion.ForeColor = System.Drawing.Color.Black;
            lblFileVersion.Name = "lblFileVersion";
            ttMain.SetToolTip(lblFileVersion, resources.GetString("lblFileVersion.ToolTip"));
            // 
            // lblIntegrityCheck
            // 
            resources.ApplyResources(lblIntegrityCheck, "lblIntegrityCheck");
            lblIntegrityCheck.ForeColor = System.Drawing.Color.Red;
            lblIntegrityCheck.Name = "lblIntegrityCheck";
            ttMain.SetToolTip(lblIntegrityCheck, resources.GetString("lblIntegrityCheck.ToolTip"));
            // 
            // Label4
            // 
            resources.ApplyResources(Label4, "Label4");
            Label4.BackColor = System.Drawing.Color.FromArgb(224, 224, 224);
            Label4.Name = "Label4";
            ttMain.SetToolTip(Label4, resources.GetString("Label4.ToolTip"));
            // 
            // imgFileType
            // 
            resources.ApplyResources(imgFileType, "imgFileType");
            imgFileType.BackColor = System.Drawing.Color.Transparent;
            imgFileType.Name = "imgFileType";
            imgFileType.TabStop = false;
            ttMain.SetToolTip(imgFileType, resources.GetString("imgFileType.ToolTip"));
            // 
            // bgrWorkSearch
            // 
            bgrWorkSearch.WorkerReportsProgress = true;
            bgrWorkSearch.WorkerSupportsCancellation = true;
            bgrWorkSearch.DoWork += bgrWorkSearch_DoWork;
            bgrWorkSearch.RunWorkerCompleted += bgrWorkSearch_RunWorkerCompleted;
            // 
            // AllesWiederherstellenToolStripMenuItem
            // 
            resources.ApplyResources(AllesWiederherstellenToolStripMenuItem, "AllesWiederherstellenToolStripMenuItem");
            AllesWiederherstellenToolStripMenuItem.Name = "AllesWiederherstellenToolStripMenuItem";
            // 
            // frmBrowser
            // 
            resources.ApplyResources(this, "$this");
            AutoScaleMode = AutoScaleMode.Dpi;
            Controls.Add(SplitContainer1);
            Controls.Add(plGlass);
            Controls.Add(Panel1);
            Controls.Add(tMain);
            Controls.Add(StatusStrip1);
            DoubleBuffered = true;
            Name = "frmBrowser";
            ttMain.SetToolTip(this, resources.GetString("$this.ToolTip"));
            FormClosing += frmBrowser_FormClosing;
            Load += frmBrowser_Load;
            SplitContainer1.Panel1.ResumeLayout(false);
            SplitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)SplitContainer1).EndInit();
            SplitContainer1.ResumeLayout(false);
            plVersions.ResumeLayout(false);
            plVersions.PerformLayout();
            plFavorites.ResumeLayout(false);
            plFavorites.PerformLayout();
            cmnuFavorits.ResumeLayout(false);
            cmnuListRight.ResumeLayout(false);
            Panel3.ResumeLayout(false);
            Panel3.PerformLayout();
            StatusStrip1.ResumeLayout(false);
            StatusStrip1.PerformLayout();
            tMain.ResumeLayout(false);
            tMain.PerformLayout();
            plGlass.ResumeLayout(false);
            Panel2.ResumeLayout(false);
            panel4.ResumeLayout(false);
            panel4.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)PictureBox3).EndInit();
            ((System.ComponentModel.ISupportInitialize)btnBack).EndInit();
            Panel1.ResumeLayout(false);
            Panel1.PerformLayout();
            flpDetails.ResumeLayout(false);
            flpDetails.PerformLayout();
            flpColumn1.ResumeLayout(false);
            flpColumn1.PerformLayout();
            flpColumn2.ResumeLayout(false);
            flpColumn2.PerformLayout();
            flpDateLastWrite.ResumeLayout(false);
            flpDateLastWrite.PerformLayout();
            flpDateCreated.ResumeLayout(false);
            flpDateCreated.PerformLayout();
            flpColumn3.ResumeLayout(false);
            flpColumn3.PerformLayout();
            flpSize.ResumeLayout(false);
            flpSize.PerformLayout();
            flpVersion.ResumeLayout(false);
            flpVersion.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)imgFileType).EndInit();
            ResumeLayout(false);
            PerformLayout();
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