using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Avalonia.Data.Converters;

namespace Laminar.Avalonia.Converters;

public class ConcatenateStringsConverter : IMultiValueConverter
{
    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        StringBuilder builder = new();
        
        foreach (var value in values)
        {
            builder.Append(value);
        }

        return builder.ToString();
    }
}