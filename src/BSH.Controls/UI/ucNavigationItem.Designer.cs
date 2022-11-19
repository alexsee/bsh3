using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace Brightbits.BSH.Main
{
    public partial class ucNavigationItem : UserControl
    {

        // UserControl überschreibt den Löschvorgang, um die Komponentenliste zu bereinigen.
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
            Label1.Click += new EventHandler(Label1_Click);
            lblText = new Label();
            lblText.Click += new EventHandler(lblText_Click);
            SuspendLayout();
            // 
            // Label1
            // 
            Label1.Font = new Font("Marlett", 9.75f, FontStyle.Bold, GraphicsUnit.Point);
            Label1.ForeColor = Color.White;
            Label1.Location = new Point(0, 3);
            Label1.Margin = new Padding(4, 0, 4, 0);
            Label1.Name = "Label1";
            Label1.Size = new Size(45, 30);
            Label1.TabIndex = 0;
            Label1.Text = "4";
            Label1.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblText
            // 
            lblText.AutoSize = true;
            lblText.Font = new Font("Segoe UI", 9.75f, FontStyle.Regular, GraphicsUnit.Point);
            lblText.ForeColor = Color.White;
            lblText.Location = new Point(32, 3);
            lblText.Margin = new Padding(4, 0, 4, 0);
            lblText.Name = "lblText";
            lblText.Size = new Size(192, 28);
            lblText.TabIndex = 1;
            lblText.Text = "#TEXT####TEXT###";
            // 
            // ucNavigationItem
            // 
            AutoScaleDimensions = new SizeF(144.0f, 144.0f);
            AutoScaleMode = AutoScaleMode.Dpi;
            BackColor = Color.Transparent;
            Controls.Add(lblText);
            Controls.Add(Label1);
            Cursor = Cursors.Hand;
            Margin = new Padding(0);
            MinimumSize = new Size(0, 30);
            Name = "ucNavigationItem";
            Size = new Size(524, 30);
            Click += new EventHandler(ucNavigationItem_Click);
            ResumeLayout(false);
            PerformLayout();
        }

        internal Label Label1;
        internal Label lblText;
    }
}