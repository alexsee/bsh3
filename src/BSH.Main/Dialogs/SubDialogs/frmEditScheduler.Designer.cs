using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace Brightbits.BSH.Main
{
    public partial class frmEditScheduler : Form
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
            var resources = new System.ComponentModel.ComponentResourceManager(typeof(frmEditScheduler));
            Panel1 = new Panel();
            cmdOK = new Button();
            cmdOK.Click += new EventHandler(cmdOK_Click);
            Label1 = new Label();
            cmdCancel = new Button();
            Label2 = new Label();
            Panel3 = new Panel();
            TabControl1 = new TabControl();
            TabPage1 = new TabPage();
            Label4 = new Label();
            cboFullBackup = new ComboBox();
            nudFullBackup = new NumericUpDown();
            chkFullBackup = new CheckBox();
            chkFullBackup.CheckedChanged += new EventHandler(chkFullBackup_CheckedChanged);
            lwTimeSchedule = new ListView();
            lwTimeSchedule.SelectedIndexChanged += new EventHandler(lwTimeSchedule_SelectedIndexChanged);
            ColumnHeader10 = new ColumnHeader();
            ColumnHeader11 = new ColumnHeader();
            cmdAddTime = new Button();
            cmdAddTime.Click += new EventHandler(cmdAddTime_Click);
            cmdDeleteTime = new Button();
            cmdDeleteTime.Click += new EventHandler(cmdDeleteTime_Click);
            TabPage2 = new TabPage();
            Label5 = new Label();
            Label3 = new Label();
            NumericUpDown1 = new NumericUpDown();
            CheckBox2 = new CheckBox();
            plDelete = new Panel();
            cboIntervall = new ComboBox();
            txtIntervall = new TextBox();
            nudIntervallHourBackups = new NumericUpDown();
            rdDeleteIntervall = new RadioButton();
            rdDeleteIntervall.CheckedChanged += new EventHandler(rdDeleteIntervall_CheckedChanged);
            rdDontDelete = new RadioButton();
            rdDeleteAuto = new RadioButton();
            rdDeleteAuto.CheckedChanged += new EventHandler(rdDeleteAuto_CheckedChanged);
            Panel1.SuspendLayout();
            Panel3.SuspendLayout();
            TabControl1.SuspendLayout();
            TabPage1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)nudFullBackup).BeginInit();
            TabPage2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)NumericUpDown1).BeginInit();
            plDelete.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)nudIntervallHourBackups).BeginInit();
            SuspendLayout();
            // 
            // Panel1
            // 
            Panel1.BackColor = SystemColors.Control;
            Panel1.Controls.Add(cmdOK);
            Panel1.Controls.Add(Label1);
            Panel1.Controls.Add(cmdCancel);
            Panel1.Dock = DockStyle.Bottom;
            Panel1.Location = new Point(0, 439);
            Panel1.Name = "Panel1";
            Panel1.Size = new Size(576, 45);
            Panel1.TabIndex = 6;
            // 
            // cmdOK
            // 
            cmdOK.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            cmdOK.Font = new Font("Segoe UI", 9.75f, FontStyle.Regular, GraphicsUnit.Point);
            cmdOK.Location = new Point(365, 10);
            cmdOK.Name = "cmdOK";
            cmdOK.Size = new Size(93, 26);
            cmdOK.TabIndex = 0;
            cmdOK.Text = "&Übernehmen";
            cmdOK.UseVisualStyleBackColor = true;
            // 
            // Label1
            // 
            Label1.BackColor = Color.DarkGray;
            Label1.Dock = DockStyle.Top;
            Label1.Location = new Point(0, 0);
            Label1.Name = "Label1";
            Label1.Size = new Size(576, 1);
            Label1.TabIndex = 5;
            // 
            // cmdCancel
            // 
            cmdCancel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            cmdCancel.DialogResult = DialogResult.Cancel;
            cmdCancel.Font = new Font("Segoe UI", 9.75f, FontStyle.Regular, GraphicsUnit.Point);
            cmdCancel.Location = new Point(464, 10);
            cmdCancel.Name = "cmdCancel";
            cmdCancel.Size = new Size(93, 26);
            cmdCancel.TabIndex = 1;
            cmdCancel.Text = "&Abbrechen";
            cmdCancel.UseVisualStyleBackColor = true;
            // 
            // Label2
            // 
            Label2.AutoSize = true;
            Label2.Font = new Font("Segoe UI", 12.0f, FontStyle.Regular, GraphicsUnit.Point);
            Label2.ForeColor = Color.FromArgb(0, 51, 153);
            Label2.Location = new Point(18, 18);
            Label2.Name = "Label2";
            Label2.Size = new Size(80, 21);
            Label2.TabIndex = 0;
            Label2.Text = "Zeitplaner";
            // 
            // Panel3
            // 
            Panel3.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;

            Panel3.AutoScroll = true;
            Panel3.Controls.Add(TabControl1);
            Panel3.Controls.Add(Label2);
            Panel3.Location = new Point(0, 0);
            Panel3.Name = "Panel3";
            Panel3.Size = new Size(576, 425);
            Panel3.TabIndex = 81;
            // 
            // TabControl1
            // 
            TabControl1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;

            TabControl1.Controls.Add(TabPage1);
            TabControl1.Controls.Add(TabPage2);
            TabControl1.Location = new Point(19, 50);
            TabControl1.Name = "TabControl1";
            TabControl1.SelectedIndex = 0;
            TabControl1.Size = new Size(538, 372);
            TabControl1.TabIndex = 21;
            // 
            // TabPage1
            // 
            TabPage1.Controls.Add(Label4);
            TabPage1.Controls.Add(cboFullBackup);
            TabPage1.Controls.Add(nudFullBackup);
            TabPage1.Controls.Add(chkFullBackup);
            TabPage1.Controls.Add(lwTimeSchedule);
            TabPage1.Controls.Add(cmdAddTime);
            TabPage1.Controls.Add(cmdDeleteTime);
            TabPage1.Location = new Point(4, 26);
            TabPage1.Name = "TabPage1";
            TabPage1.Padding = new Padding(3);
            TabPage1.Size = new Size(530, 342);
            TabPage1.TabIndex = 0;
            TabPage1.Text = "Sicherungsintervalle";
            TabPage1.UseVisualStyleBackColor = true;
            // 
            // Label4
            // 
            Label4.AutoSize = true;
            Label4.Location = new Point(256, 292);
            Label4.Name = "Label4";
            Label4.Size = new Size(164, 17);
            Label4.TabIndex = 14;
            Label4.Text = "eine Vollsicherung anlegen";
            // 
            // cboFullBackup
            // 
            cboFullBackup.DropDownStyle = ComboBoxStyle.DropDownList;
            cboFullBackup.Enabled = false;
            cboFullBackup.FormattingEnabled = true;
            cboFullBackup.Items.AddRange(new object[] { "Tage" });
            cboFullBackup.Location = new Point(129, 289);
            cboFullBackup.Name = "cboFullBackup";
            cboFullBackup.Size = new Size(121, 25);
            cboFullBackup.TabIndex = 13;
            // 
            // nudFullBackup
            // 
            nudFullBackup.Enabled = false;
            nudFullBackup.Location = new Point(69, 290);
            nudFullBackup.Name = "nudFullBackup";
            nudFullBackup.Size = new Size(54, 25);
            nudFullBackup.TabIndex = 12;
            // 
            // chkFullBackup
            // 
            chkFullBackup.AutoSize = true;
            chkFullBackup.Location = new Point(17, 291);
            chkFullBackup.Name = "chkFullBackup";
            chkFullBackup.Size = new Size(48, 21);
            chkFullBackup.TabIndex = 11;
            chkFullBackup.Text = "Alle";
            chkFullBackup.UseVisualStyleBackColor = true;
            // 
            // lwTimeSchedule
            // 
            lwTimeSchedule.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;

            lwTimeSchedule.Columns.AddRange(new ColumnHeader[] { ColumnHeader10, ColumnHeader11 });
            lwTimeSchedule.FullRowSelect = true;
            lwTimeSchedule.Location = new Point(13, 45);
            lwTimeSchedule.MultiSelect = false;
            lwTimeSchedule.Name = "lwTimeSchedule";
            lwTimeSchedule.Size = new Size(504, 226);
            lwTimeSchedule.TabIndex = 9;
            lwTimeSchedule.UseCompatibleStateImageBehavior = false;
            lwTimeSchedule.View = View.Details;
            // 
            // ColumnHeader10
            // 
            ColumnHeader10.Text = "Wiederholung";
            ColumnHeader10.Width = 213;
            // 
            // ColumnHeader11
            // 
            ColumnHeader11.Text = "Datum / Uhrzeit";
            ColumnHeader11.Width = 224;
            // 
            // cmdAddTime
            // 
            cmdAddTime.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            cmdAddTime.Font = new Font("Segoe UI", 9.75f, FontStyle.Regular, GraphicsUnit.Point);
            cmdAddTime.Location = new Point(13, 13);
            cmdAddTime.Name = "cmdAddTime";
            cmdAddTime.Size = new Size(93, 26);
            cmdAddTime.TabIndex = 6;
            cmdAddTime.Text = "&Hinzufügen";
            cmdAddTime.UseVisualStyleBackColor = true;
            // 
            // cmdDeleteTime
            // 
            cmdDeleteTime.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            cmdDeleteTime.Enabled = false;
            cmdDeleteTime.Font = new Font("Segoe UI", 9.75f, FontStyle.Regular, GraphicsUnit.Point);
            cmdDeleteTime.Location = new Point(112, 13);
            cmdDeleteTime.Name = "cmdDeleteTime";
            cmdDeleteTime.Size = new Size(93, 26);
            cmdDeleteTime.TabIndex = 10;
            cmdDeleteTime.Text = "&Löschen";
            cmdDeleteTime.UseVisualStyleBackColor = true;
            // 
            // TabPage2
            // 
            TabPage2.Controls.Add(Label5);
            TabPage2.Controls.Add(Label3);
            TabPage2.Controls.Add(NumericUpDown1);
            TabPage2.Controls.Add(CheckBox2);
            TabPage2.Controls.Add(plDelete);
            TabPage2.Controls.Add(nudIntervallHourBackups);
            TabPage2.Controls.Add(rdDeleteIntervall);
            TabPage2.Controls.Add(rdDontDelete);
            TabPage2.Controls.Add(rdDeleteAuto);
            TabPage2.Location = new Point(4, 26);
            TabPage2.Name = "TabPage2";
            TabPage2.Padding = new Padding(3);
            TabPage2.Size = new Size(530, 342);
            TabPage2.TabIndex = 1;
            TabPage2.Text = "Löschintervalle";
            TabPage2.UseVisualStyleBackColor = true;
            // 
            // Label5
            // 
            Label5.Location = new Point(100, 114);
            Label5.Name = "Label5";
            Label5.Size = new Size(424, 35);
            Label5.TabIndex = 25;
            Label5.Text = "Danach tägliche Sicherungen für einen Monat verfügbar halten. Anschließend wöchen" + "tliche Sicherungen verfügbar halten.";
            // 
            // Label3
            // 
            Label3.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            Label3.AutoSize = true;
            Label3.Location = new Point(100, 288);
            Label3.Name = "Label3";
            Label3.Size = new Size(176, 17);
            Label3.TabIndex = 24;
            Label3.Text = "Sicherungen vorhanden sind.";
            Label3.Visible = false;
            // 
            // NumericUpDown1
            // 
            NumericUpDown1.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            NumericUpDown1.Location = new Point(34, 286);
            NumericUpDown1.Name = "NumericUpDown1";
            NumericUpDown1.Size = new Size(55, 25);
            NumericUpDown1.TabIndex = 23;
            NumericUpDown1.Visible = false;
            // 
            // CheckBox2
            // 
            CheckBox2.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            CheckBox2.AutoSize = true;
            CheckBox2.Location = new Point(13, 259);
            CheckBox2.Name = "CheckBox2";
            CheckBox2.Size = new Size(245, 21);
            CheckBox2.TabIndex = 22;
            CheckBox2.Text = "Sicherung behalten, wenn weniger als";
            CheckBox2.UseVisualStyleBackColor = true;
            CheckBox2.Visible = false;
            // 
            // plDelete
            // 
            plDelete.Controls.Add(cboIntervall);
            plDelete.Controls.Add(txtIntervall);
            plDelete.Enabled = false;
            plDelete.Location = new Point(25, 35);
            plDelete.Name = "plDelete";
            plDelete.Size = new Size(378, 43);
            plDelete.TabIndex = 18;
            // 
            // cboIntervall
            // 
            cboIntervall.DropDownStyle = ComboBoxStyle.DropDownList;
            cboIntervall.FormattingEnabled = true;
            cboIntervall.Items.AddRange(new object[] { "Stunde(n)", "Tag(e)", "Woche(n)" });
            cboIntervall.Location = new Point(78, 8);
            cboIntervall.Name = "cboIntervall";
            cboIntervall.Size = new Size(121, 25);
            cboIntervall.TabIndex = 17;
            // 
            // txtIntervall
            // 
            txtIntervall.Location = new Point(9, 8);
            txtIntervall.Name = "txtIntervall";
            txtIntervall.Size = new Size(63, 25);
            txtIntervall.TabIndex = 16;
            // 
            // nudIntervallHourBackups
            // 
            nudIntervallHourBackups.Enabled = false;
            nudIntervallHourBackups.Location = new Point(34, 116);
            nudIntervallHourBackups.Maximum = new decimal(new int[] { 500, 0, 0, 0 });
            nudIntervallHourBackups.Name = "nudIntervallHourBackups";
            nudIntervallHourBackups.Size = new Size(52, 25);
            nudIntervallHourBackups.TabIndex = 20;
            // 
            // rdDeleteIntervall
            // 
            rdDeleteIntervall.AutoSize = true;
            rdDeleteIntervall.Location = new Point(13, 13);
            rdDeleteIntervall.Name = "rdDeleteIntervall";
            rdDeleteIntervall.Size = new Size(221, 21);
            rdDeleteIntervall.TabIndex = 14;
            rdDeleteIntervall.Text = "Sicherung löschen, wenn älter als:";
            rdDeleteIntervall.UseVisualStyleBackColor = true;
            // 
            // rdDontDelete
            // 
            rdDontDelete.AutoSize = true;
            rdDontDelete.Location = new Point(13, 165);
            rdDontDelete.Name = "rdDontDelete";
            rdDontDelete.Size = new Size(250, 21);
            rdDontDelete.TabIndex = 19;
            rdDontDelete.Text = "Sicherungen nicht automatisch löschen";
            rdDontDelete.UseVisualStyleBackColor = true;
            // 
            // rdDeleteAuto
            // 
            rdDeleteAuto.AutoSize = true;
            rdDeleteAuto.CheckAlign = ContentAlignment.TopLeft;
            rdDeleteAuto.Location = new Point(13, 89);
            rdDeleteAuto.Name = "rdDeleteAuto";
            rdDeleteAuto.Size = new Size(279, 21);
            rdDeleteAuto.TabIndex = 15;
            rdDeleteAuto.Text = "Sicherungen mindestens Stunden vorhalten:";
            rdDeleteAuto.UseVisualStyleBackColor = true;
            // 
            // frmEditScheduler
            // 
            AutoScaleDimensions = new SizeF(96.0f, 96.0f);
            AutoScaleMode = AutoScaleMode.Dpi;
            BackColor = Color.White;
            ClientSize = new Size(576, 484);
            Controls.Add(Panel3);
            Controls.Add(Panel1);
            Font = new Font("Segoe UI", 9.75f, FontStyle.Regular, GraphicsUnit.Point);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "frmEditScheduler";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Zeitplan bearbeiten";
            Panel1.ResumeLayout(false);
            Panel3.ResumeLayout(false);
            Panel3.PerformLayout();
            TabControl1.ResumeLayout(false);
            TabPage1.ResumeLayout(false);
            TabPage1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)nudFullBackup).EndInit();
            TabPage2.ResumeLayout(false);
            TabPage2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)NumericUpDown1).EndInit();
            plDelete.ResumeLayout(false);
            plDelete.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)nudIntervallHourBackups).EndInit();
            Load += new EventHandler(frmEditScheduler_Load);
            ResumeLayout(false);
        }

        internal Panel Panel1;
        internal Label Label1;
        internal Button cmdCancel;
        internal Label Label2;
        internal Button cmdOK;
        internal Panel Panel3;
        internal Button cmdDeleteTime;
        internal Button cmdAddTime;
        internal ListView lwTimeSchedule;
        internal ColumnHeader ColumnHeader10;
        internal ColumnHeader ColumnHeader11;
        internal RadioButton rdDeleteAuto;
        internal RadioButton rdDeleteIntervall;
        internal Panel plDelete;
        internal ComboBox cboIntervall;
        internal TextBox txtIntervall;
        internal RadioButton rdDontDelete;
        internal NumericUpDown nudIntervallHourBackups;
        internal TabControl TabControl1;
        internal TabPage TabPage1;
        internal TabPage TabPage2;
        internal Label Label3;
        internal NumericUpDown NumericUpDown1;
        internal CheckBox CheckBox2;
        internal Label Label4;
        internal ComboBox cboFullBackup;
        internal NumericUpDown nudFullBackup;
        internal CheckBox chkFullBackup;
        internal Label Label5;
    }
}