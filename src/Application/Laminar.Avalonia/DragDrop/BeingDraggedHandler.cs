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

    public static readonly RoutedEvent<BeingDraggedEventArgs> AnimateHomeStartedEvent =
        RoutedEvent.Register<BeingDraggedHandler, BeingDraggedEventArgs>(nameof(AnimateHomeStarted), RoutingStrategies.Direct);
    
    public static readonly RoutedEvent<BeingDraggedEventArgs> HoverStartedEvent =
        RoutedEvent.Register<BeingDraggedHandler, BeingDraggedEventArgs>(nameof(HoverStarted), RoutingStrategies.Direct);
    
    public static readonly RoutedEvent<BeingDraggedEventArgs> HoverEndedEvent =
        RoutedEvent.Register<BeingDraggedHandler, BeingDraggedEventArgs>(nameof(HoverEnded), RoutingStrategies.Direct);
    
    public static readonly AttachedProperty<bool> IsBeingDraggedProperty = AvaloniaProperty.RegisterAttached<BeingDraggedHandler, AvaloniaObject, bool>("IsBeingDragged", inherits: true);
    public static bool GetIsBeingDragged(AvaloniaObject control) => control.GetValue(IsBeingDraggedProperty);
    public static void SetIsBeingDragged(AvaloniaObject control, bool value) => control.SetValue(IsBeingDraggedProperty, value);
    
    public static readonly AttachedProperty<ICommand?> DragStartedCommandProperty = AvaloniaProperty.RegisterAttached<BeingDraggedHandler, AvaloniaObject, ICommand?>("DragStartedCommand");
    public static ICommand? GetDragStartedCommand(AvaloniaObject control) => control.GetValue(DragStartedCommandProperty);
    public static void SetDragStartedCommand(AvaloniaObject control, ICommand? value) => control.SetValue(DragStartedCommandProperty, value);
    
    public static readonly AttachedProperty<ICommand?> AnimateHomeStartedCommandProperty = AvaloniaProperty.RegisterAttached<BeingDraggedHandler, AvaloniaObject, ICommand?>("AnimateHomeStartedCommand");
    public static ICommand? GetAnimateHomeStartedCommand(AvaloniaObject control) => control.GetValue(AnimateHomeStartedCommandProperty);
    public static void SetAnimateHomeStartedCommand(AvaloniaObject control, ICommand? value) => control.SetValue(AnimateHomeStartedCommandProperty, value);
    
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
    
    public event EventHandler<BeingDraggedEventArgs> AnimateHomeStarted
    {
        add => AddHandler(AnimateHomeStartedEvent, value);
        remove => RemoveHandler(AnimateHomeStartedEvent, value);
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
        
        TryExecuteCommandProperty(DragStartedCommandProperty, aObject);
    }

    public static void TriggerOnAnimateHome(AvaloniaObject aObject)
    {
        if (aObject is Interactive interactive)
        {
            interactive.RaiseEvent(BeingDraggedEventArgs.AnimateHomeStarted(aObject));
        }
        
        TryExecuteCommandProperty(AnimateHomeStartedCommandProperty, aObject);
    }
    
    public static void EndDrag(AvaloniaObject aObject)
    {
        SetIsBeingDragged(aObject, false);
        if (aObject is Interactive interactive)
        {
            interactive.RaiseEvent(BeingDraggedEventArgs.DragEnded(aObject));
        }

        TryExecuteCommandProperty(DragEndedCommandProperty, aObject);
    }

    private static bool TryExecuteCommandProperty(AvaloniaProperty commandProperty, AvaloniaObject target)
    {
        object? commandPropertyValue = target.GetValue(commandProperty);
        
        if (commandPropertyValue is not ICommand command)
        {
            return false;
        }
        
        if (command.CanExecute(null))
        {
            command.Execute(null);
            return true;
        }

        if (command.CanExecute(target))
        {
            command.Execute(target);
            return true;
        }

        if (target is StyledElement styledElement && command.CanExecute(styledElement.DataContext))
        {
            command.Execute(styledElement.DataContext);
            return true;
        }

        return false;
    }
}