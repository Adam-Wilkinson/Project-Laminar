using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace Laminar_Avalonia.Converters;

public class BooleanNot : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue && targetType == typeof(bool))
        {
            return !boolValue;
        }

        return null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
