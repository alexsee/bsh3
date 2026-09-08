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

    public static DateTime ReformatVersionDate(string date)
    {
        ArgumentNullException.ThrowIfNull(date);

        date = date.Replace("-", "", StringComparison.OrdinalIgnoreCase).Replace(" ", "", StringComparison.OrdinalIgnoreCase);
        date = date.Insert(2, ".");
        date = date.Insert(5, ".");
        date = date.Insert(10, " ");
        date = date.Insert(13, ":");
        date = date.Insert(16, ":");
        return Convert.ToDateTime(date, System.Globalization.CultureInfo.CreateSpecificCulture("de-DE"));
    }
}
