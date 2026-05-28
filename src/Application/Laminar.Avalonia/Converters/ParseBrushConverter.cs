using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace Laminar.Avalonia.Converters;

public class ParseBrushConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is string inputString
            ? Brush.Parse(inputString)
            : new BindingNotification(new InvalidCastException(), BindingErrorType.Error);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is Color color
            ? color.ToString()
            : new BindingNotification(new InvalidCastException(), BindingErrorType.Error);
    }
}