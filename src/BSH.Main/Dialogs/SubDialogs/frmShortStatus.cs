﻿// Copyright 2022 Alexander Seeliger
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
using Brightbits.BSH.Engine.Models;
using System;
using System.Collections.Generic;

namespace Brightbits.BSH.Main
{
    public partial class frmShortStatus : IJobReport
    {
        public frmShortStatus()
        {
            InitializeComponent();
        }

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

            Invoke(new Action(() => lblStatus.Text = title));
        }

        public void ReportProgress(int total, int current)
        {
            if (!IsHandleCreated)
            {
                return;
            }

            Invoke(new Action(() =>
            {
                pbarStatus.Maximum = total;
                pbarStatus.Value = current;
            }));
        }

        private DateTime lastTimeRefreshed = DateTime.Now;

        public void ReportFileProgress(string file)
        {
            if (!IsHandleCreated)
            {
                return;
            }

            if (DateTime.Now - lastTimeRefreshed < TimeSpan.FromMilliseconds(100d))
            {
                return;
            }

            lastTimeRefreshed = DateTime.Now;
            Invoke(new Action(() => { if (string.IsNullOrEmpty(file)) { lblStatus.Text = "Wartungsaufgabe wird durchgeführt..."; } else { lblStatus.Text = System.IO.Path.GetFileName(file) + " wird verarbeitet..."; } }));
        }

        public void ReportExceptions(List<FileExceptionEntry> files, bool silent)
        {
        }

        public RequestOverwriteResult RequestOverwrite(FileTableRow localFile, FileTableRow remoteFile)
        {
            return RequestOverwriteResult.NoOverwriteAll;
        }
    }
}