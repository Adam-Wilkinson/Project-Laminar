using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Input.GestureRecognizers;
using Avalonia.Interactivity;
using Laminar.Avalonia.Markup;
using Laminar.Avalonia.ViewModels.Services;
using Laminar.PluginFramework.NodeSystem.Connectors;

namespace Laminar.Avalonia.Gestures;

public class MakeConnectionGesture : GestureRecognizer
{
    public static readonly StyledProperty<IConnectionInteractionHandler?> ConnectionInteractionHandlerProperty = AvaloniaProperty.Register<MakeConnectionGesture, IConnectionInteractionHandler?>(nameof(ConnectionInteractionHandler));

    private ConnectorRegistry? _connectorRegistry;
    private ConnectorTarget? _potentialSecondConnector;
    private ConnectorTarget? _firstConnector;
    private IPointer? _captured;
    
    
    public IConnectionInteractionHandler? ConnectionInteractionHandler
    {
        get => GetValue(ConnectionInteractionHandlerProperty);
        set => SetValue(ConnectionInteractionHandlerProperty, value);
    }
    
    protected override void PointerPressed(PointerPressedEventArgs e)
    {
        if (FindConnectorFromEvent(e) is not { } clickedConnector) return;

        e.Handled = true;
        e.PreventGestureRecognition();
        Capture(e.Pointer);
        _captured = e.Pointer;
        
        _connectorRegistry ??= (Target as StyledElement)?.FindResource(ConnectorRegistry.Key) as ConnectorRegistry ?? throw new InvalidOperationException("MakeConnectionGesture requires access to a connector registry");
        var targetConnector = ConnectionInteractionHandler?.GetTargetConnector(clickedConnector.Connector);
        if (targetConnector is null) return;
        _firstConnector = new ConnectorTarget(targetConnector, _connectorRegistry.GetVisualForConnector(targetConnector));
    }

    protected override void PointerMoved(PointerEventArgs e)
    {
        if (_firstConnector is null || e.Pointer != _captured) return;

        e.Handled = true;
        e.PreventGestureRecognition();
        
        (_firstConnector.Visual as InputElement)?.RaiseEvent(new MoveConnectionIndicationEventArgs(ConnectorRegistry.MoveConnectionIndicationEvent, this)
        {
            PointerEvent = e
        });
        
        if (FindConnectorFromEvent(e, c => !Equals(c, _firstConnector?.Connector)) is not { } hoverConnector)
        {
            if (_potentialSecondConnector is null) return;
            
            ConnectionInteractionHandler?.CancelConnection();
            ConnectorRegistry.SetConnectorGestureLive(_firstConnector.Visual, true);
            _potentialSecondConnector = null;
            return;   
        }
        
        if (Equals(_potentialSecondConnector?.Connector, hoverConnector.Connector)) return;

        if (_potentialSecondConnector is not null)
        {
            ConnectionInteractionHandler?.CancelConnection();
            ConnectorRegistry.SetConnectorGestureLive(_firstConnector.Visual, true);
        }

        bool connectionMade =
            ConnectionInteractionHandler?.HoverConnection(_firstConnector.Connector, hoverConnector.Connector) ??
            false; 
        
        ConnectorRegistry.SetConnectorGestureLive(_firstConnector.Visual, !connectionMade);
        _potentialSecondConnector = hoverConnector;
    }

    protected override void PointerCaptureLost(IPointer pointer)
    {
        if (pointer == _captured)
        {
            EndGesture();
        }
    }

    protected override void PointerReleased(PointerReleasedEventArgs e)
    {
        if (!EndGesture()) return;
        e.Handled = true;
        e.PreventGestureRecognition();
    }

    private bool EndGesture()
    {
        if (_firstConnector is null) return false;
        ConnectionInteractionHandler?.ConfirmConnection();
        ConnectorRegistry.SetConnectorGestureLive(_firstConnector.Visual, true);
        (_firstConnector.Visual as InputElement)?.RaiseEvent(new RoutedEventArgs(ConnectorRegistry.EndConnectionIndicationEvent, this));
        _firstConnector = null;
        
        if (_potentialSecondConnector is null) return true;
        ConnectorRegistry.SetConnectorGestureLive(_potentialSecondConnector.Visual, true);
        _potentialSecondConnector = null;
        _captured?.Capture(null);

        return true;
    }
    
    private ConnectorTarget? FindConnectorFromEvent(PointerEventArgs e, Predicate<IIOConnector>? predicate = null)
    {
        if (Target is not Visual targetVisual) return null;
        
        return Target?
            .GetInputElementsAt(e.GetPosition(targetVisual), enabledElementsOnly: false)
            .FirstOrDefault(x =>
                x is InputElement element
                && ConnectorRegistry.GetRegisteredConnector(element) is { } potential
                && (predicate?.Invoke(potential) ?? true)) is not InputElement match
            ? null
            : new ConnectorTarget(ConnectorRegistry.GetRegisteredConnector(match)!, match);
    }
    
    private record ConnectorTarget(IIOConnector Connector, Visual Visual);
}