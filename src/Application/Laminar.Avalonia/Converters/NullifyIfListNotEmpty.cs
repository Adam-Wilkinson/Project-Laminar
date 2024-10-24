using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace Laminar.Avalonia.Converters;

public class NullifyIfListNotEmpty : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value;

        //if (parameter is ICollection collection && collection.Count > 0)
        //{
        //    if (targetType == typeof(ICommand))
        //    {
        //        return ReactiveCommand.Create(() => { });
        //    }

        //    return null;
        //}

        //return value;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value;

        throw new NotImplementedException();
    }
}
