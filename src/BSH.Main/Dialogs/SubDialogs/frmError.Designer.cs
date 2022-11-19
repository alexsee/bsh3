using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace Brightbits.BSH.Main
{
    public partial class frmError : Form
    {

        // Das Formular überschreibt den Löschvorgang, um die Komponentenliste zu bereinigen.
        [DebuggerNonUserCode()]
        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing && components is object)
                {
                    components.Dispose();
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        // Wird vom Windows Form-Designer benötigt.
        private System.ComponentModel.IContainer components;

        // Hinweis: Die folgende Prozedur ist für den Windows Form-Designer erforderlich.
        // Das Bearbeiten ist mit dem Windows Form-Designer möglich.  
        // Das Bearbeiten mit dem Code-Editor ist nicht möglich.
        [DebuggerStepThrough()]
        private void InitializeComponent()
        {
            var resources = new System.ComponentModel.ComponentResourceManager(typeof(frmError));
            PictureBox1 = new PictureBox();
            Label1 = new Label();
            Button1 = new Button();
            Panel1 = new Panel();
            Button2 = new Button();
            Label2 = new Label();
            txtError = new TextBox();
            ((System.ComponentModel.ISupportInitialize)PictureBox1).BeginInit();
            Panel1.SuspendLayout();
            SuspendLayout();
            // 
            // PictureBox1
            // 
            PictureBox1.Image = (Image)resources.GetObject("PictureBox1.Image");
            PictureBox1.Location = new Point(27, 27);
            PictureBox1.Margin = new Padding(4, 4, 4, 4);
            PictureBox1.Name = "PictureBox1";
            PictureBox1.Size = new Size(48, 48);
            PictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
            PictureBox1.TabIndex = 0;
            PictureBox1.TabStop = false;
            // 
            // Label1
            // 
            Label1.Location = new Point(88, 75);
            Label1.Margin = new Padding(4, 0, 4, 0);
            Label1.Name = "Label1";
            Label1.Size = new Size(700, 108);
            Label1.TabIndex = 1;
            Label1.Text = "Es ist ein unerwartetes Problem aufgetreten, das nicht behoben werden kann. Die A" + "nwendung wird nun beendet." + '\r' + '\n' + '\r' + '\n' + "Klicken Sie auf \"Wiederholen\", um die Anwendung wi" + "eder zu starten.";
            // 
            // Button1
            // 
            Button1.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            Button1.DialogResult = DialogResult.Cancel;
            Button1.Location = new Point(650, 14);
            Button1.Margin = new Padding(4, 4, 4, 4);
            Button1.Name = "Button1";
            Button1.Size = new Size(140, 39);
            Button1.TabIndex = 2;
            Button1.Text = "Schl&ießen";
            Button1.UseVisualStyleBackColor = true;
            // 
            // Panel1
            // 
            Panel1.BackColor = SystemColors.Control;
            Panel1.Controls.Add(Button2);
            Panel1.Controls.Add(Button1);
            Panel1.Dock = DockStyle.Bottom;
            Panel1.Location = new Point(0, 454);
            Panel1.Margin = new Padding(4, 4, 4, 4);
            Panel1.Name = "Panel1";
            Panel1.Size = new Size(812, 70);
            Panel1.TabIndex = 3;
            // 
            // Button2
            // 
            Button2.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            Button2.DialogResult = DialogResult.Retry;
            Button2.Location = new Point(501, 14);
            Button2.Margin = new Padding(4, 4, 4, 4);
            Button2.Name = "Button2";
            Button2.Size = new Size(140, 39);
            Button2.TabIndex = 3;
            Button2.Text = "Wiederholen";
            Button2.UseVisualStyleBackColor = true;
            // 
            // Label2
            // 
            Label2.AutoSize = true;
            Label2.Font = new Font("Segoe UI", 12.0f, FontStyle.Regular, GraphicsUnit.Point);
            Label2.ForeColor = Color.FromArgb(0, 51, 153);
            Label2.Location = new Point(87, 27);
            Label2.Margin = new Padding(4, 0, 4, 0);
            Label2.Name = "Label2";
            Label2.Size = new Size(383, 32);
            Label2.TabIndex = 4;
            Label2.Text = "Unerwartetes Problem aufgetreten";
            // 
            // txtError
            // 
            txtError.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;

            txtError.BackColor = Color.White;
            txtError.Location = new Point(93, 200);
            txtError.Margin = new Padding(4, 4, 4, 4);
            txtError.Multiline = true;
            txtError.Name = "txtError";
            txtError.ReadOnly = true;
            txtError.ScrollBars = ScrollBars.Vertical;
            txtError.Size = new Size(694, 224);
            txtError.TabIndex = 5;
            // 
            // frmError
            // 
            AutoScaleDimensions = new SizeF(144.0f, 144.0f);
            AutoScaleMode = AutoScaleMode.Dpi;
            BackColor = Color.White;
            ClientSize = new Size(812, 524);
            Controls.Add(txtError);
            Controls.Add(Label2);
            Controls.Add(Panel1);
            Controls.Add(Label1);
            Controls.Add(PictureBox1);
            Font = new Font("Segoe UI", 9.75f, FontStyle.Regular, GraphicsUnit.Point);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Margin = new Padding(4, 4, 4, 4);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "frmError";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Backup Service Home 3";
            ((System.ComponentModel.ISupportInitialize)PictureBox1).EndInit();
            Panel1.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        internal PictureBox PictureBox1;
        internal Label Label1;
        internal Button Button1;
        internal Panel Panel1;
        internal Label Label2;
        internal Button Button2;
        internal TextBox txtError;
    }
}