using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;

namespace Laminar.Avalonia.Converters;

public class HasFlagConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is null || !value.GetType().IsEnum || parameter is null || value.GetType() != parameter.GetType())
        {
            return new BindingNotification(new Exception("Input is not an enum type"), BindingErrorType.Error);
        }

        return ((int)value & (int)parameter) != 0;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new InvalidOperationException();
    }
}