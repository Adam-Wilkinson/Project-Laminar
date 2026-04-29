using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;

namespace Laminar.Avalonia.Converters;

public class DictionaryGetter : IMultiValueConverter
{
    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values[0] is not IDictionary dictionary || values[1] is not { } key)
            return new BindingNotification(new InvalidCastException(), BindingErrorType.Error);

        return dictionary[key];
    }
}