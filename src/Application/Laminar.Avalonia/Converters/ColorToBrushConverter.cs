using System;
using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace Laminar.Avalonia.Converters;

public class ColorToBrushConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not Color color)
        {
            return new BindingNotification(new ArgumentException($"ColorToBrush requires a color input, not a {value?.GetType()}"), BindingErrorType.Error);
        }
            
        return new SolidColorBrush(color);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not SolidColorBrush brush)
        {
            return new BindingNotification(new ArgumentException($"ColorToBrush requires a SolidColorBrush output, not a {value?.GetType()}"), BindingErrorType.Error);
        }
        
        return brush.Color;
    }
}