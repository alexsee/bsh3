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
            Panel1 = new Panel();
            lblMax = new Label();
            lblClose = new Label();
            plContent = new Panel();
            plFileDetails = new Panel();
            flpFileDetails = new FlowLayoutPanel();
            lblFileType = new Label();
            lblFileName = new Label();
            picIcon = new PictureBox();
            phhcMain = new C4F.DevKit.PreviewHandler.PreviewHandlerHost.PreviewHandlerHostControl();
            Panel1.SuspendLayout();
            plContent.SuspendLayout();
            plFileDetails.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)picIcon).BeginInit();
            SuspendLayout();
            // 
            // Panel1
            // 
            Panel1.BackColor = Color.FromArgb(64, 64, 64);
            Panel1.Controls.Add(lblMax);
            Panel1.Controls.Add(lblClose);
            Panel1.Dock = DockStyle.Bottom;
            Panel1.Location = new Point(0, 746);
            Panel1.Name = "Panel1";
            Panel1.Size = new Size(803, 69);
            Panel1.TabIndex = 1;
            // 
            // lblMax
            // 
            lblMax.Anchor = AnchorStyles.Top;
            lblMax.AutoSize = true;
            lblMax.Cursor = Cursors.Hand;
            lblMax.Font = new Font("Marlett", 21.75F, FontStyle.Bold, GraphicsUnit.Point, 2);
            lblMax.ForeColor = Color.White;
            lblMax.Location = new Point(355, 11);
            lblMax.Name = "lblMax";
            lblMax.Size = new Size(64, 44);
            lblMax.TabIndex = 1;
            lblMax.Text = "1";
            lblMax.Click += lblMax_Click;
            // 
            // lblClose
            // 
            lblClose.Anchor = AnchorStyles.Top;
            lblClose.AutoSize = true;
            lblClose.Cursor = Cursors.Hand;
            lblClose.Font = new Font("Marlett", 21.75F, FontStyle.Bold, GraphicsUnit.Point, 2);
            lblClose.ForeColor = Color.White;
            lblClose.Location = new Point(404, 11);
            lblClose.Name = "lblClose";
            lblClose.Size = new Size(64, 44);
            lblClose.TabIndex = 0;
            lblClose.Text = "r";
            lblClose.Click += lblClose_Click;
            // 
            // plContent
            // 
            plContent.BackColor = Color.FromArgb(64, 64, 64);
            plContent.Controls.Add(plFileDetails);
            plContent.Controls.Add(phhcMain);
            plContent.Dock = DockStyle.Fill;
            plContent.Location = new Point(0, 0);
            plContent.Name = "plContent";
            plContent.Size = new Size(803, 746);
            plContent.TabIndex = 2;
            // 
            // plFileDetails
            // 
            plFileDetails.BackColor = Color.FromArgb(64, 64, 64);
            plFileDetails.Controls.Add(flpFileDetails);
            plFileDetails.Controls.Add(lblFileType);
            plFileDetails.Controls.Add(lblFileName);
            plFileDetails.Controls.Add(picIcon);
            plFileDetails.Font = new Font("Calibri", 9.75F, FontStyle.Regular, GraphicsUnit.Point, 0);
            plFileDetails.ForeColor = Color.White;
            plFileDetails.Location = new Point(12, 12);
            plFileDetails.Name = "plFileDetails";
            plFileDetails.Size = new Size(595, 395);
            plFileDetails.TabIndex = 2;
            plFileDetails.Visible = false;
            // 
            // flpFileDetails
            // 
            flpFileDetails.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            flpFileDetails.FlowDirection = FlowDirection.TopDown;
            flpFileDetails.Font = new Font("Calibri", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            flpFileDetails.Location = new Point(92, 115);
            flpFileDetails.Margin = new Padding(0, 3, 3, 10);
            flpFileDetails.Name = "flpFileDetails";
            flpFileDetails.Size = new Size(503, 280);
            flpFileDetails.TabIndex = 3;
            // 
            // lblFileType
            // 
            lblFileType.AutoSize = true;
            lblFileType.Font = new Font("Calibri", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            lblFileType.Location = new Point(95, 67);
            lblFileType.Name = "lblFileType";
            lblFileType.Size = new Size(276, 29);
            lblFileType.TabIndex = 2;
            lblFileType.Text = "Microsoft Word Dokument";
            // 
            // lblFileName
            // 
            lblFileName.AutoSize = true;
            lblFileName.Font = new Font("Calibri", 15.75F, FontStyle.Regular, GraphicsUnit.Point, 0);
            lblFileName.ForeColor = Color.White;
            lblFileName.Location = new Point(93, 28);
            lblFileName.Name = "lblFileName";
            lblFileName.Size = new Size(300, 39);
            lblFileName.TabIndex = 1;
            lblFileName.Text = "Mein Dokument.docx";
            // 
            // picIcon
            // 
            picIcon.Location = new Point(23, 32);
            picIcon.Name = "picIcon";
            picIcon.Size = new Size(64, 64);
            picIcon.TabIndex = 0;
            picIcon.TabStop = false;
            // 
            // phhcMain
            // 
            phhcMain.FilePath = null;
            phhcMain.Location = new Point(136, 434);
            phhcMain.Name = "phhcMain";
            phhcMain.Size = new Size(159, 117);
            phhcMain.TabIndex = 1;
            phhcMain.Visible = false;
            // 
            // frmSmartPreview
            // 
            AutoScaleDimensions = new SizeF(144F, 144F);
            AutoScaleMode = AutoScaleMode.Dpi;
            BackColor = Color.FromArgb(224, 224, 224);
            ClientSize = new Size(803, 815);
            ControlBox = false;
            Controls.Add(plContent);
            Controls.Add(Panel1);
            DoubleBuffered = true;
            KeyPreview = true;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "frmSmartPreview";
            ShowIcon = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Schnellansicht";
            TopMost = true;
            KeyUp += frmSmartPreview_KeyUp;
            Panel1.ResumeLayout(false);
            Panel1.PerformLayout();
            plContent.ResumeLayout(false);
            plFileDetails.ResumeLayout(false);
            plFileDetails.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)picIcon).EndInit();
            ResumeLayout(false);
        }

        public frmSmartPreview()
        {

            // This call is required by the Windows Form Designer.
            InitializeComponent();

            // Add any initialization after the InitializeComponent() call.
            InitMediaPlayer();

        }

        ~frmSmartPreview()
        {
        }

        internal Panel Panel1;
        internal Label lblClose;
        internal Label lblMax;
        internal Panel plContent;
        internal C4F.DevKit.PreviewHandler.PreviewHandlerHost.PreviewHandlerHostControl phhcMain;
        internal Panel plFileDetails;
        internal PictureBox picIcon;
        internal FlowLayoutPanel flpFileDetails;
        internal Label lblFileType;
        internal Label lblFileName;
    }
}