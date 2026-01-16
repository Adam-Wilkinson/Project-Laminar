using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.Media.Transformation;
using Avalonia.Reactive;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Laminar.Avalonia.Animations;
using Laminar.Avalonia.InitializationTargets;
using Microsoft.Extensions.Logging;

namespace Laminar.Avalonia.DragDrop;

public enum DragDropState
{
    None,
    ClickWithoutMove,
    Drag,
    AnimateHome,
}

public class DragDropHandler(ILogger<DragDropHandler> logger) : IAfterApplicationBuiltTarget
{
    public static readonly AttachedProperty<MouseButton> TriggerMouseButtonProperty = AvaloniaProperty.RegisterAttached<DragDropHandler, Control, MouseButton>("TriggerMouseButton", MouseButton.None);
    public static MouseButton GetTriggerMouseButton(AvaloniaObject control) => control.GetValue(TriggerMouseButtonProperty);
    public static void SetTriggerMouseButton(AvaloniaObject control, MouseButton mouseButton) => control.SetValue(TriggerMouseButtonProperty, mouseButton);

    private static DragDropState _state = DragDropState.None;
    private static ILogger<DragDropHandler>? _logger;
    private static DropTargetEventArgs? _hoverArgs;
    private static PointerPressedEventArgs? _originalClickEvent;
    private static ITransform? _controlOriginalTransform;
    private static bool? _controlIsClipToBounds;
    private static int? _controlZIndex;
    private static Vector? _controlTopLeftInTopLevel;
    private static Vector? _clickOffsetInDraggedCoords;
    private static Vector? _currentTransformVector;
    private static Point? _controlOriginalTopleft;
    private static TimeSpan? _originalControlPositionAnimationDuration;
    
    static DragDropHandler()
    {
        TriggerMouseButtonProperty.Changed.Subscribe(new AnonymousObserver<AvaloniaPropertyChangedEventArgs<MouseButton>>(TriggerMouseButtonChanged));
    }

    public static DragDropDebugRenderer? DebugRenderer { get; set; }// = new DragDropDebugRenderer<TreeViewDropAcceptor>();

    public static TimeSpan AnimateHomeAnimationDuration { get; set; } = TimeSpan.Zero;

    private static void TriggerMouseButtonChanged(AvaloniaPropertyChangedEventArgs<MouseButton> e)
    {
        if (e.Sender is not Interactive inputElementSender)
        {
            throw new Exception($"Property {nameof(TriggerMouseButtonProperty)} is only valid on objects of type {typeof(Interactive)}");
        }
        
        if (e.OldValue != MouseButton.None && e.NewValue == MouseButton.None)
        {
            inputElementSender.RemoveHandler(InputElement.PointerPressedEvent, InputElementSender_PointerPressed);
            inputElementSender.RemoveHandler(InputElement.PointerMovedEvent, InputElementSender_PointerMoved);
            inputElementSender.RemoveHandler(InputElement.PointerReleasedEvent, InputElementSender_PointerReleased);
        }
        
        if (e.NewValue.HasValue && e.NewValue.Value != MouseButton.None)
        {
            inputElementSender.AddHandler(InputElement.PointerPressedEvent, InputElementSender_PointerPressed);
            inputElementSender.AddHandler(InputElement.PointerMovedEvent, InputElementSender_PointerMoved);   
            inputElementSender.AddHandler(InputElement.PointerReleasedEvent, InputElementSender_PointerReleased);
            inputElementSender.GetPropertyChangedObservable(Visual.BoundsProperty).Subscribe(
                new AnonymousObserver<AvaloniaPropertyChangedEventArgs>(InputElementSender_BoundsChanged));
        }
        
        if (_state != DragDropState.Drag || _hoverArgs is null || !e.NewValue.HasValue || e.NewValue == MouseButton.None) return;

        if (e.Sender is not Control element ||
            !Equals(element.DataContext, _hoverArgs.DraggingControl.DataContext)) return;
        
        _hoverArgs = DropTargetEventArgs.HoverEnter(element, _hoverArgs.OriginalClickEventArgs);
        _controlOriginalTransform = element.RenderTransform ?? new TranslateTransform(0, 0);
        PositionAnimation.SetDuration(_hoverArgs.DraggingControl, TimeSpan.Zero);
    }

