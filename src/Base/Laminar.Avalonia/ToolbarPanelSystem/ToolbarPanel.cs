using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;

namespace Laminar.Avalonia.ToolbarPanelSystem;

public class ToolbarPanel : Panel
{
    public static readonly TimeSpan AnimationDuration = new(0, 0, 0, 0, 200);
    public static readonly AttachedProperty<DockDirection> DockDirectionProperty = AvaloniaProperty.RegisterAttached<ToolbarPanel, IControl, DockDirection>("DockDirection");
    public static readonly AttachedProperty<int> LevelProperty = AvaloniaProperty.RegisterAttached<ToolbarPanel, IControl, int>("Level");
    public static readonly StyledProperty<double> DefaultWidthProperty = AvaloniaProperty.Register<ToolbarPanel, double>(nameof(DefaultWidth), 150);
    public static readonly StyledProperty<double> PaddingProperty = AvaloniaProperty.Register<ToolbarPanel, double>(nameof(Padding), 10);

    private readonly SidePanel[] sortedChildren =
    {
            new SidePanel(Orientation.Horizontal, false),
            new SidePanel(Orientation.Horizontal, true),
            new SidePanel(Orientation.Vertical, false),
            new SidePanel(Orientation.Vertical, true),
        };

    private readonly List<IControl> centerControl = new();

    private DockDirection dragDirection = DockDirection.None;
    private DragContext dragContext;

    public ToolbarPanel()
    {
        this.GetObservable(DefaultWidthProperty).Subscribe((value) => SidePanelLayer.DefaultWidth = value);
        this.GetObservable(PaddingProperty).Subscribe((value) => SidePanel.Padding = value);
    }

    public enum DockDirection
    {
        Center,
        Top,
        Bottom,
        Left,
        Right,
        None,
    }

    public Canvas DrawingCanvas { get; } = new Canvas();

    public double DefaultWidth
    {
        get => GetValue(DefaultWidthProperty);
        set => SetValue(DefaultWidthProperty, value);
    }

    public double Padding
    {
        get => GetValue(PaddingProperty);
        set => SetValue(PaddingProperty, value);
    }

    public static DockDirection GetDockDirection(IControl control)
    {
        if (control == null)
        {
            throw new ArgumentNullException(nameof(control));
        }

        return control.GetValue(DockDirectionProperty);
    }

    public static void SetDockDirection(IControl control, DockDirection dockDirection)
    {
        if (control == null)
        {
            throw new ArgumentNullException(nameof(control));
        }

        control.SetValue(DockDirectionProperty, dockDirection);
    }

    public static int GetLevel(IControl control)
    {
        if (control == null)
        {
            throw new ArgumentNullException(nameof(control));
        }

        return control.GetValue(LevelProperty);
    }

    public static void SetLevel(IControl control, int level)
    {
        if (control == null)
        {
            throw new ArgumentNullException(nameof(control));
        }

        control.SetValue(LevelProperty, level);
    }

    public void DrawingCanvas_PointerMoved(object sender, PointerEventArgs e)
    {
        if (e == null)
        {
            throw new ArgumentNullException(nameof(e));
        }

        switch (dragContext.DragMode)
        {
            case DragMode.MovingControl:
                bool placed = false;
                foreach (SidePanel sp in sortedChildren)
                {
                    bool displacement = sp.TryPlaceControl(dragContext.ToolbarGrip, e.GetPosition(this));
                    if (displacement)
                    {
                        placed = true;
                    }
                }

                if (!placed && dragContext.ToolbarGrip.SidePanelLayer != null)
                {
                    dragContext.ToolbarGrip.TryRemoveFromParent();
                }

                dragContext.ToolbarGrip.UpdatePositions(new Rect(e.GetPosition(this), dragContext.ToolbarGrip.ControlSize));
                InvalidateArrange();
                break;
            case DragMode.ResizingControl:
            case DragMode.ResizingWidth:
                sortedChildren[(int)dragDirection - 1].MovedClick(e.GetPosition(this), dragContext);
                InvalidateArrange();
                break;
        }
    }

