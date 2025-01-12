using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml;

namespace BSH.MainApp.Converters;

internal sealed class VisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is bool isVisible)
        {
            return isVisible ? Visibility.Visible : Visibility.Collapsed;
        }

        return (Visibility)value == Visibility.Collapsed ? Visibility.Collapsed : Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
