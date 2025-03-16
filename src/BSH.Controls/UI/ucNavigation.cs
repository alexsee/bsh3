// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

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

        private void event_ItemClick(string sPath)
        {
            ItemClick?.Invoke(sPath);
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