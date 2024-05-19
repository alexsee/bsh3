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
            Label2.AutoSize = true;
            Label2.BackColor = System.Drawing.Color.Transparent;
            Label2.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            Label2.ForeColor = System.Drawing.Color.White;
            Label2.Location = new System.Drawing.Point(18, 18);
            Label2.Name = "Label2";
            Label2.Size = new System.Drawing.Size(324, 32);
            Label2.TabIndex = 1;
            Label2.Text = "Über Backup Service Home 3";
            // 
            // lblProduct
            // 
            lblProduct.AutoSize = true;
            lblProduct.Location = new System.Drawing.Point(39, 86);
            lblProduct.Name = "lblProduct";
            lblProduct.Size = new System.Drawing.Size(216, 28);
            lblProduct.TabIndex = 2;
            lblProduct.Text = "Backup Service Home 3";
            // 
            // lblVersion
            // 
            lblVersion.AutoSize = true;
            lblVersion.Location = new System.Drawing.Point(39, 111);
            lblVersion.Name = "lblVersion";
            lblVersion.Size = new System.Drawing.Size(137, 28);
            lblVersion.TabIndex = 3;
            lblVersion.Text = "Version 3.0.0.0";
            // 
            // Panel1
            // 
            Panel1.BackColor = System.Drawing.Color.DarkGray;
            Panel1.Controls.Add(PictureBox1);
            Panel1.Controls.Add(Label2);
            Panel1.Dock = DockStyle.Top;
            Panel1.Location = new System.Drawing.Point(0, 0);
            Panel1.Name = "Panel1";
            Panel1.Size = new System.Drawing.Size(740, 69);
            Panel1.TabIndex = 4;
            // 
            // PictureBox1
            // 
            PictureBox1.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            PictureBox1.Image = Resources.backup_service_client_32;
            PictureBox1.Location = new System.Drawing.Point(686, 10);
            PictureBox1.Name = "PictureBox1";
            PictureBox1.Size = new System.Drawing.Size(32, 32);
            PictureBox1.SizeMode = PictureBoxSizeMode.AutoSize;
            PictureBox1.TabIndex = 5;
            PictureBox1.TabStop = false;
            // 
            // Panel2
            // 
            Panel2.BackColor = System.Drawing.SystemColors.Control;
            Panel2.Controls.Add(cmdOK);
            Panel2.Controls.Add(Label1);
            Panel2.Dock = DockStyle.Bottom;
            Panel2.Location = new System.Drawing.Point(0, 579);
            Panel2.Name = "Panel2";
            Panel2.Size = new System.Drawing.Size(740, 54);
            Panel2.TabIndex = 83;
            // 
            // cmdOK
            // 
            cmdOK.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            cmdOK.DialogResult = DialogResult.OK;
            cmdOK.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            cmdOK.Location = new System.Drawing.Point(614, 10);
            cmdOK.Name = "cmdOK";
            cmdOK.Size = new System.Drawing.Size(111, 32);
            cmdOK.TabIndex = 0;
            cmdOK.Text = "&OK";
            cmdOK.UseVisualStyleBackColor = true;
            // 
            // Label1
            // 
            Label1.BackColor = System.Drawing.Color.DarkGray;
            Label1.Dock = DockStyle.Top;
            Label1.Location = new System.Drawing.Point(0, 0);
            Label1.Name = "Label1";
            Label1.Size = new System.Drawing.Size(740, 2);
            Label1.TabIndex = 5;
            // 
            // Label3
            // 
            Label3.AutoSize = true;
            Label3.Location = new System.Drawing.Point(39, 140);
            Label3.Name = "Label3";
            Label3.Size = new System.Drawing.Size(384, 56);
            Label3.TabIndex = 84;
            Label3.Text = "© 2008-2024 Alexander Seeliger Software.\r\nAlle Rechte vorbehalten.";
            // 
            // Label4
            // 
            Label4.AutoSize = true;
            Label4.Font = new System.Drawing.Font("Calibri", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            Label4.Location = new System.Drawing.Point(39, 204);
            Label4.Name = "Label4";
            Label4.Size = new System.Drawing.Size(110, 28);
            Label4.TabIndex = 85;
            Label4.Text = "Copyright:";
            // 
            // llWebsite
            // 
            llWebsite.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            llWebsite.AutoSize = true;
            llWebsite.Location = new System.Drawing.Point(498, 86);
            llWebsite.Name = "llWebsite";
            llWebsite.Size = new System.Drawing.Size(235, 28);
            llWebsite.TabIndex = 87;
            llWebsite.TabStop = true;
            llWebsite.Text = "https://www.brightbits.de";
            llWebsite.LinkClicked += llWebsite_LinkClicked;
            // 
            // TextBox1
            // 
            TextBox1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            TextBox1.BackColor = System.Drawing.Color.White;
            TextBox1.Location = new System.Drawing.Point(42, 236);
            TextBox1.Multiline = true;
            TextBox1.Name = "TextBox1";
            TextBox1.ReadOnly = true;
            TextBox1.ScrollBars = ScrollBars.Vertical;
            TextBox1.Size = new System.Drawing.Size(679, 306);
            TextBox1.TabIndex = 88;
            TextBox1.Text = resources.GetString("TextBox1.Text");
            // 
            // frmAbout
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(144F, 144F);
            AutoScaleMode = AutoScaleMode.Dpi;
            BackColor = System.Drawing.Color.White;
            ClientSize = new System.Drawing.Size(740, 633);
            Controls.Add(TextBox1);
            Controls.Add(llWebsite);
            Controls.Add(Label4);
            Controls.Add(Label3);
            Controls.Add(Panel2);
            Controls.Add(lblVersion);
            Controls.Add(lblProduct);
            Controls.Add(Panel1);
            Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "frmAbout";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Über Backup Service Home 3";
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