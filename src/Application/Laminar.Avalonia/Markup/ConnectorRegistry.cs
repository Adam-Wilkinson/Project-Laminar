using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Laminar.Avalonia.ViewModels.Services;
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

    public required Point Offset { get; init; }
}

public sealed class ConnectorRegistry : Interactive, IDisposable
{
    public const string Key = "ConnectorRegistry";
    
    private static readonly ConditionalWeakTable<InputElement, VisualTracker> TrackedVisuals = [];
    
    public static readonly AttachedProperty<IConnectionInteractionHandler?> ConnectionInteractionHandlerProperty = AvaloniaProperty.RegisterAttached<ConnectorRegistry, InputElement, IConnectionInteractionHandler?>("ConnectionInteractionHandler", inherits: true);
    public static IConnectionInteractionHandler? GetConnectionInteractionHandler(InputElement obj) => obj.GetValue(ConnectionInteractionHandlerProperty);
    public static void SetConnectionInteractionHandler(InputElement obj, IConnectionInteractionHandler? value) =>  obj.SetValue(ConnectionInteractionHandlerProperty, value);
    
    public static readonly AttachedProperty<bool> ConnectorGestureLiveProperty = AvaloniaProperty.RegisterAttached<ConnectorRegistry, InputElement, bool>("ConnectorGestureLive", defaultValue: true);
    public static bool GetConnectorGestureLive(InputElement visual) => visual.GetValue(ConnectorGestureLiveProperty);
    public static void SetConnectorGestureLive(InputElement visual, bool value) => visual.SetValue(ConnectorGestureLiveProperty, value);
    
    public static readonly AttachedProperty<IConnector?> RegisteredConnectorProperty = AvaloniaProperty.RegisterAttached<ConnectorRegistry, InputElement, IConnector?>("RegisteredConnector");
    public static IConnector? GetRegisteredConnector(InputElement obj) => obj.GetValue(RegisteredConnectorProperty);
    public static void SetRegisteredConnector(InputElement obj, IConnector? value) => obj.SetValue(RegisteredConnectorProperty, value);
    
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
        RegisteredConnectorProperty.Changed.AddClassHandler<InputElement>(RegisteredConnectorChanged);
    }

    private static void RegisteredConnectorChanged(InputElement visual, AvaloniaPropertyChangedEventArgs arg)
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
    
    private readonly Dictionary<IConnector, InputElement> _internalDictionary = [];

    private ConnectionCreationSession? _connectionCreationSession;
    
    public InputElement GetVisualForConnector(IConnector connector) => _internalDictionary[connector];
    
    public bool TryGetVisualForConnector(IConnector connector, out InputElement? visual) 
        => _internalDictionary.TryGetValue(connector, out visual);
    
    private void BeginMakeConnectionGesture(InputElement element, PointerPressedEventArgs e)
    {
        _connectionCreationSession?.Dispose();
        if (GetRegisteredConnector(element) is { } connector 
            && GetConnectionInteractionHandler(element) is { } interactionHandler
            && interactionHandler.StartConnectionFrom(connector) is { } initialConnector)
        {
            _connectionCreationSession = new(initialConnector, e, this, interactionHandler, element);
        }
    }
    
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
    
    private void SetConnectorVisual(IConnector connector, InputElement visual)
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
        private readonly InputElement _element;
        private readonly IDisposable _subscription;
        
        public VisualTracker(InputElement element)
        {
            _element = element;
            
            _subscription = element.GetResourceObservable(Key)
                .Subscribe(new Domain.Notification.Value.AnonymousObserver<object?>(ConnectorRegistryChanged));
            
            _element.DetachedFromVisualTree += OnDetachedFromElementTree;
            _element.PointerPressed += ElementOnPointerPressed; 
        }

        private void ElementOnPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            Registry?.BeginMakeConnectionGesture(_element, e);
        }

        public ConnectorRegistry? Registry { get; private set; }
        
        private void OnDetachedFromElementTree(object? sender, VisualTreeAttachmentEventArgs e)
        {
            Dispose();
        }

        private void ConnectorRegistryChanged(object? newValue)
        {
            if (Equals(Registry, newValue)) return;

            var oldRegistry = Registry;
            var newRegistry = newValue as ConnectorRegistry;
            if (GetRegisteredConnector(_element) is { } connector)
            {
                oldRegistry?.RemoveConnectorVisual(connector);
                newRegistry?.SetConnectorVisual(connector, _element);
            }

            Registry = newRegistry;
        }
        
        public void Dispose()
        {
            if (GetRegisteredConnector(_element) is { } connector)
            {
                Registry?.RemoveConnectorVisual(connector);
            }
            
            TrackedVisuals.Remove(_element);
            _element.DetachedFromVisualTree -= OnDetachedFromElementTree;
            _element.PointerPressed -= ElementOnPointerPressed; 
            _subscription.Dispose();
        }
    }

    public void Dispose()
    {
        _connectionCreationSession?.Dispose();
    }
}