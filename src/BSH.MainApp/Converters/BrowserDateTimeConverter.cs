// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System.Globalization;
using CommunityToolkit.WinUI;
using Microsoft.UI.Xaml.Data;

namespace BSH.MainApp.Converters;

public class BrowserDateTimeConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is not DateTime date)
        {
            return string.Empty;
        }

        if (date.Kind == DateTimeKind.Utc)
        {
            date = date.ToLocalTime();
        }

        if (parameter is not "Version")
        {
            return date.ToString("g", CultureInfo.CurrentCulture);
        }

        var time = date.ToString("HH:mm", CultureInfo.CurrentCulture);
        if (date.Date == DateTime.Today)
        {
            return string.Format("Browser_Date_Today".GetLocalized() ?? "Today, {0}", time);
        }

        if (date.Date == DateTime.Today.AddDays(-1))
        {
            return string.Format("Browser_Date_Yesterday".GetLocalized() ?? "Yesterday, {0}", time);
        }

        return date.ToString("d. MMMM yyyy HH:mm", CultureInfo.CurrentCulture);
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotSupportedException();
}
