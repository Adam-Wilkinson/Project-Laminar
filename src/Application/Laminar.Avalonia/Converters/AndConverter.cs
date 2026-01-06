using System;
using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;

namespace Laminar.Avalonia.Converters;

public class AndConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not bool valueBool || parameter is not bool parameterBool)
        {
            return new BindingNotification(new Exception("And converter requires to boolean values"),
                BindingErrorType.Error);
        }

        return valueBool && parameterBool;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value;
    }
}