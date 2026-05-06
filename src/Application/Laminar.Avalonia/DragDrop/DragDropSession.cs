using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.Rendering.Composition;
using Avalonia.VisualTree;
using Laminar.Avalonia.Animations;

namespace Laminar.Avalonia.DragDrop;

public class DragDropSession : IDisposable
{
    private const double SquaredMinimumDragDistance = 40;

    private readonly PointerPressedEventArgs _startingEvent;
    private readonly Point _localClickOffset;
    private readonly Point _globalOriginalClickPoint;
    private readonly DropTargetEventArgs _hoverEnterEventArgs;
    private readonly DropTargetEventArgs _hoverLeaveEventArgs;
    private readonly DropTargetEventArgs _dropEventArgs;
    
    private State _state;
    private int _draggingControlOriginalZIndex;
    private bool _draggingControlOriginalClipToBounds;
    private TimeSpan _draggingControlOriginalOffsetDuration;
    private PointerEventArgs? _mostRecentMoveEvent;
    private TopLevel? _topLevel;
    
    public DragDropSession(Control senderControl, PointerPressedEventArgs startingEvent)
    {
        _localClickOffset = startingEvent.GetPosition(senderControl);
        _globalOriginalClickPoint = startingEvent.GetPosition(null);
        _startingEvent = startingEvent;

        _hoverEnterEventArgs = DropTargetEventArgs.HoverEnter(this);
        _hoverLeaveEventArgs = DropTargetEventArgs.HoverLeave(this);
        _dropEventArgs = DropTargetEventArgs.Drop(this);
        
        DraggingControl = senderControl;
        
        _state = State.ClickWithoutDrag;
        startingEvent.Handled = true;
    }

    public DragDropDebugRenderer? DebugRenderer { get; set; }
    
    public HoverInfo? CurrentHoverInfo { get; private set; }

    public Control DraggingControl
    {
        get;
        set
        {
            CleanupDraggingControl(field);
            
            field = value;

            _draggingControlOriginalZIndex = value.ZIndex;
            value.ZIndex = int.MaxValue;

            _draggingControlOriginalClipToBounds = value.ClipToBounds;
            value.ClipToBounds = false;

            _draggingControlOriginalOffsetDuration = PositionAnimation.GetDuration(value);
            PositionAnimation.SetDuration(value, TimeSpan.Zero);

            if (!value.IsLoaded)
            {
                value.Loaded += (_, _) => UpdateTopLevel(TopLevel.GetTopLevel(value));
            }
            else
            {   
                UpdateTopLevel(TopLevel.GetTopLevel(value));
            }

            if (_state is State.Drag)
            {
                _startingEvent.Pointer.Capture(value);
                BeingDraggedHandler.StartDrag(DraggingControl);
                // if (_mostRecentMoveEvent is not null) PointerMoved(null, _mostRecentMoveEvent);
            }
        }
    }

    private void PointerMoved(object? sender, PointerEventArgs e)
    {
        if (_state is State.None or State.AnimateHome) return;

        _mostRecentMoveEvent = e;
        Point currentGlobalMousePosition = e.GetPosition(null);
        Vector totalMouseMovement = _globalOriginalClickPoint - currentGlobalMousePosition;

        if (_state == State.ClickWithoutDrag && totalMouseMovement.SquaredLength < SquaredMinimumDragDistance) return;

        // There is a valid drag occuring and the pointer was moved, time for action
        if (_state == State.ClickWithoutDrag)
        {
            e.Handled = true;
            _state = State.Drag;
            e.Pointer.Capture(DraggingControl);
            BeingDraggedHandler.StartDrag(DraggingControl);
        }
        
        var visualTranslationVector = e.GetPosition(DraggingControl) - _localClickOffset;
        ElementComposition.GetElementVisual(DraggingControl)?.Translation = new Vector3D(visualTranslationVector.X, visualTranslationVector.Y, 10);
        
        if (CurrentHoverInfo is { } hoverInfo && hoverInfo.TopLevelReceptacleGeometry.FillContains(currentGlobalMousePosition))
        {
            return;
        }
        
        HoverInfo? previousHoverInfo = CurrentHoverInfo;
        if (!HandlePotentialHoverChange(e)) return;

        if (previousHoverInfo is null)
        {
            DraggingControl.RaiseEvent(BeingDraggedEventArgs.HoverStarted(DraggingControl));
        }
        
        if (CurrentHoverInfo is null)
        {
            DraggingControl.RaiseEvent(BeingDraggedEventArgs.HoverEnded(DraggingControl));
        }
        
        if (previousHoverInfo is { } previous)
        {
            DropTargetHandler.RaiseEvent(_hoverLeaveEventArgs);
        }
    }

    private void PointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (_state is State.None or State.AnimateHome) return;

        if (_state is State.ClickWithoutDrag)
        {
            _startingEvent.Handled = false;
            DraggingControl.RaiseEvent(_startingEvent);
            Completed?.Invoke(this, EventArgs.Empty);
            return;
        }

