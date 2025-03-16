// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;

namespace Brightbits.BSH.Engine.Utils;

public static class DateUtils
{
    public static DateTime GetDateToWeekDay(DayOfWeek weekDay, DateTime date)
    {
        if (date.DayOfWeek == weekDay)
        {
            return date;
        }
        else
        {
            return GetDateToWeekDay(weekDay, date.Subtract(new TimeSpan(1, 0, 0, 0)));
        }
    }

    public static DateTime GetDateToMonth(int day, DateTime date)
    {
        if (date.Day == day)
        {
            return date;
        }
        else
        {
            return GetDateToMonth(day, date.Subtract(new TimeSpan(1, 0, 0, 0)));
        }
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
