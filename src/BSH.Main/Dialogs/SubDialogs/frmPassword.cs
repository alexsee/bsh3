// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Windows.Forms;

namespace Brightbits.BSH.Main;

public partial class frmPassword
{
    public frmPassword()
    {
        InitializeComponent();
    }

    private void txtPassword_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Enter)
        {
            Button1.PerformClick();
        }
    }

    private void txtPassword_TextChanged(object sender, EventArgs e)
    {
        txtPassword.Text = txtPassword.Text.Replace("\"", "");
        txtPassword.Text = txtPassword.Text.Replace("'", "");
    }
}