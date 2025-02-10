using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;

namespace Laminar.Avalonia.Converters;

public class TernaryConverter : IMultiValueConverter
{
    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Count != 3)
            return new BindingNotification(new ArgumentException("Ternary operator needs three arguments",
                nameof(values)));

        return values[0] is true ? values[1] : values[2];
    }
}