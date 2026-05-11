using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Laminar.PluginFramework.NodeSystem.Connectors;

namespace Laminar.Avalonia.Converters;

public class ConnectorToVisualConverter : AvaloniaObject, IValueConverter
{
    private static readonly Dictionary<IIOConnector, AvaloniaObject> ConnectorRegistrations = []; 
    
    public static readonly AttachedProperty<IIOConnector?> RegisteredConnectorProperty = AvaloniaProperty.RegisterAttached<ConnectorToVisualConverter, AvaloniaObject, IIOConnector?>("RegisteredConnector");
    public static IIOConnector? GetRegisteredConnector(AvaloniaObject obj) => obj.GetValue(RegisteredConnectorProperty);
    public static void SetRegisteredConnector(AvaloniaObject obj, IIOConnector? value) => obj.SetValue(RegisteredConnectorProperty, value);

    public static readonly ConnectorToVisualConverter Instance = new();
    
    static ConnectorToVisualConverter()
    {
        RegisteredConnectorProperty.Changed.AddClassHandler<AvaloniaObject>(RegisteredConnectorChanged);
    }

    private static void RegisteredConnectorChanged(AvaloniaObject obj, AvaloniaPropertyChangedEventArgs arg)
    {
        var (oldValue, newValue) = arg.GetOldAndNewValue<IIOConnector?>();
        if (oldValue is not null) ConnectorRegistrations.Remove(oldValue);
        if (newValue is null) return;
        if (ConnectorRegistrations.TryGetValue(newValue, out var oldRegistration))
        {
            SetRegisteredConnector(oldRegistration, null);
        }

        ConnectorRegistrations[newValue] = obj;
    }

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not IIOConnector connector)
            return new BindingNotification(new InvalidCastException(), BindingErrorType.Error);

        return ConnectorRegistrations[connector];
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not AvaloniaObject obj)
            return new BindingNotification(new InvalidCastException(), BindingErrorType.Error);

        return GetRegisteredConnector(obj);
    }
}