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

using System;

namespace Brightbits.BSH.Main
{
    public partial class ucNavigationItem
    {
        public ucNavigationItem()
        {
            InitializeComponent();
        }

        public event ItemClickEventHandler ItemClick;

        public delegate void ItemClickEventHandler(string sPath);

        public int getWidth()
        {

            // Größe anpassen
            return lblText.Left + lblText.Width;
        }

        private void ucNavigationItem_Click(object sender, EventArgs e)
        {
            ItemClick?.Invoke(Tag.ToString());
        }

        private void lblText_Click(object sender, EventArgs e)
        {
            ItemClick?.Invoke(Tag.ToString());
        }

        private void Label1_Click(object sender, EventArgs e)
        {
            ItemClick?.Invoke(Tag.ToString());
        }
    }
}