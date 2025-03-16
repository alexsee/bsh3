// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Text;
using System.Windows.Forms;

namespace Brightbits.BSH.Main;

public partial class frmFileNotCopied
{
    public frmFileNotCopied()
    {
        InitializeComponent();
    }

    private void KopierenToolStripMenuItem_Click(object sender, EventArgs e)
    {
        llCopyToClipboard_LinkClicked(sender, null);
    }

    private void llCopyToClipboard_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"{Program.APP_TITLE} Version {Program.CurrentVersion}");
        sb.AppendLine($"Datum: {DateTime.Now.ToLongDateString()} {DateTime.Now.ToLongTimeString()}");
        sb.AppendLine();

        foreach (ListViewItem item in lvFiles.Items)
        {
            sb.AppendLine($"{item.Text} - {item.SubItems[1].Text}");
        }

        var t = new System.Threading.Thread((s) => Clipboard.SetText((string)s));

        t.SetApartmentState(System.Threading.ApartmentState.STA); // Wichtig!!
        t.Start(sb.ToString());
    }
}