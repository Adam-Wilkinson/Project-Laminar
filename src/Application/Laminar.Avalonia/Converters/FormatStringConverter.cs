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

        return string.Format(culture, formatString, values.Skip(1));
    }
}