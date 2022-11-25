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

namespace Brightbits.BSH.Main
{
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
}