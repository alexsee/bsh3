// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using BSH.Main.Properties;

namespace Brightbits.BSH.Main;

public partial class frmAddSchedule
{
    public frmAddSchedule()
    {
        InitializeComponent();
    }

    private void cbIntervall_SelectedIndexChanged(object sender, EventArgs e)
    {
        switch (cbIntervall.SelectedIndex)
        {
            case 0:
                // once
                dtpStartTime.CustomFormat = Resources.DLG_ADD_SCHEDULE_FORMAT_ONCE;
                dtpStartTime.ShowUpDown = false;

                break;

            case 1:
                // hourly
                dtpStartTime.CustomFormat = Resources.DLG_ADD_SCHEDULE_FORMAT_HOURLY;
                dtpStartTime.ShowUpDown = true;

                break;

            case 2:
                // daily
                dtpStartTime.CustomFormat = Resources.DLG_ADD_SCHEDULE_FORMAT_DAILY;
                dtpStartTime.ShowUpDown = true;

                break;

            case 3:
                // weekly
                dtpStartTime.CustomFormat = Resources.DLG_ADD_SCHEDULE_FORMAT_WEEKLY;
                dtpStartTime.ShowUpDown = false;

                break;

            case 4:
                // monthly
                dtpStartTime.CustomFormat = Resources.DLG_ADD_SCHEDULE_FORMAT_MONTHLY;
                dtpStartTime.ShowUpDown = true;

                break;
        }

        dtpStartTime.Enabled = true;
    }
}