        e.Handled = true;
        e.PreventGestureRecognition();
        e.Pointer.Capture(null);
        DropTargetHandler.RaiseEvent(_dropEventArgs);
        _ = AnimateHome();
    }

    public event EventHandler? Completed;

    private async Task AnimateHome()
    {
        try
        {
            if (_state is not (State.Drag or State.ClickWithoutDrag)) return;
            _state = State.AnimateHome;
            BeingDraggedHandler.TriggerOnAnimateHome(DraggingControl);
            TranslationAnimation.SetDuration(DraggingControl, DragDrop.AnimateHomeAnimationDuration);
            ElementComposition.GetElementVisual(DraggingControl)?.Translation = new Vector3D(0, 0, 0);
            await Task.Delay(DragDrop.AnimateHomeAnimationDuration);
            TranslationAnimation.SetDuration(DraggingControl, TimeSpan.Zero);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Exception: " + ex.Message);
        }
        finally
        {
            Completed?.Invoke(this, EventArgs.Empty);
        }
    }
    
    public void Dispose()
    {
        CleanupDraggingControl(DraggingControl);
        UpdateTopLevel(null);
        BeingDraggedHandler.EndDrag(DraggingControl);
        DebugRenderer?.EndAll();
        _state = State.None;
        GC.SuppressFinalize(this);
    }
    
    /// <summary>
    /// Calculates a new hover target from a given pointer event. Handles the updating of CurrentHoverInfo
    /// </summary>
    /// <param name="pointerEventArgs"></param>
    /// <returns>True if the CurrentHoverInfo was changed, false otherwise</returns>
    private bool HandlePotentialHoverChange(PointerEventArgs pointerEventArgs)
    {
        if (_topLevel is null) return false;
        HoverInfo? hoverInfoCache = CurrentHoverInfo;
        _hoverEnterEventArgs.Handled = false;
        
        foreach (Visual visualAtPoint in GetAllElementsAtPoint<Visual>(pointerEventArgs, _topLevel))
        {
            if (Equals(visualAtPoint, DraggingControl))
            {
                continue;
            }
            
            if (DebugRenderer is not null && visualAtPoint is Control control)
            {
                DebugRenderer.EnsureAttachedAndUpdated(control);
            }
            
            if (DropTargetHandler.GetDropAcceptor(visualAtPoint).AcceptDrop(visualAtPoint, pointerEventArgs) is not { } receptacle)
            {
                continue;
            }
            
            if (Equals(visualAtPoint, CurrentHoverInfo?.HoverTarget) && Equals(receptacle.Tag, CurrentHoverInfo?.ReceptacleTag))
            {
                CurrentHoverInfo = hoverInfoCache;
                return false;
            }
            
            if (visualAtPoint is Interactive interactiveAtPoint)
            {
                CurrentHoverInfo = new(interactiveAtPoint, receptacle.Tag, receptacle.AcceptsDropRegion);
                DropTargetHandler.RaiseEvent(_hoverEnterEventArgs);
            }
            
            if (_hoverEnterEventArgs.Handled)
            {
                DraggingControl.RaiseEvent(BeingDraggedEventArgs.HoverStarted(DraggingControl));
            
                Matrix matrixToTopLevel = Matrix.Identity;
            
                if (CurrentHoverInfo?.TopLevelReceptacleGeometry.Transform?.Value is { } geometryTransform)
                {
                    matrixToTopLevel *=  geometryTransform;
                }
            
                if (CurrentHoverInfo?.HoverTarget.GetTransformedBounds()?.Transform is { } currentHoverOverTransform)
                {
                    matrixToTopLevel *= currentHoverOverTransform;
                }
                
                CurrentHoverInfo?.TopLevelReceptacleGeometry.Transform = new MatrixTransform(matrixToTopLevel);
                return true;
            }
        }

        CurrentHoverInfo = null;
        return true;
    }
    
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
    
    private void UpdateTopLevel(TopLevel? topLevel)
    {
        if (Equals(_topLevel, topLevel)) return;
        
        _topLevel?.RemoveHandler(InputElement.PointerReleasedEvent, PointerReleased);
        _topLevel?.RemoveHandler(InputElement.PointerMovedEvent, PointerMoved);
        topLevel?.AddHandler(InputElement.PointerReleasedEvent, PointerReleased, handledEventsToo: true);
        topLevel?.AddHandler(InputElement.PointerMovedEvent, PointerMoved,  handledEventsToo: true);
        _topLevel = topLevel;
    }
    
    private void CleanupDraggingControl(Control? control)
    {
        if (control is null) return;
        BeingDraggedHandler.EndDrag(control);
        control.RemoveHandler(InputElement.PointerReleasedEvent, PointerReleased);
        control.RemoveHandler(InputElement.PointerMovedEvent, PointerMoved);
        control.ZIndex = _draggingControlOriginalZIndex;
        control.ClipToBounds = _draggingControlOriginalClipToBounds;
        PositionAnimation.SetDuration(control, _draggingControlOriginalOffsetDuration);
    }
    
    private enum State
    {
        None,
        ClickWithoutDrag,
        Drag,
        AnimateHome,
    }

}
    
public record struct HoverInfo(Interactive HoverTarget, object? ReceptacleTag, Geometry TopLevelReceptacleGeometry);
