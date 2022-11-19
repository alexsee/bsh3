using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace Brightbits.BSH.Main
{
    public partial class frmEditVersion : Form
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
            var resources = new System.ComponentModel.ComponentResourceManager(typeof(frmEditVersion));
            Panel1 = new Panel();
            Button1 = new Button();
            Label1 = new Label();
            cmdCancel = new Button();
            Label2 = new Label();
            Label3 = new Label();
            txtTitle = new TextBox();
            txtTitle.TextChanged += new EventHandler(txtTitle_TextChanged);
            Label4 = new Label();
            txtDescription = new TextBox();
            txtDescription.TextChanged += new EventHandler(txtDescription_TextChanged);
            Panel1.SuspendLayout();
            SuspendLayout();
            // 
            // Panel1
            // 
            Panel1.BackColor = SystemColors.Control;
            Panel1.Controls.Add(Button1);
            Panel1.Controls.Add(Label1);
            Panel1.Controls.Add(cmdCancel);
            Panel1.Dock = DockStyle.Bottom;
            Panel1.Location = new Point(0, 271);
            Panel1.Name = "Panel1";
            Panel1.Size = new Size(466, 45);
            Panel1.TabIndex = 6;
            // 
            // Button1
            // 
            Button1.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            Button1.DialogResult = DialogResult.OK;
            Button1.Font = new Font("Segoe UI", 9.75f, FontStyle.Regular, GraphicsUnit.Point);
            Button1.Location = new Point(255, 10);
            Button1.Name = "Button1";
            Button1.Size = new Size(93, 26);
            Button1.TabIndex = 0;
            Button1.Text = "&Ändern";
            Button1.UseVisualStyleBackColor = true;
            // 
            // Label1
            // 
            Label1.BackColor = Color.DarkGray;
            Label1.Dock = DockStyle.Top;
            Label1.Location = new Point(0, 0);
            Label1.Name = "Label1";
            Label1.Size = new Size(466, 1);
            Label1.TabIndex = 5;
            // 
            // cmdCancel
            // 
            cmdCancel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            cmdCancel.DialogResult = DialogResult.Cancel;
            cmdCancel.Font = new Font("Segoe UI", 9.75f, FontStyle.Regular, GraphicsUnit.Point);
            cmdCancel.Location = new Point(354, 10);
            cmdCancel.Name = "cmdCancel";
            cmdCancel.Size = new Size(93, 26);
            cmdCancel.TabIndex = 1;
            cmdCancel.Text = "&Abbrechen";
            cmdCancel.UseVisualStyleBackColor = true;
            // 
            // Label2
            // 
            Label2.AutoSize = true;
            Label2.Font = new Font("Segoe UI", 12.0f, FontStyle.Regular, GraphicsUnit.Point);
            Label2.ForeColor = Color.FromArgb(0, 51, 153);
            Label2.Location = new Point(18, 18);
            Label2.Name = "Label2";
            Label2.Size = new Size(140, 21);
            Label2.TabIndex = 0;
            Label2.Text = "Version bearbeiten";
            // 
            // Label3
            // 
            Label3.AutoSize = true;
            Label3.Location = new Point(70, 54);
            Label3.Name = "Label3";
            Label3.Size = new Size(35, 17);
            Label3.TabIndex = 1;
            Label3.Text = "Titel:";
            // 
            // txtTitle
            // 
            txtTitle.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtTitle.Location = new Point(111, 51);
            txtTitle.Name = "txtTitle";
            txtTitle.Size = new Size(337, 25);
            txtTitle.TabIndex = 2;
            // 
            // Label4
            // 
            Label4.AutoSize = true;
            Label4.Location = new Point(16, 83);
            Label4.Name = "Label4";
            Label4.Size = new Size(89, 17);
            Label4.TabIndex = 3;
            Label4.Text = "Beschreibung:";
            // 
            // txtDescription
            // 
            txtDescription.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtDescription.Location = new Point(111, 80);
            txtDescription.Multiline = true;
            txtDescription.Name = "txtDescription";
            txtDescription.Size = new Size(337, 160);
            txtDescription.TabIndex = 4;
            // 
            // frmEditVersion
            // 
            AutoScaleDimensions = new SizeF(96.0f, 96.0f);
            AutoScaleMode = AutoScaleMode.Dpi;
            BackColor = Color.White;
            ClientSize = new Size(466, 316);
            Controls.Add(txtDescription);
            Controls.Add(Label4);
            Controls.Add(txtTitle);
            Controls.Add(Label3);
            Controls.Add(Label2);
            Controls.Add(Panel1);
            Font = new Font("Segoe UI", 9.75f, FontStyle.Regular, GraphicsUnit.Point);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "frmEditVersion";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Version bearbeiten";
            Panel1.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        internal Panel Panel1;
        internal Label Label1;
        internal Button cmdCancel;
        internal Label Label2;
        internal Button Button1;
        internal Label Label3;
        internal TextBox txtTitle;
        internal Label Label4;
        internal TextBox txtDescription;
    }
}