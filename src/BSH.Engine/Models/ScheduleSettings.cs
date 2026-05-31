// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Brightbits.BSH.Engine.Models;

public sealed class ScheduleSettings
{
    public List<ScheduleEntry> Entries
    {
        get;
    } = new();

    public ScheduleRetentionMode RetentionMode
    {
        get; set;
    }

    public ScheduleRetentionIntervalUnit RetentionIntervalUnit
    {
        get; set;
    } = ScheduleRetentionIntervalUnit.Day;

    public int RetentionInterval
    {
        get; set;
    } = 1;

    public int AutomaticHourlyBackupThreshold
    {
        get; set;
    } = 24;

    public bool EnableScheduledFullBackups
    {
        get; set;
    }

    public int ScheduledFullBackupDays
    {
        get; set;
    } = 1;

    public bool PerformMissedBackupsLater
    {
        get; set;
    }

    public ScheduleEntry AddSchedule(ScheduleEntryKind kind, DateTime date)
    {
        var entry = new ScheduleEntry
        {
            Type = (int)kind,
            Date = date
        };

        Entries.Add(entry);
        return entry;
    }

    public ScheduleEntry AddSchedule(
        ScheduleEntryKind kind,
        DateTimeOffset startDate,
        TimeSpan startTime,
        DayOfWeek weeklyDay,
        int monthlyDay)
    {
        return AddSchedule(kind, BuildScheduleDate(kind, startDate, startTime, weeklyDay, monthlyDay));
    }

    public static DateTime BuildScheduleDate(
        ScheduleEntryKind kind,
        DateTimeOffset startDate,
        TimeSpan startTime,
        DayOfWeek weeklyDay,
        int monthlyDay)
    {
        var date = startDate.Date;
        return kind switch
        {
            ScheduleEntryKind.Once => date.Add(startTime),
            ScheduleEntryKind.Hourly => date.Add(startTime),
            ScheduleEntryKind.Daily => date.Add(startTime),
            ScheduleEntryKind.Weekly => GetWeeklyDate(date, startTime, weeklyDay),
            ScheduleEntryKind.Monthly => GetMonthlyDate(date, startTime, monthlyDay),
            _ => date.Add(startTime),
        };
    }

    public bool DeleteSchedule(ScheduleEntry entry)
    {
        return Entries.Remove(entry);
    }

    public bool DeleteSchedule(DateTime date, ScheduleEntryKind kind)
    {
        var entry = Entries.FirstOrDefault(x => x.Type == (int)kind && x.Date == date);
        return entry != null && Entries.Remove(entry);
    }

    private static DateTime GetWeeklyDate(DateTime baseDate, TimeSpan startTime, DayOfWeek weeklyDay)
    {
        var daysUntilSelectedDay = ((int)weeklyDay - (int)baseDate.DayOfWeek + 7) % 7;
        return baseDate.AddDays(daysUntilSelectedDay).Add(startTime);
    }

    private static DateTime GetMonthlyDate(DateTime baseDate, TimeSpan startTime, int monthlyDay)
    {
        var day = Math.Clamp(monthlyDay, 1, DateTime.DaysInMonth(baseDate.Year, baseDate.Month));
        return new DateTime(baseDate.Year, baseDate.Month, day).Add(startTime);
    }
}
