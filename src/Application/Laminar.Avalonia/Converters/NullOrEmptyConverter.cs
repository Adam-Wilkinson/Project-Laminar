using System;
using System.Collections;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Input;

namespace Laminar.Avalonia.Converters;

public class NullOrEmptyConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value switch
        {
            ICollection collection => collection.Count == 0,
            string str => string.IsNullOrWhiteSpace(str),
            KeyGesture keyGesture => keyGesture is { Key: Key.None, KeyModifiers: KeyModifiers.None },
            bool boolean => boolean,
            _ => value is null
        };

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}