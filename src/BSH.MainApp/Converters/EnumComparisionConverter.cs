// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace BSH.MainApp.Converters;

public class EnumComparisonConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value == null || parameter == null)
        {
            return false;
        }

        return value.Equals(parameter) || ((int)value).Equals((int)parameter);
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        return value?.Equals(true) == true ? parameter : DependencyProperty.UnsetValue;
    }
}
