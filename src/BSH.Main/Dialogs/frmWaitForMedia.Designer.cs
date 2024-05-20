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
            var resources = new System.ComponentModel.ComponentResourceManager(typeof(frmWaitForMedia));
            Label1 = new Label();
            Panel1 = new Panel();
            Label2 = new Label();
            cmdCancel = new Button();
            lblText = new Label();
            LoadingCircle1 = new MRG.Controls.UI.LoadingCircle();
            Panel1.SuspendLayout();
            SuspendLayout();
            // 
            // Label1
            // 
            resources.ApplyResources(Label1, "Label1");
            Label1.Name = "Label1";
            // 
            // Panel1
            // 
            resources.ApplyResources(Panel1, "Panel1");
            Panel1.BackColor = SystemColors.Control;
            Panel1.Controls.Add(Label2);
            Panel1.Controls.Add(cmdCancel);
            Panel1.Name = "Panel1";
            // 
            // Label2
            // 
            resources.ApplyResources(Label2, "Label2");
            Label2.BackColor = Color.DarkGray;
            Label2.Name = "Label2";
            // 
            // cmdCancel
            // 
            resources.ApplyResources(cmdCancel, "cmdCancel");
            cmdCancel.DialogResult = DialogResult.Cancel;
            cmdCancel.Name = "cmdCancel";
            cmdCancel.UseVisualStyleBackColor = true;
            cmdCancel.Click += cmdCancel_Click;
            // 
            // lblText
            // 
            resources.ApplyResources(lblText, "lblText");
            lblText.ForeColor = Color.FromArgb(0, 51, 153);
            lblText.Name = "lblText";
            // 
            // LoadingCircle1
            // 
            resources.ApplyResources(LoadingCircle1, "LoadingCircle1");
            LoadingCircle1.Active = true;
            LoadingCircle1.Color = Color.DarkGray;
            LoadingCircle1.InnerCircleRadius = 8;
            LoadingCircle1.Name = "LoadingCircle1";
            LoadingCircle1.NumberSpoke = 24;
            LoadingCircle1.OuterCircleRadius = 9;
            LoadingCircle1.RotationSpeed = 50;
            LoadingCircle1.SpokeThickness = 4;
            LoadingCircle1.StylePreset = MRG.Controls.UI.LoadingCircle.StylePresets.IE7;
            // 
            // frmWaitForMedia
            // 
            resources.ApplyResources(this, "$this");
            AutoScaleMode = AutoScaleMode.Dpi;
            BackColor = Color.White;
            ControlBox = false;
            Controls.Add(lblText);
            Controls.Add(Panel1);
            Controls.Add(Label1);
            Controls.Add(LoadingCircle1);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Name = "frmWaitForMedia";
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