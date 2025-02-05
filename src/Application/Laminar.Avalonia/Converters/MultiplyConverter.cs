using System;
using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;

namespace Laminar.Avalonia.Converters;

public class MultiplyConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not double doubleVal || parameter is not double doubleParam) return new BindingNotification(new InvalidCastException(), BindingErrorType.Error);

        return doubleVal * doubleParam;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not double doubleVal || parameter is not double doubleParam) return new BindingNotification(new InvalidCastException(), BindingErrorType.Error);

        return doubleVal / doubleParam;
    }
}