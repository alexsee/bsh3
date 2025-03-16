// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;

namespace Brightbits.BSH.Main;

public partial class frmChangePath
{
    public frmChangePath()
    {
        InitializeComponent();
    }

    private void cboMethod_SelectedIndexChanged(object sender, EventArgs e)
    {
        Button1.Enabled = true;
    }
}