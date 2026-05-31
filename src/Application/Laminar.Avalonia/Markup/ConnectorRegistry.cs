using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Reactive;
using Laminar.PluginFramework.NodeSystem.Connectors;

namespace Laminar.Avalonia.Markup;

public class ConnectorRegistrationEventArgs(RoutedEvent routedEvent, object? sender) : RoutedEventArgs(routedEvent, sender)
{
    public required IConnector Connector { get; init; }
    public required Visual Visual { get; init; }
}

public class MoveConnectionIndicationEventArgs(RoutedEvent routedEvent, object? sender)
    : RoutedEventArgs(routedEvent, sender)
{
    public required PointerEventArgs PointerEvent { get; init; }
}

public class ConnectorRegistry : Interactive
{
    public const string Key = "ConnectorRegistry";
    
    private static readonly ConditionalWeakTable<Visual, VisualTracker> TrackedVisuals = [];
    
    public static readonly AttachedProperty<bool> ConnectorGestureLiveProperty = AvaloniaProperty.RegisterAttached<ConnectorRegistry, Visual, bool>("ConnectorGestureLive", defaultValue: true);
    public static bool GetConnectorGestureLive(Visual visual) => visual.GetValue(ConnectorGestureLiveProperty);
    public static void SetConnectorGestureLive(Visual visual, bool value) => visual.SetValue(ConnectorGestureLiveProperty, value);
    
    public static readonly AttachedProperty<IConnector?> RegisteredConnectorProperty = AvaloniaProperty.RegisterAttached<ConnectorRegistry, Visual, IConnector?>("RegisteredConnector");
    public static IConnector? GetRegisteredConnector(Visual obj) => obj.GetValue(RegisteredConnectorProperty);
    public static void SetRegisteredConnector(Visual obj, IConnector? value) => obj.SetValue(RegisteredConnectorProperty, value);
    
    public static readonly RoutedEvent<ConnectorRegistrationEventArgs> ConnectorRegisteredEvent = RoutedEvent.Register<ConnectorRegistry, ConnectorRegistrationEventArgs>(nameof(ConnectorRegistered), RoutingStrategies.Direct);
    public event EventHandler<ConnectorRegistrationEventArgs>? ConnectorRegistered
    {
        add => AddHandler(ConnectorRegisteredEvent, value);
        remove => RemoveHandler(ConnectorRegisteredEvent, value);
    }
    
    public static readonly RoutedEvent<ConnectorRegistrationEventArgs> ConnectorUnregisteredEvent = RoutedEvent.Register<ConnectorRegistry, ConnectorRegistrationEventArgs>(nameof(ConnectorUnregistered), RoutingStrategies.Direct);
    public event EventHandler<ConnectorRegistrationEventArgs>? ConnectorUnregistered
    {
        add => AddHandler(ConnectorUnregisteredEvent, value);
        remove => RemoveHandler(ConnectorUnregisteredEvent, value);
    }
    
    public static readonly RoutedEvent<MoveConnectionIndicationEventArgs> MoveConnectionIndicationEvent = RoutedEvent.Register<ConnectorRegistry, MoveConnectionIndicationEventArgs>("MoveConnectionIndication", RoutingStrategies.Direct);

    public static readonly RoutedEvent<RoutedEventArgs> EndConnectionIndicationEvent = RoutedEvent.Register<ConnectorRegistry, RoutedEventArgs>("EndConnectorIndication", RoutingStrategies.Direct);
    
    static ConnectorRegistry()
    {
        RegisteredConnectorProperty.Changed.AddClassHandler<Visual>(RegisteredConnectorChanged);
    }

    private static void RegisteredConnectorChanged(Visual visual, AvaloniaPropertyChangedEventArgs arg)
    {
        var (oldValue, newValue) = arg.GetOldAndNewValue<IConnector?>();
        var tracker = TrackedVisuals.GetValue(visual, v => new VisualTracker(v));

        if (oldValue is not null)
        {
            tracker.Registry?.RemoveConnectorVisual(oldValue);
        }

        if (newValue is not null)
        {
            tracker.Registry?.SetConnectorVisual(newValue, visual);
        }
    }
    
    private readonly Dictionary<IConnector, Visual> _internalDictionary = [];
    
    public Visual GetVisualForConnector(IConnector connector) =>  _internalDictionary[connector];

    private void RemoveConnectorVisual(IConnector connector)
    {
        if (_internalDictionary.TryGetValue(connector, out var oldOwner))
        {
            RaiseEvent(new ConnectorRegistrationEventArgs(ConnectorUnregisteredEvent, this)
            {
                Connector = connector,
                Visual = oldOwner,
            });
        }
        
        _internalDictionary.Remove(connector);
    }
    
    private void SetConnectorVisual(IConnector connector, Visual visual)
    {
        if (_internalDictionary.TryGetValue(connector, out var oldOwner))
        {
            if (Equals(oldOwner, visual)) return;
            
            SetRegisteredConnector(oldOwner, null);
        }
        
        _internalDictionary[connector] = visual;
        RaiseEvent(new ConnectorRegistrationEventArgs(ConnectorRegisteredEvent, this)
        {
            Connector = connector,
            Visual = visual,
        });
    }
    
    private class VisualTracker : IDisposable
    {
        private readonly Visual _visual;
        private readonly IDisposable _subscription;
        
        public VisualTracker(Visual visual)
        {
            _visual = visual;
            
            _subscription = visual.GetResourceObservable(Key)
                .Subscribe(new AnonymousObserver<object?>(ConnectorRegistryChanged));

            _visual.DetachedFromVisualTree += OnDetachedFromVisualTree;
        }

        public ConnectorRegistry? Registry { get; private set; }
        
        private void OnDetachedFromVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
        {
            Dispose();
        }

        private void ConnectorRegistryChanged(object? newValue)
        {
            if (Equals(Registry, newValue)) return;

            var oldRegistry = Registry;
            var newRegistry = newValue as ConnectorRegistry;
            if (GetRegisteredConnector(_visual) is { } connector)
            {
                oldRegistry?.RemoveConnectorVisual(connector);
                newRegistry?.SetConnectorVisual(connector, _visual);
            }

            Registry = newRegistry;
        }
        
        public void Dispose()
        {
            if (GetRegisteredConnector(_visual) is { } connector)
            {
                Registry?.RemoveConnectorVisual(connector);
            }
            
            TrackedVisuals.Remove(_visual);
            _visual.DetachedFromVisualTree -= OnDetachedFromVisualTree;
            _subscription.Dispose();
        }
    }
}