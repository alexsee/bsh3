// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System.Globalization;
using Brightbits.BSH.Engine.Models;
using CommunityToolkit.WinUI;

namespace BSH.MainApp.Helpers;

public static class ScheduleEditorDisplayText
{
    public static string GetScheduleKind(ScheduleEntryKind kind)
    {
        return $"Schedule_Kind_{kind}".GetLocalized() ?? kind.ToString();
    }

    public static string GetRetentionUnit(ScheduleRetentionIntervalUnit unit)
    {
        return $"Schedule_RetentionUnit_{unit}".GetLocalized() ?? unit.ToString();
    }

    public static string DescribeEntry(ScheduleEntry entry)
    {
        var date = entry.Date;
        var time = date.ToString("t", CultureInfo.CurrentCulture);
        return (ScheduleEntryKind)entry.Type switch
        {
            ScheduleEntryKind.Once => Format("Schedule_Entry_Once", date.ToString("g", CultureInfo.CurrentCulture)),
            ScheduleEntryKind.Hourly => Format("Schedule_Entry_Hourly", date.Minute.ToString("00", CultureInfo.CurrentCulture)),
            ScheduleEntryKind.Daily => Format("Schedule_Entry_Daily", time),
            ScheduleEntryKind.Weekly => Format("Schedule_Entry_Weekly", CultureInfo.CurrentCulture.DateTimeFormat.GetDayName(date.DayOfWeek), time),
            ScheduleEntryKind.Monthly => Format("Schedule_Entry_Monthly", date.Day, time),
            _ => date.ToString("g", CultureInfo.CurrentCulture),
        };
    }

    public static string DescribeRetention(ScheduleSettings settings)
    {
        return settings.RetentionMode switch
        {
            ScheduleRetentionMode.Automatic => "Schedule_Summary_KeepAuto".GetLocalized(),
            ScheduleRetentionMode.Interval => Format("Schedule_Summary_KeepInterval", Math.Max(1, settings.RetentionInterval), GetRetentionUnit(settings.RetentionIntervalUnit)),
            _ => "Schedule_Summary_KeepAll".GetLocalized(),
        };
    }

    /// <summary>
    /// One line for the backup mode card, e.g. "Every day at 14:00 · Old backups thinned out automatically".
    /// </summary>
    public static string Summarize(ScheduleSettings settings)
    {
        var times = settings.Entries.Count switch
        {
            0 => "Schedule_Summary_None".GetLocalized(),
            1 => DescribeEntry(settings.Entries[0]),
            var count => Format("Schedule_Summary_Times", count),
        };

        return $"{times} · {DescribeRetention(settings)}";
    }

    private static string Format(string resourceKey, params object[] args)
    {
        return string.Format(CultureInfo.CurrentCulture, resourceKey.GetLocalized() ?? resourceKey, args);
    }
}
