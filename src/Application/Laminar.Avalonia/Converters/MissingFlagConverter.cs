using System.Globalization;
using Avalonia.Data.Converters;

namespace Laminar.Avalonia.Converters;

public class MissingFlagConverter : IValueConverter
{
    private readonly HasFlagConverter _hasFlagConverter = new();
    
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => _hasFlagConverter.Convert(value, targetType, parameter, culture) is false;

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new InvalidOperationException();
    }
}