using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;

namespace Laminar.Avalonia.Converters;

public class CollectionContainsConverter : IMultiValueConverter
{
    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values[0] is not IEnumerable enumerable)
            return new BindingNotification(new InvalidCastException(), BindingErrorType.Error);

        var enumerator = enumerable.GetEnumerator();
        using var disposable = (IDisposable)enumerator;
        
        while (enumerator.MoveNext())
        {
            if (enumerator.Current?.Equals(values[1]) == true) return true;
        }

        return false;
    }
}