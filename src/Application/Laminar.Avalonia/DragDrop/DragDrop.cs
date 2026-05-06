using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;

namespace Laminar.Avalonia.DragDrop;

public class DragDrop
{
    /// <summary>
    ///  The mouse button that is listened to for drag/drop starting
    /// </summary>
    public static readonly AttachedProperty<MouseButton> TriggerMouseButtonProperty = AvaloniaProperty.RegisterAttached<DragDrop, Control, MouseButton>("TriggerMouseButton");
    public static MouseButton GetTriggerMouseButton(AvaloniaObject control) => control.GetValue(TriggerMouseButtonProperty);
    public static void SetTriggerMouseButton(AvaloniaObject control, MouseButton mouseButton) => control.SetValue(TriggerMouseButtonProperty, mouseButton); 
    
    /// <summary>
    /// An identifier that must be unique amongst all controls with drag/drop enabled. If null, DataContext will be used
    /// </summary>
    public static readonly AttachedProperty<object?> IdentifierProperty = AvaloniaProperty.RegisterAttached<DragDrop, Control, object?>("Identifier");
    public static object? GetIdentifier(AvaloniaObject control) => control.GetValue(IdentifierProperty);
    public static void SetIdentifier(AvaloniaObject control, object? identifier) => control.SetValue(IdentifierProperty, identifier);
    
    private static DragDropSession? _activeSession;
    
    static DragDrop()
    {
        TriggerMouseButtonProperty.Changed.AddClassHandler<Control>(TriggerMouseButtonChanged);
        IdentifierProperty.Changed.AddClassHandler<Control>(IdentifierChanged);
    }
    
    public static TimeSpan AnimateHomeAnimationDuration { get; set; } = TimeSpan.Zero;

    private static void IdentifierChanged(Control control, AvaloniaPropertyChangedEventArgs arg2)
    {
        ReplaceCurrentDragIfIdentifierMatches(control);
    }

    private static void TriggerMouseButtonChanged(Control control, AvaloniaPropertyChangedEventArgs args)
    {
        if (args.NewValue is MouseButton.None)
        {
            control.RemoveHandler(InputElement.PointerPressedEvent, DraggablePointerPressed);
        }
        else
        {
            control.AddHandler(InputElement.PointerPressedEvent, DraggablePointerPressed, handledEventsToo: true);
            ReplaceCurrentDragIfIdentifierMatches(control);
        }
    }

    private static void DraggablePointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (_activeSession is not null 
            || sender is not Control senderControl 
            || !PointerHasMouseButton(e.GetCurrentPoint(null), GetTriggerMouseButton(senderControl))) return;
        
        _activeSession = new DragDropSession(senderControl, e);
        _activeSession.Completed += (_, _) =>
        {
            _activeSession.Dispose();
            _activeSession = null;
        };
    }

    private static void ReplaceCurrentDragIfIdentifierMatches(Control potentialMatch)
    {
        if (_activeSession is null) return;
        
        if (!Equals(GetIdentifierOrDataContext(potentialMatch), GetIdentifierOrDataContext(_activeSession.DraggingControl))) 
            return;
        
        if (_activeSession.DraggingControl.IsLoaded)
            throw new InvalidOperationException("New identifier matches currently active drag operation");
            
        _activeSession.DraggingControl = potentialMatch;
    }

    private static object GetIdentifierOrDataContext(Control control) 
        => GetIdentifier(control) ?? control.DataContext ?? throw new InvalidOperationException("Identifier not found");

    private static bool PointerHasMouseButton(PointerPoint pointer, MouseButton? button) => button switch
    {
        MouseButton.Left => pointer.Properties.IsLeftButtonPressed,
        MouseButton.Right => pointer.Properties.IsRightButtonPressed,
        MouseButton.Middle => pointer.Properties.IsMiddleButtonPressed,
        MouseButton.XButton1 => pointer.Properties.IsXButton1Pressed,
        MouseButton.XButton2 => pointer.Properties.IsXButton2Pressed,
        _ => false,
    };
}