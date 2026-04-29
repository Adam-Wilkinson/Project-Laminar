using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Laminar.Avalonia.ToolSystem;

namespace Laminar.Avalonia.Converters;

public class QuickAccessInstancerConverter : IMultiValueConverter
{
    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values[0] is not { } target || values[1] is not Dictionary<string, Toolbox> repository || values[2] is not string key)
            return new BindingNotification(new InvalidCastException(), BindingErrorType.Error);

        return repository[key].Build(target);
    }
}