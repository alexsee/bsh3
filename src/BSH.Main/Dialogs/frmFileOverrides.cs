// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Windows.Forms;

namespace Brightbits.BSH.Main;

public partial class frmFileOverrides
{
    public frmFileOverrides()
    {
        InitializeComponent();
    }

    private void plReplace_MouseClick(object sender, MouseEventArgs e)
    {
        DialogResult = DialogResult.OK;
        Close();
    }

    private void lblOverride_Click(object sender, EventArgs e)
    {
        plReplace_MouseClick(null, null);
    }

    private void lblOverride2_Click(object sender, EventArgs e)
    {
        plReplace_MouseClick(null, null);
    }

    private void picIco1_Click(object sender, EventArgs e)
    {
        plReplace_MouseClick(null, null);
    }

    private void lblFileName1_Click(object sender, EventArgs e)
    {
        plReplace_MouseClick(null, null);
    }

    private void lblFileSize1_Click(object sender, EventArgs e)
    {
        plReplace_MouseClick(null, null);
    }

    private void lblFileDateChanged1_Click(object sender, EventArgs e)
    {
        plReplace_MouseClick(null, null);
    }

    private void plCancel_MouseClick(object sender, MouseEventArgs e)
    {
        DialogResult = DialogResult.Ignore;
        Close();
    }

    private void Label6_Click(object sender, EventArgs e)
    {
        plCancel_MouseClick(null, null);
    }

    private void Label5_Click(object sender, EventArgs e)
    {
        plCancel_MouseClick(null, null);
    }

    private void picIco2_Click(object sender, EventArgs e)
    {
        plCancel_MouseClick(null, null);
    }

    private void lblFileName2_Click(object sender, EventArgs e)
    {
        plCancel_MouseClick(null, null);
    }

    private void lblFileSize2_Click(object sender, EventArgs e)
    {
        plCancel_MouseClick(null, null);
    }

    private void lblFileDateChanged2_Click(object sender, EventArgs e)
    {
        plCancel_MouseClick(null, null);
    }

    private void frmFileOverrides_Shown(object sender, EventArgs e)
    {
        this.TopMost = true;
    }
}