using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace Brightbits.BSH.Main
{
    public partial class frmMultiVersionDeletion : Form
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
            var resources = new System.ComponentModel.ComponentResourceManager(typeof(frmMultiVersionDeletion));
            Label2 = new Label();
            Panel1 = new Panel();
            cmdOK = new Button();
            Label1 = new Label();
            cmdCancel = new Button();
            lstVersions = new ListView();
            ColumnHeader1 = new ColumnHeader();
            Panel1.SuspendLayout();
            SuspendLayout();
            // 
            // Label2
            // 
            Label2.AutoSize = true;
            Label2.Font = new Font("Segoe UI", 12.0f, FontStyle.Regular, GraphicsUnit.Point);
            Label2.ForeColor = Color.FromArgb(0, 51, 153);
            Label2.Location = new Point(18, 18);
            Label2.Name = "Label2";
            Label2.Size = new Size(351, 21);
            Label2.TabIndex = 1;
            Label2.Text = "Versionen auswählen, die gelöscht werden sollen.";
            // 
            // Panel1
            // 
            Panel1.BackColor = SystemColors.Control;
            Panel1.Controls.Add(cmdOK);
            Panel1.Controls.Add(Label1);
            Panel1.Controls.Add(cmdCancel);
            Panel1.Dock = DockStyle.Bottom;
            Panel1.Location = new Point(0, 348);
            Panel1.Name = "Panel1";
            Panel1.Size = new Size(611, 45);
            Panel1.TabIndex = 83;
            // 
            // cmdOK
            // 
            cmdOK.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            cmdOK.DialogResult = DialogResult.OK;
            cmdOK.Font = new Font("Segoe UI", 9.75f, FontStyle.Regular, GraphicsUnit.Point);
            cmdOK.Location = new Point(398, 7);
            cmdOK.Name = "cmdOK";
            cmdOK.Size = new Size(93, 26);
            cmdOK.TabIndex = 0;
            cmdOK.Text = "&OK";
            cmdOK.UseVisualStyleBackColor = true;
            // 
            // Label1
            // 
            Label1.BackColor = Color.DarkGray;
            Label1.Dock = DockStyle.Top;
            Label1.Location = new Point(0, 0);
            Label1.Name = "Label1";
            Label1.Size = new Size(611, 1);
            Label1.TabIndex = 5;
            // 
            // cmdCancel
            // 
            cmdCancel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            cmdCancel.DialogResult = DialogResult.Cancel;
            cmdCancel.Font = new Font("Segoe UI", 9.75f, FontStyle.Regular, GraphicsUnit.Point);
            cmdCancel.Location = new Point(497, 7);
            cmdCancel.Name = "cmdCancel";
            cmdCancel.Size = new Size(93, 26);
            cmdCancel.TabIndex = 1;
            cmdCancel.Text = "&Abbrechen";
            cmdCancel.UseVisualStyleBackColor = true;
            // 
            // lstVersions
            // 
            lstVersions.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;

            lstVersions.CheckBoxes = true;
            lstVersions.Columns.AddRange(new ColumnHeader[] { ColumnHeader1 });
            lstVersions.Location = new Point(22, 55);
            lstVersions.Name = "lstVersions";
            lstVersions.Size = new Size(568, 264);
            lstVersions.TabIndex = 6;
            lstVersions.UseCompatibleStateImageBehavior = false;
            lstVersions.View = View.Details;
            // 
            // ColumnHeader1
            // 
            ColumnHeader1.Text = "Sicherungsdatum";
            ColumnHeader1.Width = 300;
            // 
            // frmMultiVersionDeletion
            // 
            AutoScaleDimensions = new SizeF(96.0f, 96.0f);
            AutoScaleMode = AutoScaleMode.Dpi;
            BackColor = Color.White;
            ClientSize = new Size(611, 393);
            Controls.Add(lstVersions);
            Controls.Add(Panel1);
            Controls.Add(Label2);
            Font = new Font("Segoe UI", 9.75f, FontStyle.Regular, GraphicsUnit.Point);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "frmMultiVersionDeletion";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Mehrere Versionen löschen";
            Panel1.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        internal Label Label2;
        internal Panel Panel1;
        internal Button cmdOK;
        internal Label Label1;
        internal Button cmdCancel;
        internal ListView lstVersions;
        internal ColumnHeader ColumnHeader1;
    }
}