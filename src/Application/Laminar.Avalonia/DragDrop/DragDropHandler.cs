using System;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Transformation;
using Avalonia.Reactive;
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace Laminar.Avalonia.DragDrop;

public class DragDropHandler
{
    public static readonly AttachedProperty<MouseButton?> TriggerMouseButtonProperty = AvaloniaProperty.RegisterAttached<DragDropHandler, Control, MouseButton?>("TriggerMouseButton");
    public static MouseButton? GetTriggerMouseButton(AvaloniaObject control) => control.GetValue(TriggerMouseButtonProperty);
    public static void SetTriggerMouseButton(AvaloniaObject control, MouseButton? mouseButton) => control.SetValue(TriggerMouseButtonProperty, mouseButton);
    
    private static DropTargetEventArgs? _hoverArgs;
    private static PointerPressedEventArgs? _originalClickEvent;
    private static Point? _clickOffset;
    private static ITransform? _controlOriginalTransform;
    private static bool? _controlIsClipToBounds;
    private static int? _controlZIndex;
    private static bool _dragActive = false;
    
    static DragDropHandler()
    {
        TriggerMouseButtonProperty.Changed.Subscribe(new AnonymousObserver<AvaloniaPropertyChangedEventArgs<MouseButton?>>(TriggerMouseButtonChanged));
    }

    public static DragDropDebugRenderer? DebugRenderer { get; set; }

    private static void TriggerMouseButtonChanged(AvaloniaPropertyChangedEventArgs<MouseButton?> e)
    {
        if (e.Sender is not Interactive inputElementSender)
        {
            throw new Exception($"Property {nameof(TriggerMouseButtonProperty)} is only valid on objects of type {typeof(Interactive)}");
        }
        
        inputElementSender.AddHandler(InputElement.PointerPressedEvent, InputElementSender_PointerPressed);
        inputElementSender.AddHandler(InputElement.PointerReleasedEvent, InputElementSender_PointerReleased);
        inputElementSender.AddHandler(InputElement.PointerMovedEvent, InputElementSender_PointerMoved);
    }

    private static void InputElementSender_PointerMoved(object? sender, PointerEventArgs e)
    {
        if (_hoverArgs is null || !_clickOffset.HasValue || _controlOriginalTransform is null)
        {
            return;
        }

        // There is a valid drag occuring and the pointer was moved, time for action
        if (!_dragActive)
        {
            e.Handled = true;
            _dragActive = true;
            e.Pointer.Capture(_hoverArgs.DraggingControl);
            BeingDraggedHandler.StartDrag(_hoverArgs.DraggingControl);   
        }
        
        var currentClickOffset = e.GetCurrentPoint(null).Position; 
        var transform = TransformOperations.CreateBuilder(2);
        transform.AppendMatrix(_controlOriginalTransform.Value);
        transform.AppendTranslate(currentClickOffset.X - _clickOffset.Value.X, currentClickOffset.Y - _clickOffset.Value.Y);
        _hoverArgs.DraggingControl.RenderTransform = transform.Build();

        var oldHoverInteractive = _hoverArgs.CurrentHoverOver;
        var oldHoverReceptacleTag = _hoverArgs.ReceptacleTag;
        ExecuteDragEventAtPointer(e, _hoverArgs);
        if (oldHoverInteractive is not null && (oldHoverInteractive != _hoverArgs.CurrentHoverOver || oldHoverReceptacleTag != _hoverArgs.ReceptacleTag))
        {
            oldHoverInteractive.RaiseEvent(DropTargetEventArgs.HoverLeave(_hoverArgs.DraggingControl, _hoverArgs.OriginalClickEventArgs, currentHoverInteractive: oldHoverInteractive, receptacleTag: oldHoverReceptacleTag));
            _hoverArgs.DraggingControl.RaiseEvent(BeingDraggedEventArgs.DragStarted(_hoverArgs.DraggingControl));
        }
    }

