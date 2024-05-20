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
            resources.ApplyResources(PictureBox1, "PictureBox1");
            PictureBox1.Image = Main.Properties.Resources.error_icon_48;
            PictureBox1.Name = "PictureBox1";
            PictureBox1.TabStop = false;
            // 
            // Label1
            // 
            resources.ApplyResources(Label1, "Label1");
            Label1.Name = "Label1";
            // 
            // Button1
            // 
            resources.ApplyResources(Button1, "Button1");
            Button1.DialogResult = DialogResult.Cancel;
            Button1.Name = "Button1";
            Button1.UseVisualStyleBackColor = true;
            // 
            // Panel1
            // 
            resources.ApplyResources(Panel1, "Panel1");
            Panel1.BackColor = SystemColors.Control;
            Panel1.Controls.Add(Button2);
            Panel1.Controls.Add(Button1);
            Panel1.Name = "Panel1";
            // 
            // Button2
            // 
            resources.ApplyResources(Button2, "Button2");
            Button2.DialogResult = DialogResult.Retry;
            Button2.Name = "Button2";
            Button2.UseVisualStyleBackColor = true;
            // 
            // Label2
            // 
            resources.ApplyResources(Label2, "Label2");
            Label2.ForeColor = Color.FromArgb(0, 51, 153);
            Label2.Name = "Label2";
            // 
            // txtError
            // 
            resources.ApplyResources(txtError, "txtError");
            txtError.BackColor = Color.White;
            txtError.Name = "txtError";
            txtError.ReadOnly = true;
            // 
            // frmError
            // 
            resources.ApplyResources(this, "$this");
            AutoScaleMode = AutoScaleMode.Dpi;
            BackColor = Color.White;
            Controls.Add(txtError);
            Controls.Add(Label2);
            Controls.Add(Panel1);
            Controls.Add(Label1);
            Controls.Add(PictureBox1);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "frmError";
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