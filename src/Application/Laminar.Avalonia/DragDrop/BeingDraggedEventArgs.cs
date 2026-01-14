using System;
using Avalonia;
using Avalonia.Interactivity;

namespace Laminar.Avalonia.DragDrop;

public class BeingDraggedEventArgs : RoutedEventArgs
{
    public static BeingDraggedEventArgs DragStarted(AvaloniaObject targetObject) => new(BeingDraggedHandler.DragStartedEvent, targetObject);

    public static BeingDraggedEventArgs AnimateHomeStarted(AvaloniaObject targetObject) => new(BeingDraggedHandler.AnimateHomeStartedEvent, targetObject);
    
    public static BeingDraggedEventArgs DragEnded(AvaloniaObject targetObject) => new(BeingDraggedHandler.DragEndedEvent, targetObject);
    
    public static BeingDraggedEventArgs HoverStarted(AvaloniaObject targetObject) => new(BeingDraggedHandler.HoverStartedEvent, targetObject);
    
    public static BeingDraggedEventArgs HoverEnded(AvaloniaObject targetObject) => new(BeingDraggedHandler.HoverEndedEvent, targetObject);
    
    private BeingDraggedEventArgs(RoutedEvent<BeingDraggedEventArgs> routedEvent, AvaloniaObject draggedObject) : base(routedEvent, draggedObject)
    {
    }
}