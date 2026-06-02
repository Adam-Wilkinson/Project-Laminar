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
using AvaloniaPoint = Avalonia.Point;

namespace Laminar.Avalonia.ViewModels;

public partial class ScriptEditorViewModel(IScript script, IScriptEditor editor, IUserActionManager userActionManager)
    : DropTargetViewModel, IConnectionInteractionHandler
{
    private IUserActionSession? _userActionSession;

    [ObservableProperty]
    public partial IReadOnlyList<object>? CurrentSelection { get; set; }

    public IReadOnlyObservableCollection<ScriptEditorItemModel> VisualElements { get; } =
        new FlattenedObservableTree<ScriptEditorItemModel>(
                script.NodeTreeView.Nodes.ObservableMap(node => new ScriptEditorItemModel(node)),
                script.NodeTreeView.Connections.ObservableMap(connection => new ScriptEditorItemModel(connection)));
    
    public override bool Drop(object? payload, AvaloniaPoint location, object? receptacleTag)
    {
        if (payload is not IWrappedNode wrapped) return false;

        var addNodeAction =
            editor.AddMatchingNodeAction(script, wrapped, new LaminarPoint { X = location.X, Y = location.Y });
        userActionManager.ExecuteAction(addNodeAction);
        return true;
    }

    public IConnector? StartConnectionFrom(IConnector connector)
    {
        if (connector.Flags.HasFlag(ConnectorFlags.AcceptsConnections))
        {
            _userActionSession = userActionManager.BeginSession();
            return connector;            
        }

        if (connector.Flags == (ConnectorFlags.HasConnections | ConnectorFlags.ConnectionsSaturated))
        {
            var connections = script.NodeTreeView.GetConnectionsTo(connector);
            if (connections.Count == 0) return null;
            var connectionInfo = connections.First();

            _userActionSession ??= userActionManager.BeginSession();
            _userActionSession.ExecuteAction(editor.DeleteConnectionAction(script, connectionInfo.Connection));

            return connectionInfo.OppositeConnector;
        }

        return null;
    }

    public bool HoverConnection(IConnector first, IConnector second)
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
        _userActionSession?.Pop();
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