using Avalonia;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using Laminar.Avalonia.ViewModels.Services;
using Laminar.PluginFramework.NodeSystem.Connectors;

namespace Laminar.Avalonia.Markup;

public sealed class ConnectionCreationSession : AvaloniaObject, IDisposable
{
    private readonly IConnector _firstClickedConnector;
    private readonly InputElement _targetElement;
    private readonly IConnectionInteractionHandler _interactionHandler;
    private readonly ConnectorRegistry _connectorRegistry;
    private readonly IPointer _captured;
    private readonly Point _clickOffset;
    
    private bool _isDisposed;
    private InputElement? _potentialSecondConnector;
    
    public ConnectionCreationSession(
        IConnector targetConnector, 
        PointerPressedEventArgs triggeringEvent,
        ConnectorRegistry connectorRegistry,
        IConnectionInteractionHandler interactionHandler,
        InputElement clickedElement)
    {
        _firstClickedConnector = targetConnector;
        _targetElement = connectorRegistry.GetVisualForConnector(targetConnector);
        _connectorRegistry = connectorRegistry;
        _interactionHandler = interactionHandler;
        _captured = triggeringEvent.Pointer;
        _clickOffset = triggeringEvent.GetPosition(clickedElement);
        triggeringEvent.Pointer.Capture(_targetElement);
        triggeringEvent.Handled = true;
        triggeringEvent.PreventGestureRecognition();
        
        _targetElement.PointerMoved += TargetElementOnPointerMoved;
        _targetElement.PointerReleased += TargetElementOnPointerReleased;
        _targetElement.PointerCaptureLost += TargetElementOnPointerCaptureLost;
        
        TargetElementOnPointerMoved(triggeringEvent.Source, triggeringEvent);
    }

    private void TargetElementOnPointerMoved(object? sender, PointerEventArgs e)
    {
        if (!Equals(e.Pointer, _captured) || _isDisposed) return;
        
        e.Handled = true;
        e.PreventGestureRecognition();
        
        _targetElement.RaiseEvent(new MoveConnectionIndicationEventArgs(ConnectorRegistry.MoveConnectionIndicationEvent, this)
        {
            PointerEvent = e,
            Offset = _clickOffset
        });
        
        if (_potentialSecondConnector is not null)
        {
            // No change
            if (_potentialSecondConnector?.InputHitTest(e.GetPosition(_potentialSecondConnector)) is not null)
            {
                return;
            }
            
            // Remove old connection
            _interactionHandler.CancelCurrentConnection();
            _potentialSecondConnector = null;
            ConnectorRegistry.SetConnectorGestureLive(_targetElement, true);
        }
        
        if (_targetElement.GetPresentationSource()?.RootVisual is not { } visualRoot) return;

        if (visualRoot.GetVisualsAt(e.GetPosition(visualRoot))
                .OfType<InputElement>()
                .Select(ConnectorRegistry.GetRegisteredConnector)
                .OfType<IConnector>()
                .FirstOrDefault(x =>
                    _connectorRegistry.TryGetVisualForConnector(x, out _) &&
                    _interactionHandler.HoverConnection(_firstClickedConnector, x))
            is not { } newSecondConnector)
        {
            return;
        }
        
        _potentialSecondConnector = _connectorRegistry.GetVisualForConnector(newSecondConnector);
        ConnectorRegistry.SetConnectorGestureLive(_targetElement, false);
    }

    private void TargetElementOnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (_isDisposed) return;
        _interactionHandler.ConfirmCurrentConnection();
        Dispose();
    }

    private void TargetElementOnPointerCaptureLost(object? sender, PointerCaptureLostEventArgs e)
    {
        Dispose();
    }
    
    public void Dispose()
    {
        if (_isDisposed) return;
        _isDisposed = true;
        _interactionHandler.ExitInteraction();
        _captured.Capture(null);
        _targetElement.RaiseEvent(new RoutedEventArgs(ConnectorRegistry.EndConnectionIndicationEvent, this));
        _targetElement.PointerMoved -= TargetElementOnPointerMoved;
        _targetElement.PointerReleased -= TargetElementOnPointerReleased;
        _targetElement.PointerCaptureLost -= TargetElementOnPointerCaptureLost;
        ConnectorRegistry.SetConnectorGestureLive(_targetElement, true);
    }
}