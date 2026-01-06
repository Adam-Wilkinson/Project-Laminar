using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;

namespace Laminar.Avalonia.Converters;

public class MultiMultiplyConverter : IMultiValueConverter
{
    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        double result = 1;
        foreach (object? value in values)
        {
            switch (value)
            {
                case double doubleVal:
                    result *= doubleVal;
                    break;
                case int intVal:
                    result *= intVal;
                    break;
                default:
                    return new BindingNotification(new ArgumentException("MultiMultiplyConverter requires all number inputs"), BindingErrorType.Error);
            }
        }

        return result;
    }
}