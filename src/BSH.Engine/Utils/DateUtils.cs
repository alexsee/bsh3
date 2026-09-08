// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;

namespace Brightbits.BSH.Engine.Utils;

public static class DateUtils
{
    public static DateTime GetDateToWeekDay(DayOfWeek weekDay, DateTime date)
    {
        var daysBack = ((int)date.DayOfWeek - (int)weekDay + 7) % 7;
        return date.AddDays(-daysBack);
    }

    public static DateTime GetDateToMonth(int day, DateTime date)
    {
        var current = date;
        while (current.Day != day)
        {
            current = current.AddDays(-1);
        }

        return current;
    }
}
