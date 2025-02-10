using System;
using System.Globalization;
using Avalonia;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Metadata;

namespace Laminar.Avalonia.Converters;

public record SwitchItem(object? SwitchKey, object? Item);

public class SwitchItemConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) => new SwitchItem(parameter, value);

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is SwitchItem switchItem ? switchItem.Item : new BindingNotification(new InvalidCastException(), BindingErrorType.Error);
    }
}

public class SwitchItemBinding : Binding
{
    public SwitchItemBinding()
    {
        Converter = new SwitchItemConverter();
    }

    public required object? SwitchKey
    {
        get => ConverterParameter;
        set => ConverterParameter = value;
    }

    [Content]
    public object? SourceContent
    {
        get => Source;
        set => Source = value;
    }
}