    private static void InputElementSender_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is not Control senderControl || !PointerHasMouseButton(e.GetCurrentPoint(null), GetTriggerMouseButton(senderControl)) || _hoverArgs is not null)
        {
            return;
        }
        
        _originalClickEvent = e;
        _clickOffset = e.GetCurrentPoint(null).Position;
        _controlIsClipToBounds = senderControl.ClipToBounds;
        senderControl.ClipToBounds = false;
        
        _controlZIndex = senderControl.ZIndex;
        senderControl.ZIndex = int.MaxValue;

        // If the pointer is pressed during an active drag (e.g. while it's animating back), do not change the transform
        if (!_dragActive)
        {
            _controlOriginalTransform = senderControl.RenderTransform ?? new TranslateTransform();
        }
        
        _hoverArgs = DropTargetEventArgs.HoverEnter(senderControl, e);   
        
        e.Handled = true;
    }

    private static void InputElementSender_PointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (_hoverArgs is null || _controlOriginalTransform is null || _clickOffset is null)
        {
            return;
        }

        // A draggable control was clicked on, but it was not dragged.
        if (!_dragActive && _originalClickEvent is not null && _hoverArgs.DraggingControl is Interactive inputElementSender)
        {
            _originalClickEvent.Handled = false;
            inputElementSender.RaiseEvent(_originalClickEvent);
            _hoverArgs = null;
            return;
        }
        
        e.Handled = true;
        e.PreventGestureRecognition();
        DebugRenderer?.EndAll();
        e.Pointer.Capture(null);
        
        ExecuteDragEventAtPointer(e, DropTargetEventArgs.Drop(_hoverArgs.DraggingControl, _hoverArgs.OriginalClickEventArgs));
        AnimateHome(_hoverArgs.DraggingControl, _controlOriginalTransform);
        _clickOffset = null;
    }

    private static void ExecuteDragEventAtPointer(PointerEventArgs pointerEventArgs, DropTargetEventArgs dropTargetEvent)
    {
        dropTargetEvent.Handled = false;
        Interactive? currentHoverVisual = dropTargetEvent.CurrentHoverOver;
        object? currentReceptacleTag = dropTargetEvent.ReceptacleTag;
        
        if (TopLevel.GetTopLevel(dropTargetEvent.DraggingControl) is not { } topLevel) return;

        foreach (Visual visualAtPoint in topLevel.GetVisualsAt(pointerEventArgs.GetPosition(topLevel),
                     visual => visual != dropTargetEvent.DraggingControl))
        {
            if (DebugRenderer is not null && visualAtPoint is Control control)
            {
                DebugRenderer.EnsureAttached(control);
            }
            
            if (!DropTargetHandler.GetDropAcceptor(visualAtPoint).AcceptDrop(visualAtPoint, pointerEventArgs, out object? receptacleTag))
            {
                continue;
            }

            if (visualAtPoint == currentHoverVisual && receptacleTag == currentReceptacleTag)
            {
                dropTargetEvent.CurrentHoverOver = currentHoverVisual;
                dropTargetEvent.ReceptacleTag = dropTargetEvent.ReceptacleTag;
                return;
            }
            
            if (visualAtPoint is Interactive interactiveAtPoint)
            {
                dropTargetEvent.CurrentHoverOver = interactiveAtPoint;
                dropTargetEvent.ReceptacleTag = receptacleTag;
                interactiveAtPoint.RaiseEvent(dropTargetEvent);
                dropTargetEvent.DraggingControl.RaiseEvent(BeingDraggedEventArgs.HoverStarted(dropTargetEvent.DraggingControl));
            }

            if (dropTargetEvent.Handled) return;
        }

        dropTargetEvent.CurrentHoverOver = null;
        dropTargetEvent.ReceptacleTag = null;
    }
    
    private static void AnimateHome(Visual visual, ITransform originalTransform)
    {
        TransformOperationsTransition transformTransition = new()
        {
            Property = Visual.RenderTransformProperty, 
            Duration = new TimeSpan(0, 0, 0, 0, 300),
            Easing = new QuadraticEaseInOut()
        };
        
        visual.Transitions ??= [];
        visual.Transitions.Add(transformTransition);
        
        TransformOperations.Builder transform = TransformOperations.CreateBuilder(2);
        transform.AppendMatrix(originalTransform.Value);
        transform.AppendTranslate(0, 0);
        visual.RenderTransform = transform.Build();

        _ = Task.Run(() =>
        {
            Thread.Sleep(transformTransition.Duration);
            Dispatcher.UIThread.Post(() =>
            {
                visual.Transitions?.Remove(transformTransition);
                visual.ClipToBounds = _controlIsClipToBounds!.Value;
                visual.ZIndex = _controlZIndex!.Value;
                visual.Transitions?.Remove(transformTransition);
                _dragActive = false;
                _hoverArgs = null;
                BeingDraggedHandler.EndDrag(visual);
            });
        });
    }

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