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
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Brightbits.BSH.Main
{
    public partial class ucNavigation
    {
        public ucNavigation()
        {
            InitializeComponent();
        }

        public event ItemClickEventHandler ItemClick;

        public delegate Task ItemClickEventHandler(string sPath);

        public void CreateNavi(string CompletePath, bool NoClick = false)
        {
            // clear all controls
            flpNavi.Controls.Clear();

            // construct new path controls
            string[] sPath = txtPath.Text.Split('\\');
            string[] sPathLocalized = PathLocalized.Split('\\');

            if (sPath.Length != sPathLocalized.Length)
            {
                sPathLocalized = sPath;
            }

            CompletePath = CompletePath.Replace(Path, "");

            for (int i = 0; i < sPath.Length; i++)
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
                string tmpPath = "";
                for (int j = 0; j <= i; j++)
                {
                    tmpPath += sPath[j] + @"\";
                }

                newEntry.Tag = tmpPath;
                if (!NoClick)
                {
                    newEntry.ItemClick += event_ItemClick;
                }

                flpNavi.Controls.Add(newEntry);              
            }

            ucNavigation_Resize(null, null);
        }

        private void event_ItemClick(string sPath)
        {
            ItemClick?.Invoke(sPath);
        }

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

        public string PathLocalized { get; set; }

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