using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace Brightbits.BSH.Main
{
    public partial class aVersionListItem : UserControl
    {

        // UserControl überschreibt den Löschvorgang, um die Komponentenliste zu bereinigen.
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
            var resources = new System.ComponentModel.ComponentResourceManager(typeof(aVersionListItem));
            lblVersionID = new Label();
            lblVersionID.Click += new EventHandler(lblVersionID_Click);
            lblVersionDate = new Label();
            lblVersionDate.Click += new EventHandler(lblVersionDate_Click);
            tt = new ToolTip(components);
            lblStable = new Label();
            PictureBox3 = new PictureBox();
            ((System.ComponentModel.ISupportInitialize)PictureBox3).BeginInit();
            SuspendLayout();
            // 
            // lblVersionID
            // 
            lblVersionID.BackColor = Color.Gray;
            lblVersionID.Font = new Font(new FontFamily("Tahoma"), 14.25f, FontStyle.Regular, GraphicsUnit.Point);
            lblVersionID.ForeColor = Color.White;
            lblVersionID.Location = new Point(3, 55);
            lblVersionID.Name = "lblVersionID";
            lblVersionID.Size = new Size(42, 35);
            lblVersionID.TabIndex = 0;
            lblVersionID.Text = "59";
            lblVersionID.TextAlign = ContentAlignment.MiddleCenter;
            lblVersionID.Visible = false;
            // 
            // lblVersionDate
            // 
            lblVersionDate.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            lblVersionDate.AutoEllipsis = true;
            lblVersionDate.BackColor = Color.Transparent;
            lblVersionDate.Font = new Font(new FontFamily("Segoe UI"), 9.75f, FontStyle.Regular, GraphicsUnit.Point);
            lblVersionDate.ForeColor = Color.Black;
            lblVersionDate.Location = new Point(29, 0);
            lblVersionDate.Name = "lblVersionDate";
            lblVersionDate.Size = new Size(307, 40);
            lblVersionDate.TabIndex = 1;
            lblVersionDate.Text = "12. September 2007";
            lblVersionDate.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // tt
            // 
            tt.ToolTipIcon = ToolTipIcon.Info;
            // 
            // lblStable
            // 
            lblStable.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lblStable.BackColor = Color.LimeGreen;
            lblStable.Location = new Point(297, 15);
            lblStable.Name = "lblStable";
            lblStable.Size = new Size(10, 10);
            lblStable.TabIndex = 4;
            tt.SetToolTip(lblStable, "Version fixiert.");
            lblStable.Visible = false;
            // 
            // PictureBox3
            // 
            PictureBox3.BackColor = Color.Transparent;
            PictureBox3.Image = (Image)resources.GetObject("PictureBox3.Image");
            PictureBox3.Location = new Point(10, 14);
            PictureBox3.Name = "PictureBox3";
            PictureBox3.Size = new Size(16, 16);
            PictureBox3.SizeMode = PictureBoxSizeMode.AutoSize;
            PictureBox3.TabIndex = 5;
            PictureBox3.TabStop = false;
            // 
            // aVersionListItem
            // 
            AutoScaleMode = AutoScaleMode.None;
            BackColor = Color.White;
            Controls.Add(PictureBox3);
            Controls.Add(lblStable);
            Controls.Add(lblVersionDate);
            Controls.Add(lblVersionID);
            Cursor = Cursors.Hand;
            Name = "aVersionListItem";
            Size = new Size(318, 40);
            ((System.ComponentModel.ISupportInitialize)PictureBox3).EndInit();
            Click += new EventHandler(aVersionListItem_Click);
            ResumeLayout(false);
            PerformLayout();
        }

        internal Label lblVersionID;
        internal Label lblVersionDate;
        internal ToolTip tt;
        internal Label lblStable;
        internal PictureBox PictureBox3;
    }
}