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
    
    private IIOConnector? _firstClickedConnector;
    private IIOConnector? _potentialSecondConnector;

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

        if (FindConnectorFromEvent(e, c => !Equals(c, _firstClickedConnector)) is not { } hoverConnector)
        {
            if (_potentialSecondConnector is null) return;
            
            ConnectionInteractionHandler?.CancelConnection();
            _potentialSecondConnector = null;
            return;   
        }
        
        if (Equals(_potentialSecondConnector, hoverConnector)) return;

        if (_potentialSecondConnector is not null)
        {
            ConnectionInteractionHandler?.CancelConnection();
        }
        
        ConnectionInteractionHandler?.HoverConnection(_firstClickedConnector, hoverConnector);
        _potentialSecondConnector = hoverConnector;
    }

    private void OnAllReleases(PointerReleasedEventArgs _)
    {
        if (_firstClickedConnector is null) return;
        ConnectionInteractionHandler?.ConfirmConnection();
        _firstClickedConnector = null;
        _potentialSecondConnector = null;
    }

    private void OnAllClicks(PointerPressedEventArgs args)
    {
        _firstClickedConnector = FindConnectorFromEvent(args);
    }

    private IIOConnector? FindConnectorFromEvent(PointerEventArgs e, Predicate<IIOConnector>? predicate = null)
    {
        return (SelectAndMove.ItemsPanelRoot?
            .GetInputElementsAt(e.GetPosition(SelectAndMove.ItemsPanelRoot))
            .FirstOrDefault(x => 
                x is InputElement element
                && ConnectorRegistry.GetRegisteredConnector(element) is { } potential
                && (predicate?.Invoke(potential) ?? true)) as StyledElement)?.DataContext as IIOConnector;
    }
}