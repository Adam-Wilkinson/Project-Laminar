using System.Collections.Generic;
using System.Diagnostics;
using Avalonia;
using Laminar.Avalonia.ViewModels.Services;
using Laminar.Contracts.Base.ActionSystem;
using Laminar.Contracts.Scripting;
using Laminar.Contracts.Scripting.NodeWrapping;
using Laminar.Domain.Notification;
using Laminar.PluginFramework.NodeSystem.Connectors;
using LaminarPoint = Laminar.Domain.ValueObjects.Point;

namespace Laminar.Avalonia.ViewModels;

public partial class ScriptEditorViewModel(IScript script, IScriptEditor editor, IUserActionManager userActionManager)
    : DropTargetViewModel, IConnectionInteractionHandler
{
    private IUserActionSession? _userActionSession;
    
    public IReadOnlyObservableCollection<ScriptEditorItemModel> VisualElements { get; } =
        new FlattenedObservableTree<ScriptEditorItemModel>(
            new IEnumerable<ScriptEditorItemModel>[]
            {
                script.Nodes.ObservableMap(node => new ScriptEditorItemModel(node)),
                script.Connections.ObservableMap(connection => new ScriptEditorItemModel(connection))
            });
    
    public override bool Drop(object? payload, Point location, object? receptacleTag)
    {
        if (payload is not IWrappedNode wrapped) return false;

        var newNode = editor.AddCopyOfNode(script, wrapped);
        newNode.Location.Value = new LaminarPoint { X = location.X, Y = location.Y };
        return true;
    }

    public bool HoverConnection(IIOConnector first, IIOConnector second)
    {
        _userActionSession ??= userActionManager.BeginSession();

        if (editor.FindBridgeConnectorsAction(script, first, second) is not { } bridgeAction) return false;
        
        _userActionSession.ExecuteAction(bridgeAction);
        return true;

    }

    public void CancelConnection()
    {
        _userActionSession?.Reset();
    }

    public void ConfirmConnection()
    {
        _userActionSession?.Dispose();
        _userActionSession = null;
    }
}