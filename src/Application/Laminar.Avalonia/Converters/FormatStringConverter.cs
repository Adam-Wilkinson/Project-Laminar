using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;

namespace Laminar.Avalonia.Converters;

public class FormatStringConverter : IMultiValueConverter
{
    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values[0] is not string formatString)
        {
            return new BindingNotification(new InvalidCastException(), BindingErrorType.Error);
        }

        return values.Count switch
        {
            1 => string.Format(culture, formatString),
            2 => string.Format(culture, formatString, values[1]),
            3 => string.Format(culture, formatString, values[1], values[2]),
            4 => string.Format(culture, formatString, values[1], values[2], values[3]),
            _ => string.Format(culture, formatString, values.Skip(1).ToArray()) 
        };
    }
}