    private static void InputElementSender_BoundsChanged(AvaloniaPropertyChangedEventArgs obj)
    {
        if (_hoverArgs is null || _hoverArgs.DraggingControl != obj.Sender) return;
        
        var transformedBounds = _hoverArgs.DraggingControl.GetTransformedBounds();
        var globalCoordsBounds = transformedBounds.Value.Bounds.TransformToAABB(transformedBounds.Value.Transform);

        var controlCurrentTopLeft = globalCoordsBounds.TopLeft;
        var changeInTopLeft = controlCurrentTopLeft - _controlOriginalTopleft;
        _currentTransformVector -= changeInTopLeft;
        _controlOriginalTopleft = controlCurrentTopLeft;
        
        TransformOperations.Builder transform = TransformOperations.CreateBuilder(2);
        transform.AppendMatrix(_controlOriginalTransform.Value);
        transform.AppendTranslate(_currentTransformVector.Value.X, _currentTransformVector.Value.Y);
        _hoverArgs.DraggingControl.RenderTransform = transform.Build();
    }
    
    private static void InputElementSender_PointerMoved(object? sender, PointerEventArgs e)
    {
        if (_hoverArgs is null || !_hoverArgs.DraggingControl.IsLoaded || _controlOriginalTransform is null || _currentTransformVector is null || _clickOffsetInDraggedCoords is null)
        {
            return;
        }
        
        if (_state is DragDropState.None or DragDropState.AnimateHome) return;
        // There is a valid drag occuring and the pointer was moved, time for action
        if (_state == DragDropState.ClickWithoutMove)
        {
            e.Handled = true;
            _state = DragDropState.Drag;
            e.Pointer.Capture(_hoverArgs.DraggingControl);
            _originalControlPositionAnimationDuration = PositionAnimation.GetDuration(_hoverArgs.DraggingControl);
            PositionAnimation.SetDuration(_hoverArgs.DraggingControl, TimeSpan.Zero);
            BeingDraggedHandler.StartDrag(_hoverArgs.DraggingControl);   
        }

        _currentTransformVector += e.GetPosition(_hoverArgs.DraggingControl) - _clickOffsetInDraggedCoords;
        TransformOperations.Builder transform = TransformOperations.CreateBuilder(2);
        transform.AppendMatrix(_controlOriginalTransform.Value);
        transform.AppendTranslate(_currentTransformVector.Value.X, _currentTransformVector.Value.Y);
        _hoverArgs.DraggingControl.RenderTransform = transform.Build();
        
        Interactive? oldHoverInteractive = _hoverArgs.CurrentHoverOver;
        object? oldHoverReceptacleTag = _hoverArgs.ReceptacleTag;
        ExecuteDragEventAtPointer(e, _hoverArgs);
        if (oldHoverInteractive is not null && (oldHoverInteractive != _hoverArgs.CurrentHoverOver || oldHoverReceptacleTag != _hoverArgs.ReceptacleTag))
        {
            DropTargetHandler.RaiseEvent(DropTargetEventArgs.HoverLeave(_hoverArgs.DraggingControl, _hoverArgs.OriginalClickEventArgs, currentHoverInteractive: oldHoverInteractive, receptacleTag: oldHoverReceptacleTag));
            _hoverArgs.DraggingControl.RaiseEvent(BeingDraggedEventArgs.HoverStarted(_hoverArgs.DraggingControl));
        }
    }

