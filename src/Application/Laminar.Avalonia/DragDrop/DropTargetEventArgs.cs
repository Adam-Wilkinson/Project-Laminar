using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace Laminar.Avalonia.DragDrop;

public class DropTargetEventArgs : RoutedEventArgs
{
    public static DropTargetEventArgs HoverEnter(DragDropSession session)
        => new(DropTargetHandler.HoverEnterEvent, session) { EventType =  DropTargetEventType.HoverEnter };

    public static DropTargetEventArgs HoverLeave(DragDropSession session) 
        => new(DropTargetHandler.HoverLeaveEvent, session) { EventType =  DropTargetEventType.HoverLeave };
    
    public static DropTargetEventArgs Drop(DragDropSession session)
        => new(DropTargetHandler.DropEvent, session) { EventType =  DropTargetEventType.Drop };

    private readonly DragDropSession _session;
    
    private DropTargetEventArgs(RoutedEvent<DropTargetEventArgs> routedEvent, DragDropSession session)
        :base(routedEvent, session.DraggingControl)
    {
        _session = session;
    }

    public Control DraggingControl => _session.DraggingControl;

    public PointerEventArgs? PointerEventArgs { get; set; }

    public bool AnimateHome { get; set; } = true;

    public Interactive? CurrentHoverOver => _session.CurrentHoverInfo?.HoverTarget;

    public object? ReceptacleTag => _session.CurrentHoverInfo?.ReceptacleTag;

    public Vector OriginalClickOffset => _session.OriginalClickOffset;
    
    public DropTargetEventType EventType { get; private init; }
}

public enum DropTargetEventType
{
    HoverEnter,
    HoverLeave,
    Drop,
}