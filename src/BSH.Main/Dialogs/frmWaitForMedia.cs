// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using BSH.Main.Properties;

namespace Brightbits.BSH.Main;

public partial class frmWaitForMedia
{
    public frmWaitForMedia()
    {
        InitializeComponent();
    }

    public event OnAbort_ClickEventHandler OnAbort_Click;

    public delegate void OnAbort_ClickEventHandler();

    private void cmdCancel_Click(object sender, EventArgs e)
    {
        cmdCancel.Text = Resources.DLG_WAIT_MEDIA_STATUS_CANCELED_TEXT;
        cmdCancel.Enabled = false;
        OnAbort_Click?.Invoke();
    }
}