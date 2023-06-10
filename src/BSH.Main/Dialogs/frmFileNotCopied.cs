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
using System.Text;
using System.Windows.Forms;

namespace Brightbits.BSH.Main;

public partial class frmFileNotCopied
{
    public frmFileNotCopied()
    {
        InitializeComponent();
    }

    private void KopierenToolStripMenuItem_Click(object sender, EventArgs e)
    {
        llCopyToClipboard_LinkClicked(sender, null);
    }

    private void llCopyToClipboard_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"{Program.APP_TITLE} Version {Program.CurrentVersion}");
        sb.AppendLine($"Datum: {DateTime.Now.ToLongDateString()} {DateTime.Now.ToLongTimeString()}");
        sb.AppendLine();

        foreach (ListViewItem item in lvFiles.Items)
        {
            sb.AppendLine($"{item.Text} - {item.SubItems[1].Text}");
        }

        var t = new System.Threading.Thread((s) => Clipboard.SetText((string)s));

        t.SetApartmentState(System.Threading.ApartmentState.STA); // Wichtig!!
        t.Start(sb.ToString());
    }
}