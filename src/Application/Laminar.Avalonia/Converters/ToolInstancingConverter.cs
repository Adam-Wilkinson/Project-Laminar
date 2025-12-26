using System;
using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Laminar.Avalonia.ToolSystem;

namespace Laminar.Avalonia.Converters;

public class ToolInstancingConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (parameter is not Tool template)
        {
            return new BindingNotification(new ArgumentException($@"{nameof(parameter)} must be a {nameof(Tool)}", nameof(parameter)), BindingErrorType.Error);
        }

        return template.Build(value);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not ToolInstance instanceInfo)
        {
            return new BindingNotification(
                new ArgumentException($"{nameof(value)} must be of type {nameof(ToolInstance)}"), BindingErrorType.Error);
        }

        return instanceInfo.DataContext;
    }
}