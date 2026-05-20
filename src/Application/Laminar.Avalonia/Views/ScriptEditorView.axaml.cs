using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Laminar.Avalonia.Markup;
using Laminar.Avalonia.ViewModels.Services;
using Laminar.PluginFramework.NodeSystem.Connectors;

namespace Laminar.Avalonia.Views;

public partial class ScriptEditorView : UserControl
{
    public static readonly StyledProperty<IConnectionInteractionHandler?> ConnectionInteractionHandlerProperty = AvaloniaProperty.Register<ScriptEditorView, IConnectionInteractionHandler?>(nameof(ConnectionInteractionHandler));
    
    private ConnectorTarget? _firstClickedConnector;
    private ConnectorTarget? _potentialSecondConnector;

    static ScriptEditorView()
    {
        PointerPressedEvent.AddClassHandler<ScriptEditorView>((sev, args) => sev.OnAllClicks(args), handledEventsToo: true);
        PointerReleasedEvent.AddClassHandler<ScriptEditorView>((sev, args) => sev.OnAllReleases(args), handledEventsToo: true);
    }

    public ScriptEditorView()
    {
        InitializeComponent();
    }

    public IConnectionInteractionHandler? ConnectionInteractionHandler
    {
        get => GetValue(ConnectionInteractionHandlerProperty);
        set => SetValue(ConnectionInteractionHandlerProperty, value);
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        if (_firstClickedConnector is null) return;

        if (FindConnectorFromEvent(e, c => !Equals(c, _firstClickedConnector?.Connector)) is not { } hoverConnector)
        {
            if (_potentialSecondConnector is null) return;
            
            ConnectionInteractionHandler?.CancelConnection();
            ConnectorRegistry.SetConnectorGestureLive(_firstClickedConnector.Visual, true);
            _potentialSecondConnector = null;
            return;   
        }
        
        if (Equals(_potentialSecondConnector?.Connector, hoverConnector.Connector)) return;

        if (_potentialSecondConnector is not null)
        {
            ConnectionInteractionHandler?.CancelConnection();
            ConnectorRegistry.SetConnectorGestureLive(_firstClickedConnector.Visual, true);
        }

        bool connectionMade =
            ConnectionInteractionHandler?.HoverConnection(_firstClickedConnector.Connector, hoverConnector.Connector) ??
            false; 
        
        ConnectorRegistry.SetConnectorGestureLive(_firstClickedConnector.Visual, !connectionMade);
        _potentialSecondConnector = hoverConnector;
    }

    private void OnAllReleases(PointerReleasedEventArgs _)
    {
        if (_firstClickedConnector is null) return;
        ConnectionInteractionHandler?.ConfirmConnection();
        ConnectorRegistry.SetConnectorGestureLive(_firstClickedConnector.Visual, true);
        _firstClickedConnector = null;
        
        if (_potentialSecondConnector is null) return;
        ConnectorRegistry.SetConnectorGestureLive(_potentialSecondConnector.Visual, true);
        _potentialSecondConnector = null;
    }

    private void OnAllClicks(PointerPressedEventArgs args)
    {
        _firstClickedConnector = FindConnectorFromEvent(args);
    }

    private ConnectorTarget? FindConnectorFromEvent(PointerEventArgs e, Predicate<IIOConnector>? predicate = null)
    {
        return SelectAndMove.ItemsPanelRoot?
            .GetInputElementsAt(e.GetPosition(SelectAndMove.ItemsPanelRoot), enabledElementsOnly: false)
            .FirstOrDefault(x =>
                x is InputElement element
                && ConnectorRegistry.GetRegisteredConnector(element) is { } potential
                && (predicate?.Invoke(potential) ?? true)) is not InputElement match
            ? null
            : new ConnectorTarget(ConnectorRegistry.GetRegisteredConnector(match)!, match);
    }

    private record ConnectorTarget(IIOConnector Connector, Visual Visual);
}