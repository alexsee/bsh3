using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace Brightbits.BSH.Main
{
    public partial class aVersionList : UserControl
    {

        // UserControl1 überschreibt den Löschvorgang, um die Komponentenliste zu bereinigen.
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
            plMain = new Panel();
            flpMain = new FlowLayoutPanel();
            flpMain.Resize += new EventHandler(flpMain_Resize);
            plMain.SuspendLayout();
            SuspendLayout();
            // 
            // plMain
            // 
            plMain.AutoScroll = true;
            plMain.BackColor = Color.Transparent;
            plMain.BackgroundImageLayout = ImageLayout.Stretch;
            plMain.Controls.Add(flpMain);
            plMain.Dock = DockStyle.Fill;
            plMain.Location = new Point(0, 0);
            plMain.Margin = new Padding(0);
            plMain.Name = "plMain";
            plMain.Size = new Size(397, 287);
            plMain.TabIndex = 2;
            // 
            // flpMain
            // 
            flpMain.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            flpMain.BackColor = Color.Transparent;
            flpMain.FlowDirection = FlowDirection.TopDown;
            flpMain.Location = new Point(0, 0);
            flpMain.Margin = new Padding(3, 0, 3, 0);
            flpMain.Name = "flpMain";
            flpMain.Size = new Size(397, 127);
            flpMain.TabIndex = 2;
            // 
            // aVersionList
            // 
            AutoScaleMode = AutoScaleMode.None;
            BackColor = Color.White;
            Controls.Add(plMain);
            Name = "aVersionList";
            Size = new Size(397, 287);
            plMain.ResumeLayout(false);
            Load += new EventHandler(aVersionList_Load);
            ResumeLayout(false);
        }

        internal Panel plMain;
        internal FlowLayoutPanel flpMain;
    }
}