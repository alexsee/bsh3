using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using BSH.Main.Properties;

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
            resources.ApplyResources(Panel1, "Panel1");
            Panel1.BackColor = SystemColors.Control;
            Panel1.Controls.Add(Button2);
            Panel1.Controls.Add(Button1);
            Panel1.Controls.Add(Label1);
            Panel1.Controls.Add(cmdCancel);
            Panel1.Name = "Panel1";
            // 
            // Button2
            // 
            resources.ApplyResources(Button2, "Button2");
            Button2.DialogResult = DialogResult.OK;
            Button2.Name = "Button2";
            Button2.UseVisualStyleBackColor = true;
            // 
            // Button1
            // 
            resources.ApplyResources(Button1, "Button1");
            Button1.DialogResult = DialogResult.Ignore;
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
            Label2.ForeColor = Color.FromArgb(0, 51, 153);
            Label2.Name = "Label2";
            // 
            // Label3
            // 
            resources.ApplyResources(Label3, "Label3");
            Label3.Name = "Label3";
            // 
            // PictureBox1
            // 
            resources.ApplyResources(PictureBox1, "PictureBox1");
            PictureBox1.Image = Resources.storage_icon_48;
            PictureBox1.Name = "PictureBox1";
            PictureBox1.TabStop = false;
            // 
            // frmChangePath
            // 
            resources.ApplyResources(this, "$this");
            AutoScaleMode = AutoScaleMode.Dpi;
            BackColor = Color.White;
            Controls.Add(Label3);
            Controls.Add(PictureBox1);
            Controls.Add(Label2);
            Controls.Add(Panel1);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "frmChangePath";
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