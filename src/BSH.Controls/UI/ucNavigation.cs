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
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Brightbits.BSH.Main
{
    public partial class ucNavigation
    {
        public ucNavigation()
        {
            InitializeComponent();
            flpNavi.ControlRemoved += ItemRemoved;
        }

        public event ItemClickEventHandler ItemClick;

        public delegate Task ItemClickEventHandler(string sPath);

        public void CreateNavi(string CompletePath, bool NoClick = false)
        {
            while (flpNavi.Controls.Count > 0)
            {
                var entry = flpNavi.Controls[0];
                flpNavi.Controls.Remove(entry);
                entry.Dispose();
            }

            // construct new path controls
            string[] sPath = txtPath.Text.Split('\\');
            string[] sPathLocalized = PathLocalized.Split('\\');

            if (sPath.Length != sPathLocalized.Length)
            {
                sPathLocalized = sPath;
            }

            CompletePath = CompletePath.Replace(Path, "");

            for (var i = 0; i < sPath.Length; i++)
            {
                // create item
                var newEntry = new ucNavigationItem();
                if (i == 0)
                {
                    newEntry.lblText.Text = CompletePath + sPathLocalized[i];
                }
                else
                {
                    newEntry.lblText.Text = sPathLocalized[i];
                }

                newEntry.Height = flpNavi.Height;
                newEntry.Width = newEntry.getWidth();

                // build original path
                var tmpPath = new StringBuilder();
                for (var j = 0; j <= i; j++)
                {
                    tmpPath.Append(sPath[j] + @"\");
                }

                newEntry.Tag = tmpPath.ToString();
                if (!NoClick)
                {
                    newEntry.ItemClick += event_ItemClick;
                }

                flpNavi.Controls.Add(newEntry);
            }

            ucNavigation_Resize(null, null);
        }

        private void ItemRemoved(object sender, ControlEventArgs e)
        {
            if (e.Control is ucNavigationItem entry)
            {
                entry.ItemClick -= event_ItemClick;
            }
        }

        private async void event_ItemClick(string sPath)
        {
            var handlers = ItemClick;
            if (handlers != null)
            {
                foreach (ItemClickEventHandler handler in handlers.GetInvocationList())
                {
                    await handler(sPath);
                }
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public string Path
        {
            get
            {
                return txtPath.Text;
            }

            set
            {
                txtPath.Text = value;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public string PathLocalized
        {
            get; set;
        }

        private void ucNavigation_Resize(object sender, EventArgs e)
        {
            // adjust width
            int maxWidth = 0;
            foreach (Control entry in flpNavi.Controls)
            {
                maxWidth += entry.Width;
            }

            flpNavi.Width = maxWidth;

            // reset navigation area
            if (maxWidth > plNavi.Width)
            {
                flpNavi.Left = plNavi.Width - maxWidth - 10;
            }
            else
            {
                flpNavi.Left = 0;
            }
        }
    }
}