// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Windows.Forms;

namespace Brightbits.BSH.Main;

public partial class frmEditVersion
{
    public frmEditVersion()
    {
        InitializeComponent();
    }

    private void txtTitle_TextChanged(object sender, EventArgs e)
    {
        SanitizeQuotes(txtTitle);
    }

    private void txtDescription_TextChanged(object sender, EventArgs e)
    {
        SanitizeQuotes(txtDescription);
    }

    private static void SanitizeQuotes(TextBox textBox)
    {
        var sanitized = textBox.Text.Replace("\"", "").Replace("'", "");
        if (sanitized != textBox.Text)
        {
            textBox.Text = sanitized;
        }
    }
}