// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Drawing;
using System.Windows.Forms;
using BSH.Main.Properties;
using Humanizer;

namespace BSH.Main.Dialogs.SubDialogs;

public sealed class frmBackupSpaceWarning : Form
{
    public frmBackupSpaceWarning(long estimatedRequiredSpace, long availableSpace)
    {
        InitializeComponent(estimatedRequiredSpace, availableSpace);
    }

    private void InitializeComponent(long estimatedRequiredSpace, long availableSpace)
    {
        SuspendLayout();

        AutoScaleMode = AutoScaleMode.Font;
        BackColor = Color.White;
        ClientSize = new Size(620, 230);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        Name = nameof(frmBackupSpaceWarning);
        ShowIcon = false;
        StartPosition = FormStartPosition.CenterParent;
        Text = Resources.INFO_BACKUP_SPACE_WARNING_TITLE;
        TopMost = true;

        var icon = new PictureBox
        {
            Image = SystemIcons.Warning.ToBitmap(),
            Location = new Point(24, 24),
            Name = "picWarning",
            Size = new Size(48, 48),
            SizeMode = PictureBoxSizeMode.CenterImage,
        };

        var heading = new Label
        {
            AutoSize = false,
            Font = new Font(Font, FontStyle.Bold),
            Location = new Point(88, 20),
            Name = "lblHeading",
            Size = new Size(498, 28),
            Text = Resources.INFO_BACKUP_SPACE_WARNING_HEADING,
        };

        var details = new Label
        {
            AutoSize = false,
            Location = new Point(88, 52),
            Name = "lblDetails",
            Size = new Size(498, 100),
            Text = string.Format(
                Resources.INFO_BACKUP_SPACE_WARNING_TEXT,
                estimatedRequiredSpace.Bytes().Humanize(),
                availableSpace.Bytes().Humanize()),
        };

        var buttonPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            FlowDirection = FlowDirection.RightToLeft,
            Height = 56,
            Padding = new Padding(12, 10, 12, 12),
        };

        var continueButton = new Button
        {
            DialogResult = DialogResult.Yes,
            Margin = new Padding(6, 0, 0, 0),
            Name = "cmdContinue",
            Text = Resources.INFO_BACKUP_SPACE_WARNING_CONTINUE,
            Width = 110,
        };

        var cancelButton = new Button
        {
            DialogResult = DialogResult.No,
            Margin = new Padding(6, 0, 0, 0),
            Name = "cmdCancel",
            Text = Resources.INFO_BACKUP_SPACE_WARNING_CANCEL,
            Width = 110,
        };

        buttonPanel.Controls.Add(continueButton);
        buttonPanel.Controls.Add(cancelButton);

        AcceptButton = continueButton;
        CancelButton = cancelButton;

        Controls.Add(details);
        Controls.Add(heading);
        Controls.Add(icon);
        Controls.Add(buttonPanel);

        ResumeLayout(false);
    }
}
