using System.Diagnostics;
using System.Windows.Forms;

namespace Brightbits.BSH.Main
{
    public partial class frmCreateBackup : Form
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmCreateBackup));
            this.Panel1 = new System.Windows.Forms.Panel();
            this.cmdOK = new System.Windows.Forms.Button();
            this.Label1 = new System.Windows.Forms.Label();
            this.cmdCancel = new System.Windows.Forms.Button();
            this.Label2 = new System.Windows.Forms.Label();
            this.Label3 = new System.Windows.Forms.Label();
            this.txtTitle = new System.Windows.Forms.TextBox();
            this.txtDescription = new System.Windows.Forms.TextBox();
            this.Label4 = new System.Windows.Forms.Label();
            this.cbFullBackup = new System.Windows.Forms.CheckBox();
            this.ttMain = new System.Windows.Forms.ToolTip(this.components);
            this.chkShutdownPC = new System.Windows.Forms.CheckBox();
            this.TabControl1 = new System.Windows.Forms.TabControl();
            this.TabPage1 = new System.Windows.Forms.TabPage();
            this.TabPage2 = new System.Windows.Forms.TabPage();
            this.clstSources = new System.Windows.Forms.CheckedListBox();
            this.Panel1.SuspendLayout();
            this.TabControl1.SuspendLayout();
            this.TabPage1.SuspendLayout();
            this.TabPage2.SuspendLayout();
            this.SuspendLayout();
            // 
            // Panel1
            // 
            this.Panel1.BackColor = System.Drawing.SystemColors.Control;
            this.Panel1.Controls.Add(this.cmdOK);
            this.Panel1.Controls.Add(this.Label1);
            this.Panel1.Controls.Add(this.cmdCancel);
            this.Panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.Panel1.Location = new System.Drawing.Point(0, 524);
            this.Panel1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Panel1.Name = "Panel1";
            this.Panel1.Size = new System.Drawing.Size(825, 68);
            this.Panel1.TabIndex = 6;
            // 
            // cmdOK
            // 
            this.cmdOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.cmdOK.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmdOK.Location = new System.Drawing.Point(508, 15);
            this.cmdOK.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.cmdOK.Name = "cmdOK";
            this.cmdOK.Size = new System.Drawing.Size(140, 39);
            this.cmdOK.TabIndex = 0;
            this.cmdOK.Text = "&OK";
            this.cmdOK.UseVisualStyleBackColor = true;
            // 
            // Label1
            // 
            this.Label1.BackColor = System.Drawing.Color.DarkGray;
            this.Label1.Dock = System.Windows.Forms.DockStyle.Top;
            this.Label1.Location = new System.Drawing.Point(0, 0);
            this.Label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.Label1.Name = "Label1";
            this.Label1.Size = new System.Drawing.Size(825, 2);
            this.Label1.TabIndex = 5;
            // 
            // cmdCancel
            // 
            this.cmdCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cmdCancel.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmdCancel.Location = new System.Drawing.Point(657, 15);
            this.cmdCancel.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.cmdCancel.Name = "cmdCancel";
            this.cmdCancel.Size = new System.Drawing.Size(140, 39);
            this.cmdCancel.TabIndex = 1;
            this.cmdCancel.Text = "&Abbrechen";
            this.cmdCancel.UseVisualStyleBackColor = true;
            // 
            // Label2
            // 
            this.Label2.AutoSize = true;
            this.Label2.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Label2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(153)))));
            this.Label2.Location = new System.Drawing.Point(27, 27);
            this.Label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.Label2.Name = "Label2";
            this.Label2.Size = new System.Drawing.Size(280, 32);
            this.Label2.TabIndex = 0;
            this.Label2.Text = "Datensicherung erstellen";
            // 
            // Label3
            // 
            this.Label3.AutoSize = true;
            this.Label3.Location = new System.Drawing.Point(106, 30);
            this.Label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.Label3.Name = "Label3";
            this.Label3.Size = new System.Drawing.Size(53, 28);
            this.Label3.TabIndex = 7;
            this.Label3.Text = "Titel:";
            // 
            // txtTitle
            // 
            this.txtTitle.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtTitle.Location = new System.Drawing.Point(168, 26);
            this.txtTitle.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.txtTitle.Name = "txtTitle";
            this.txtTitle.Size = new System.Drawing.Size(564, 33);
            this.txtTitle.TabIndex = 8;
            // 
            // txtDescription
            // 
            this.txtDescription.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtDescription.Location = new System.Drawing.Point(168, 69);
            this.txtDescription.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.txtDescription.Multiline = true;
            this.txtDescription.Name = "txtDescription";
            this.txtDescription.Size = new System.Drawing.Size(564, 158);
            this.txtDescription.TabIndex = 10;
            // 
            // Label4
            // 
            this.Label4.AutoSize = true;
            this.Label4.Location = new System.Drawing.Point(26, 74);
            this.Label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.Label4.Name = "Label4";
            this.Label4.Size = new System.Drawing.Size(133, 28);
            this.Label4.TabIndex = 9;
            this.Label4.Text = "Beschreibung:";
            // 
            // cbFullBackup
            // 
            this.cbFullBackup.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.cbFullBackup.AutoSize = true;
            this.cbFullBackup.Location = new System.Drawing.Point(168, 252);
            this.cbFullBackup.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.cbFullBackup.Name = "cbFullBackup";
            this.cbFullBackup.Size = new System.Drawing.Size(229, 32);
            this.cbFullBackup.TabIndex = 12;
            this.cbFullBackup.Text = "Vollsicherung anlegen";
            this.ttMain.SetToolTip(this.cbFullBackup, resources.GetString("cbFullBackup.ToolTip"));
            this.cbFullBackup.UseVisualStyleBackColor = true;
            // 
            // ttMain
            // 
            this.ttMain.ToolTipIcon = System.Windows.Forms.ToolTipIcon.Info;
            this.ttMain.ToolTipTitle = "Quickinfo";
            // 
            // chkShutdownPC
            // 
            this.chkShutdownPC.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.chkShutdownPC.AutoSize = true;
            this.chkShutdownPC.Location = new System.Drawing.Point(168, 292);
            this.chkShutdownPC.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.chkShutdownPC.Name = "chkShutdownPC";
            this.chkShutdownPC.Size = new System.Drawing.Size(431, 32);
            this.chkShutdownPC.TabIndex = 13;
            this.chkShutdownPC.Text = "Computer nach Fertigstellung herunterfahren";
            this.chkShutdownPC.UseVisualStyleBackColor = true;
            // 
            // TabControl1
            // 
            this.TabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.TabControl1.Controls.Add(this.TabPage1);
            this.TabControl1.Controls.Add(this.TabPage2);
            this.TabControl1.Location = new System.Drawing.Point(33, 86);
            this.TabControl1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.TabControl1.Name = "TabControl1";
            this.TabControl1.SelectedIndex = 0;
            this.TabControl1.Size = new System.Drawing.Size(778, 406);
            this.TabControl1.TabIndex = 14;
            // 
            // TabPage1
            // 
            this.TabPage1.Controls.Add(this.txtTitle);
            this.TabPage1.Controls.Add(this.chkShutdownPC);
            this.TabPage1.Controls.Add(this.Label3);
            this.TabPage1.Controls.Add(this.cbFullBackup);
            this.TabPage1.Controls.Add(this.txtDescription);
            this.TabPage1.Controls.Add(this.Label4);
            this.TabPage1.Location = new System.Drawing.Point(4, 37);
            this.TabPage1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.TabPage1.Name = "TabPage1";
            this.TabPage1.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.TabPage1.Size = new System.Drawing.Size(770, 365);
            this.TabPage1.TabIndex = 0;
            this.TabPage1.Text = "Eigenschaften";
            this.TabPage1.UseVisualStyleBackColor = true;
            // 
            // TabPage2
            // 
            this.TabPage2.Controls.Add(this.clstSources);
            this.TabPage2.Location = new System.Drawing.Point(4, 37);
            this.TabPage2.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.TabPage2.Name = "TabPage2";
            this.TabPage2.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.TabPage2.Size = new System.Drawing.Size(770, 365);
            this.TabPage2.TabIndex = 1;
            this.TabPage2.Text = "Quellordnerauswahl";
            this.TabPage2.UseVisualStyleBackColor = true;
            // 
            // clstSources
            // 
            this.clstSources.Dock = System.Windows.Forms.DockStyle.Fill;
            this.clstSources.FormattingEnabled = true;
            this.clstSources.HorizontalScrollbar = true;
            this.clstSources.IntegralHeight = false;
            this.clstSources.Location = new System.Drawing.Point(4, 4);
            this.clstSources.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.clstSources.Name = "clstSources";
            this.clstSources.Size = new System.Drawing.Size(762, 357);
            this.clstSources.TabIndex = 0;
            // 
            // frmCreateBackup
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(144F, 144F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(825, 592);
            this.Controls.Add(this.TabControl1);
            this.Controls.Add(this.Label2);
            this.Controls.Add(this.Panel1);
            this.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmCreateBackup";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Datensicherung erstellen";
            this.Load += new System.EventHandler(this.frmCreateBackup_Load);
            this.Panel1.ResumeLayout(false);
            this.TabControl1.ResumeLayout(false);
            this.TabPage1.ResumeLayout(false);
            this.TabPage1.PerformLayout();
            this.TabPage2.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        internal Panel Panel1;
        internal Label Label1;
        internal Button cmdCancel;
        internal Label Label2;
        internal Button cmdOK;
        internal Label Label3;
        internal TextBox txtTitle;
        internal TextBox txtDescription;
        internal Label Label4;
        internal CheckBox cbFullBackup;
        internal ToolTip ttMain;
        internal CheckBox chkShutdownPC;
        internal TabControl TabControl1;
        internal TabPage TabPage1;
        internal TabPage TabPage2;
        internal CheckedListBox clstSources;
    }
}