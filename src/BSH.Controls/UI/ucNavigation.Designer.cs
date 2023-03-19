using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace Brightbits.BSH.Main
{
    public partial class ucNavigation : UserControl
    {

        // UserControl1 überschreibt den Löschvorgang, um die Komponentenliste zu bereinigen.
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ucNavigation));
            this.plTextbox = new System.Windows.Forms.Panel();
            this.txtPath = new System.Windows.Forms.TextBox();
            this.plNavi = new System.Windows.Forms.Panel();
            this.flpNavi = new System.Windows.Forms.FlowLayoutPanel();
            this.PictureBox1 = new System.Windows.Forms.PictureBox();
            this.plTextbox.SuspendLayout();
            this.plNavi.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.PictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // plTextbox
            // 
            this.plTextbox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.plTextbox.Controls.Add(this.txtPath);
            this.plTextbox.Location = new System.Drawing.Point(38, 2);
            this.plTextbox.Margin = new System.Windows.Forms.Padding(4);
            this.plTextbox.Name = "plTextbox";
            this.plTextbox.Size = new System.Drawing.Size(939, 33);
            this.plTextbox.TabIndex = 1;
            this.plTextbox.Visible = false;
            // 
            // txtPath
            // 
            this.txtPath.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.txtPath.BackColor = System.Drawing.Color.WhiteSmoke;
            this.txtPath.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtPath.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.txtPath.Location = new System.Drawing.Point(4, 4);
            this.txtPath.Margin = new System.Windows.Forms.Padding(4);
            this.txtPath.Name = "txtPath";
            this.txtPath.Size = new System.Drawing.Size(928, 24);
            this.txtPath.TabIndex = 0;
            this.txtPath.Text = "\\Server\\Client";
            // 
            // plNavi
            // 
            this.plNavi.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.plNavi.BackColor = System.Drawing.Color.Transparent;
            this.plNavi.Controls.Add(this.flpNavi);
            this.plNavi.Location = new System.Drawing.Point(38, 2);
            this.plNavi.Margin = new System.Windows.Forms.Padding(4);
            this.plNavi.Name = "plNavi";
            this.plNavi.Size = new System.Drawing.Size(939, 32);
            this.plNavi.TabIndex = 2;
            // 
            // flpNavi
            // 
            this.flpNavi.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.flpNavi.BackColor = System.Drawing.Color.Transparent;
            this.flpNavi.Location = new System.Drawing.Point(0, 0);
            this.flpNavi.Margin = new System.Windows.Forms.Padding(0);
            this.flpNavi.Name = "flpNavi";
            this.flpNavi.Size = new System.Drawing.Size(296, 32);
            this.flpNavi.TabIndex = 0;
            this.flpNavi.WrapContents = false;
            // 
            // PictureBox1
            // 
            this.PictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("PictureBox1.Image")));
            this.PictureBox1.Location = new System.Drawing.Point(4, 3);
            this.PictureBox1.Margin = new System.Windows.Forms.Padding(4);
            this.PictureBox1.Name = "PictureBox1";
            this.PictureBox1.Size = new System.Drawing.Size(30, 30);
            this.PictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.PictureBox1.TabIndex = 0;
            this.PictureBox1.TabStop = false;
            // 
            // ucNavigation
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(144F, 144F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.BackColor = System.Drawing.Color.Transparent;
            this.Controls.Add(this.plNavi);
            this.Controls.Add(this.plTextbox);
            this.Controls.Add(this.PictureBox1);
            this.DoubleBuffered = true;
            this.Font = new System.Drawing.Font("Segoe UI", 9.75F);
            this.ForeColor = System.Drawing.Color.White;
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "ucNavigation";
            this.Size = new System.Drawing.Size(980, 36);
            this.Resize += new System.EventHandler(this.ucNavigation_Resize);
            this.plTextbox.ResumeLayout(false);
            this.plTextbox.PerformLayout();
            this.plNavi.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.PictureBox1)).EndInit();
            this.ResumeLayout(false);

        }

        internal PictureBox PictureBox1;
        internal Panel plTextbox;
        internal TextBox txtPath;
        internal Panel plNavi;
        internal FlowLayoutPanel flpNavi;
    }
}