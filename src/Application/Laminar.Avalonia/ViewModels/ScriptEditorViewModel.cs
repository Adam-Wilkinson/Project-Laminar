using System.Collections.Generic;
using System.Diagnostics;
using Avalonia;
using Laminar.Avalonia.ViewModels.Services;
using Laminar.Contracts.Scripting;
using Laminar.Contracts.Scripting.NodeWrapping;
using Laminar.Domain.Notification;
using Laminar.PluginFramework.NodeSystem.Connectors;
using LaminarPoint = Laminar.Domain.ValueObjects.Point;

namespace Laminar.Avalonia.ViewModels;

public partial class ScriptEditorViewModel : DropTargetViewModel, IConnectionInteractionHandler
{
    private readonly IScript _script;
    private readonly IScriptEditor _editor;

    public ScriptEditorViewModel(IScript script, IScriptEditor editor)
    {
        _script = script;
        _editor = editor;
        VisualElements = new FlattenedObservableTree<ScriptEditorItemModel>(
            new IEnumerable<ScriptEditorItemModel>[] {
                script.Nodes.ObservableMap(node => new ScriptEditorItemModel(node)),
                script.Connections.ObservableMap(connection => new ScriptEditorItemModel(connection))
            });
    }

    public IReadOnlyObservableCollection<ScriptEditorItemModel> VisualElements { get; }
    
    public override bool Drop(object? payload, Point location, object? receptacleTag)
    {
        if (payload is not IWrappedNode wrapped) return false;

        var newNode = _editor.AddCopyOfNode(_script, wrapped);
        newNode.Location.Value = new LaminarPoint { X = location.X, Y = location.Y };
        return true;
    }

    public void HoverConnection(IIOConnector first, IIOConnector second)
    {
        Debug.WriteLine($"Potential connection between {first} and {second}");
    }

    public void CancelConnection()
    {
        Debug.WriteLine("Never mind");
    }

    public void ConfirmConnection()
    {
        Debug.WriteLine("Confirmed connection");
    }
}