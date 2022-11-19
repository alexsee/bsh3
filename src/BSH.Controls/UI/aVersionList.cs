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
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Brightbits.BSH.Main
{
    [Serializable()]
    public partial class aVersionList
    {
        public List<aVersionListItem> Items { get; set; } = new List<aVersionListItem>();

        public event ItemClickEventHandler ItemClick;

        public delegate Task ItemClickEventHandler(aVersionListItem sender);

        public aVersionList()
        {
            InitializeComponent();
        }

        public void DrawItems()
        {
            // draw items
            flpMain.Controls.Clear();
            int flpMainHeight = 0;
            flpMain.Width = plMain.Width;

            foreach (aVersionListItem entry in Items)
            {
                // set up items
                entry.Width = flpMain.Width - 6;
                entry.Margin = new Padding(3, 0, 3, 0);
                entry.ItemClick += ItemClick_Handler;

                flpMain.Controls.Add(entry);
                flpMainHeight += 40;
            }

            flpMain.Height = flpMainHeight;
        }

        public void DeactivateAll()
        {
            foreach (Control entry in flpMain.Controls)
            {
                try
                {
                    aVersionListItem ItemControl = (aVersionListItem)entry;
                    ItemControl.Deactivate();
                }
                catch
                {
                    // ignore error
                }
            }
        }

        public void SelectItem(string versionID, bool click = true)
        {
            // jump to corresponding item
            foreach (var control in flpMain.Controls)
            {
                try
                {
                    aVersionListItem ItemControl = (aVersionListItem)control;
                    if ((ItemControl.VersionID ?? "") == (versionID ?? ""))
                    {
                        flpMain.ScrollControlIntoView(ItemControl);
                        DeactivateAll();
                        ItemControl.Activate();

                        if (click)
                        {
                            ItemClick?.Invoke(ItemControl);
                        }
                        return;
                    }
                }
                catch
                {
                    // ignore error
                }
            }
        }

        private void ItemClick_Handler(aVersionListItem sender)
        {
            // Handler weitergeben
            DeactivateAll();
            sender.Activate();
            ItemClick?.Invoke(sender);
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