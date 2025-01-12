// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using Microsoft.UI.Xaml.Data;

namespace BSH.MainApp.Converters;

public class BooleanInverterConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return value is bool ? !((bool)value) : (object)false;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        return value is bool ? !((bool)value) : (object)false;
    }
}
