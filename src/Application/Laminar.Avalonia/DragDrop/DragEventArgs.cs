using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace Laminar.Avalonia.DragDrop;

public class DragEventArgs : RoutedEventArgs
{
    public static DragEventArgs HoverEnter(Control draggingVisual, PointerPressedEventArgs clickEvent, Interactive? currentHoverInteractive = null, object? receptacleTag = null)
        => new DragEventArgs(DropHandler.HoverEnterEvent, draggingVisual)
            { OriginalClickEventArgs = clickEvent, DraggingControl = draggingVisual, HoverOverInteractive = currentHoverInteractive, ReceptacleTag = receptacleTag };

    public static DragEventArgs HoverLeave(Control draggingVisual, PointerPressedEventArgs clickEvent, Interactive? currentHoverInteractive = null, object? receptacleTag = null) 
        => new DragEventArgs(DropHandler.HoverLeaveEvent, draggingVisual)
            { OriginalClickEventArgs = clickEvent, DraggingControl = draggingVisual, HoverOverInteractive = currentHoverInteractive, ReceptacleTag = receptacleTag };
    
    public static DragEventArgs Drop(Control draggingVisual, PointerPressedEventArgs clickEvent, Interactive? currentHoverInteractive = null, object? receptacleTag = null)
        => new DragEventArgs(DropHandler.DropEvent, draggingVisual)
            { OriginalClickEventArgs = clickEvent, DraggingControl = draggingVisual, HoverOverInteractive = currentHoverInteractive, ReceptacleTag = receptacleTag };
    
    private DragEventArgs(RoutedEvent<DragEventArgs> routedEvent, Visual draggingControl)
        :base(routedEvent, draggingControl)
    {
    }

    public required Control DraggingControl { get; init; }

    public required PointerPressedEventArgs OriginalClickEventArgs { get; init; }

    public Interactive? HoverOverInteractive { get; set; }

    public object? ReceptacleTag { get; set; }
}