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

// BOOK-KEEPING: Global point is with respect to top level, LocalPoint is with respect to currently dragged control
using GlobalPoint = Avalonia.Point;
using LocalPoint = Avalonia.Point;

namespace Laminar.Avalonia.DragDrop;

public enum DragDropState
{
    None,
    ClickWithoutDrag,
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
    private static CurrentDragInfo? _currentDragInfo;
    private static Vector? _currentTransformVector;
    private static Geometry? _currentReceptacleGeometryTopLevel;
    
    static DragDropHandler()
    {
        TriggerMouseButtonProperty.Changed.Subscribe(new AnonymousObserver<AvaloniaPropertyChangedEventArgs<MouseButton>>(TriggerMouseButtonChanged));
    }

    public static DragDropDebugRenderer? DebugRenderer { get; set; }

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
        
        if (_state != DragDropState.Drag || _currentDragInfo is not { } currentDragInfo || !e.NewValue.HasValue || e.NewValue == MouseButton.None) return;

        if (e.Sender is not Control senderControl || !Equals(senderControl.DataContext, currentDragInfo.EventArgs.DraggingControl.DataContext)) return;
        
        PositionAnimation.SetDuration(senderControl, TimeSpan.Zero);
        _currentDragInfo = currentDragInfo with
        {
            EventArgs = DropTargetEventArgs.HoverEnter(senderControl, currentDragInfo.OriginalClickEventArgs),
            ControlOriginalTransform = senderControl.RenderTransform ?? new TranslateTransform(0, 0),
        };
    }

    private static void InputElementSender_BoundsChanged(AvaloniaPropertyChangedEventArgs obj)
    {
        if (_currentDragInfo is not { } currentDragInfo || _currentTransformVector is not { } currentTransformVector) return;
        if (currentDragInfo.EventArgs.DraggingControl.GetTransformedBounds() is not { } transformedBounds || currentDragInfo.EventArgs.DraggingControl != obj.Sender) return;
        
        Rect globalCoordsBounds = transformedBounds.Bounds.TransformToAABB(transformedBounds.Transform);
        GlobalPoint changeInTopLeft = globalCoordsBounds.TopLeft - currentDragInfo.ControlOriginalTopLeft;
        currentTransformVector -= changeInTopLeft;
        _currentDragInfo = currentDragInfo with { ControlOriginalTopLeft = globalCoordsBounds.TopLeft } ;
        
        TransformOperations.Builder transform = TransformOperations.CreateBuilder(2);
        transform.AppendMatrix(_currentDragInfo.Value.ControlOriginalTransform.Value);
        transform.AppendTranslate(currentTransformVector.X, currentTransformVector.Y);
        _currentDragInfo.Value.EventArgs.DraggingControl.RenderTransform = transform.Build();
        _currentTransformVector = currentTransformVector;
    }
    
    private static void InputElementSender_PointerMoved(object? sender, PointerEventArgs e)
    {
        if (_currentDragInfo is not { } currentDragInfo || !currentDragInfo.EventArgs.DraggingControl.IsLoaded || _currentTransformVector is not { } currentTransformVector) return;
        if (_state is DragDropState.None or DragDropState.AnimateHome) return;
        
        // There is a valid drag occuring and the pointer was moved, time for action
        if (_state == DragDropState.ClickWithoutDrag)
        {
            e.Handled = true;
            _state = DragDropState.Drag;
            e.Pointer.Capture(currentDragInfo.EventArgs.DraggingControl);
            PositionAnimation.SetDuration(currentDragInfo.EventArgs.DraggingControl, TimeSpan.Zero);
            BeingDraggedHandler.StartDrag(currentDragInfo.EventArgs.DraggingControl);
        }

        currentTransformVector += e.GetPosition(currentDragInfo.EventArgs.DraggingControl) - currentDragInfo.ClickOffset;
        TransformOperations.Builder transform = TransformOperations.CreateBuilder(2);
        transform.AppendMatrix(currentDragInfo.ControlOriginalTransform.Value);
        transform.AppendTranslate(currentTransformVector.X, currentTransformVector.Y);
        currentDragInfo.EventArgs.DraggingControl.RenderTransform = transform.Build();
        _currentTransformVector = currentTransformVector;

        if (_currentReceptacleGeometryTopLevel is not null && _currentReceptacleGeometryTopLevel.FillContains(e.GetPosition(null)))
        {
            return;
        }
        
        Interactive? oldHoverInteractive = currentDragInfo.EventArgs.CurrentHoverOver;
        object? oldHoverReceptacleTag = currentDragInfo.EventArgs.ReceptacleTag;
        ExecuteDragEventAtPointer(e, currentDragInfo.EventArgs);
        if (oldHoverInteractive is not null && (oldHoverInteractive != currentDragInfo.EventArgs.CurrentHoverOver || oldHoverReceptacleTag != currentDragInfo.EventArgs.ReceptacleTag))
        {
            DropTargetHandler.RaiseEvent(DropTargetEventArgs.HoverLeave(currentDragInfo.EventArgs.DraggingControl, currentDragInfo.OriginalClickEventArgs, currentHoverInteractive: oldHoverInteractive, receptacleTag: oldHoverReceptacleTag));
            currentDragInfo.EventArgs.DraggingControl.RaiseEvent(BeingDraggedEventArgs.HoverStarted(currentDragInfo.EventArgs.DraggingControl));
        }
    }

    private static void InputElementSender_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is not Control senderControl 
            || !PointerHasMouseButton(e.GetCurrentPoint(null), GetTriggerMouseButton(senderControl))
            || _state is DragDropState.AnimateHome or DragDropState.Drag or DragDropState.ClickWithoutDrag
            || senderControl.GetTransformedBounds() is not { } transformedBounds)
        {
            return;
        }

        _currentDragInfo = new CurrentDragInfo
        {
            OriginalClickEventArgs = e,
            EventArgs = DropTargetEventArgs.HoverEnter(senderControl, e),
            ControlOriginalTransform = senderControl.RenderTransform ?? new TranslateTransform(0, 0),
            ClickOffset = e.GetPosition(senderControl),
            ControlOriginalTopLeft = transformedBounds.Bounds.TransformToAABB(transformedBounds.Transform).TopLeft,
            ControlOriginalPositionAnimationDuration = PositionAnimation.GetDuration(senderControl),
            ControlOriginalZIndex =  senderControl.ZIndex,
            ControlWasClipToBounds = senderControl.ClipToBounds
        };
        
        _currentTransformVector = Vector.Zero;
        senderControl.ClipToBounds = false;
        senderControl.ZIndex = int.MaxValue;
        _state = DragDropState.ClickWithoutDrag;
        e.Handled = true;
    }

    private static void InputElementSender_PointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (_state is DragDropState.None or DragDropState.AnimateHome || _currentDragInfo is not { } currentDragInfo)
        {
            return;
        }
        
        if (_state == DragDropState.ClickWithoutDrag)
        {
            currentDragInfo.OriginalClickEventArgs.Handled = false;
            currentDragInfo.EventArgs.DraggingControl.RaiseEvent(currentDragInfo.OriginalClickEventArgs);
            _currentDragInfo = null;
            _state = DragDropState.None;
            return;
        }
        
        e.Handled = true;
        e.PreventGestureRecognition();
        DebugRenderer?.EndAll();
        e.Pointer.Capture(null);
        
        ExecuteDragEventAtPointer(e, DropTargetEventArgs.Drop(currentDragInfo.EventArgs.DraggingControl, currentDragInfo.OriginalClickEventArgs));
        AnimateHome(currentDragInfo.EventArgs.DraggingControl, currentDragInfo.ControlOriginalTransform);
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
            
            if (DropTargetHandler.GetDropAcceptor(visualAtPoint).AcceptDrop(visualAtPoint, pointerEventArgs) is not { } receptacle)
            {
                continue;
            }
            
            if (Equals(visualAtPoint, currentHoverVisual) && Equals(receptacle.Tag, currentReceptacleTag))
            {
                dropTargetEvent.CurrentHoverOver = currentHoverVisual;
                dropTargetEvent.ReceptacleTag = currentReceptacleTag;
                return;
            }
            
            if (visualAtPoint is Interactive interactiveAtPoint)
            {
                dropTargetEvent.CurrentHoverOver = interactiveAtPoint;
                dropTargetEvent.ReceptacleTag = receptacle.Tag;
                DropTargetHandler.RaiseEvent(dropTargetEvent);
            }
            
            if (dropTargetEvent.Handled)
            {
                dropTargetEvent.DraggingControl.RaiseEvent(BeingDraggedEventArgs.HoverStarted(dropTargetEvent.DraggingControl));
                _currentReceptacleGeometryTopLevel = receptacle.AcceptsDropRegion;

                Matrix matrixToTopLevel = Matrix.Identity;

                if (_currentReceptacleGeometryTopLevel.Transform?.Value is { } geometryTransform)
                {
                    matrixToTopLevel *=  geometryTransform;
                }

                if (dropTargetEvent.CurrentHoverOver?.GetTransformedBounds()?.Transform is { } currentHoverOverTransform)
                {
                    matrixToTopLevel *= currentHoverOverTransform;
                }
                
                _currentReceptacleGeometryTopLevel.Transform = new MatrixTransform(matrixToTopLevel);
                return;
            }
        }
        
        dropTargetEvent.CurrentHoverOver = null;
        dropTargetEvent.ReceptacleTag = null;
    }
    
    private static void AnimateHome(Visual visual, ITransform originalTransform)
    {
        if (_currentDragInfo is not { } currentDragInfo) return;
        
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
                visual.ClipToBounds = currentDragInfo.ControlWasClipToBounds;
                visual.ZIndex = currentDragInfo.ControlOriginalZIndex;
                BeingDraggedHandler.EndDrag(visual);
                PositionAnimation.SetDuration(visual, currentDragInfo.ControlOriginalPositionAnimationDuration);

                _state = DragDropState.None;
                _currentDragInfo = null;
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
        foreach (Visual visual in topLevel.GetVisualsAt(args.GetPosition(topLevel)))
        {
            foreach (ILogical ancestor in visual.GetLogicalAncestors())
            {
                if (ancestor is T correctType)
                {
                    yield return correctType;
                }
            }
        }
    }

    private record struct CurrentDragInfo(
        DropTargetEventArgs EventArgs,
        PointerPressedEventArgs OriginalClickEventArgs,
        ITransform ControlOriginalTransform,
        LocalPoint ClickOffset,
        GlobalPoint ControlOriginalTopLeft,
        bool ControlWasClipToBounds,
        int ControlOriginalZIndex,
        TimeSpan ControlOriginalPositionAnimationDuration
    );

    public void OnApplicationBuilt()
    {
        _logger = logger;
    }
}