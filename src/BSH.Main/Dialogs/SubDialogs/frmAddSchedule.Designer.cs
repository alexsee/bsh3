using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace Brightbits.BSH.Main
{
    public partial class frmAddSchedule : Form
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
            var resources = new System.ComponentModel.ComponentResourceManager(typeof(frmAddSchedule));
            dtpStartTime = new DateTimePicker();
            Label3 = new Label();
            cbIntervall = new ComboBox();
            Label4 = new Label();
            Label2 = new Label();
            cmdOK = new Button();
            Panel1 = new Panel();
            Label1 = new Label();
            cmdCancel = new Button();
            Panel1.SuspendLayout();
            SuspendLayout();
            // 
            // dtpStartTime
            // 
            resources.ApplyResources(dtpStartTime, "dtpStartTime");
            dtpStartTime.Format = DateTimePickerFormat.Custom;
            dtpStartTime.Name = "dtpStartTime";
            // 
            // Label3
            // 
            resources.ApplyResources(Label3, "Label3");
            Label3.Name = "Label3";
            // 
            // cbIntervall
            // 
            resources.ApplyResources(cbIntervall, "cbIntervall");
            cbIntervall.DropDownStyle = ComboBoxStyle.DropDownList;
            cbIntervall.FormattingEnabled = true;
            cbIntervall.Items.AddRange(new object[] { resources.GetString("cbIntervall.Items"), resources.GetString("cbIntervall.Items1"), resources.GetString("cbIntervall.Items2"), resources.GetString("cbIntervall.Items3"), resources.GetString("cbIntervall.Items4") });
            cbIntervall.Name = "cbIntervall";
            cbIntervall.SelectedIndexChanged += cbIntervall_SelectedIndexChanged;
            // 
            // Label4
            // 
            resources.ApplyResources(Label4, "Label4");
            Label4.Name = "Label4";
            // 
            // Label2
            // 
            resources.ApplyResources(Label2, "Label2");
            Label2.ForeColor = Color.FromArgb(0, 51, 153);
            Label2.Name = "Label2";
            // 
            // cmdOK
            // 
            resources.ApplyResources(cmdOK, "cmdOK");
            cmdOK.DialogResult = DialogResult.OK;
            cmdOK.Name = "cmdOK";
            cmdOK.UseVisualStyleBackColor = true;
            // 
            // Panel1
            // 
            resources.ApplyResources(Panel1, "Panel1");
            Panel1.BackColor = SystemColors.Control;
            Panel1.Controls.Add(cmdOK);
            Panel1.Controls.Add(Label1);
            Panel1.Controls.Add(cmdCancel);
            Panel1.Name = "Panel1";
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
            // frmAddSchedule
            // 
            resources.ApplyResources(this, "$this");
            AutoScaleMode = AutoScaleMode.Dpi;
            BackColor = Color.White;
            Controls.Add(dtpStartTime);
            Controls.Add(Label3);
            Controls.Add(Panel1);
            Controls.Add(cbIntervall);
            Controls.Add(Label2);
            Controls.Add(Label4);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "frmAddSchedule";
            Panel1.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        internal DateTimePicker dtpStartTime;
        internal Label Label3;
        internal ComboBox cbIntervall;
        internal Label Label4;
        internal Label Label2;
        internal Button cmdOK;
        internal Panel Panel1;
        internal Label Label1;
        internal Button cmdCancel;
    }
}