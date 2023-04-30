using Humanizer;
using Microsoft.UI.Xaml.Data;

namespace BSH.MainApp.Helpers;
public class FileSizeConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return value is double ? ((double)value).Bytes() : value;
    }
    public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotImplementedException();
}
