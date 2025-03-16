// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Collections;

namespace Brightbits.BSH.Controls.UI
{
    public class DateTimeComparer : IComparer
    {
        public int Compare(object x, object y)
        {
            DateTime dateX = Convert.ToDateTime(x);
            DateTime dateY = Convert.ToDateTime(y);

            return DateTime.Compare(dateX, dateY);
        }
    }
}
