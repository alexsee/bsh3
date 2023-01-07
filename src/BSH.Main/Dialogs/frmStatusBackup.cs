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

using Brightbits.BSH.Engine;
using Brightbits.BSH.Engine.Jobs;
using BSH.Main.Properties;
using Humanizer;
using System;

namespace Brightbits.BSH.Main
{
    public partial class frmStatusBackup : IStatusReport
    {
        public frmStatusBackup()
        {
            InitializeComponent();
        }

        public bool abort = false;

        public void ReportAction(ActionType action, bool silent)
        {
        }

        public void ReportState(JobState jobState)
        {
        }

        public void ReportStatus(string title, string text)
        {
            if (!IsHandleCreated)
            {
                return;
            }

            Invoke(new Action(() => lblStatus.Text = text));
        }

        private DateTime lastTimeProgressRefreshed = DateTime.Now;

        public void ReportProgress(int total, int current)
        {
            if (DateTime.Now - lastTimeProgressRefreshed < TimeSpan.FromMilliseconds(100d))
            {
                return;
            }

            if (!IsHandleCreated)
            {
                return;
            }

            lastTimeProgressRefreshed = DateTime.Now;
            Invoke(new Action(() =>
            {
                pbarTotal.Maximum = total;
                pbarTotal.Value = current;
                lblFiles.Text = Resources.DLG_STATUS_FILES_PROCESSED_TEXT.FormatWith(current, total);
            }));
        }

        private DateTime lastTimeRefreshed = DateTime.Now;

        public void ReportFileProgress(string file)
        {
            if (DateTime.Now - lastTimeRefreshed < TimeSpan.FromMilliseconds(100d))
            {
                return;
            }

            if (!IsHandleCreated)
            {
                return;
            }

            lastTimeRefreshed = DateTime.Now;
            Invoke(new Action(() => lblFile.Text = file));
        }

        public void ReportSystemStatus(SystemStatus systemStatus)
        {
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            BackupLogic.BackupController.Cancel();
            cmdCancel.Enabled = false;
            cmdCancel.Text = Resources.DLG_STATUS_STATUS_CANCELED_TEXT;
        }

        private void chkOptions_CheckedChanged(object sender, EventArgs e)
        {
            cboOptions.Enabled = chkOptions.Checked;
        }
    }
}