    public void EndControlDrag()
    {
        if (dragContext.ToolbarGrip.SidePanelLayer == null && dragContext.SidePanelLayer != null)
        {
            dragContext.SidePanelLayer.AddControlAt(dragContext.ControlIndex, dragContext.ToolbarGrip);
        }

        dragContext.DragMode = DragMode.None;
        dragContext.ToolbarGrip.SetZIndex(0);
        dragContext.ToolbarGrip.IsPlaced = true;
        foreach (SidePanel sp in sortedChildren)
        {
            sp.FinishControlDrag();
        }

        InvalidateArrange();
    }

    internal void BeginControlDrag(DragContext dragContext)
    {
        this.dragContext = dragContext;
        InvalidateArrange();
        foreach (SidePanel sp in sortedChildren)
        {
            sp.PrepForToolbarDrag();
        }
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        AddChildren(Children);
        Children.Add(DrawingCanvas);
        DrawingCanvas.PointerPressed += DrawingCanvas_PointerPressed;
        DrawingCanvas.PointerMoved += DrawingCanvas_PointerMoved;
        DrawingCanvas.PointerReleased += DrawingCanvas_PointerReleased;
    }

    protected override void ChildrenChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        if (e == null)
        {
            throw new ArgumentNullException(nameof(e));
        }

        base.ChildrenChanged(sender, e);
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                AddChildren(e.NewItems);
                break;
            case NotifyCollectionChangedAction.Remove:
                break;
            case NotifyCollectionChangedAction.Move:
                break;
            case NotifyCollectionChangedAction.Replace:
                break;
            case NotifyCollectionChangedAction.Reset:
                break;
        }
    }

    protected override Size ArrangeOverride(Size arrangeSize)
    {
        Rect remainingSpace = new(arrangeSize);
        DrawingCanvas.Arrange(remainingSpace);

        foreach (SidePanel sp in sortedChildren)
        {
            remainingSpace = sp.Arrange(remainingSpace);
        }

        foreach (IControl control in centerControl)
        {
            if (remainingSpace.Height > 0)
            {
                control.Arrange(remainingSpace);
            }
        }

        return arrangeSize;
    }

    private void AddChildren(System.Collections.IList children)
    {
        foreach (IControl control in children)
        {
            control.GetObservable(DockDirectionProperty).Subscribe((dd) =>
            {
                centerControl.Remove(control);
                if (dd != DockDirection.Center && dd != DockDirection.None && !ToolbarManager.GetGrip(control))
                {
                    ToolbarManager newAdorner = new(control, this);
                    ToolbarManager.SetGrip(control, true);
                    sortedChildren[(int)dd - 1].AddToolbar(newAdorner);
                }

                if (dd == DockDirection.Center && control != DrawingCanvas)
                {
                    centerControl.Add(control);
                }
            });
        }
    }

    private void DrawingCanvas_PointerReleased(object sender, PointerReleasedEventArgs e)
    {
        if (dragContext.DragMode == DragMode.ResizingControl)
        {
            dragContext.ToolbarGrip.ReinstateEndTransition();
        }
        else if (dragContext.DragMode == DragMode.ResizingWidth)
        {
            SidePanel.EndClick(dragContext);
        }

        dragDirection = DockDirection.Center;
        dragContext.DragMode = DragMode.None;
    }

    private void DrawingCanvas_PointerPressed(object sender, PointerPressedEventArgs e)
    {
        for (int i = 0; i < sortedChildren.Length; i++)
        {
            dragContext = sortedChildren[i].SendClick(e.GetPosition(this));
            if (dragContext.DragMode != DragMode.None)
            {
                dragDirection = (DockDirection)(i + 1);
                break;
            }
        }
    }
}
