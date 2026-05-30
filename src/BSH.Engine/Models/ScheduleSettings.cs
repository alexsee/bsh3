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

    public bool DeleteSchedule(ScheduleEntry entry)
    {
        return Entries.Remove(entry);
    }

    public bool DeleteSchedule(DateTime date, ScheduleEntryKind kind)
    {
        var entry = Entries.FirstOrDefault(x => x.Type == (int)kind && x.Date == date);
        return entry != null && Entries.Remove(entry);
    }
}
