using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Rendering.Composition;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Laminar.Avalonia.Animations;
using Laminar.Domain.ValueObjects;
using Point = Avalonia.Point;

namespace Laminar.Avalonia.DragDrop;

public record struct HoverInfo(Interactive HoverTarget, object? ReceptacleTag, Geometry TopLevelReceptacleGeometry);

public class DragDropSession : IDisposable
{
    private const double SquaredMinimumDragDistance = 40;

    private static readonly CompiledBinding ZIndexBinding = int.MaxValue.AsStaticBinding(BindingPriority.Animation);
    private static readonly CompiledBinding ClipToBoundsBinding = false.AsStaticBinding(BindingPriority.Animation);
    private static readonly CompiledBinding OffsetAnimationDurationBinding = TimeSpan.Zero.AsStaticBinding(BindingPriority.Animation);

    private readonly PointerPressedEventArgs _startingEvent;
    private readonly DropTargetEventArgs _hoverEnterEventArgs;
    private readonly DropTargetEventArgs _hoverLeaveEventArgs;
    private readonly DropTargetEventArgs _dropEventArgs;
    private readonly CompositeDisposable _avaloniaValueOverrides = [];
    
    private State _state;
    private PointerEventArgs? _mostRecentMoveEvent;
    private TopLevel? _topLevel;
    
    public DragDropSession(Control senderControl, PointerPressedEventArgs startingEvent)
    {
        OriginalClickOffset = startingEvent.GetPosition(senderControl);
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

    public Point OriginalClickOffset { get; }
    
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
            }
        }
    }

    private void PointerMoved(object? sender, PointerEventArgs e)
    {
        if (_state is State.None or State.AnimateHome) return;

        _mostRecentMoveEvent = e;
        Vector totalMouseMovement = _mostRecentMoveEvent.GetPosition(DraggingControl) - OriginalClickOffset;

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
            _hoverLeaveEventArgs.PointerEventArgs = e;
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
        _dropEventArgs.PointerEventArgs = e;
        
        if (CurrentHoverInfo is null && _topLevel is not null)
        {
            _dropEventArgs.Handled = false;
            foreach (var interactive in GetAllElementsAtPoint<Interactive>(e, _topLevel))
            {
                if (DropTargetHandler.GetDropAcceptor(interactive).AcceptDrop(interactive, e) is not { } receptacle)
                {
                    continue;
                }
                
                CurrentHoverInfo = new(interactive, receptacle.Tag, receptacle.AcceptsDropRegion);
                DropTargetHandler.RaiseEvent(_dropEventArgs);
                if (_dropEventArgs.Handled) break;
            }
        }
        else
        {
            DropTargetHandler.RaiseEvent(_dropEventArgs);
        }

        if (CurrentHoverInfo?.HoverTarget.GetValue(DropTargetHandler.AnimateHomeOnDropProperty) is true)
        {
            _ = AnimateHome();
        }
        else
        {
            Completed?.Invoke(this, EventArgs.Empty);
        }
    }

    private void UpdateActiveControlPosition()
    {
        if (_mostRecentMoveEvent is null) throw new InvalidOperationException();
        Dispatcher.UIThread.Post(() =>
        {
            var visualTranslationVector = _mostRecentMoveEvent.GetPosition(DraggingControl) - OriginalClickOffset;
            ElementComposition.GetElementVisual(DraggingControl)?.Translation = new Vector3D(visualTranslationVector.X, visualTranslationVector.Y, 0);
        }, DispatcherPriority.Loaded);
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
        _hoverEnterEventArgs.PointerEventArgs = pointerEventArgs;
        
        foreach (var interactive in GetAllElementsAtPoint<Interactive>(pointerEventArgs, _topLevel))
        {
            if (Equals(interactive, DraggingControl))
            {
                continue;
            }
            
            if (DebugRenderer is not null && interactive is Control control)
            {
                DebugRenderer.EnsureAttachedAndUpdated(control);
            }
            
            if (DropTargetHandler.GetDropAcceptor(interactive).AcceptDrop(interactive, pointerEventArgs) is not { } receptacle)
            {
                continue;
            }
            
            if (Equals(interactive, CurrentHoverInfo?.HoverTarget) && Equals(receptacle.Tag, CurrentHoverInfo?.ReceptacleTag))
            {
                CurrentHoverInfo = hoverInfoCache;
                return false;
            }
            
            CurrentHoverInfo = new(interactive, receptacle.Tag, receptacle.AcceptsDropRegion);
            DropTargetHandler.RaiseEvent(_hoverEnterEventArgs);
            
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
            if (args.Source is Visual sourceVisual && visual.GetSelfAndVisualAncestors().Contains(sourceVisual))
            {
                continue;
            }
            
            foreach (var ancestor in visual.GetVisualAncestors())
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
        if (_mostRecentMoveEvent is not null) UpdateActiveControlPosition();
        (attached as Layoutable)?.LayoutUpdated += DraggingControlLayoutUpdated;
        
        foreach (var parent in attached.GetSelfAndVisualAncestors())
        {
            if (parent is TopLevel) break;
            _avaloniaValueOverrides.Add(parent.Bind(Visual.ZIndexProperty, ZIndexBinding));
            _avaloniaValueOverrides.Add(parent.Bind(Visual.ClipToBoundsProperty, ClipToBoundsBinding));
        }
        
        attached.AttachedToVisualTree -= DraggingControlAttachedToVisualTree;
    }

    private void CleanupDraggingControl(Control? control)
    {
        if (control is null) return;
        BeingDraggedHandler.EndDrag(control);
        control.LayoutUpdated -= DraggingControlLayoutUpdated;
        control.RemoveHandler(InputElement.PointerReleasedEvent, PointerReleased);
        control.RemoveHandler(InputElement.PointerMovedEvent, PointerMoved);
        _avaloniaValueOverrides.Clear();
    }

    private void DraggingControlLayoutUpdated(object? sender, EventArgs e)
    {
        if (_mostRecentMoveEvent is not null) UpdateActiveControlPosition();
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
    
    private enum State
    {
        None,
        ClickWithoutDrag,
        Drag,
        AnimateHome,
    }
}