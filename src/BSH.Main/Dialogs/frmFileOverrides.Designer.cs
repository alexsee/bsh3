using System.Diagnostics;
using System.Windows.Forms;

namespace Brightbits.BSH.Main
{
    public partial class frmFileOverrides : Form
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
            var resources = new System.ComponentModel.ComponentResourceManager(typeof(frmFileOverrides));
            Panel1 = new Panel();
            chkAllConflicts = new CheckBox();
            Label1 = new Label();
            cmdCancel = new Button();
            Label2 = new Label();
            lblText = new Label();
            lblOverride = new Label();
            lblOverride2 = new Label();
            Label5 = new Label();
            Label6 = new Label();
            plReplace = new Panel();
            lblFileDateChanged1 = new Label();
            lblFileSize1 = new Label();
            lblFileName1 = new Label();
            picIco1 = new PictureBox();
            plCancel = new Panel();
            lblFileDateChanged2 = new Label();
            lblFileSize2 = new Label();
            lblFileName2 = new Label();
            picIco2 = new PictureBox();
            Panel1.SuspendLayout();
            plReplace.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)picIco1).BeginInit();
            plCancel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)picIco2).BeginInit();
            SuspendLayout();
            // 
            // Panel1
            // 
            resources.ApplyResources(Panel1, "Panel1");
            Panel1.BackColor = System.Drawing.SystemColors.Control;
            Panel1.Controls.Add(chkAllConflicts);
            Panel1.Controls.Add(Label1);
            Panel1.Controls.Add(cmdCancel);
            Panel1.Name = "Panel1";
            // 
            // chkAllConflicts
            // 
            resources.ApplyResources(chkAllConflicts, "chkAllConflicts");
            chkAllConflicts.Name = "chkAllConflicts";
            chkAllConflicts.UseVisualStyleBackColor = true;
            // 
            // Label1
            // 
            resources.ApplyResources(Label1, "Label1");
            Label1.BackColor = System.Drawing.Color.DarkGray;
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
            Label2.ForeColor = System.Drawing.Color.FromArgb(0, 51, 153);
            Label2.Name = "Label2";
            // 
            // lblText
            // 
            resources.ApplyResources(lblText, "lblText");
            lblText.Name = "lblText";
            // 
            // lblOverride
            // 
            resources.ApplyResources(lblOverride, "lblOverride");
            lblOverride.BackColor = System.Drawing.Color.Transparent;
            lblOverride.ForeColor = System.Drawing.Color.FromArgb(0, 51, 153);
            lblOverride.Name = "lblOverride";
            lblOverride.Click += lblOverride_Click;
            // 
            // lblOverride2
            // 
            resources.ApplyResources(lblOverride2, "lblOverride2");
            lblOverride2.BackColor = System.Drawing.Color.Transparent;
            lblOverride2.ForeColor = System.Drawing.Color.Black;
            lblOverride2.Name = "lblOverride2";
            lblOverride2.Click += lblOverride2_Click;
            // 
            // Label5
            // 
            resources.ApplyResources(Label5, "Label5");
            Label5.BackColor = System.Drawing.Color.Transparent;
            Label5.ForeColor = System.Drawing.Color.Black;
            Label5.Name = "Label5";
            Label5.Click += Label5_Click;
            // 
            // Label6
            // 
            resources.ApplyResources(Label6, "Label6");
            Label6.BackColor = System.Drawing.Color.Transparent;
            Label6.ForeColor = System.Drawing.Color.FromArgb(0, 51, 153);
            Label6.Name = "Label6";
            Label6.Click += Label6_Click;
            // 
            // plReplace
            // 
            resources.ApplyResources(plReplace, "plReplace");
            plReplace.Controls.Add(lblFileDateChanged1);
            plReplace.Controls.Add(lblFileSize1);
            plReplace.Controls.Add(lblFileName1);
            plReplace.Controls.Add(picIco1);
            plReplace.Controls.Add(lblOverride);
            plReplace.Controls.Add(lblOverride2);
            plReplace.Name = "plReplace";
            plReplace.MouseClick += plReplace_MouseClick;
            // 
            // lblFileDateChanged1
            // 
            resources.ApplyResources(lblFileDateChanged1, "lblFileDateChanged1");
            lblFileDateChanged1.Name = "lblFileDateChanged1";
            lblFileDateChanged1.Click += lblFileDateChanged1_Click;
            // 
            // lblFileSize1
            // 
            resources.ApplyResources(lblFileSize1, "lblFileSize1");
            lblFileSize1.Name = "lblFileSize1";
            lblFileSize1.Click += lblFileSize1_Click;
            // 
            // lblFileName1
            // 
            resources.ApplyResources(lblFileName1, "lblFileName1");
            lblFileName1.Name = "lblFileName1";
            lblFileName1.Click += lblFileName1_Click;
            // 
            // picIco1
            // 
            resources.ApplyResources(picIco1, "picIco1");
            picIco1.Name = "picIco1";
            picIco1.TabStop = false;
            picIco1.Click += picIco1_Click;
            // 
            // plCancel
            // 
            resources.ApplyResources(plCancel, "plCancel");
            plCancel.Controls.Add(lblFileDateChanged2);
            plCancel.Controls.Add(lblFileSize2);
            plCancel.Controls.Add(lblFileName2);
            plCancel.Controls.Add(picIco2);
            plCancel.Controls.Add(Label6);
            plCancel.Controls.Add(Label5);
            plCancel.Name = "plCancel";
            plCancel.MouseClick += plCancel_MouseClick;
            // 
            // lblFileDateChanged2
            // 
            resources.ApplyResources(lblFileDateChanged2, "lblFileDateChanged2");
            lblFileDateChanged2.Name = "lblFileDateChanged2";
            lblFileDateChanged2.Click += lblFileDateChanged2_Click;
            // 
            // lblFileSize2
            // 
            resources.ApplyResources(lblFileSize2, "lblFileSize2");
            lblFileSize2.Name = "lblFileSize2";
            lblFileSize2.Click += lblFileSize2_Click;
            // 
            // lblFileName2
            // 
            resources.ApplyResources(lblFileName2, "lblFileName2");
            lblFileName2.Name = "lblFileName2";
            lblFileName2.Click += lblFileName2_Click;
            // 
            // picIco2
            // 
            resources.ApplyResources(picIco2, "picIco2");
            picIco2.Name = "picIco2";
            picIco2.TabStop = false;
            picIco2.Click += picIco2_Click;
            // 
            // frmFileOverrides
            // 
            AcceptButton = cmdCancel;
            resources.ApplyResources(this, "$this");
            AutoScaleMode = AutoScaleMode.Dpi;
            BackColor = System.Drawing.Color.White;
            Controls.Add(plCancel);
            Controls.Add(plReplace);
            Controls.Add(lblText);
            Controls.Add(Label2);
            Controls.Add(Panel1);
            ForeColor = System.Drawing.Color.Black;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "frmFileOverrides";
            Shown += frmFileOverrides_Shown;
            Panel1.ResumeLayout(false);
            Panel1.PerformLayout();
            plReplace.ResumeLayout(false);
            plReplace.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)picIco1).EndInit();
            plCancel.ResumeLayout(false);
            plCancel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)picIco2).EndInit();
            ResumeLayout(false);
        }

        internal Panel Panel1;
        internal Label Label1;
        internal Button cmdCancel;
        internal Label Label2;
        internal Label lblText;
        internal Label lblOverride;
        internal Label lblOverride2;
        internal Label Label5;
        internal Label Label6;
        internal Panel plReplace;
        internal Panel plCancel;
        internal Label lblFileDateChanged1;
        internal Label lblFileSize1;
        internal Label lblFileName1;
        internal PictureBox picIco1;
        internal Label lblFileDateChanged2;
        internal Label lblFileSize2;
        internal Label lblFileName2;
        internal PictureBox picIco2;
        internal CheckBox chkAllConflicts;
    }
}