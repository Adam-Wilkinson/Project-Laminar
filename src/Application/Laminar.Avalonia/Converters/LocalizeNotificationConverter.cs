using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Laminar.Avalonia.Controls;
using Laminar.Avalonia.Localizers;
using Laminar.Domain.Notifications;

namespace Laminar.Avalonia.Converters;

public class LocalizeNotificationConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not NotificationBase notification)
        {
            return new BindingNotification(new InvalidCastException(), BindingErrorType.Error);
        }
        
        return NotificationLocalizer.Localize(notification);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new InvalidOperationException();
    }
}