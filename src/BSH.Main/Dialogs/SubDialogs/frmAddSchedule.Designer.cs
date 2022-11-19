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
            cbIntervall.SelectedIndexChanged += new EventHandler(cbIntervall_SelectedIndexChanged);
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
            dtpStartTime.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            dtpStartTime.CustomFormat = "dd.MM.yyyy HH:mm:ss";
            dtpStartTime.Enabled = false;
            dtpStartTime.Font = new Font("Segoe UI", 9.75f, FontStyle.Regular, GraphicsUnit.Point);
            dtpStartTime.Format = DateTimePickerFormat.Custom;
            dtpStartTime.Location = new Point(112, 94);
            dtpStartTime.Name = "dtpStartTime";
            dtpStartTime.Size = new Size(389, 25);
            dtpStartTime.TabIndex = 16;
            // 
            // Label3
            // 
            Label3.AutoSize = true;
            Label3.Font = new Font("Segoe UI", 9.75f, FontStyle.Regular, GraphicsUnit.Point);
            Label3.Location = new Point(19, 100);
            Label3.Name = "Label3";
            Label3.Size = new Size(58, 17);
            Label3.TabIndex = 19;
            Label3.Text = "Startzeit:";
            // 
            // cbIntervall
            // 
            cbIntervall.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            cbIntervall.DropDownStyle = ComboBoxStyle.DropDownList;
            cbIntervall.Font = new Font("Segoe UI", 9.75f, FontStyle.Regular, GraphicsUnit.Point);
            cbIntervall.FormattingEnabled = true;
            cbIntervall.Items.AddRange(new object[] { "Einmalig", "Stündlich", "Täglich", "Wöchentlich", "Monatlich" });
            cbIntervall.Location = new Point(112, 54);
            cbIntervall.Name = "cbIntervall";
            cbIntervall.Size = new Size(388, 25);
            cbIntervall.TabIndex = 17;
            // 
            // Label4
            // 
            Label4.AutoSize = true;
            Label4.Font = new Font("Segoe UI", 9.75f, FontStyle.Regular, GraphicsUnit.Point);
            Label4.Location = new Point(19, 57);
            Label4.Name = "Label4";
            Label4.Size = new Size(56, 17);
            Label4.TabIndex = 18;
            Label4.Text = "Intervall:";
            // 
            // Label2
            // 
            Label2.AutoSize = true;
            Label2.Font = new Font("Segoe UI", 12.0f, FontStyle.Regular, GraphicsUnit.Point);
            Label2.ForeColor = Color.FromArgb(0, 51, 153);
            Label2.Location = new Point(18, 18);
            Label2.Name = "Label2";
            Label2.Size = new Size(139, 21);
            Label2.TabIndex = 0;
            Label2.Text = "Termin hinzufügen";
            // 
            // cmdOK
            // 
            cmdOK.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            cmdOK.DialogResult = DialogResult.OK;
            cmdOK.Font = new Font("Segoe UI", 9.75f, FontStyle.Regular, GraphicsUnit.Point);
            cmdOK.Location = new Point(310, 10);
            cmdOK.Name = "cmdOK";
            cmdOK.Size = new Size(93, 26);
            cmdOK.TabIndex = 0;
            cmdOK.Text = "&OK";
            cmdOK.UseVisualStyleBackColor = true;
            // 
            // Panel1
            // 
            Panel1.BackColor = SystemColors.Control;
            Panel1.Controls.Add(cmdOK);
            Panel1.Controls.Add(Label1);
            Panel1.Controls.Add(cmdCancel);
            Panel1.Dock = DockStyle.Bottom;
            Panel1.Location = new Point(0, 154);
            Panel1.Name = "Panel1";
            Panel1.Size = new Size(521, 45);
            Panel1.TabIndex = 82;
            // 
            // Label1
            // 
            Label1.BackColor = Color.DarkGray;
            Label1.Dock = DockStyle.Top;
            Label1.Location = new Point(0, 0);
            Label1.Name = "Label1";
            Label1.Size = new Size(521, 1);
            Label1.TabIndex = 5;
            // 
            // cmdCancel
            // 
            cmdCancel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            cmdCancel.DialogResult = DialogResult.Cancel;
            cmdCancel.Font = new Font("Segoe UI", 9.75f, FontStyle.Regular, GraphicsUnit.Point);
            cmdCancel.Location = new Point(409, 10);
            cmdCancel.Name = "cmdCancel";
            cmdCancel.Size = new Size(93, 26);
            cmdCancel.TabIndex = 1;
            cmdCancel.Text = "&Abbrechen";
            cmdCancel.UseVisualStyleBackColor = true;
            // 
            // frmAddSchedule
            // 
            AutoScaleDimensions = new SizeF(96.0f, 96.0f);
            AutoScaleMode = AutoScaleMode.Dpi;
            BackColor = Color.White;
            ClientSize = new Size(521, 199);
            Controls.Add(dtpStartTime);
            Controls.Add(Label3);
            Controls.Add(Panel1);
            Controls.Add(cbIntervall);
            Controls.Add(Label2);
            Controls.Add(Label4);
            Font = new Font("Segoe UI", 9.75f, FontStyle.Regular, GraphicsUnit.Point);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "frmAddSchedule";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Termin hinzufügen";
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