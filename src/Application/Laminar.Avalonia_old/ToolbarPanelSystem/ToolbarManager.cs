using System;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Laminar.Avalonia.Utils;
using Laminar.Avalonia.Utils.CloningSystem;

namespace Laminar.Avalonia.ToolbarPanelSystem;

public class ToolbarManager : Animatable
{
    public static readonly AttachedProperty<bool> HasGripProperty = AvaloniaProperty.RegisterAttached<ToolbarPanel, IControl, bool>("HasGrip");
    public static readonly StyledProperty<double> ToolbarStartProperty = AvaloniaProperty.Register<ToolbarManager, double>(nameof(ToolbarStart), 0.0);
    public static readonly StyledProperty<double> ToolbarEndProperty = AvaloniaProperty.Register<ToolbarManager, double>(nameof(ToolbarEnd), 1.0);

    private readonly DoubleTransition[] positionTransitions = new DoubleTransition[]
    {
        new DoubleTransition() { Property = ToolbarStartProperty, Duration = ToolbarPanel.AnimationDuration, Easing = new ExponentialEaseOut() },
        new DoubleTransition() { Property = ToolbarEndProperty, Duration = ToolbarPanel.AnimationDuration, Easing = new ExponentialEaseOut() },
    };

    private readonly IControl childToolbar;
    private readonly ToolbarPanel parent;
    private readonly Grip grip;
    private Orientation orientation;

    public ToolbarManager(IControl control, ToolbarPanel parent)
    {
        if (parent == null)
        {
            throw new ArgumentNullException(nameof(parent));
        }

        grip = new Grip();
        ToolbarPanel.SetDockDirection(grip, DockDirection.None);
        grip.PointerPressed += ClickArea_PointerPressed;
        grip.PointerReleased += ClickArea_PointerReleased;
        grip.PointerMoved += ClickArea_PointerMoved;

        this.GetObservable(ToolbarEndProperty).Subscribe((_) =>
        {
            parent?.InvalidateArrange();
        });

        Transitions = new Transitions();

        Transitions.AddRange(positionTransitions);

        this.parent = parent;
        childToolbar = control;
        parent.Children.Add(grip);
    }

    public double ToolbarStart
    {
        get => GetValue(ToolbarStartProperty);
        set => SetValue(ToolbarStartProperty, value);
    }

    public double ToolbarEnd
    {
        get => GetValue(ToolbarEndProperty);
        set => SetValue(ToolbarEndProperty, value);
    }

    public Canvas GripCanvas => parent.DrawingCanvas;

    public DockDirection DockDirection
    {
        set
        {
            ToolbarPanel.SetDockDirection(childToolbar, value);
        }
    }

    public Size DesiredSize => childToolbar.DesiredSize;

    public int Level => ToolbarPanel.GetLevel(childToolbar);

    public double MinDepth => Orientation == Orientation.Horizontal ? childToolbar.Height : childToolbar.Width;

    public Orientation Orientation
    {
        get => orientation;
        set
        {
            if (value != orientation)
            {
                childToolbar.SetValue(StackPanel.OrientationProperty, value);
                orientation = value;
                UpdatePositions(childToolbar.Bounds);
            }
        }
    }

    public int Index { get; set; }

    public Line Resizer { get; private set; } = new Line() { Classes = new Classes("ResizingLine") };

    public double WidthDelta { get; set; }

    public double ToolbarWidth { get; set; }

    public bool IsPlaced { get; set; } = true;

    public Size ControlSize { get; set; }

    internal SidePanelLayer SidePanelLayer { get; set; }

    public static bool GetGrip(IControl control)
    {
        if (control == null)
        {
            throw new ArgumentNullException(nameof(control));
        }

        return control.GetValue(HasGripProperty);
    }

    public static void SetGrip(IControl control, bool value)
    {
        if (control == null)
        {
            throw new ArgumentNullException(nameof(control));
        }

        control.SetValue(HasGripProperty, value);
    }

    public void UpdatePositions(Rect bounds)
    {
        double width = double.IsNaN(grip.Width) ? 0.0 : grip.Width;
        grip.RenderTransformOrigin = new RelativePoint(new Point(0, 0), RelativeUnit.Relative);

        if (Orientation == Orientation.Vertical)
        {
            grip.RenderTransform = new MatrixTransform(MatrixHelper.Rotation(Math.PI / 2, new Vector(bounds.Width / 2, bounds.Width / 2)));
            grip.Arrange(new Rect(bounds.X, bounds.Y, width, bounds.Width));
            childToolbar?.Arrange(new Rect(bounds.X, bounds.Y + width, bounds.Width, Math.Max(0.0, bounds.Height - width)));
        }
        else
        {
            grip.RenderTransform = new MatrixTransform(MatrixHelper.Rotation(0));
            grip.Arrange(new Rect(bounds.X, bounds.Y, width, bounds.Height));
            childToolbar?.Arrange(new Rect(bounds.X + width, bounds.Y, Math.Max(0.0, bounds.Width - width), bounds.Height));
        }
    }

    public void SetZIndex(int index)
    {
        childToolbar.ZIndex = index;
        grip.ZIndex = index;
    }

    public void TryRemoveFromParent() => SidePanelLayer?.RemoveControlAt(Index);

    public void Measure(Size childConstraint) => childToolbar.Measure(childConstraint);

    public void RemoveEndTransition()
    {
        Transitions.Remove(positionTransitions[1]);
        SidePanelLayer.Controls[Index + 1].Transitions.Remove(SidePanelLayer.Controls[Index + 1].positionTransitions[0]);
    }

    public void ReinstateEndTransition()
    {
        ToolbarWidth += WidthDelta;
        SidePanelLayer.Controls[Index + 1].ToolbarWidth -= WidthDelta;
        WidthDelta = 0;
        SidePanelLayer.Controls[Index + 1].WidthDelta = 0;
        Transitions.Add(positionTransitions[1]);
        SidePanelLayer.Controls[Index + 1].Transitions.Add(SidePanelLayer.Controls[Index + 1].positionTransitions[0]);
    }

    private void ClickArea_PointerPressed(object sender, PointerPressedEventArgs e)
    {
        ToolbarManager toMove;
        if (e.KeyModifiers == KeyModifiers.Control && ToolbarPanel.GetCloneCommand(childToolbar) is Func<IControl, IControl> controlGenerator)
        {
            IControl newControl = controlGenerator(childToolbar);
            ToolbarPanel.SetDockDirection(newControl, DockDirection.None);
            parent.Children.Add(newControl);
            toMove = new ToolbarManager(newControl, parent)
            {
                Orientation = Orientation,
            };
            SetGrip(newControl, true);
        }
        else
        {
            TryRemoveFromParent();
            toMove = this;
        }

        toMove.SetZIndex(10);
        toMove.IsPlaced = false;

        parent.BeginControlDrag(new DragContext()
        {
            DragMode = DragMode.MovingControl,
            ControlIndex = Index,
            SidePanelLayer = SidePanelLayer,
            InitialClickPoint = e.GetPosition(parent),
            ToolbarGrip = toMove,
        });
        e.Handled = true;
    }

    private void ClickArea_PointerReleased(object sender, PointerReleasedEventArgs e)
    {
        parent.EndControlDrag();
        e.Handled = true;
    }

    private void ClickArea_PointerMoved(object sender, PointerEventArgs e)
    {
        parent.DrawingCanvas_PointerMoved(sender, e);
        e.Handled = true;
    }
}
