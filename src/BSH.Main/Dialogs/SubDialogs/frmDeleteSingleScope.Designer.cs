using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace Brightbits.BSH.Main
{
    public partial class frmDeleteSingleScope : Form
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
            var resources = new System.ComponentModel.ComponentResourceManager(typeof(frmDeleteSingleScope));
            tlpContent = new TableLayoutPanel();
            lblIntro = new Label();
            radioAll = new RadioButton();
            radioLastN = new RadioButton();
            numLastN = new NumericUpDown();
            radioLastDays = new RadioButton();
            numLastDays = new NumericUpDown();
            radioSelected = new RadioButton();
            lstVersions = new ListView();
            ColumnHeader1 = new ColumnHeader();
            Panel1 = new Panel();
            cmdOK = new Button();
            cmdCancel = new Button();
            Label1 = new Label();
            tlpContent.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numLastN).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numLastDays).BeginInit();
            Panel1.SuspendLayout();
            SuspendLayout();
            // 
            // tlpContent
            // 
            tlpContent.ColumnCount = 2;
            tlpContent.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tlpContent.ColumnStyles.Add(new ColumnStyle());
            tlpContent.Controls.Add(lblIntro, 0, 0);
            tlpContent.Controls.Add(radioAll, 0, 1);
            tlpContent.Controls.Add(radioLastN, 0, 2);
            tlpContent.Controls.Add(numLastN, 1, 2);
            tlpContent.Controls.Add(radioLastDays, 0, 3);
            tlpContent.Controls.Add(numLastDays, 1, 3);
            tlpContent.Controls.Add(radioSelected, 0, 4);
            tlpContent.Controls.Add(lstVersions, 0, 5);
            tlpContent.Dock = DockStyle.Fill;
            tlpContent.Location = new Point(0, 0);
            tlpContent.Name = "tlpContent";
            tlpContent.Padding = new Padding(22, 18, 22, 8);
            tlpContent.RowCount = 6;
            tlpContent.RowStyles.Add(new RowStyle());
            tlpContent.RowStyles.Add(new RowStyle());
            tlpContent.RowStyles.Add(new RowStyle());
            tlpContent.RowStyles.Add(new RowStyle());
            tlpContent.RowStyles.Add(new RowStyle());
            tlpContent.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tlpContent.Size = new Size(560, 475);
            tlpContent.TabIndex = 0;
            // 
            // lblIntro
            // 
            lblIntro.AutoSize = true;
            tlpContent.SetColumnSpan(lblIntro, 2);
            lblIntro.Font = new Font("Segoe UI", 12F);
            lblIntro.ForeColor = Color.FromArgb(0, 51, 153);
            lblIntro.Location = new Point(22, 18);
            lblIntro.Margin = new Padding(0, 0, 0, 16);
            lblIntro.MaximumSize = new Size(500, 0);
            lblIntro.Name = "lblIntro";
            lblIntro.Size = new Size(485, 42);
            lblIntro.TabIndex = 0;
            lblIntro.Text = "Wählen Sie, aus welchen Sicherungen diese(r) Datei entfernt werden soll:";
            // 
            // radioAll
            // 
            radioAll.AutoSize = true;
            radioAll.Checked = true;
            tlpContent.SetColumnSpan(radioAll, 2);
            radioAll.Location = new Point(22, 80);
            radioAll.Margin = new Padding(0, 4, 12, 4);
            radioAll.Name = "radioAll";
            radioAll.Padding = new Padding(0, 2, 0, 2);
            radioAll.Size = new Size(153, 25);
            radioAll.TabIndex = 1;
            radioAll.TabStop = true;
            radioAll.Text = "Aus allen Sicherungen";
            radioAll.UseVisualStyleBackColor = true;
            radioAll.CheckedChanged += radioScope_CheckedChanged;
            // 
            // radioLastN
            // 
            radioLastN.Anchor = AnchorStyles.Left;
            radioLastN.AutoSize = true;
            radioLastN.Location = new Point(22, 113);
            radioLastN.Margin = new Padding(0, 4, 12, 4);
            radioLastN.Name = "radioLastN";
            radioLastN.Padding = new Padding(0, 2, 0, 2);
            radioLastN.Size = new Size(204, 25);
            radioLastN.TabIndex = 2;
            radioLastN.Text = "Aus den letzten N Sicherungen";
            radioLastN.UseVisualStyleBackColor = true;
            radioLastN.CheckedChanged += radioScope_CheckedChanged;
            // 
            // numLastN
            // 
            numLastN.Anchor = AnchorStyles.Left;
            numLastN.Enabled = false;
            numLastN.Location = new Point(448, 113);
            numLastN.Margin = new Padding(0, 4, 0, 4);
            numLastN.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numLastN.Name = "numLastN";
            numLastN.Size = new Size(90, 25);
            numLastN.TabIndex = 3;
            numLastN.TextAlign = HorizontalAlignment.Right;
            numLastN.Value = new decimal(new int[] { 3, 0, 0, 0 });
            numLastN.Enter += numLastN_Enter;
            // 
            // radioLastDays
            // 
            radioLastDays.Anchor = AnchorStyles.Left;
            radioLastDays.AutoSize = true;
            radioLastDays.Location = new Point(22, 146);
            radioLastDays.Margin = new Padding(0, 4, 12, 4);
            radioLastDays.Name = "radioLastDays";
            radioLastDays.Padding = new Padding(0, 2, 0, 2);
            radioLastDays.Size = new Size(232, 25);
            radioLastDays.TabIndex = 4;
            radioLastDays.Text = "Aus Sicherungen der letzten X Tage";
            radioLastDays.UseVisualStyleBackColor = true;
            radioLastDays.CheckedChanged += radioScope_CheckedChanged;
            // 
            // numLastDays
            // 
            numLastDays.Anchor = AnchorStyles.Left;
            numLastDays.Enabled = false;
            numLastDays.Location = new Point(448, 146);
            numLastDays.Margin = new Padding(0, 4, 0, 4);
            numLastDays.Maximum = new decimal(new int[] { 3650, 0, 0, 0 });
            numLastDays.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numLastDays.Name = "numLastDays";
            numLastDays.Size = new Size(90, 25);
            numLastDays.TabIndex = 5;
            numLastDays.TextAlign = HorizontalAlignment.Right;
            numLastDays.Value = new decimal(new int[] { 30, 0, 0, 0 });
            numLastDays.Enter += numLastDays_Enter;
            // 
            // radioSelected
            // 
            radioSelected.AutoSize = true;
            tlpContent.SetColumnSpan(radioSelected, 2);
            radioSelected.Location = new Point(22, 179);
            radioSelected.Margin = new Padding(0, 4, 12, 4);
            radioSelected.Name = "radioSelected";
            radioSelected.Padding = new Padding(0, 2, 0, 2);
            radioSelected.Size = new Size(205, 25);
            radioSelected.TabIndex = 6;
            radioSelected.Text = "Aus ausgewählten Sicherungen";
            radioSelected.UseVisualStyleBackColor = true;
            radioSelected.CheckedChanged += radioScope_CheckedChanged;
            // 
            // lstVersions
            // 
            lstVersions.CheckBoxes = true;
            lstVersions.Columns.AddRange(new ColumnHeader[] { ColumnHeader1 });
            tlpContent.SetColumnSpan(lstVersions, 2);
            lstVersions.Dock = DockStyle.Fill;
            lstVersions.Enabled = false;
            lstVersions.FullRowSelect = true;
            lstVersions.HeaderStyle = ColumnHeaderStyle.Nonclickable;
            lstVersions.Location = new Point(44, 212);
            lstVersions.Margin = new Padding(22, 4, 0, 0);
            lstVersions.Name = "lstVersions";
            lstVersions.Size = new Size(494, 255);
            lstVersions.TabIndex = 7;
            lstVersions.UseCompatibleStateImageBehavior = false;
            lstVersions.View = View.Details;
            lstVersions.SizeChanged += lstVersions_SizeChanged;
            lstVersions.Enter += lstVersions_Enter;
            // 
            // ColumnHeader1
            // 
            ColumnHeader1.Text = "Sicherungsdatum";
            ColumnHeader1.Width = 360;
            // 
            // Panel1
            // 
            Panel1.BackColor = SystemColors.Control;
            Panel1.Controls.Add(cmdOK);
            Panel1.Controls.Add(cmdCancel);
            Panel1.Controls.Add(Label1);
            Panel1.Dock = DockStyle.Bottom;
            Panel1.Location = new Point(0, 475);
            Panel1.Name = "Panel1";
            Panel1.Size = new Size(560, 45);
            Panel1.TabIndex = 1;
            // 
            // cmdOK
            // 
            cmdOK.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            cmdOK.DialogResult = DialogResult.OK;
            cmdOK.Location = new Point(346, 9);
            cmdOK.Name = "cmdOK";
            cmdOK.Size = new Size(93, 26);
            cmdOK.TabIndex = 0;
            cmdOK.Text = "&OK";
            cmdOK.UseVisualStyleBackColor = true;
            // 
            // cmdCancel
            // 
            cmdCancel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            cmdCancel.DialogResult = DialogResult.Cancel;
            cmdCancel.Location = new Point(445, 9);
            cmdCancel.Name = "cmdCancel";
            cmdCancel.Size = new Size(93, 26);
            cmdCancel.TabIndex = 1;
            cmdCancel.Text = "&Abbrechen";
            cmdCancel.UseVisualStyleBackColor = true;
            // 
            // Label1
            // 
            Label1.BackColor = Color.DarkGray;
            Label1.Dock = DockStyle.Top;
            Label1.Location = new Point(0, 0);
            Label1.Name = "Label1";
            Label1.Size = new Size(560, 1);
            Label1.TabIndex = 2;
            // 
            // frmDeleteSingleScope
            // 
            AcceptButton = cmdOK;
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            BackColor = Color.White;
            CancelButton = cmdCancel;
            ClientSize = new Size(560, 520);
            Controls.Add(tlpContent);
            Controls.Add(Panel1);
            Font = new Font("Segoe UI", 9.75F);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "frmDeleteSingleScope";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Aus Sicherungen löschen";
            Shown += frmDeleteSingleScope_Shown;
            tlpContent.ResumeLayout(false);
            tlpContent.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numLastN).EndInit();
            ((System.ComponentModel.ISupportInitialize)numLastDays).EndInit();
            Panel1.ResumeLayout(false);
            ResumeLayout(false);
        }

        internal TableLayoutPanel tlpContent;
        internal Label lblIntro;
        internal RadioButton radioAll;
        internal RadioButton radioLastN;
        internal NumericUpDown numLastN;
        internal RadioButton radioLastDays;
        internal NumericUpDown numLastDays;
        internal RadioButton radioSelected;
        internal ListView lstVersions;
        internal ColumnHeader ColumnHeader1;
        internal Panel Panel1;
        internal Button cmdOK;
        internal Label Label1;
        internal Button cmdCancel;
    }
}
