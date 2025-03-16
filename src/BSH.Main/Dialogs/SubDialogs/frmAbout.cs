// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace Brightbits.BSH.Main;

public partial class frmAbout
{
    public frmAbout()
    {
        InitializeComponent();
    }

    private void frmAbout_Load(object sender, EventArgs e)
    {
        lblVersion.Text = "Version " + Application.ProductVersion.ToString();
    }

    private void llWebsite_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
        try
        {
            Process.Start("https://www.brightbits.de/?pk_campaign=software_link&pk_kwd=about&pk_source=bsh-3");
        }
        catch
        {
            // ignore error
        }
    }
}