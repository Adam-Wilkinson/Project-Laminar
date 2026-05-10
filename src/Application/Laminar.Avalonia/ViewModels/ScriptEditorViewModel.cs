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

public partial class ScriptEditorViewModel : ViewModelBase
{
    private readonly IScript _script;
    private readonly IScriptEditor _editor;

    /// <inheritdoc/>
    public ScriptEditorViewModel(IScript script, IScriptEditor editor)
    {
        _script = script;
        _editor = editor;
    }

    public IReadOnlyObservableCollection<IWrappedNode> Nodes => _script.Nodes;

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
        newNode.Location.Value = new Point { X = dropPosition.X, Y = dropPosition.Y };
    }
}