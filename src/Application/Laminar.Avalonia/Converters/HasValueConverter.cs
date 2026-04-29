using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace Laminar.Avalonia.Converters;

public class HasValueConverter : IValueConverter
{
    private static readonly NullOrEmptyConverter NullOrEmptyConverter = new();
    
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) 
        => NullOrEmptyConverter.Convert(value, targetType, parameter, culture) is false;

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new InvalidOperationException();
    }
}