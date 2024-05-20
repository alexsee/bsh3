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
            components = new System.ComponentModel.Container();
            var resources = new System.ComponentModel.ComponentResourceManager(typeof(frmCreateBackup));
            Panel1 = new Panel();
            cmdOK = new Button();
            Label1 = new Label();
            cmdCancel = new Button();
            Label2 = new Label();
            Label3 = new Label();
            txtTitle = new TextBox();
            txtDescription = new TextBox();
            Label4 = new Label();
            cbFullBackup = new CheckBox();
            ttMain = new ToolTip(components);
            chkShutdownPC = new CheckBox();
            TabControl1 = new TabControl();
            TabPage1 = new TabPage();
            TabPage2 = new TabPage();
            clstSources = new CheckedListBox();
            Panel1.SuspendLayout();
            TabControl1.SuspendLayout();
            TabPage1.SuspendLayout();
            TabPage2.SuspendLayout();
            SuspendLayout();
            // 
            // Panel1
            // 
            resources.ApplyResources(Panel1, "Panel1");
            Panel1.BackColor = System.Drawing.SystemColors.Control;
            Panel1.Controls.Add(cmdOK);
            Panel1.Controls.Add(Label1);
            Panel1.Controls.Add(cmdCancel);
            Panel1.Name = "Panel1";
            ttMain.SetToolTip(Panel1, resources.GetString("Panel1.ToolTip"));
            // 
            // cmdOK
            // 
            resources.ApplyResources(cmdOK, "cmdOK");
            cmdOK.DialogResult = DialogResult.OK;
            cmdOK.Name = "cmdOK";
            ttMain.SetToolTip(cmdOK, resources.GetString("cmdOK.ToolTip"));
            cmdOK.UseVisualStyleBackColor = true;
            // 
            // Label1
            // 
            resources.ApplyResources(Label1, "Label1");
            Label1.BackColor = System.Drawing.Color.DarkGray;
            Label1.Name = "Label1";
            ttMain.SetToolTip(Label1, resources.GetString("Label1.ToolTip"));
            // 
            // cmdCancel
            // 
            resources.ApplyResources(cmdCancel, "cmdCancel");
            cmdCancel.DialogResult = DialogResult.Cancel;
            cmdCancel.Name = "cmdCancel";
            ttMain.SetToolTip(cmdCancel, resources.GetString("cmdCancel.ToolTip"));
            cmdCancel.UseVisualStyleBackColor = true;
            // 
            // Label2
            // 
            resources.ApplyResources(Label2, "Label2");
            Label2.ForeColor = System.Drawing.Color.FromArgb(0, 51, 153);
            Label2.Name = "Label2";
            ttMain.SetToolTip(Label2, resources.GetString("Label2.ToolTip"));
            // 
            // Label3
            // 
            resources.ApplyResources(Label3, "Label3");
            Label3.Name = "Label3";
            ttMain.SetToolTip(Label3, resources.GetString("Label3.ToolTip"));
            // 
            // txtTitle
            // 
            resources.ApplyResources(txtTitle, "txtTitle");
            txtTitle.Name = "txtTitle";
            ttMain.SetToolTip(txtTitle, resources.GetString("txtTitle.ToolTip"));
            // 
            // txtDescription
            // 
            resources.ApplyResources(txtDescription, "txtDescription");
            txtDescription.Name = "txtDescription";
            ttMain.SetToolTip(txtDescription, resources.GetString("txtDescription.ToolTip"));
            // 
            // Label4
            // 
            resources.ApplyResources(Label4, "Label4");
            Label4.Name = "Label4";
            ttMain.SetToolTip(Label4, resources.GetString("Label4.ToolTip"));
            // 
            // cbFullBackup
            // 
            resources.ApplyResources(cbFullBackup, "cbFullBackup");
            cbFullBackup.Name = "cbFullBackup";
            ttMain.SetToolTip(cbFullBackup, resources.GetString("cbFullBackup.ToolTip"));
            cbFullBackup.UseVisualStyleBackColor = true;
            // 
            // ttMain
            // 
            ttMain.ToolTipIcon = ToolTipIcon.Info;
            ttMain.ToolTipTitle = "Quickinfo";
            // 
            // chkShutdownPC
            // 
            resources.ApplyResources(chkShutdownPC, "chkShutdownPC");
            chkShutdownPC.Name = "chkShutdownPC";
            ttMain.SetToolTip(chkShutdownPC, resources.GetString("chkShutdownPC.ToolTip"));
            chkShutdownPC.UseVisualStyleBackColor = true;
            // 
            // TabControl1
            // 
            resources.ApplyResources(TabControl1, "TabControl1");
            TabControl1.Controls.Add(TabPage1);
            TabControl1.Controls.Add(TabPage2);
            TabControl1.Name = "TabControl1";
            TabControl1.SelectedIndex = 0;
            ttMain.SetToolTip(TabControl1, resources.GetString("TabControl1.ToolTip"));
            // 
            // TabPage1
            // 
            resources.ApplyResources(TabPage1, "TabPage1");
            TabPage1.Controls.Add(txtTitle);
            TabPage1.Controls.Add(chkShutdownPC);
            TabPage1.Controls.Add(Label3);
            TabPage1.Controls.Add(cbFullBackup);
            TabPage1.Controls.Add(txtDescription);
            TabPage1.Controls.Add(Label4);
            TabPage1.Name = "TabPage1";
            ttMain.SetToolTip(TabPage1, resources.GetString("TabPage1.ToolTip"));
            TabPage1.UseVisualStyleBackColor = true;
            // 
            // TabPage2
            // 
            resources.ApplyResources(TabPage2, "TabPage2");
            TabPage2.Controls.Add(clstSources);
            TabPage2.Name = "TabPage2";
            ttMain.SetToolTip(TabPage2, resources.GetString("TabPage2.ToolTip"));
            TabPage2.UseVisualStyleBackColor = true;
            // 
            // clstSources
            // 
            resources.ApplyResources(clstSources, "clstSources");
            clstSources.FormattingEnabled = true;
            clstSources.Name = "clstSources";
            ttMain.SetToolTip(clstSources, resources.GetString("clstSources.ToolTip"));
            // 
            // frmCreateBackup
            // 
            resources.ApplyResources(this, "$this");
            AutoScaleMode = AutoScaleMode.Dpi;
            BackColor = System.Drawing.Color.White;
            Controls.Add(TabControl1);
            Controls.Add(Label2);
            Controls.Add(Panel1);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "frmCreateBackup";
            ttMain.SetToolTip(this, resources.GetString("$this.ToolTip"));
            Load += frmCreateBackup_Load;
            Panel1.ResumeLayout(false);
            TabControl1.ResumeLayout(false);
            TabPage1.ResumeLayout(false);
            TabPage1.PerformLayout();
            TabPage2.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
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