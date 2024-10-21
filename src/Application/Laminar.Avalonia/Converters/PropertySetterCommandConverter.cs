using System;
using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;
using ReactiveUI;

namespace Laminar.Avalonia.Converters;

public class PropertySetterCommandConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (parameter is not AvaloniaProperty avaloniaProperty|| value is not AvaloniaObject avaloniaObject)
        {
            return null;
        }

        return ReactiveCommand.Create<object>((value) => avaloniaObject.SetValue(avaloniaProperty, value));
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
