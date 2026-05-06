using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.Rendering.Composition;
using Avalonia.VisualTree;
using Laminar.Avalonia.Animations;
using Laminar.Domain.ValueObjects;
using Point = Avalonia.Point;

namespace Laminar.Avalonia.DragDrop;

public record struct HoverInfo(Interactive HoverTarget, object? ReceptacleTag, Geometry TopLevelReceptacleGeometry);

public class DragDropSession : IDisposable
{
    private const double SquaredMinimumDragDistance = 40;

    private static readonly CompiledBinding ZIndexBinding =
        CompiledBinding.Create<int, int>(x => x, source: int.MaxValue, priority: BindingPriority.Animation);
    private static readonly CompiledBinding ClipToBoundsBinding =
        CompiledBinding.Create<bool, bool>(x => x, source: false, priority: BindingPriority.Animation);
    private static readonly CompiledBinding OffsetAnimationDurationBinding =
        CompiledBinding.Create<TimeSpan, TimeSpan>(x => x, source: TimeSpan.Zero, priority: BindingPriority.Animation);

    private readonly PointerPressedEventArgs _startingEvent;
    private readonly Point _localClickOffset;
    private readonly DropTargetEventArgs _hoverEnterEventArgs;
    private readonly DropTargetEventArgs _hoverLeaveEventArgs;
    private readonly DropTargetEventArgs _dropEventArgs;
    private readonly CompositeDisposable _avaloniaValueOverrides = [];
    
    private State _state;
    private PointerEventArgs? _mostRecentMoveEvent;
    private TopLevel? _topLevel;
    
    public DragDropSession(Control senderControl, PointerPressedEventArgs startingEvent)
    {
        _localClickOffset = startingEvent.GetPosition(senderControl);
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
            
            _avaloniaValueOverrides.Add(value.Bind(PositionAnimation.DurationProperty, OffsetAnimationDurationBinding));
            
            if (value.IsAttachedToVisualTree())
            {
                DraggingControlAttachedToVisualTree(value, null);
            }
            else
            {   
                value.AttachedToVisualTree += DraggingControlAttachedToVisualTree;
            }

            if (_state is State.Drag)
            {
                _startingEvent.Pointer.Capture(value);
                BeingDraggedHandler.StartDrag(DraggingControl);
                if (_mostRecentMoveEvent is not null) UpdateActiveControlPosition();
            }
        }
    }

    private void PointerMoved(object? sender, PointerEventArgs e)
    {
        if (_state is State.None or State.AnimateHome) return;

        _mostRecentMoveEvent = e;
        Vector totalMouseMovement = _mostRecentMoveEvent.GetPosition(DraggingControl) - _localClickOffset;

        if (_state == State.ClickWithoutDrag && totalMouseMovement.SquaredLength < SquaredMinimumDragDistance) return;

        // There is a valid drag occuring and the pointer was moved, time for action
        if (_state == State.ClickWithoutDrag)
        {
            e.Handled = true;
            _state = State.Drag;
            e.Pointer.Capture(DraggingControl);
            BeingDraggedHandler.StartDrag(DraggingControl);
        }
        
        UpdateActiveControlPosition();
        
        if (CurrentHoverInfo is { } hoverInfo && hoverInfo.TopLevelReceptacleGeometry.FillContains(e.GetPosition(null)))
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

    private void UpdateActiveControlPosition()
    {
        if (_mostRecentMoveEvent is null) throw new InvalidOperationException();
        var visualTranslationVector = _mostRecentMoveEvent.GetPosition(DraggingControl) - _localClickOffset;
        ElementComposition.GetElementVisual(DraggingControl)?.Translation = new Vector3D(visualTranslationVector.X, visualTranslationVector.Y, 0);
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
            ElementComposition.GetElementVisual(DraggingControl)?.Translation = default;
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
        ElementComposition.GetElementVisual(DraggingControl)?.Translation = default;
        DebugRenderer?.EndAll();
        _avaloniaValueOverrides.Dispose();
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
    
    private void DraggingControlAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs? _)
    {
        if (sender is not Visual attached) throw new InvalidOperationException();
        UpdateTopLevel(TopLevel.GetTopLevel(attached));
        foreach (var parent in attached.GetSelfAndVisualAncestors())
        {
            if (parent is TopLevel) break;
            _avaloniaValueOverrides.Add(parent.Bind(Visual.ZIndexProperty, ZIndexBinding));
            _avaloniaValueOverrides.Add(parent.Bind(Visual.ClipToBoundsProperty, ClipToBoundsBinding));
        }

        attached.AttachedToVisualTree -= DraggingControlAttachedToVisualTree;
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
        _avaloniaValueOverrides.Clear();
    }
    
    private enum State
    {
        None,
        ClickWithoutDrag,
        Drag,
        AnimateHome,
    }
}