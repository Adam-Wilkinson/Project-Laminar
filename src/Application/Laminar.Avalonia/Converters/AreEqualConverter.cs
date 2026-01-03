using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Data.Converters;

namespace Laminar.Avalonia.Converters;

public class AreEqualConverter : IMultiValueConverter
{
    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Count == 0)
        {
            return true;
        }

        var lastVal = values[0];
        foreach (var value in values)
        {
            if (lastVal is double lastValDouble && value is double valueDouble &&
                (lastValDouble - valueDouble) < double.Epsilon)
            {
                lastVal = value;
                continue;
            }
            
            if (!Equals(value, lastVal))
            {
                return false;
            }
            
            lastVal = value;
        }

        return true;
    }
}