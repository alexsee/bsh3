using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace Brightbits.BSH.Main
{
    public partial class frmWaitForMedia : Form
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
            Label1 = new Label();
            Panel1 = new Panel();
            Label2 = new Label();
            cmdCancel = new Button();
            cmdCancel.Click += new EventHandler(cmdCancel_Click);
            lblText = new Label();
            LoadingCircle1 = new MRG.Controls.UI.LoadingCircle();
            Panel1.SuspendLayout();
            SuspendLayout();
            // 
            // Label1
            // 
            Label1.AutoSize = true;
            Label1.Font = new Font("Segoe UI", 9.0f, FontStyle.Regular, GraphicsUnit.Point);
            Label1.Location = new Point(98, 72);
            Label1.Margin = new Padding(4, 0, 4, 0);
            Label1.Name = "Label1";
            Label1.Size = new Size(526, 125);
            Label1.TabIndex = 1;
            Label1.Text = "Backup Service Home 3 wartet auf das Datensicherungsmedium, " + '\r' + '\n' + "um die geplanete Ak" + "tion fortzusetzen. Schließen Sie nun dieses " + '\r' + '\n' + "an Ihren Computer an." + '\r' + '\n' + '\r' + '\n' + "Das Mediu" + "m wird automatisch erkannt.";
            // 
            // Panel1
            // 
            Panel1.BackColor = SystemColors.Control;
            Panel1.Controls.Add(Label2);
            Panel1.Controls.Add(cmdCancel);
            Panel1.Dock = DockStyle.Bottom;
            Panel1.Location = new Point(0, 226);
            Panel1.Margin = new Padding(4);
            Panel1.Name = "Panel1";
            Panel1.Size = new Size(658, 68);
            Panel1.TabIndex = 6;
            // 
            // Label2
            // 
            Label2.BackColor = Color.DarkGray;
            Label2.Dock = DockStyle.Top;
            Label2.Location = new Point(0, 0);
            Label2.Margin = new Padding(4, 0, 4, 0);
            Label2.Name = "Label2";
            Label2.Size = new Size(658, 2);
            Label2.TabIndex = 5;
            // 
            // cmdCancel
            // 
            cmdCancel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            cmdCancel.DialogResult = DialogResult.Cancel;
            cmdCancel.Font = new Font("Segoe UI", 9.0f, FontStyle.Regular, GraphicsUnit.Point);
            cmdCancel.Location = new Point(490, 15);
            cmdCancel.Margin = new Padding(4);
            cmdCancel.Name = "cmdCancel";
            cmdCancel.Size = new Size(140, 39);
            cmdCancel.TabIndex = 4;
            cmdCancel.Text = "&Abbrechen";
            cmdCancel.UseVisualStyleBackColor = true;
            // 
            // lblText
            // 
            lblText.AutoSize = true;
            lblText.Font = new Font("Segoe UI", 12.0f, FontStyle.Regular, GraphicsUnit.Point);
            lblText.ForeColor = Color.FromArgb(0, 51, 153);
            lblText.Location = new Point(96, 21);
            lblText.Margin = new Padding(4, 0, 4, 0);
            lblText.Name = "lblText";
            lblText.Size = new Size(472, 32);
            lblText.TabIndex = 7;
            lblText.Text = "Backup Service Home wartet auf Medium...";
            // 
            // LoadingCircle1
            // 
            LoadingCircle1.Active = true;
            LoadingCircle1.Color = Color.DarkGray;
            LoadingCircle1.InnerCircleRadius = 8;
            LoadingCircle1.Location = new Point(32, 21);
            LoadingCircle1.Margin = new Padding(4);
            LoadingCircle1.Name = "LoadingCircle1";
            LoadingCircle1.NumberSpoke = 24;
            LoadingCircle1.OuterCircleRadius = 9;
            LoadingCircle1.RotationSpeed = 50;
            LoadingCircle1.Size = new Size(48, 44);
            LoadingCircle1.SpokeThickness = 4;
            LoadingCircle1.StylePreset = MRG.Controls.UI.LoadingCircle.StylePresets.IE7;
            LoadingCircle1.TabIndex = 0;
            LoadingCircle1.Text = "LoadingCircle1";
            // 
            // frmWaitForMedia
            // 
            AutoScaleDimensions = new SizeF(144.0f, 144.0f);
            AutoScaleMode = AutoScaleMode.Dpi;
            BackColor = Color.White;
            ClientSize = new Size(658, 294);
            ControlBox = false;
            Controls.Add(lblText);
            Controls.Add(Panel1);
            Controls.Add(Label1);
            Controls.Add(LoadingCircle1);
            Font = new Font("Segoe UI", 9.0f, FontStyle.Regular, GraphicsUnit.Point);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Margin = new Padding(4);
            Name = "frmWaitForMedia";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Backup Service Home wartet auf Medium...";
            Panel1.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        internal MRG.Controls.UI.LoadingCircle LoadingCircle1;
        internal Label Label1;
        internal Panel Panel1;
        internal Label Label2;
        internal Button cmdCancel;
        internal Label lblText;
    }
}