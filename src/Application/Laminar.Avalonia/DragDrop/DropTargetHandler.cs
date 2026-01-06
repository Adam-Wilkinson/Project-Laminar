using System;
using Avalonia;
using Avalonia.Interactivity;

namespace Laminar.Avalonia.DragDrop;

public class DropTargetHandler : Interactive
{
    public static readonly RoutedEvent<DropTargetEventArgs> DropEvent = RoutedEvent.Register<DropTargetHandler, DropTargetEventArgs>(nameof(Drop), RoutingStrategies.Direct);
    
    public static readonly RoutedEvent<DropTargetEventArgs> HoverEnterEvent = RoutedEvent.Register<DropTargetHandler, DropTargetEventArgs>(nameof(HoverEnter), RoutingStrategies.Direct);

    public static readonly RoutedEvent<DropTargetEventArgs> HoverLeaveEvent = RoutedEvent.Register<DropTargetHandler, DropTargetEventArgs>(nameof(HoverLeave), RoutingStrategies.Direct);
    
    public static readonly AttachedProperty<DropAcceptor> DropAcceptorProperty = AvaloniaProperty.RegisterAttached<DropTargetHandler, Visual, DropAcceptor>(nameof(DropAcceptor), defaultValue: new DropAcceptor());
    
    public static DropAcceptor GetDropAcceptor(Visual visual) => visual.GetValue(DropAcceptorProperty);
    public static void SetDropAcceptor(Visual visual, DropAcceptor value) => visual.SetValue(DropAcceptorProperty, value);
    
    public event EventHandler<DropTargetEventArgs> HoverLeave
    {
        add => AddHandler(HoverLeaveEvent, value);
        remove => RemoveHandler(HoverLeaveEvent, value);
    }
    
    public event EventHandler<DropTargetEventArgs> Drop
    {
        add => AddHandler(DropEvent, value);
        remove => RemoveHandler(DropEvent, value);
    }
    
    public event EventHandler<DropTargetEventArgs> HoverEnter
    {
        add => AddHandler(HoverEnterEvent, value);
        remove => RemoveHandler(HoverEnterEvent, value);
    }
}