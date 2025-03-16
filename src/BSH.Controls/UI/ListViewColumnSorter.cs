// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Collections;
using System.Windows.Forms;

namespace Brightbits.BSH.Controls.UI
{
    /// <summary>
    /// This class is an implementation of the 'IComparer' interface.
    /// </summary>
    public class ListViewColumnSorter : IComparer
    {
        private readonly IComparer dateTimeComparer = new DateTimeComparer();
        private readonly IComparer stringComparer = new CaseInsensitiveComparer();

        /// <summary>
        /// Class constructor.  Initializes various elements
        /// </summary>
        public ListViewColumnSorter()
        {
            // Initialize the column to '0'
            SortColumn = 0;

            // Initialize the sort order to 'none'
            Order = SortOrder.None;
        }

        /// <summary>
        /// This method is inherited from the IComparer interface.  It compares the two objects passed using a case insensitive comparison.
        /// </summary>
        /// <param name="x">First object to be compared</param>
        /// <param name="y">Second object to be compared</param>
        /// <returns>The result of the comparison. "0" if equal, negative if 'x' is less than 'y' and positive if 'x' is greater than 'y'</returns>
        public int Compare(object x, object y)
        {
            int compareResult;
            ListViewItem listviewX, listviewY;

            // Cast the objects to be compared to ListViewItem objects
            listviewX = (ListViewItem)x;
            listviewY = (ListViewItem)y;

            // Compare the two items
            try
            {
                if (listviewX.SubItems.Count <= SortColumn || listviewY.SubItems.Count <= SortColumn)
                {
                    return 0;
                }

                if (SortColumn == 0)
                {
                    compareResult = stringComparer.Compare(listviewX.Text, listviewY.Text);
                }
                else if (listviewX.SubItems[SortColumn].Tag is DateTime &&
                    listviewY.SubItems[SortColumn].Tag is DateTime)
                {
                    compareResult = dateTimeComparer.Compare(listviewX.SubItems[SortColumn].Tag, listviewY.SubItems[SortColumn].Tag);
                }
                else if (listviewX.SubItems[SortColumn].Tag is string &&
                    listviewY.SubItems[SortColumn].Tag is string)
                {
                    compareResult = stringComparer.Compare(listviewX.SubItems[SortColumn].Tag, listviewY.SubItems[SortColumn].Tag);
                }
                else if (listviewX.SubItems[SortColumn].Tag is double &&
                    listviewY.SubItems[SortColumn].Tag is double)
                {
                    compareResult = ((double)listviewX.SubItems[SortColumn].Tag).CompareTo((double)listviewY.SubItems[SortColumn].Tag);
                }
                else
                {
                    compareResult = 0;
                }
            }
            catch
            {
                compareResult = 0;
            }

            // Calculate correct return value based on object comparison
            if (Order == SortOrder.Ascending)
            {
                // Ascending sort is selected, return normal result of compare operation
                return compareResult;
            }
            else if (Order == SortOrder.Descending)
            {
                // Descending sort is selected, return negative result of compare operation
                return -compareResult;
            }
            else
            {
                // Return '0' to indicate they are equal
                return 0;
            }
        }

        /// <summary>
        /// Gets or sets the number of the column to which to apply the sorting operation (Defaults to '0').
        /// </summary>
        public int SortColumn
        {
            set; get;
        }

        /// <summary>
        /// Gets or sets the order of sorting to apply (for example, 'Ascending' or 'Descending').
        /// </summary>
        public SortOrder Order
        {
            set; get;
        }

    }
}