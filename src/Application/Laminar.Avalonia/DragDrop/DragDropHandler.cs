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
using Laminar.Avalonia.InitializationTargets;
using Microsoft.Extensions.Logging;

namespace Laminar.Avalonia.DragDrop;

public class DragDropHandler(ILogger<DragDropHandler> logger) : IAfterApplicationBuiltTarget
{
    public static readonly AttachedProperty<MouseButton?> TriggerMouseButtonProperty = AvaloniaProperty.RegisterAttached<DragDropHandler, Control, MouseButton?>("TriggerMouseButton");
    public static MouseButton? GetTriggerMouseButton(AvaloniaObject control) => control.GetValue(TriggerMouseButtonProperty);
    public static void SetTriggerMouseButton(AvaloniaObject control, MouseButton? mouseButton) => control.SetValue(TriggerMouseButtonProperty, mouseButton);

    private static ILogger<DragDropHandler>? _logger;
    private static DropTargetEventArgs? _hoverArgs;
    private static PointerPressedEventArgs? _originalClickEvent;
    private static ITransform? _controlOriginalTransform;
    private static bool? _controlIsClipToBounds;
    private static int? _controlZIndex;
    private static bool _dragActive = false;
    private static Vector? _clickOffsetInDraggedCoords;
    private static Vector? _currentTransformVector;
    
    static DragDropHandler()
    {
        TriggerMouseButtonProperty.Changed.Subscribe(new AnonymousObserver<AvaloniaPropertyChangedEventArgs<MouseButton?>>(TriggerMouseButtonChanged));
    }

    public static DragDropDebugRenderer? DebugRenderer { get; set; } = new DragDropDebugRenderer<TreeViewDropAcceptor>();

    public static TimeSpan AnimateHomeAnimationDuration { get; set; } = TimeSpan.Zero;

    private static void TriggerMouseButtonChanged(AvaloniaPropertyChangedEventArgs<MouseButton?> e)
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
            inputElementSender.RemoveHandler(Control.LoadedEvent, InputElementSender_Loaded);
            inputElementSender.RemoveHandler(Control.UnloadedEvent, InputElementSender_Unloaded);
        }

        if (e.NewValue != MouseButton.None)
        {
            inputElementSender.AddHandler(InputElement.PointerPressedEvent, InputElementSender_PointerPressed);
            inputElementSender.AddHandler(InputElement.PointerMovedEvent, InputElementSender_PointerMoved);   
            inputElementSender.AddHandler(InputElement.PointerReleasedEvent, InputElementSender_PointerReleased);
            inputElementSender.AddHandler(Control.LoadedEvent, InputElementSender_Loaded);
            inputElementSender.AddHandler(Control.UnloadedEvent, InputElementSender_Unloaded);
        }
    }

    private static void InputElementSender_Unloaded(object? sender, RoutedEventArgs e)
    {
        if (_hoverArgs is null || e.Source is not Visual unloadedVisual) return;
    }

    private static void InputElementSender_Loaded(object? sender, RoutedEventArgs e)
    {
        if (_hoverArgs is null) return;

        if (e.Source is Control element && Equals(element.DataContext, _hoverArgs.DraggingControl.DataContext))
        {
            _hoverArgs = DropTargetEventArgs.HoverEnter(element, _hoverArgs.OriginalClickEventArgs);
        }
    }

    private static void InputElementSender_PointerMoved(object? sender, PointerEventArgs e)
    {
        if (_hoverArgs is null || _controlOriginalTransform is null)
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

        var currentClickOffsetLocal = e.GetPosition(_hoverArgs.DraggingControl);
        var neededChangeInTranslation = currentClickOffsetLocal - _clickOffsetInDraggedCoords;
        _currentTransformVector += neededChangeInTranslation;
        
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
        if (sender is not Control senderControl || !PointerHasMouseButton(e.GetCurrentPoint(null), GetTriggerMouseButton(senderControl)) || _hoverArgs is not null)
        {
            return;
        }
        
        _originalClickEvent = e;
        _clickOffsetInDraggedCoords = e.GetPosition(senderControl);
        _currentTransformVector = Vector.Zero;
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
        if (_hoverArgs is null || _controlOriginalTransform is null)
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