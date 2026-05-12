using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Reactive;
using Laminar.PluginFramework.NodeSystem.Connectors;

namespace Laminar.Avalonia.Markup;

public class ConnectorRegistry
{
    public const string Key = "ConnectorRegistry";
    
    private static readonly ConditionalWeakTable<Visual, VisualTracker> TrackedVisuals = [];
    
    public static readonly AttachedProperty<IIOConnector?> RegisteredConnectorProperty = AvaloniaProperty.RegisterAttached<ConnectorRegistry, Visual, IIOConnector?>("RegisteredConnector");
    public static IIOConnector? GetRegisteredConnector(Visual obj) => obj.GetValue(RegisteredConnectorProperty);
    public static void SetRegisteredConnector(Visual obj, IIOConnector? value) => obj.SetValue(RegisteredConnectorProperty, value);
    
    static ConnectorRegistry()
    {
        RegisteredConnectorProperty.Changed.AddClassHandler<Visual>(RegisteredConnectorChanged);
    }

    private static void RegisteredConnectorChanged(Visual visual, AvaloniaPropertyChangedEventArgs arg)
    {
        var (oldValue, newValue) = arg.GetOldAndNewValue<IIOConnector?>();
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
    
    private readonly Dictionary<IIOConnector, Visual> _internalDictionary = [];
    
    public Visual GetVisualForConnector(IIOConnector connector) =>  _internalDictionary[connector];

    private void RemoveConnectorVisual(IIOConnector connector) => _internalDictionary.Remove(connector);
    
    private void SetConnectorVisual(IIOConnector connector, Visual visual)
    {
        if (_internalDictionary.TryGetValue(connector, out var oldOwner))
        {
            if (Equals(oldOwner, visual)) return;
            
            SetRegisteredConnector(oldOwner, null);
        }
        
        _internalDictionary[connector] = visual;
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