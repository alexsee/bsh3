using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace Brightbits.BSH.Main
{
    public partial class frmShortStatus : Form
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
            lblStatus = new Label();
            pbarStatus = new ProgressBar();
            SuspendLayout();
            // 
            // lblStatus
            // 
            lblStatus.AutoSize = true;
            lblStatus.Font = new Font("Segoe UI", 9.75f, FontStyle.Regular, GraphicsUnit.Point);
            lblStatus.Location = new Point(12, 14);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(82, 15);
            lblStatus.TabIndex = 0;
            lblStatus.Text = "Bitte warten...";
            // 
            // pbarStatus
            // 
            pbarStatus.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            pbarStatus.Location = new Point(12, 40);
            pbarStatus.Name = "pbarStatus";
            pbarStatus.Size = new Size(388, 18);
            pbarStatus.Style = ProgressBarStyle.Marquee;
            pbarStatus.TabIndex = 1;
            // 
            // frmShortStatus
            // 
            AutoScaleDimensions = new SizeF(96.0f, 96.0f);
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(412, 70);
            Controls.Add(pbarStatus);
            Controls.Add(lblStatus);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "frmShortStatus";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Backup Service Home";
            ResumeLayout(false);
            PerformLayout();
        }

        internal Label lblStatus;
        internal ProgressBar pbarStatus;
    }
}