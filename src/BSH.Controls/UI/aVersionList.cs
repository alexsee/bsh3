// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Collections.Generic;
using System.ComponentModel;
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