using System.Collections.Generic;
using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.Input;
using Laminar.Avalonia.DragDrop;
using Laminar.Avalonia.ViewModels.Services;
using Laminar.Contracts.Scripting;
using Laminar.Contracts.Scripting.NodeWrapping;
using Laminar.Domain.Notification;
using Laminar.PluginFramework.NodeSystem.Connectors;
using LaminarPoint = Laminar.Domain.ValueObjects.Point;

namespace Laminar.Avalonia.ViewModels;

public partial class ScriptEditorViewModel : ViewModelBase, IConnectionInteractionHandler
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

    [RelayCommand]
    private void OnDrop(DropTargetEventArgs args)
    {
        if (args.DraggingControl.DataContext is not IWrappedNode droppedNode 
            || args.PointerEventArgs is not { } pointerEvent
            || args.CurrentHoverOver is not Visual hoverVisual) return;
        
        args.Handled = true;
        args.AnimateHome = false;
        
        var newNode = _editor.AddCopyOfNode(_script, droppedNode);

        var positionReference = hoverVisual is ItemsControl { ItemsPanelRoot: { } panelRoot } ? panelRoot : hoverVisual;
        var dropPosition = pointerEvent.GetPosition(positionReference) - args.OriginalClickOffset;
        newNode.Location.Value = new LaminarPoint { X = dropPosition.X, Y = dropPosition.Y };
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