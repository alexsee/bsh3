using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace Brightbits.BSH.Main
{
    public partial class frmChangePath : Form
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
            var resources = new System.ComponentModel.ComponentResourceManager(typeof(frmChangePath));
            Panel1 = new Panel();
            Button2 = new Button();
            Button1 = new Button();
            Label1 = new Label();
            cmdCancel = new Button();
            Label2 = new Label();
            Label3 = new Label();
            PictureBox1 = new PictureBox();
            Panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)PictureBox1).BeginInit();
            SuspendLayout();
            // 
            // Panel1
            // 
            Panel1.BackColor = SystemColors.Control;
            Panel1.Controls.Add(Button2);
            Panel1.Controls.Add(Button1);
            Panel1.Controls.Add(Label1);
            Panel1.Controls.Add(cmdCancel);
            Panel1.Dock = DockStyle.Bottom;
            Panel1.Location = new Point(0, 378);
            Panel1.Margin = new Padding(4, 4, 4, 4);
            Panel1.Name = "Panel1";
            Panel1.Size = new Size(801, 68);
            Panel1.TabIndex = 6;
            // 
            // Button2
            // 
            Button2.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            Button2.DialogResult = DialogResult.OK;
            Button2.Font = new Font("Segoe UI", 9.75f, FontStyle.Regular, GraphicsUnit.Point);
            Button2.Location = new Point(336, 15);
            Button2.Margin = new Padding(4, 4, 4, 4);
            Button2.Name = "Button2";
            Button2.Size = new Size(140, 39);
            Button2.TabIndex = 6;
            Button2.Text = "&Verschieben";
            Button2.UseVisualStyleBackColor = true;
            // 
            // Button1
            // 
            Button1.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            Button1.DialogResult = DialogResult.Ignore;
            Button1.Font = new Font("Segoe UI", 9.75f, FontStyle.Regular, GraphicsUnit.Point);
            Button1.Location = new Point(484, 15);
            Button1.Margin = new Padding(4, 4, 4, 4);
            Button1.Name = "Button1";
            Button1.Size = new Size(140, 39);
            Button1.TabIndex = 0;
            Button1.Text = "&Verwenden";
            Button1.UseVisualStyleBackColor = true;
            // 
            // Label1
            // 
            Label1.BackColor = Color.DarkGray;
            Label1.Dock = DockStyle.Top;
            Label1.Location = new Point(0, 0);
            Label1.Margin = new Padding(4, 0, 4, 0);
            Label1.Name = "Label1";
            Label1.Size = new Size(801, 2);
            Label1.TabIndex = 5;
            // 
            // cmdCancel
            // 
            cmdCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            cmdCancel.DialogResult = DialogResult.Cancel;
            cmdCancel.Font = new Font("Segoe UI", 9.75f, FontStyle.Regular, GraphicsUnit.Point);
            cmdCancel.Location = new Point(633, 15);
            cmdCancel.Margin = new Padding(4, 4, 4, 4);
            cmdCancel.Name = "cmdCancel";
            cmdCancel.Size = new Size(140, 39);
            cmdCancel.TabIndex = 1;
            cmdCancel.Text = "&Abbrechen";
            cmdCancel.UseVisualStyleBackColor = true;
            // 
            // Label2
            // 
            Label2.AutoSize = true;
            Label2.Font = new Font("Segoe UI", 12.0f, FontStyle.Regular, GraphicsUnit.Point);
            Label2.ForeColor = Color.FromArgb(0, 51, 153);
            Label2.Location = new Point(27, 27);
            Label2.Margin = new Padding(4, 0, 4, 0);
            Label2.Name = "Label2";
            Label2.Size = new Size(320, 32);
            Label2.TabIndex = 0;
            Label2.Text = "Ändern des Backupmediums";
            // 
            // Label3
            // 
            Label3.Location = new Point(96, 87);
            Label3.Margin = new Padding(4, 0, 4, 0);
            Label3.Name = "Label3";
            Label3.Size = new Size(676, 268);
            Label3.TabIndex = 8;
            Label3.Text = resources.GetString("Label3.Text");
            // 
            // PictureBox1
            // 
            PictureBox1.Image = (Image)resources.GetObject("PictureBox1.Image");
            PictureBox1.Location = new Point(33, 87);
            PictureBox1.Margin = new Padding(4, 4, 4, 4);
            PictureBox1.Name = "PictureBox1";
            PictureBox1.Size = new Size(48, 48);
            PictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
            PictureBox1.TabIndex = 7;
            PictureBox1.TabStop = false;
            // 
            // frmChangePath
            // 
            AutoScaleDimensions = new SizeF(144.0f, 144.0f);
            AutoScaleMode = AutoScaleMode.Dpi;
            BackColor = Color.White;
            ClientSize = new Size(801, 446);
            Controls.Add(Label3);
            Controls.Add(PictureBox1);
            Controls.Add(Label2);
            Controls.Add(Panel1);
            Font = new Font("Segoe UI", 9.75f, FontStyle.Regular, GraphicsUnit.Point);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Icon = (Icon)resources.GetObject("$this.Icon");
            Margin = new Padding(4, 4, 4, 4);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "frmChangePath";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Ändern des Backupmediums";
            Panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)PictureBox1).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        internal Panel Panel1;
        internal Label Label1;
        internal Button cmdCancel;
        internal Label Label2;
        internal Button Button1;
        internal Label Label3;
        internal PictureBox PictureBox1;
        internal Button Button2;
    }
}