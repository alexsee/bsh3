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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(aVersionListItem));
            this.lblVersionID = new System.Windows.Forms.Label();
            this.lblVersionDate = new System.Windows.Forms.Label();
            this.tt = new System.Windows.Forms.ToolTip(this.components);
            this.lblStable = new System.Windows.Forms.Label();
            this.PictureBox3 = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.PictureBox3)).BeginInit();
            this.SuspendLayout();
            // 
            // lblVersionID
            // 
            this.lblVersionID.BackColor = System.Drawing.Color.Gray;
            this.lblVersionID.Font = new System.Drawing.Font("Tahoma", 14.25F);
            this.lblVersionID.ForeColor = System.Drawing.Color.White;
            this.lblVersionID.Location = new System.Drawing.Point(3, 55);
            this.lblVersionID.Name = "lblVersionID";
            this.lblVersionID.Size = new System.Drawing.Size(42, 35);
            this.lblVersionID.TabIndex = 0;
            this.lblVersionID.Text = "59";
            this.lblVersionID.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblVersionID.Visible = false;
            this.lblVersionID.Click += new System.EventHandler(this.lblVersionID_Click);
            // 
            // lblVersionDate
            // 
            this.lblVersionDate.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblVersionDate.AutoEllipsis = true;
            this.lblVersionDate.BackColor = System.Drawing.Color.Transparent;
            this.lblVersionDate.Font = new System.Drawing.Font("Segoe UI", 9.75F);
            this.lblVersionDate.ForeColor = System.Drawing.Color.Black;
            this.lblVersionDate.Location = new System.Drawing.Point(36, 0);
            this.lblVersionDate.Name = "lblVersionDate";
            this.lblVersionDate.Size = new System.Drawing.Size(282, 40);
            this.lblVersionDate.TabIndex = 1;
            this.lblVersionDate.Text = "12. September 2007";
            this.lblVersionDate.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblVersionDate.UseMnemonic = false;
            this.lblVersionDate.Click += new System.EventHandler(this.lblVersionDate_Click);
            // 
            // tt
            // 
            this.tt.ToolTipIcon = System.Windows.Forms.ToolTipIcon.Info;
            // 
            // lblStable
            // 
            this.lblStable.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblStable.BackColor = System.Drawing.Color.LimeGreen;
            this.lblStable.Location = new System.Drawing.Point(297, 15);
            this.lblStable.Name = "lblStable";
            this.lblStable.Size = new System.Drawing.Size(10, 10);
            this.lblStable.TabIndex = 4;
            this.tt.SetToolTip(this.lblStable, "Version fixiert.");
            this.lblStable.Visible = false;
            // 
            // PictureBox3
            // 
            this.PictureBox3.BackColor = System.Drawing.Color.Transparent;
            this.PictureBox3.Image = ((System.Drawing.Image)(resources.GetObject("PictureBox3.Image")));
            this.PictureBox3.Location = new System.Drawing.Point(10, 11);
            this.PictureBox3.Name = "PictureBox3";
            this.PictureBox3.Size = new System.Drawing.Size(20, 20);
            this.PictureBox3.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.PictureBox3.TabIndex = 5;
            this.PictureBox3.TabStop = false;
            // 
            // aVersionListItem
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.White;
            this.Controls.Add(this.PictureBox3);
            this.Controls.Add(this.lblStable);
            this.Controls.Add(this.lblVersionDate);
            this.Controls.Add(this.lblVersionID);
            this.Cursor = System.Windows.Forms.Cursors.Hand;
            this.Name = "aVersionListItem";
            this.Size = new System.Drawing.Size(318, 40);
            this.Click += new System.EventHandler(this.aVersionListItem_Click);
            ((System.ComponentModel.ISupportInitialize)(this.PictureBox3)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        internal Label lblVersionID;
        internal Label lblVersionDate;
        internal ToolTip tt;
        internal Label lblStable;
        internal PictureBox PictureBox3;
    }
}