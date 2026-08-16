// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System.Globalization;
using Brightbits.BSH.Engine.Models;
using CommunityToolkit.WinUI;
using Microsoft.UI.Xaml.Data;

namespace BSH.MainApp.Converters;

public sealed class ScheduleOptionDisplayConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return value switch
        {
            ScheduleEntryKind kind => ScheduleEditorDisplayText.GetScheduleKind(kind),
            ScheduleRetentionMode mode => $"Schedule_RetentionMode_{mode}".GetLocalized() ?? mode.ToString(),
            ScheduleRetentionIntervalUnit unit => $"Schedule_RetentionUnit_{unit}".GetLocalized() ?? unit.ToString(),
            DayOfWeek day => CultureInfo.CurrentCulture.DateTimeFormat.GetDayName(day),
            _ => value?.ToString() ?? string.Empty,
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotSupportedException();
    }
}
