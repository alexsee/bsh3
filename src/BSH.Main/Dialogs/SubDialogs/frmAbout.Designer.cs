using System.Diagnostics;
using System.Windows.Forms;
using BSH.Main.Properties;

namespace Brightbits.BSH.Main
{
    public partial class frmAbout : Form
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
            var resources = new System.ComponentModel.ComponentResourceManager(typeof(frmAbout));
            Label2 = new Label();
            lblProduct = new Label();
            lblVersion = new Label();
            Panel1 = new Panel();
            PictureBox1 = new PictureBox();
            Panel2 = new Panel();
            cmdOK = new Button();
            Label1 = new Label();
            Label3 = new Label();
            Label4 = new Label();
            llWebsite = new LinkLabel();
            TextBox1 = new TextBox();
            Panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)PictureBox1).BeginInit();
            Panel2.SuspendLayout();
            SuspendLayout();
            // 
            // Label2
            // 
            resources.ApplyResources(Label2, "Label2");
            Label2.BackColor = System.Drawing.Color.Transparent;
            Label2.ForeColor = System.Drawing.Color.White;
            Label2.Name = "Label2";
            // 
            // lblProduct
            // 
            resources.ApplyResources(lblProduct, "lblProduct");
            lblProduct.Name = "lblProduct";
            // 
            // lblVersion
            // 
            resources.ApplyResources(lblVersion, "lblVersion");
            lblVersion.Name = "lblVersion";
            // 
            // Panel1
            // 
            resources.ApplyResources(Panel1, "Panel1");
            Panel1.BackColor = System.Drawing.Color.DarkGray;
            Panel1.Controls.Add(PictureBox1);
            Panel1.Controls.Add(Label2);
            Panel1.Name = "Panel1";
            // 
            // PictureBox1
            // 
            resources.ApplyResources(PictureBox1, "PictureBox1");
            PictureBox1.Image = Resources.backup_service_client_32;
            PictureBox1.Name = "PictureBox1";
            PictureBox1.TabStop = false;
            // 
            // Panel2
            // 
            resources.ApplyResources(Panel2, "Panel2");
            Panel2.BackColor = System.Drawing.SystemColors.Control;
            Panel2.Controls.Add(cmdOK);
            Panel2.Controls.Add(Label1);
            Panel2.Name = "Panel2";
            // 
            // cmdOK
            // 
            resources.ApplyResources(cmdOK, "cmdOK");
            cmdOK.DialogResult = DialogResult.OK;
            cmdOK.Name = "cmdOK";
            cmdOK.UseVisualStyleBackColor = true;
            // 
            // Label1
            // 
            resources.ApplyResources(Label1, "Label1");
            Label1.BackColor = System.Drawing.Color.DarkGray;
            Label1.Name = "Label1";
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
            // llWebsite
            // 
            resources.ApplyResources(llWebsite, "llWebsite");
            llWebsite.Name = "llWebsite";
            llWebsite.TabStop = true;
            llWebsite.LinkClicked += llWebsite_LinkClicked;
            // 
            // TextBox1
            // 
            resources.ApplyResources(TextBox1, "TextBox1");
            TextBox1.BackColor = System.Drawing.Color.White;
            TextBox1.Name = "TextBox1";
            TextBox1.ReadOnly = true;
            // 
            // frmAbout
            // 
            resources.ApplyResources(this, "$this");
            AutoScaleMode = AutoScaleMode.Dpi;
            BackColor = System.Drawing.Color.White;
            Controls.Add(TextBox1);
            Controls.Add(llWebsite);
            Controls.Add(Label4);
            Controls.Add(Label3);
            Controls.Add(Panel2);
            Controls.Add(lblVersion);
            Controls.Add(lblProduct);
            Controls.Add(Panel1);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "frmAbout";
            Load += frmAbout_Load;
            Panel1.ResumeLayout(false);
            Panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)PictureBox1).EndInit();
            Panel2.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        internal Label Label2;
        internal Label lblProduct;
        internal Label lblVersion;
        internal Panel Panel1;
        internal PictureBox PictureBox1;
        internal Panel Panel2;
        internal Button cmdOK;
        internal Label Label1;
        internal Label Label3;
        internal Label Label4;
        internal LinkLabel llWebsite;
        internal TextBox TextBox1;
    }
}