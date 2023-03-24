using Microsoft.UI.Xaml.Data;

namespace BSH.MainApp.Helpers;

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
