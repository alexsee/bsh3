// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using Humanizer;
using Microsoft.UI.Xaml.Data;

namespace BSH.MainApp.Converters;

public class FileSizeConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return value switch
        {
            double d => d.Bytes().ToString(),
            long l => l.Bytes().ToString(),
            int i => ((long)i).Bytes().ToString(),
            _ => value
        };
    }
    public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotImplementedException();
}
