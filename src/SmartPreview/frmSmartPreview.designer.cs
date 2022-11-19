using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace SmartPreview
{
    public partial class frmSmartPreview : Form
    {

        // Form overrides dispose to clean up the component list.
        [DebuggerNonUserCode()]
        protected override void Dispose(bool disposing)
        {
            if (disposing && components is object)
            {
                components.Dispose();
            }

            base.Dispose(disposing);
        }

        // Required by the Windows Form Designer
        private System.ComponentModel.IContainer components;

        // NOTE: The following procedure is required by the Windows Form Designer
        // It can be modified using the Windows Form Designer.  
        // Do not modify it using the code editor.
        [DebuggerStepThrough()]
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmSmartPreview));
            this.Panel1 = new System.Windows.Forms.Panel();
            this.lblMax = new System.Windows.Forms.Label();
            this.lblClose = new System.Windows.Forms.Label();
            this.plContent = new System.Windows.Forms.Panel();
            this.plFileDetails = new System.Windows.Forms.Panel();
            this.flpFileDetails = new System.Windows.Forms.FlowLayoutPanel();
            this.lblFileType = new System.Windows.Forms.Label();
            this.lblFileName = new System.Windows.Forms.Label();
            this.picIcon = new System.Windows.Forms.PictureBox();
            this.phhcMain = new C4F.DevKit.PreviewHandler.PreviewHandlerHost.PreviewHandlerHostControl();
            this.wmp = new AxWMPLib.AxWindowsMediaPlayer();
            this.Panel1.SuspendLayout();
            this.plContent.SuspendLayout();
            this.plFileDetails.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picIcon)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.wmp)).BeginInit();
            this.SuspendLayout();
            // 
            // Panel1
            // 
            this.Panel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.Panel1.Controls.Add(this.lblMax);
            this.Panel1.Controls.Add(this.lblClose);
            this.Panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.Panel1.Location = new System.Drawing.Point(0, 746);
            this.Panel1.Name = "Panel1";
            this.Panel1.Size = new System.Drawing.Size(803, 69);
            this.Panel1.TabIndex = 1;
            // 
            // lblMax
            // 
            this.lblMax.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.lblMax.AutoSize = true;
            this.lblMax.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lblMax.Font = new System.Drawing.Font("Marlett", 21.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(2)));
            this.lblMax.ForeColor = System.Drawing.Color.White;
            this.lblMax.Location = new System.Drawing.Point(355, 11);
            this.lblMax.Name = "lblMax";
            this.lblMax.Size = new System.Drawing.Size(64, 44);
            this.lblMax.TabIndex = 1;
            this.lblMax.Text = "1";
            this.lblMax.Click += new System.EventHandler(this.lblMax_Click);
            // 
            // lblClose
            // 
            this.lblClose.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.lblClose.AutoSize = true;
            this.lblClose.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lblClose.Font = new System.Drawing.Font("Marlett", 21.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(2)));
            this.lblClose.ForeColor = System.Drawing.Color.White;
            this.lblClose.Location = new System.Drawing.Point(404, 11);
            this.lblClose.Name = "lblClose";
            this.lblClose.Size = new System.Drawing.Size(64, 44);
            this.lblClose.TabIndex = 0;
            this.lblClose.Text = "r";
            this.lblClose.Click += new System.EventHandler(this.lblClose_Click);
            // 
            // plContent
            // 
            this.plContent.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.plContent.Controls.Add(this.plFileDetails);
            this.plContent.Controls.Add(this.phhcMain);
            this.plContent.Controls.Add(this.wmp);
            this.plContent.Dock = System.Windows.Forms.DockStyle.Fill;
            this.plContent.Location = new System.Drawing.Point(0, 0);
            this.plContent.Name = "plContent";
            this.plContent.Size = new System.Drawing.Size(803, 746);
            this.plContent.TabIndex = 2;
            // 
            // plFileDetails
            // 
            this.plFileDetails.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.plFileDetails.Controls.Add(this.flpFileDetails);
            this.plFileDetails.Controls.Add(this.lblFileType);
            this.plFileDetails.Controls.Add(this.lblFileName);
            this.plFileDetails.Controls.Add(this.picIcon);
            this.plFileDetails.Font = new System.Drawing.Font("Calibri", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.plFileDetails.ForeColor = System.Drawing.Color.White;
            this.plFileDetails.Location = new System.Drawing.Point(12, 12);
            this.plFileDetails.Name = "plFileDetails";
            this.plFileDetails.Size = new System.Drawing.Size(595, 395);
            this.plFileDetails.TabIndex = 2;
            this.plFileDetails.Visible = false;
            // 
            // flpFileDetails
            // 
            this.flpFileDetails.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.flpFileDetails.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flpFileDetails.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.flpFileDetails.Location = new System.Drawing.Point(92, 115);
            this.flpFileDetails.Margin = new System.Windows.Forms.Padding(0, 3, 3, 10);
            this.flpFileDetails.Name = "flpFileDetails";
            this.flpFileDetails.Size = new System.Drawing.Size(503, 280);
            this.flpFileDetails.TabIndex = 3;
            // 
            // lblFileType
            // 
            this.lblFileType.AutoSize = true;
            this.lblFileType.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblFileType.Location = new System.Drawing.Point(95, 67);
            this.lblFileType.Name = "lblFileType";
            this.lblFileType.Size = new System.Drawing.Size(276, 29);
            this.lblFileType.TabIndex = 2;
            this.lblFileType.Text = "Microsoft Word Dokument";
            // 
            // lblFileName
            // 
            this.lblFileName.AutoSize = true;
            this.lblFileName.Font = new System.Drawing.Font("Calibri", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblFileName.ForeColor = System.Drawing.Color.White;
            this.lblFileName.Location = new System.Drawing.Point(93, 28);
            this.lblFileName.Name = "lblFileName";
            this.lblFileName.Size = new System.Drawing.Size(300, 39);
            this.lblFileName.TabIndex = 1;
            this.lblFileName.Text = "Mein Dokument.docx";
            // 
            // picIcon
            // 
            this.picIcon.Location = new System.Drawing.Point(23, 32);
            this.picIcon.Name = "picIcon";
            this.picIcon.Size = new System.Drawing.Size(64, 64);
            this.picIcon.TabIndex = 0;
            this.picIcon.TabStop = false;
            // 
            // phhcMain
            // 
            this.phhcMain.FilePath = null;
            this.phhcMain.Location = new System.Drawing.Point(136, 434);
            this.phhcMain.Name = "phhcMain";
            this.phhcMain.Size = new System.Drawing.Size(159, 117);
            this.phhcMain.TabIndex = 1;
            this.phhcMain.Visible = false;
            // 
            // wmp
            // 
            this.wmp.Enabled = true;
            this.wmp.Location = new System.Drawing.Point(628, 613);
            this.wmp.Name = "wmp";
            this.wmp.OcxState = ((System.Windows.Forms.AxHost.State)(resources.GetObject("wmp.OcxState")));
            this.wmp.Size = new System.Drawing.Size(156, 119);
            this.wmp.TabIndex = 0;
            this.wmp.Visible = false;
            this.wmp.PreviewKeyDown += new System.Windows.Forms.PreviewKeyDownEventHandler(this.wmp_PreviewKeyDown);
            // 
            // frmSmartPreview
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(144F, 144F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.ClientSize = new System.Drawing.Size(803, 815);
            this.ControlBox = false;
            this.Controls.Add(this.plContent);
            this.Controls.Add(this.Panel1);
            this.DoubleBuffered = true;
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmSmartPreview";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Schnellansicht";
            this.TopMost = true;
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.frmSmartPreview_KeyUp);
            this.Panel1.ResumeLayout(false);
            this.Panel1.PerformLayout();
            this.plContent.ResumeLayout(false);
            this.plFileDetails.ResumeLayout(false);
            this.plFileDetails.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picIcon)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.wmp)).EndInit();
            this.ResumeLayout(false);

        }

        public frmSmartPreview()
        {

            // This call is required by the Windows Form Designer.
            InitializeComponent();

            // Add any initialization after the InitializeComponent() call.

        }

        ~frmSmartPreview()
        {
        }

        internal Panel Panel1;
        internal Label lblClose;
        internal Label lblMax;
        internal Panel plContent;
        internal AxWMPLib.AxWindowsMediaPlayer wmp;
        internal C4F.DevKit.PreviewHandler.PreviewHandlerHost.PreviewHandlerHostControl phhcMain;
        internal Panel plFileDetails;
        internal PictureBox picIcon;
        internal FlowLayoutPanel flpFileDetails;
        internal Label lblFileType;
        internal Label lblFileName;
    }
}