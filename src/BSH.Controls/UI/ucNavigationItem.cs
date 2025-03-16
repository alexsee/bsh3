// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

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