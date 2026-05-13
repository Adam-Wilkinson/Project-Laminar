using System;
using System.Reflection.Metadata.Ecma335;
using System.Windows.Input;
using Avalonia;
using Avalonia.Interactivity;

namespace Laminar.Avalonia.DragDrop;

public class DropTargetHandler : Interactive
{
    public static readonly RoutedEvent<DropTargetEventArgs> DropEvent = RoutedEvent.Register<DropTargetHandler, DropTargetEventArgs>(nameof(OnDrop), RoutingStrategies.Direct);
    
    public static readonly RoutedEvent<DropTargetEventArgs> HoverEnterEvent = RoutedEvent.Register<DropTargetHandler, DropTargetEventArgs>(nameof(OnHoverEnter), RoutingStrategies.Direct);

    public static readonly RoutedEvent<DropTargetEventArgs> HoverLeaveEvent = RoutedEvent.Register<DropTargetHandler, DropTargetEventArgs>(nameof(OnHoverLeave), RoutingStrategies.Direct);

    public static readonly AttachedProperty<ICommand?> DropCommandProperty = AvaloniaProperty.RegisterAttached<DropTargetHandler, Visual, ICommand?>("DropCommand");
    public static ICommand? GetDropCommand(Visual visual) => visual.GetValue(DropCommandProperty);
    public static void SetDropCommand(Visual visual, ICommand? command) => visual.SetValue(DropCommandProperty, command);
    
    public static readonly AttachedProperty<ICommand?> HoverEnterCommandProperty = AvaloniaProperty.RegisterAttached<DropTargetHandler, Visual, ICommand?>("HoverEnterCommand", inherits: false);
    public static ICommand? GetHoverEnterCommand(Visual visual) => visual.GetValue(HoverEnterCommandProperty);
    public static void SetHoverEnterCommand(Visual visual, ICommand? command) => visual.SetValue(HoverEnterCommandProperty, command);
    
    public static readonly AttachedProperty<ICommand?> HoverLeaveCommandProperty = AvaloniaProperty.RegisterAttached<DropTargetHandler, Visual, ICommand?>("HoverLeaveCommand");
    public static ICommand? GetHoverLeaveCommand(Visual visual) => visual.GetValue(HoverLeaveCommandProperty);
    public static void SetHoverLeaveCommand(Visual visual, ICommand? command) => visual.SetValue(HoverLeaveCommandProperty, command);
    
    public static readonly AttachedProperty<DropAcceptor> DropAcceptorProperty = AvaloniaProperty.RegisterAttached<DropTargetHandler, Visual, DropAcceptor>(nameof(DropAcceptor), defaultValue: new DropAcceptor(), inherits: false);
    public static DropAcceptor GetDropAcceptor(Visual visual) => visual.GetValue(DropAcceptorProperty);
    public static void SetDropAcceptor(Visual visual, DropAcceptor value) => visual.SetValue(DropAcceptorProperty, value);
    
    public static readonly AttachedProperty<IDropTarget?> DropTargetProperty = AvaloniaProperty.RegisterAttached<DropTargetHandler, Visual, IDropTarget?>("DropTarget");
    public static IDropTarget? GetDropTarget(Visual visual) => visual.GetValue(DropTargetProperty);
    public static void SetDropTarget(Visual visual, IDropTarget? value) => visual.SetValue(DropTargetProperty, value);
    
    public static readonly AttachedProperty<bool> AnimateHomeOnDropProperty = AvaloniaProperty.RegisterAttached<DropTargetHandler, Visual, bool>("AnimateHomeOnDrop", defaultValue: true);
    public static bool GetAnimateHomeOnDrop(Visual visual) => visual.GetValue(AnimateHomeOnDropProperty);
    public static void SetAnimateHomeOnDrop(Visual visual, bool value) => visual.SetValue(AnimateHomeOnDropProperty, value);
    
    public static readonly AttachedProperty<Visual?> DropPositionRelativeToProperty = AvaloniaProperty.RegisterAttached<DropTargetHandler, Visual, Visual?>("DropPositionRelativeTo");
    public static Visual? GetDropPositionRelativeTo(Visual visual) => visual.GetValue(DropPositionRelativeToProperty);
    public static void SetDropPositionRelativeTo(Visual visual, Visual? value) => visual.SetValue(DropPositionRelativeToProperty, value);
    
    public event EventHandler<DropTargetEventArgs> OnHoverLeave
    {
        add => AddHandler(HoverLeaveEvent, value);
        remove => RemoveHandler(HoverLeaveEvent, value);
    }
    
    public event EventHandler<DropTargetEventArgs> OnDrop
    {
        add => AddHandler(DropEvent, value);
        remove => RemoveHandler(DropEvent, value);
    }
    
    public event EventHandler<DropTargetEventArgs> OnHoverEnter
    {
        add => AddHandler(HoverEnterEvent, value);
        remove => RemoveHandler(HoverEnterEvent, value);
    }

    public static void RaiseEvent(DropTargetEventArgs eventArgs)
    {
        if (eventArgs.CurrentHoverOver is null) return;
        eventArgs.CurrentHoverOver.RaiseEvent(eventArgs);

        ICommand? command = null;

        Visual dropPositionRelativeTo = GetDropPositionRelativeTo(eventArgs.CurrentHoverOver) ?? eventArgs.CurrentHoverOver;
        
        object? payload = eventArgs.DraggingControl.DataContext;
        Point location =
            eventArgs.PointerEventArgs!.GetPosition(dropPositionRelativeTo) - eventArgs.OriginalClickOffset;
        object receptacleTag = eventArgs.ReceptacleTag;
        
        
        switch (eventArgs.EventType)
        {
            case DropTargetEventType.Drop:
                command = GetDropCommand(eventArgs.CurrentHoverOver);
                eventArgs.Handled = GetDropTarget(eventArgs.CurrentHoverOver)?.Drop(payload, location, receptacleTag) ?? false;
                break;
            case DropTargetEventType.HoverEnter:
                command = GetHoverEnterCommand(eventArgs.CurrentHoverOver);
                eventArgs.Handled = GetDropTarget(eventArgs.CurrentHoverOver)?.HoverEnter(payload, location, receptacleTag) ?? false;
                break;
            case DropTargetEventType.HoverLeave:
                command = GetHoverLeaveCommand(eventArgs.CurrentHoverOver);
                eventArgs.Handled = GetDropTarget(eventArgs.CurrentHoverOver)?.HoverLeave(payload, location, receptacleTag) ?? false;
                break;
        }
        
        if (command?.CanExecute(eventArgs) is true)
        {
            command.Execute(eventArgs);
        }
    }
}