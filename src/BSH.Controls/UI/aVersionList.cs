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
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Brightbits.BSH.Main
{
    [Serializable()]
    public partial class aVersionList
    {
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public List<aVersionListItem> Items { get; set; } = new List<aVersionListItem>();

        public event ItemClickEventHandler ItemClick;

        public delegate Task ItemClickEventHandler(aVersionListItem sender);

        public aVersionList()
        {
            InitializeComponent();
            flpMain.ControlAdded += ItemAdded;
            flpMain.ControlRemoved += ItemRemoved;
        }

        public void DrawItems()
        {
            foreach (var entry in flpMain.Controls.OfType<aVersionListItem>().ToArray())
            {
                if (!Items.Contains(entry))
                {
                    flpMain.Controls.Remove(entry);
                    entry.Dispose();
                }
            }

            var flpMainHeight = 0;
            flpMain.Width = plMain.Width;

            for (var i = 0; i < Items.Count; i++)
            {
                var entry = Items[i];
                entry.Width = flpMain.Width - 6;
                entry.Margin = new Padding(3, 0, 3, 0);

                if (!flpMain.Controls.Contains(entry))
                {
                    flpMain.Controls.Add(entry);
                }

                flpMain.Controls.SetChildIndex(entry, i);
                flpMainHeight += 40;
            }

            flpMain.Height = flpMainHeight;
        }

        public void DeactivateAll()
        {
            foreach (var entry in flpMain.Controls.OfType<aVersionListItem>())
            {
                entry.Deactivate();
            }
        }

        public async void SelectItem(string versionID, bool click = true)
        {
            foreach (var entry in flpMain.Controls.OfType<aVersionListItem>())
            {
                if ((entry.VersionID ?? "") == (versionID ?? ""))
                {
                    flpMain.ScrollControlIntoView(entry);
                    DeactivateAll();
                    entry.Activate();

                    if (click)
                    {
                        await RaiseItemClickAsync(entry);
                    }
                    return;
                }
            }
        }

        private void ItemAdded(object sender, ControlEventArgs e)
        {
            if (e.Control is aVersionListItem entry)
            {
                entry.ItemClick += ItemClick_Handler;
            }
        }

        private void ItemRemoved(object sender, ControlEventArgs e)
        {
            if (e.Control is aVersionListItem entry)
            {
                entry.ItemClick -= ItemClick_Handler;
            }
        }

        private async void ItemClick_Handler(aVersionListItem sender)
        {
            DeactivateAll();
            sender.Activate();
            await RaiseItemClickAsync(sender);
        }

        private async Task RaiseItemClickAsync(aVersionListItem sender)
        {
            var handlers = ItemClick;
            if (handlers != null)
            {
                foreach (ItemClickEventHandler handler in handlers.GetInvocationList())
                {
                    await handler(sender);
                }
            }
        }

        private void aVersionList_Load(object sender, EventArgs e)
        {
            Items = new List<aVersionListItem>();
        }

        private void flpMain_Resize(object sender, EventArgs e)
        {
            foreach (Control entry in flpMain.Controls)
            {
                entry.Width = flpMain.Width - 6;
            }
        }
    }
}