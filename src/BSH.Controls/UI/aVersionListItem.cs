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
using System.ComponentModel;
using System.Drawing;
using Brightbits.BSH.Engine.Models;

namespace Brightbits.BSH.Main
{
    public partial class aVersionListItem
    {
        public aVersionListItem()
        {
            InitializeComponent();
        }

        public event ItemClickEventHandler ItemClick;

        public delegate void ItemClickEventHandler(aVersionListItem sender);

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public bool VersionStable
        {
            get
            {
                return lblStable.Visible;
            }
            set
            {
                lblStable.Visible = value;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public VersionDetails Version
        {
            get; set;
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public string ToolTipTitle
        {
            get
            {
                return tt.ToolTipTitle;
            }
            set
            {
                tt.ToolTipTitle = value;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public string ToolTip
        {
            get
            {
                return tt.Tag.ToString();
            }
            set
            {
                tt.Tag = value;
                tt.RemoveAll();
                if (string.IsNullOrEmpty(value))
                {
                    return;
                }

                tt.SetToolTip(this, value);
                tt.SetToolTip(lblVersionID, value);
                tt.SetToolTip(lblVersionDate, value);
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public string VersionID
        {
            get
            {
                return lblVersionID.Text;
            }
            set
            {
                lblVersionID.Text = value;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public string VersionDate
        {
            get
            {
                return lblVersionDate.Text;
            }
            set
            {
                lblVersionDate.Text = value;
            }
        }

        public int GetWidth()
        {
            return lblVersionDate.Left + lblVersionDate.Width + 10;
        }

        public void Activate()
        {
            BackColor = Color.LightGray;
        }

        public void Deactivate()
        {
            BackColor = Color.Transparent;
        }

        private void aVersionListItem_Click(object sender, EventArgs e)
        {
            ItemClick?.Invoke(this);
        }

        private void lblVersionID_Click(object sender, EventArgs e)
        {
            ItemClick?.Invoke(this);
        }

        private void lblVersionTitle_Click(object sender, EventArgs e)
        {
            ItemClick?.Invoke(this);
        }

        private void lblVersionDate_Click(object sender, EventArgs e)
        {
            ItemClick?.Invoke(this);
        }
    }
}