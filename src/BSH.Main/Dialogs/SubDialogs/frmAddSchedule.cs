// Copyright 2022 Alexander Seeliger
//
// Licensed under the Apache License, Version 2.0 (the "License")
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using BSH.Main.Properties;
using System;

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