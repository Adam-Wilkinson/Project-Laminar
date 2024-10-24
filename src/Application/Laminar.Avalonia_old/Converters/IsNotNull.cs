namespace Laminar_Avalonia.Converters;

using System;
using System.Globalization;
using Avalonia.Data.Converters;

public class IsNotNull : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value != null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return null;
    }
}
