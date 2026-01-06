using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace Laminar.Avalonia.DragDrop;

public class DropTargetEventArgs : RoutedEventArgs
{
    public static DropTargetEventArgs HoverEnter(Control draggingVisual, PointerPressedEventArgs clickEvent, Interactive? currentHoverInteractive = null, object? receptacleTag = null)
        => new(DropTargetHandler.HoverEnterEvent, draggingVisual)
            { OriginalClickEventArgs = clickEvent, DraggingControl = draggingVisual, CurrentHoverOver = currentHoverInteractive, ReceptacleTag = receptacleTag };

    public static DropTargetEventArgs HoverLeave(Control draggingVisual, PointerPressedEventArgs clickEvent, Interactive? currentHoverInteractive = null, object? receptacleTag = null) 
        => new(DropTargetHandler.HoverLeaveEvent, draggingVisual)
            { OriginalClickEventArgs = clickEvent, DraggingControl = draggingVisual, CurrentHoverOver = currentHoverInteractive, ReceptacleTag = receptacleTag };
    
    public static DropTargetEventArgs Drop(Control draggingVisual, PointerPressedEventArgs clickEvent, Interactive? currentHoverInteractive = null, object? receptacleTag = null)
        => new(DropTargetHandler.DropEvent, draggingVisual)
            { OriginalClickEventArgs = clickEvent, DraggingControl = draggingVisual, CurrentHoverOver = currentHoverInteractive, ReceptacleTag = receptacleTag };
    
    private DropTargetEventArgs(RoutedEvent<DropTargetEventArgs> routedEvent, Visual draggingControl)
        :base(routedEvent, draggingControl)
    {
    }

    public required Control DraggingControl { get; init; }

    public required PointerPressedEventArgs OriginalClickEventArgs { get; init; }

    public Interactive? CurrentHoverOver { get; set; }

    public object? ReceptacleTag { get; set; }
}