using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;

namespace Laminar.Avalonia.Converters;

public class TreeViewItemIndentConverter : IMultiValueConverter
{
    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Count > 1 && values[0] is int level && values[1] is double indent)
        {
            return new Thickness(indent * level, 0, 0, values.Count > 2 && values[2] is double bottom ? bottom : 0);
        }

        return new Thickness(0);
    }
}