using System.Collections.Specialized;
using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.Input;
using Laminar.Avalonia.DragDrop;
using Laminar.Contracts.Scripting;
using Laminar.Contracts.Scripting.NodeWrapping;
using Laminar.Domain.Notification;
using Point = Laminar.Domain.ValueObjects.Point;

namespace Laminar.Avalonia.ViewModels;

public partial class ScriptEditorViewModel(IScript script, IScriptEditor editor) : ViewModelBase
{
    public IReadOnlyObservableCollection<IWrappedNode> VisualElements => script.Nodes;

    [RelayCommand]
    private void OnDrop(DropTargetEventArgs args)
    {
        if (args.DraggingControl.DataContext is not IWrappedNode droppedNode 
            || args.PointerEventArgs is not { } pointerEvent
            || args.CurrentHoverOver is not Visual hoverVisual) return;
        
        args.Handled = true;
        args.AnimateHome = false;
        
        var newNode = editor.AddCopyOfNode(script, droppedNode);

        var positionReference = hoverVisual is ItemsControl { ItemsPanelRoot: { } panelRoot } ? panelRoot : hoverVisual;
        var dropPosition = pointerEvent.GetPosition(positionReference) - args.OriginalClickOffset;
        newNode.Location.Value = new Point { X = dropPosition.X, Y = dropPosition.Y };
    }
}