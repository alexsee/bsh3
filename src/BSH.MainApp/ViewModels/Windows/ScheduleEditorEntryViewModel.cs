// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System.Globalization;
using Brightbits.BSH.Engine.Models;

namespace BSH.MainApp.ViewModels.Windows;

public sealed class ScheduleEditorEntryViewModel
{
    public ScheduleEditorEntryViewModel(ScheduleEntry entry)
    {
        Entry = entry;
    }

    public ScheduleEntry Entry
    {
        get;
    }

    public ScheduleEntryKind Kind => (ScheduleEntryKind)Entry.Type;

    public string RepeatText => Kind switch
    {
        ScheduleEntryKind.Once => "Once",
        ScheduleEntryKind.Hourly => "Hourly",
        ScheduleEntryKind.Daily => "Daily",
        ScheduleEntryKind.Weekly => "Weekly",
        ScheduleEntryKind.Monthly => "Monthly",
        _ => "Unknown",
    };

    public string SummaryText => Kind switch
    {
        ScheduleEntryKind.Once => $"Once on {Entry.Date:g}",
        ScheduleEntryKind.Hourly => $"Every hour at {Entry.Date.Minute:00} minutes past",
        ScheduleEntryKind.Daily => $"Every day at {Entry.Date:t}",
        ScheduleEntryKind.Weekly => $"Every {Entry.Date:dddd} at {Entry.Date:t}",
        ScheduleEntryKind.Monthly => $"Day {Entry.Date.Day} of every month at {Entry.Date:t}",
        _ => Entry.Date.ToString("g", CultureInfo.CurrentCulture),
    };
}
