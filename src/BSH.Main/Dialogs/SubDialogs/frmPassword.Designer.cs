using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace Brightbits.BSH.Main
{
    public partial class frmPassword : Form
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
            var resources = new System.ComponentModel.ComponentResourceManager(typeof(frmPassword));
            Panel1 = new Panel();
            Button1 = new Button();
            Label1 = new Label();
            cmdCancel = new Button();
            Label2 = new Label();
            txtPassword = new TextBox();
            txtPassword.KeyDown += new KeyEventHandler(txtPassword_KeyDown);
            txtPassword.TextChanged += new EventHandler(txtPassword_TextChanged);
            PictureBox1 = new PictureBox();
            Label3 = new Label();
            Label4 = new Label();
            Panel2 = new Panel();
            chkSavePwd = new CheckBox();
            Panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)PictureBox1).BeginInit();
            Panel2.SuspendLayout();
            SuspendLayout();
            // 
            // Panel1
            // 
            Panel1.BackColor = SystemColors.Control;
            Panel1.Controls.Add(Button1);
            Panel1.Controls.Add(Label1);
            Panel1.Controls.Add(cmdCancel);
            Panel1.Dock = DockStyle.Bottom;
            Panel1.Location = new Point(0, 268);
            Panel1.Margin = new Padding(4);
            Panel1.Name = "Panel1";
            Panel1.Size = new Size(717, 68);
            Panel1.TabIndex = 6;
            // 
            // Button1
            // 
            Button1.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            Button1.DialogResult = DialogResult.OK;
            Button1.Font = new Font("Segoe UI", 9.0f, FontStyle.Regular, GraphicsUnit.Point);
            Button1.Location = new Point(401, 15);
            Button1.Margin = new Padding(4);
            Button1.Name = "Button1";
            Button1.Size = new Size(140, 39);
            Button1.TabIndex = 0;
            Button1.Text = "&OK";
            Button1.UseVisualStyleBackColor = true;
            // 
            // Label1
            // 
            Label1.BackColor = Color.DarkGray;
            Label1.Dock = DockStyle.Top;
            Label1.Location = new Point(0, 0);
            Label1.Margin = new Padding(4, 0, 4, 0);
            Label1.Name = "Label1";
            Label1.Size = new Size(717, 2);
            Label1.TabIndex = 5;
            // 
            // cmdCancel
            // 
            cmdCancel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            cmdCancel.DialogResult = DialogResult.Cancel;
            cmdCancel.Font = new Font("Segoe UI", 9.0f, FontStyle.Regular, GraphicsUnit.Point);
            cmdCancel.Location = new Point(549, 15);
            cmdCancel.Margin = new Padding(4);
            cmdCancel.Name = "cmdCancel";
            cmdCancel.Size = new Size(140, 39);
            cmdCancel.TabIndex = 1;
            cmdCancel.Text = "&Abbrechen";
            cmdCancel.UseVisualStyleBackColor = true;
            // 
            // Label2
            // 
            Label2.AutoSize = true;
            Label2.BackColor = Color.Transparent;
            Label2.Font = new Font("Segoe UI", 12.0f, FontStyle.Regular, GraphicsUnit.Point);
            Label2.ForeColor = Color.Black;
            Label2.Location = new Point(60, 14);
            Label2.Margin = new Padding(4, 0, 4, 0);
            Label2.Name = "Label2";
            Label2.Size = new Size(332, 32);
            Label2.TabIndex = 0;
            Label2.Text = "Kennworteingabe erforderlich";
            // 
            // txtPassword
            // 
            txtPassword.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtPassword.Location = new Point(285, 184);
            txtPassword.Margin = new Padding(4);
            txtPassword.Name = "txtPassword";
            txtPassword.Size = new Size(410, 31);
            txtPassword.TabIndex = 2;
            txtPassword.UseSystemPasswordChar = true;
            // 
            // PictureBox1
            // 
            PictureBox1.BackColor = Color.Transparent;
            PictureBox1.Image = (Image)resources.GetObject("PictureBox1.Image");
            PictureBox1.Location = new Point(18, 14);
            PictureBox1.Margin = new Padding(4);
            PictureBox1.Name = "PictureBox1";
            PictureBox1.Size = new Size(33, 33);
            PictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
            PictureBox1.TabIndex = 7;
            PictureBox1.TabStop = false;
            // 
            // Label3
            // 
            Label3.Location = new Point(18, 81);
            Label3.Margin = new Padding(4, 0, 4, 0);
            Label3.Name = "Label3";
            Label3.Size = new Size(693, 80);
            Label3.TabIndex = 8;
            Label3.Text = "Backup Service Home erfordert die Eingabe Ihres Kennwortes, um den Vorgang fortzu" + "setzen. Wenn Sie das Kennwort eingegeben haben, werden Sie für diese Sitzung nic" + "ht noch einmal danach gefragt.";
            // 
            // Label4
            // 
            Label4.AutoSize = true;
            Label4.Location = new Point(122, 189);
            Label4.Margin = new Padding(4, 0, 4, 0);
            Label4.Name = "Label4";
            Label4.Size = new Size(154, 25);
            Label4.TabIndex = 9;
            Label4.Text = "Kennworteingabe:";
            // 
            // Panel2
            // 
            Panel2.BackgroundImage = global::BSH.Main.Properties.Resources.topPassword;
            Panel2.BackgroundImageLayout = ImageLayout.Stretch;
            Panel2.Controls.Add(Label2);
            Panel2.Controls.Add(PictureBox1);
            Panel2.Dock = DockStyle.Top;
            Panel2.Location = new Point(0, 0);
            Panel2.Margin = new Padding(4);
            Panel2.Name = "Panel2";
            Panel2.Size = new Size(717, 60);
            Panel2.TabIndex = 10;
            // 
            // chkSavePwd
            // 
            chkSavePwd.AutoSize = true;
            chkSavePwd.Location = new Point(285, 228);
            chkSavePwd.Margin = new Padding(4);
            chkSavePwd.Name = "chkSavePwd";
            chkSavePwd.Size = new Size(193, 29);
            chkSavePwd.TabIndex = 11;
            chkSavePwd.Text = "Kennwort speichern";
            chkSavePwd.UseVisualStyleBackColor = true;
            // 
            // frmPassword
            // 
            AutoScaleDimensions = new SizeF(144.0f, 144.0f);
            AutoScaleMode = AutoScaleMode.Dpi;
            BackColor = Color.White;
            ClientSize = new Size(717, 336);
            Controls.Add(chkSavePwd);
            Controls.Add(Panel2);
            Controls.Add(Label4);
            Controls.Add(Label3);
            Controls.Add(txtPassword);
            Controls.Add(Panel1);
            Font = new Font("Segoe UI", 9.0f, FontStyle.Regular, GraphicsUnit.Point);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Icon = (Icon)resources.GetObject("$this.Icon");
            Margin = new Padding(4);
            MaximizeBox = false;
            MaximumSize = new Size(739, 392);
            MinimizeBox = false;
            MinimumSize = new Size(739, 392);
            Name = "frmPassword";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Backup Service Home - Kennwort eingeben";
            Panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)PictureBox1).EndInit();
            Panel2.ResumeLayout(false);
            Panel2.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        internal Panel Panel1;
        internal Label Label1;
        internal Button cmdCancel;
        internal Label Label2;
        internal Button Button1;
        internal TextBox txtPassword;
        internal PictureBox PictureBox1;
        internal Label Label3;
        internal Label Label4;
        internal Panel Panel2;
        internal CheckBox chkSavePwd;
    }
}