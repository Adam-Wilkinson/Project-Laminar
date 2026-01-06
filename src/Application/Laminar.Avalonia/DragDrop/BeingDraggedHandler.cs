using System;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Laminar.Avalonia.DragDrop;

public class BeingDraggedHandler : Interactive
{
    public static readonly RoutedEvent<BeingDraggedEventArgs> DragStartedEvent =
        RoutedEvent.Register<BeingDraggedHandler, BeingDraggedEventArgs>(nameof(DragStarted), RoutingStrategies.Direct);
    
    public static readonly RoutedEvent<BeingDraggedEventArgs> DragEndedEvent = 
        RoutedEvent.Register<BeingDraggedHandler, BeingDraggedEventArgs>(nameof(DragEnded),  RoutingStrategies.Direct);
    
    public static readonly RoutedEvent<BeingDraggedEventArgs> HoverStartedEvent =
        RoutedEvent.Register<BeingDraggedHandler, BeingDraggedEventArgs>(nameof(HoverStarted), RoutingStrategies.Direct);
    
    public static readonly RoutedEvent<BeingDraggedEventArgs> HoverEndedEvent =
        RoutedEvent.Register<BeingDraggedHandler, BeingDraggedEventArgs>(nameof(HoverEnded), RoutingStrategies.Direct);
    
    public static readonly AttachedProperty<bool> IsBeingDraggedProperty = AvaloniaProperty.RegisterAttached<BeingDraggedHandler, AvaloniaObject, bool>("IsBeingDragged");
    public static bool GetIsBeingDragged(AvaloniaObject control) => control.GetValue(IsBeingDraggedProperty);
    public static void SetIsBeingDragged(AvaloniaObject control, bool value) => control.SetValue(IsBeingDraggedProperty, value);
    
    public static readonly AttachedProperty<ICommand?> DragStartedCommandProperty = AvaloniaProperty.RegisterAttached<BeingDraggedHandler, AvaloniaObject, ICommand?>("DragStartedCommand");
    public static ICommand? GetDragStartedCommand(AvaloniaObject control) => control.GetValue(DragStartedCommandProperty);
    public static void SetDragStartedCommand(AvaloniaObject control, ICommand? value) => control.SetValue(DragStartedCommandProperty, value);
    
    public static readonly AttachedProperty<ICommand?> DragEndedCommandProperty = AvaloniaProperty.RegisterAttached<BeingDraggedHandler, AvaloniaObject, ICommand?>("DragEndedCommand");
    public static ICommand? GetDragEndedCommand(AvaloniaObject control) => control.GetValue(DragEndedCommandProperty);
    public static void SetDragEndedCommand(AvaloniaObject control, ICommand? value) => control.SetValue(DragEndedCommandProperty, value);

    public event EventHandler<BeingDraggedEventArgs> DragStarted
    {
        add => AddHandler(DragStartedEvent, value);
        remove => RemoveHandler(DragStartedEvent, value);
    }

    public event EventHandler<BeingDraggedEventArgs> DragEnded
    {
        add => AddHandler(DragEndedEvent, value);
        remove  => RemoveHandler(DragEndedEvent, value);
    }
    
    public event EventHandler<BeingDraggedEventArgs> HoverStarted
    {
        add => AddHandler(HoverStartedEvent, value);
        remove => RemoveHandler(HoverStartedEvent, value);
    }
    
    public event EventHandler<BeingDraggedEventArgs> HoverEnded
    {
        add => AddHandler(HoverEndedEvent, value);
        remove => RemoveHandler(HoverEndedEvent, value);
    }

    public static void StartDrag(AvaloniaObject aObject)
    {
        SetIsBeingDragged(aObject, true);
        if (aObject is Interactive interactive)
        {
            interactive.RaiseEvent(BeingDraggedEventArgs.DragStarted(aObject));
        }
        
        if (GetDragStartedCommand(aObject) is not { } command) return;
        
        if (command.CanExecute(null))
        {
            command.Execute(null);
        }
        else if (command.CanExecute(aObject))
        {
            command.Execute(aObject);
        }
    }

    public static void EndDrag(AvaloniaObject aObject)
    {
        SetIsBeingDragged(aObject, false);
        if (aObject is Interactive interactive)
        {
            interactive.RaiseEvent(BeingDraggedEventArgs.DragEnded(aObject));
        }
        
        if (GetDragEndedCommand(aObject) is not { } command) return;
        
        if (command.CanExecute(null))
        {
            command.Execute(null);
        }
        else if (command.CanExecute(aObject))
        {
            command.Execute(aObject);
        }
    }
}