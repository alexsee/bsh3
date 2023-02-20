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

using System;
using System.Windows.Forms;

namespace Brightbits.BSH.Main
{
    public partial class frmFileOverrides
    {
        public frmFileOverrides()
        {
            InitializeComponent();
        }

        private void plReplace_MouseClick(object sender, MouseEventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        private void lblOverride_Click(object sender, EventArgs e)
        {
            plReplace_MouseClick(null, null);
        }

        private void lblOverride2_Click(object sender, EventArgs e)
        {
            plReplace_MouseClick(null, null);
        }

        private void picIco1_Click(object sender, EventArgs e)
        {
            plReplace_MouseClick(null, null);
        }

        private void lblFileName1_Click(object sender, EventArgs e)
        {
            plReplace_MouseClick(null, null);
        }

        private void lblFileSize1_Click(object sender, EventArgs e)
        {
            plReplace_MouseClick(null, null);
        }

        private void lblFileDateChanged1_Click(object sender, EventArgs e)
        {
            plReplace_MouseClick(null, null);
        }

        private void plCancel_MouseClick(object sender, MouseEventArgs e)
        {
            DialogResult = DialogResult.Ignore;
            Close();
        }

        private void Label6_Click(object sender, EventArgs e)
        {
            plCancel_MouseClick(null, null);
        }

        private void Label5_Click(object sender, EventArgs e)
        {
            plCancel_MouseClick(null, null);
        }

        private void picIco2_Click(object sender, EventArgs e)
        {
            plCancel_MouseClick(null, null);
        }

        private void lblFileName2_Click(object sender, EventArgs e)
        {
            plCancel_MouseClick(null, null);
        }

        private void lblFileSize2_Click(object sender, EventArgs e)
        {
            plCancel_MouseClick(null, null);
        }

        private void lblFileDateChanged2_Click(object sender, EventArgs e)
        {
            plCancel_MouseClick(null, null);
        }

        private void frmFileOverrides_Shown(object sender, EventArgs e)
        {
            this.TopMost = true;
        }
    }
}