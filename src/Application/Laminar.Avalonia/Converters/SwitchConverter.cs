using System.Globalization;
using Avalonia.Data.Converters;

namespace Laminar.Avalonia.Converters;

public class SwitchConverter : IMultiValueConverter
{
    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        var targetValue = values.FirstOrDefault();
        
        foreach (var value in values.Skip(1))
        {
            if (value is SwitchItem { SwitchKey: not null } switchItem && switchItem.SwitchKey.Equals(targetValue))
            {
                return switchItem.Item;
            }
        }

        return parameter;
    }
}