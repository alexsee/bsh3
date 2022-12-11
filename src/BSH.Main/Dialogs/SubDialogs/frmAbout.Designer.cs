using System.Diagnostics;
using System.Windows.Forms;

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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmAbout));
            this.Label2 = new System.Windows.Forms.Label();
            this.lblProduct = new System.Windows.Forms.Label();
            this.lblVersion = new System.Windows.Forms.Label();
            this.Panel1 = new System.Windows.Forms.Panel();
            this.PictureBox1 = new System.Windows.Forms.PictureBox();
            this.Panel2 = new System.Windows.Forms.Panel();
            this.cmdOK = new System.Windows.Forms.Button();
            this.Label1 = new System.Windows.Forms.Label();
            this.Label3 = new System.Windows.Forms.Label();
            this.Label4 = new System.Windows.Forms.Label();
            this.llWebsite = new System.Windows.Forms.LinkLabel();
            this.TextBox1 = new System.Windows.Forms.TextBox();
            this.Panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.PictureBox1)).BeginInit();
            this.Panel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // Label2
            // 
            this.Label2.AutoSize = true;
            this.Label2.BackColor = System.Drawing.Color.Transparent;
            this.Label2.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Label2.ForeColor = System.Drawing.Color.White;
            this.Label2.Location = new System.Drawing.Point(18, 18);
            this.Label2.Name = "Label2";
            this.Label2.Size = new System.Drawing.Size(324, 32);
            this.Label2.TabIndex = 1;
            this.Label2.Text = "Über Backup Service Home 3";
            // 
            // lblProduct
            // 
            this.lblProduct.AutoSize = true;
            this.lblProduct.Location = new System.Drawing.Point(39, 86);
            this.lblProduct.Name = "lblProduct";
            this.lblProduct.Size = new System.Drawing.Size(216, 28);
            this.lblProduct.TabIndex = 2;
            this.lblProduct.Text = "Backup Service Home 3";
            // 
            // lblVersion
            // 
            this.lblVersion.AutoSize = true;
            this.lblVersion.Location = new System.Drawing.Point(39, 111);
            this.lblVersion.Name = "lblVersion";
            this.lblVersion.Size = new System.Drawing.Size(137, 28);
            this.lblVersion.TabIndex = 3;
            this.lblVersion.Text = "Version 3.0.0.0";
            // 
            // Panel1
            // 
            this.Panel1.BackColor = System.Drawing.Color.DarkGray;
            this.Panel1.Controls.Add(this.PictureBox1);
            this.Panel1.Controls.Add(this.Label2);
            this.Panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.Panel1.Location = new System.Drawing.Point(0, 0);
            this.Panel1.Name = "Panel1";
            this.Panel1.Size = new System.Drawing.Size(740, 69);
            this.Panel1.TabIndex = 4;
            // 
            // PictureBox1
            // 
            this.PictureBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.PictureBox1.Image = global::BSH.Main.Properties.Resources.backup_service_client_32;
            this.PictureBox1.Location = new System.Drawing.Point(686, 10);
            this.PictureBox1.Name = "PictureBox1";
            this.PictureBox1.Size = new System.Drawing.Size(32, 32);
            this.PictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.PictureBox1.TabIndex = 5;
            this.PictureBox1.TabStop = false;
            // 
            // Panel2
            // 
            this.Panel2.BackColor = System.Drawing.SystemColors.Control;
            this.Panel2.Controls.Add(this.cmdOK);
            this.Panel2.Controls.Add(this.Label1);
            this.Panel2.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.Panel2.Location = new System.Drawing.Point(0, 579);
            this.Panel2.Name = "Panel2";
            this.Panel2.Size = new System.Drawing.Size(740, 54);
            this.Panel2.TabIndex = 83;
            // 
            // cmdOK
            // 
            this.cmdOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.cmdOK.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmdOK.Location = new System.Drawing.Point(614, 10);
            this.cmdOK.Name = "cmdOK";
            this.cmdOK.Size = new System.Drawing.Size(111, 32);
            this.cmdOK.TabIndex = 0;
            this.cmdOK.Text = "&OK";
            this.cmdOK.UseVisualStyleBackColor = true;
            // 
            // Label1
            // 
            this.Label1.BackColor = System.Drawing.Color.DarkGray;
            this.Label1.Dock = System.Windows.Forms.DockStyle.Top;
            this.Label1.Location = new System.Drawing.Point(0, 0);
            this.Label1.Name = "Label1";
            this.Label1.Size = new System.Drawing.Size(740, 2);
            this.Label1.TabIndex = 5;
            // 
            // Label3
            // 
            this.Label3.AutoSize = true;
            this.Label3.Location = new System.Drawing.Point(39, 140);
            this.Label3.Name = "Label3";
            this.Label3.Size = new System.Drawing.Size(384, 56);
            this.Label3.TabIndex = 84;
            this.Label3.Text = "© 2008-2022 Alexander Seeliger Software.\r\nAlle Rechte vorbehalten.";
            // 
            // Label4
            // 
            this.Label4.AutoSize = true;
            this.Label4.Font = new System.Drawing.Font("Calibri", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Label4.Location = new System.Drawing.Point(39, 204);
            this.Label4.Name = "Label4";
            this.Label4.Size = new System.Drawing.Size(110, 28);
            this.Label4.TabIndex = 85;
            this.Label4.Text = "Copyright:";
            // 
            // llWebsite
            // 
            this.llWebsite.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.llWebsite.AutoSize = true;
            this.llWebsite.Location = new System.Drawing.Point(498, 86);
            this.llWebsite.Name = "llWebsite";
            this.llWebsite.Size = new System.Drawing.Size(235, 28);
            this.llWebsite.TabIndex = 87;
            this.llWebsite.TabStop = true;
            this.llWebsite.Text = "https://www.brightbits.de";
            this.llWebsite.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.llWebsite_LinkClicked);
            // 
            // TextBox1
            // 
            this.TextBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.TextBox1.BackColor = System.Drawing.Color.White;
            this.TextBox1.Location = new System.Drawing.Point(42, 236);
            this.TextBox1.Multiline = true;
            this.TextBox1.Name = "TextBox1";
            this.TextBox1.ReadOnly = true;
            this.TextBox1.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.TextBox1.Size = new System.Drawing.Size(679, 306);
            this.TextBox1.TabIndex = 88;
            this.TextBox1.Text = resources.GetString("TextBox1.Text");
            // 
            // frmAbout
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(144F, 144F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(740, 633);
            this.Controls.Add(this.TextBox1);
            this.Controls.Add(this.llWebsite);
            this.Controls.Add(this.Label4);
            this.Controls.Add(this.Label3);
            this.Controls.Add(this.Panel2);
            this.Controls.Add(this.lblVersion);
            this.Controls.Add(this.lblProduct);
            this.Controls.Add(this.Panel1);
            this.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmAbout";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Über Backup Service Home 3";
            this.Load += new System.EventHandler(this.frmAbout_Load);
            this.Panel1.ResumeLayout(false);
            this.Panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.PictureBox1)).EndInit();
            this.Panel2.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

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