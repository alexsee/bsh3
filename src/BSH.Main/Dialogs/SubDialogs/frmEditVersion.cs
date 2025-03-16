// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;

namespace Brightbits.BSH.Main;

public partial class frmEditVersion
{
    public frmEditVersion()
    {
        InitializeComponent();
    }

    private void txtTitle_TextChanged(object sender, EventArgs e)
    {
        txtTitle.Text = txtTitle.Text.Replace("\"", "");
        txtTitle.Text = txtTitle.Text.Replace("'", "");
    }

    private void txtDescription_TextChanged(object sender, EventArgs e)
    {
        txtTitle.Text = txtTitle.Text.Replace("\"", "");
        txtTitle.Text = txtTitle.Text.Replace("'", "");
    }
}