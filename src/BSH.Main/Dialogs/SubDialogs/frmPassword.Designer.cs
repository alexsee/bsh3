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
            resources.ApplyResources(Panel1, "Panel1");
            Panel1.BackColor = SystemColors.Control;
            Panel1.Controls.Add(Button1);
            Panel1.Controls.Add(Label1);
            Panel1.Controls.Add(cmdCancel);
            Panel1.Name = "Panel1";
            // 
            // Button1
            // 
            resources.ApplyResources(Button1, "Button1");
            Button1.DialogResult = DialogResult.OK;
            Button1.Name = "Button1";
            Button1.UseVisualStyleBackColor = true;
            // 
            // Label1
            // 
            resources.ApplyResources(Label1, "Label1");
            Label1.BackColor = Color.DarkGray;
            Label1.Name = "Label1";
            // 
            // cmdCancel
            // 
            resources.ApplyResources(cmdCancel, "cmdCancel");
            cmdCancel.DialogResult = DialogResult.Cancel;
            cmdCancel.Name = "cmdCancel";
            cmdCancel.UseVisualStyleBackColor = true;
            // 
            // Label2
            // 
            resources.ApplyResources(Label2, "Label2");
            Label2.BackColor = Color.Transparent;
            Label2.ForeColor = Color.Black;
            Label2.Name = "Label2";
            // 
            // txtPassword
            // 
            resources.ApplyResources(txtPassword, "txtPassword");
            txtPassword.Name = "txtPassword";
            txtPassword.UseSystemPasswordChar = true;
            txtPassword.TextChanged += txtPassword_TextChanged;
            txtPassword.KeyDown += txtPassword_KeyDown;
            // 
            // PictureBox1
            // 
            resources.ApplyResources(PictureBox1, "PictureBox1");
            PictureBox1.BackColor = Color.Transparent;
            PictureBox1.Image = Main.Properties.Resources.password_icon_48;
            PictureBox1.Name = "PictureBox1";
            PictureBox1.TabStop = false;
            // 
            // Label3
            // 
            resources.ApplyResources(Label3, "Label3");
            Label3.Name = "Label3";
            // 
            // Label4
            // 
            resources.ApplyResources(Label4, "Label4");
            Label4.Name = "Label4";
            // 
            // Panel2
            // 
            resources.ApplyResources(Panel2, "Panel2");
            Panel2.BackgroundImage = Main.Properties.Resources.topPassword;
            Panel2.Controls.Add(Label2);
            Panel2.Controls.Add(PictureBox1);
            Panel2.Name = "Panel2";
            // 
            // chkSavePwd
            // 
            resources.ApplyResources(chkSavePwd, "chkSavePwd");
            chkSavePwd.Name = "chkSavePwd";
            chkSavePwd.UseVisualStyleBackColor = true;
            // 
            // frmPassword
            // 
            resources.ApplyResources(this, "$this");
            AutoScaleMode = AutoScaleMode.Dpi;
            BackColor = Color.White;
            Controls.Add(chkSavePwd);
            Controls.Add(Panel2);
            Controls.Add(Label4);
            Controls.Add(Label3);
            Controls.Add(txtPassword);
            Controls.Add(Panel1);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "frmPassword";
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