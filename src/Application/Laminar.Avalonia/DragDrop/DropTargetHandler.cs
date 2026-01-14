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
    
    public static readonly AttachedProperty<ICommand?> HoverEnterCommandProperty = AvaloniaProperty.RegisterAttached<DropTargetHandler, Visual, ICommand?>("HoverEnterCommand");
    public static ICommand? GetHoverEnterCommand(Visual visual) => visual.GetValue(HoverEnterCommandProperty);
    public static void SetHoverEnterCommand(Visual visual, ICommand? command) => visual.SetValue(HoverEnterCommandProperty, command);
    
    public static readonly AttachedProperty<ICommand?> HoverLeaveCommandProperty = AvaloniaProperty.RegisterAttached<DropTargetHandler, Visual, ICommand?>("HoverLeaveCommand");
    public static ICommand? GetHoverLeaveCommand(Visual visual) => visual.GetValue(HoverLeaveCommandProperty);
    public static void SetHoverLeaveCommand(Visual visual, ICommand? command) => visual.SetValue(HoverLeaveCommandProperty, command);
    
    public static readonly AttachedProperty<DropAcceptor> DropAcceptorProperty = AvaloniaProperty.RegisterAttached<DropTargetHandler, Visual, DropAcceptor>(nameof(DropAcceptor), defaultValue: new DropAcceptor());
    public static DropAcceptor GetDropAcceptor(Visual visual) => visual.GetValue(DropAcceptorProperty);
    public static void SetDropAcceptor(Visual visual, DropAcceptor value) => visual.SetValue(DropAcceptorProperty, value);
    
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

        ICommand? command = eventArgs.EventType switch
        {
            DropTargetEventType.Drop => GetDropCommand(eventArgs.CurrentHoverOver),
            DropTargetEventType.HoverEnter => GetHoverEnterCommand(eventArgs.CurrentHoverOver),
            DropTargetEventType.HoverLeave => GetHoverLeaveCommand(eventArgs.CurrentHoverOver),
            _ => null
        };
        
        if (command?.CanExecute(eventArgs) is true)
        {
            command.Execute(eventArgs);
        }
    }
}