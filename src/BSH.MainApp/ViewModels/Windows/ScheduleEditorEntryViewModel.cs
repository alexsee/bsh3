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

    public string ScheduleText => Kind switch
    {
        ScheduleEntryKind.Once => Entry.Date.ToString("g", CultureInfo.CurrentCulture),
        ScheduleEntryKind.Hourly => $"At minute {Entry.Date.Minute:00}",
        ScheduleEntryKind.Daily => Entry.Date.ToString("t", CultureInfo.CurrentCulture),
        ScheduleEntryKind.Weekly => $"{Entry.Date:dddd}, {Entry.Date:t}",
        ScheduleEntryKind.Monthly => $"Day {Entry.Date.Day}, {Entry.Date:t}",
        _ => Entry.Date.ToString("g", CultureInfo.CurrentCulture),
    };
}
