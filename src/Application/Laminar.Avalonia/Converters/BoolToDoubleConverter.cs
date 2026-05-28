using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;

namespace Laminar.Avalonia.Converters;

public class BoolToDoubleConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not bool boolValue)
            return new BindingNotification(new InvalidCastException(), BindingErrorType.Error);
        
        return boolValue ? 1.0 : 0.0;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not double doubleValue)
            return new BindingNotification(new InvalidCastException(), BindingErrorType.Error);
        
        return doubleValue >= 0.9;
    }
}