using System;
using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;

namespace Laminar.Avalonia.Converters;

public class MillisecondsFromTimespanConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not TimeSpan timeSpan)
        {
            return new BindingNotification(new InvalidCastException(nameof(value)), BindingErrorType.Error);
        }

        return timeSpan.TotalMilliseconds;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not double milliseconds)
        {
            return new BindingNotification(new InvalidCastException(nameof(value)), BindingErrorType.Error);
        }

        return TimeSpan.FromMilliseconds(milliseconds);
    }
}