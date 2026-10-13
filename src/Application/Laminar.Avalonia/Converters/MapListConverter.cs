using System.Collections.Specialized;
using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Laminar.Domain.Observables.Collections;

namespace Laminar.Avalonia.Converters;

public class MapListConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not IEnumerable<object> inputEnumerable ||
            parameter is not IValueConverter itemConverter)
        {
            return new BindingNotification(new InvalidCastException(), BindingErrorType.Error);
        }

        var selector = (object x) => itemConverter.Convert(x, typeof(object), null, culture);
        
        return inputEnumerable is INotifyCollectionChanged notifier
            ? new MappedObservableList<object, object?>(notifier, selector) 
            : inputEnumerable.Select(selector).ToList();
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}