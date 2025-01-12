// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace BSH.MainApp.Converters;

internal sealed class VisibilityInvertConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is bool isVisible)
        {
            return isVisible ? Visibility.Collapsed : Visibility.Visible;
        }

        return (Visibility)value == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}