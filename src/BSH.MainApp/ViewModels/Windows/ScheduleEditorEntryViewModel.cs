// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System.Globalization;
using Brightbits.BSH.Engine.Models;
using BSH.MainApp.Helpers;
using CommunityToolkit.WinUI;

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

    public string RepeatText => ScheduleEditorDisplayText.GetScheduleKind(Kind);

    public string ScheduleText => Kind switch
    {
        ScheduleEntryKind.Once => Entry.Date.ToString("g", CultureInfo.CurrentCulture),
        ScheduleEntryKind.Hourly => string.Format("Schedule_Text_AtMinute".GetLocalized() ?? "Schedule_Text_AtMinute", Entry.Date.Minute.ToString("00", CultureInfo.CurrentCulture)),
        ScheduleEntryKind.Daily => Entry.Date.ToString("t", CultureInfo.CurrentCulture),
        ScheduleEntryKind.Weekly => $"{Entry.Date:dddd}, {Entry.Date:t}",
        ScheduleEntryKind.Monthly => string.Format("Schedule_Text_DayOfMonth".GetLocalized() ?? "Schedule_Text_DayOfMonth", Entry.Date.Day, Entry.Date.ToString("t", CultureInfo.CurrentCulture)),
        _ => Entry.Date.ToString("g", CultureInfo.CurrentCulture),
    };

    public string AccessibleText => $"{RepeatText}: {ScheduleText}";
}
