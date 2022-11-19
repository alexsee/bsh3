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
            var resources = new System.ComponentModel.ComponentResourceManager(typeof(ucNavigation));
            plTextbox = new Panel();
            txtPath = new TextBox();
            plNavi = new Panel();
            flpNavi = new FlowLayoutPanel();
            PictureBox1 = new PictureBox();
            plTextbox.SuspendLayout();
            plNavi.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)PictureBox1).BeginInit();
            SuspendLayout();
            // 
            // plTextbox
            // 
            plTextbox.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;

            plTextbox.Controls.Add(txtPath);
            plTextbox.Location = new Point(38, 2);
            plTextbox.Margin = new Padding(4, 4, 4, 4);
            plTextbox.Name = "plTextbox";
            plTextbox.Size = new Size(939, 33);
            plTextbox.TabIndex = 1;
            plTextbox.Visible = false;
            // 
            // txtPath
            // 
            txtPath.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            txtPath.BackColor = Color.WhiteSmoke;
            txtPath.BorderStyle = BorderStyle.None;
            txtPath.Font = new Font("Segoe UI", 9.0f, FontStyle.Regular, GraphicsUnit.Point);
            txtPath.Location = new Point(4, 4);
            txtPath.Margin = new Padding(4, 4, 4, 4);
            txtPath.Name = "txtPath";
            txtPath.Size = new Size(928, 24);
            txtPath.TabIndex = 0;
            txtPath.Text = @"\Server\Client";
            // 
            // plNavi
            // 
            plNavi.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;

            plNavi.BackColor = Color.Transparent;
            plNavi.Controls.Add(flpNavi);
            plNavi.Location = new Point(38, 2);
            plNavi.Margin = new Padding(4, 4, 4, 4);
            plNavi.Name = "plNavi";
            plNavi.Size = new Size(939, 32);
            plNavi.TabIndex = 2;
            // 
            // flpNavi
            // 
            flpNavi.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            flpNavi.BackColor = Color.Transparent;
            flpNavi.Location = new Point(0, 0);
            flpNavi.Margin = new Padding(0);
            flpNavi.Name = "flpNavi";
            flpNavi.Size = new Size(296, 32);
            flpNavi.TabIndex = 0;
            flpNavi.WrapContents = false;
            // 
            // PictureBox1
            // 
            PictureBox1.Image = (Image)resources.GetObject("PictureBox1.Image");
            PictureBox1.Location = new Point(4, 3);
            PictureBox1.Margin = new Padding(4, 4, 4, 4);
            PictureBox1.Name = "PictureBox1";
            PictureBox1.Size = new Size(30, 30);
            PictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
            PictureBox1.TabIndex = 0;
            PictureBox1.TabStop = false;
            // 
            // ucNavigation
            // 
            AutoScaleDimensions = new SizeF(144.0f, 144.0f);
            AutoScaleMode = AutoScaleMode.Dpi;
            BackColor = Color.Transparent;
            Controls.Add(plNavi);
            Controls.Add(plTextbox);
            Controls.Add(PictureBox1);
            DoubleBuffered = true;
            Font = new Font("Segoe UI", 9.75f, FontStyle.Regular, GraphicsUnit.Point);
            ForeColor = Color.White;
            Margin = new Padding(4, 4, 4, 4);
            Name = "ucNavigation";
            Size = new Size(980, 36);
            plTextbox.ResumeLayout(false);
            plTextbox.PerformLayout();
            plNavi.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)PictureBox1).EndInit();
            Resize += new EventHandler(ucNavigation_Resize);
            ResumeLayout(false);
        }

        internal PictureBox PictureBox1;
        internal Panel plTextbox;
        internal TextBox txtPath;
        internal Panel plNavi;
        internal FlowLayoutPanel flpNavi;
    }
}