    private static void InputElementSender_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is not Control senderControl 
            || !PointerHasMouseButton(e.GetCurrentPoint(null), GetTriggerMouseButton(senderControl))
            || _state is DragDropState.AnimateHome or DragDropState.Drag
            || senderControl.GetTransformedBounds() is not { } transformedBounds)
        {
            return;
        }
        
        _originalClickEvent = e;
        _controlOriginalTransform = senderControl.RenderTransform ?? new TranslateTransform(0, 0);
        _clickOffsetInDraggedCoords = e.GetPosition(senderControl);
        _controlOriginalTopleft = transformedBounds.Bounds.TransformToAABB(transformedBounds.Transform).TopLeft;
        _currentTransformVector = Vector.Zero;
        _controlIsClipToBounds = senderControl.ClipToBounds;
        _controlZIndex = senderControl.ZIndex;
        senderControl.ClipToBounds = false;
        senderControl.ZIndex = int.MaxValue;
        
        _hoverArgs = DropTargetEventArgs.HoverEnter(senderControl, e);
        _state = DragDropState.ClickWithoutMove;
        e.Handled = true;
    }

    private static void InputElementSender_PointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (_state is DragDropState.None or DragDropState.AnimateHome ||_hoverArgs is null || _controlOriginalTransform is null)
        {
            return;
        }
        
        if (_state == DragDropState.ClickWithoutMove && _originalClickEvent is not null)
        {
            _originalClickEvent.Handled = false;
            _hoverArgs.DraggingControl.RaiseEvent(_originalClickEvent);
            _hoverArgs = null;
            return;
        }
        
        e.Handled = true;
        e.PreventGestureRecognition();
        DebugRenderer?.EndAll();
        e.Pointer.Capture(null);
        
        ExecuteDragEventAtPointer(e, DropTargetEventArgs.Drop(_hoverArgs.DraggingControl, _hoverArgs.OriginalClickEventArgs));
        AnimateHome(_hoverArgs.DraggingControl, _controlOriginalTransform);
    }

    private static void ExecuteDragEventAtPointer(PointerEventArgs pointerEventArgs, DropTargetEventArgs dropTargetEvent)
    {
        dropTargetEvent.Handled = false;
        Interactive? currentHoverVisual = dropTargetEvent.CurrentHoverOver;
        object? currentReceptacleTag = dropTargetEvent.ReceptacleTag;
        
        if (TopLevel.GetTopLevel(dropTargetEvent.DraggingControl) is not { } topLevel) return;
        
        foreach (Visual visualAtPoint in GetAllElementsAtPoint<Visual>(pointerEventArgs, topLevel))
        {
            if (Equals(visualAtPoint, dropTargetEvent.DraggingControl))
            {
                continue;
            }
            
            if (DebugRenderer is not null && visualAtPoint is Control control && dropTargetEvent.EventType == DropTargetEventType.HoverEnter)
            {
                DebugRenderer.EnsureAttachedAndUpdated(control);
            }
            
            if (!DropTargetHandler.GetDropAcceptor(visualAtPoint).AcceptDrop(visualAtPoint, pointerEventArgs, out object? receptacleTag))
            {
                continue;
            }
            
            if (Equals(visualAtPoint, currentHoverVisual) && Equals(receptacleTag, currentReceptacleTag))
            {
                dropTargetEvent.CurrentHoverOver = currentHoverVisual;
                dropTargetEvent.ReceptacleTag = currentReceptacleTag;
                return;
            }
            
            if (visualAtPoint is Interactive interactiveAtPoint)
            {
                dropTargetEvent.CurrentHoverOver = interactiveAtPoint;
                dropTargetEvent.ReceptacleTag = receptacleTag;
                DropTargetHandler.RaiseEvent(dropTargetEvent);
            }
            
            if (dropTargetEvent.Handled)
            {
                dropTargetEvent.DraggingControl.RaiseEvent(BeingDraggedEventArgs.HoverStarted(dropTargetEvent.DraggingControl));
                return;
            }
        }
        
        dropTargetEvent.CurrentHoverOver = null;
        dropTargetEvent.ReceptacleTag = null;
    }
    
    private static void AnimateHome(Visual visual, ITransform originalTransform)
    {
        _state = DragDropState.AnimateHome;
        BeingDraggedHandler.TriggerOnAnimateHome(visual);
        
        TransformOperationsTransition transformTransition = new()
        {
            Property = Visual.RenderTransformProperty, 
            Duration = AnimateHomeAnimationDuration,
            Easing = new QuadraticEaseOut()
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
                _state = DragDropState.None;
                _hoverArgs = null;
                BeingDraggedHandler.EndDrag(visual);
                if (_originalControlPositionAnimationDuration is { } timeSpan)
                {
                    PositionAnimation.SetDuration(visual, timeSpan);
                }
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

    private static IEnumerable<T> GetAllElementsAtPoint<T>(PointerEventArgs args, TopLevel topLevel)
    {
        foreach (var visual in topLevel.GetVisualsAt(args.GetPosition(topLevel)))
        {
            foreach (var ancestor in visual.GetLogicalAncestors())
            {
                if (ancestor is T correctType)
                {
                    yield return correctType;
                }
            }
        }
    }
    
    public void OnApplicationBuilt()
    {
        _logger = logger;
    }
}