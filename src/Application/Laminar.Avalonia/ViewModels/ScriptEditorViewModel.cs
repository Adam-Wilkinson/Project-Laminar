using Avalonia;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Laminar.Avalonia.ViewModels.Services;
using Laminar.Contracts.Base.ActionSystem;
using Laminar.Contracts.Scripting;
using Laminar.Contracts.Scripting.Connection;
using Laminar.Contracts.Scripting.NodeWrapping;
using Laminar.Domain.Notification;
using Laminar.PluginFramework.NodeSystem.Connectors;
using LaminarPoint = Laminar.Domain.ValueObjects.Point;

namespace Laminar.Avalonia.ViewModels;

public partial class ScriptEditorViewModel(IScript script, IScriptEditor editor, IUserActionManager userActionManager)
    : DropTargetViewModel, IConnectionInteractionHandler
{
    private IUserActionSession? _userActionSession;

    [ObservableProperty]
    public partial IReadOnlyList<object>? CurrentSelection { get; set; }

    public IReadOnlyObservableCollection<ScriptEditorItemModel> VisualElements { get; } =
        new FlattenedObservableTree<ScriptEditorItemModel>(
                script.Nodes.ObservableMap(node => new ScriptEditorItemModel(node)),
                script.Connections.ObservableMap(connection => new ScriptEditorItemModel(connection)));
    
    public override bool Drop(object? payload, Point location, object? receptacleTag)
    {
        if (payload is not IWrappedNode wrapped) return false;

        var newNode = editor.AddMatchingNode(script, wrapped);
        newNode.Location.Value = new LaminarPoint { X = location.X, Y = location.Y };
        return true;
    }

    public IIOConnector? StartConnectionFrom(IIOConnector connector)
    {
        _userActionSession = userActionManager.BeginSession();
        if (connector.AcceptsConnections)
        {
            return connector;
        }

        var connections = script.NodeTree.GetConnections(connector);
        if (connections.Count == 0) return null;
        var connectionInfo = connections.First();

        _userActionSession.ExecuteAction(editor.DeleteConnectionAction(script, connectionInfo.Connection));

        return connectionInfo.OppositeConnector;
    }

    public bool HoverConnection(IIOConnector first, IIOConnector second)
    {
        _userActionSession ??= userActionManager.BeginSession();

        if (editor.FindBridgeConnectorsAction(script, first, second) is not { } bridgeAction) return false;
        
        _userActionSession.ExecuteAction(bridgeAction);
        return true;

    }

    [RelayCommand(CanExecute = nameof(CanDeleteSelection))]
    private void DeleteSelection()
    {
        if (CurrentSelection is null || CurrentSelection.Count == 0) return;

        using var session = userActionManager.BeginSession();
        foreach (var connection in CurrentSelection.Select(x => (x as ScriptEditorItemModel)?.CoreElement)
                     .OfType<IConnection>())
        {
            session.ExecuteAction(editor.DeleteConnectionAction(script, connection));
        }

        foreach (var connection in CurrentSelection.Select(x => (x as ScriptEditorItemModel)?.CoreElement)
                     .OfType<IWrappedNode>())
        {
            session.ExecuteAction(editor.DeleteNodeAction(script, connection));
        }
    }

    public bool CanDeleteSelection => CurrentSelection is not null && CurrentSelection.Count > 0; 

    public void CancelConnection()
    {
        _userActionSession?.Reset();
    }

    public void ConfirmConnection()
    {
        _userActionSession?.Dispose();
        _userActionSession = null;
    }

    partial void OnCurrentSelectionChanged(IReadOnlyList<object>? value)
    {
        OnPropertyChanged(nameof(CanDeleteSelection));
    }
}