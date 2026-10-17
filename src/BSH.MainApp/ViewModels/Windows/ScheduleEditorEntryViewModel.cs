// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System.Windows.Input;
using Brightbits.BSH.Engine.Models;
using BSH.MainApp.Helpers;

namespace BSH.MainApp.ViewModels.Windows;

public sealed class ScheduleEditorEntryViewModel
{
    public ScheduleEditorEntryViewModel(ScheduleEntry entry, ICommand deleteCommand)
    {
        Entry = entry;
        DeleteCommand = deleteCommand;
    }

    public ScheduleEntry Entry
    {
        get;
    }

    public ICommand DeleteCommand
    {
        get;
    }

    public ScheduleEntryKind Kind => (ScheduleEntryKind)Entry.Type;

    public string Text => ScheduleEditorDisplayText.DescribeEntry(Entry);

    public string Glyph => Kind == ScheduleEntryKind.Once ? "\uE787" : "\uE895";
}
