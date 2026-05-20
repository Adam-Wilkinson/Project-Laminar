using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Media;
using Laminar.Avalonia.Shapes;

namespace Laminar.Avalonia.Controls;

public enum ConnectorDragMode
{
    None,
    MoveStart,
    MoveEnd,
}

[PseudoClasses(DragActivePseudoclass)]
public class Connector : Shape
{
    private const string DragActivePseudoclass =  ":drag-active";
    
    public static readonly StyledProperty<Point> StartpointProperty = Connection.StartpointProperty.AddOwner<Connector>();
    
    public static readonly StyledProperty<Point> EndpointProperty = Connection.EndpointProperty.AddOwner<Connector>();

    public static readonly StyledProperty<double> ConnectionWidthProperty = Connection.ConnectionWidthProperty.AddOwner<Connector>();

    public static readonly StyledProperty<double> TipLengthProperty = Connection.TipLengthProperty.AddOwner<Connector>();
    
    public static readonly StyledProperty<ConnectorDragMode> DragModeProperty = AvaloniaProperty.Register<Connector, ConnectorDragMode>(nameof(DragMode));

    public static readonly StyledProperty<TimeSpan> AnimateHomeDurationProperty = AvaloniaProperty.Register<Connector, TimeSpan>(nameof(AnimateHomeDuration));
    
    static Connector()
    {
        AffectsGeometry<Connector>(StartpointProperty, EndpointProperty, ConnectionWidthProperty, TipLengthProperty, DragModeProperty);
    }

    private bool _isDragging;
    private Point? _originalClickOffset;

    private PointTransition _endpointAnimateHomeTransition;
    private PointTransition _startpointAnimateHomeTransition;
    
    public Connector()
    {
        _endpointAnimateHomeTransition = new PointTransition()
        {
            Property = EndpointProperty,
            [!TransitionBase.DurationProperty] = this[!AnimateHomeDurationProperty]
        };
        _startpointAnimateHomeTransition = new PointTransition()
        {
            Property = StartpointProperty,
            [!TransitionBase.DurationProperty] = this[!AnimateHomeDurationProperty]
        };
    }
    
    public Point Startpoint
    {
        get => GetValue(StartpointProperty);
        set => SetValue(StartpointProperty, value);
    }
    
    public Point Endpoint
    {
        get => GetValue(EndpointProperty);
        set => SetValue(EndpointProperty, value);
    }

    public ConnectorDragMode DragMode
    {
        get => GetValue(DragModeProperty);
        set => SetValue(DragModeProperty, value);
    }
    
    public double ConnectionWidth
    {
        get => GetValue(ConnectionWidthProperty);
        set => SetValue(ConnectionWidthProperty, value);
    }

    public double TipLength
    {
        get => GetValue(TipLengthProperty);
        set => SetValue(TipLengthProperty, value);
    }

    public TimeSpan AnimateHomeDuration
    {
        get => GetValue(AnimateHomeDurationProperty);
        set => SetValue(AnimateHomeDurationProperty, value);
    }
    
    protected override Geometry CreateDefiningGeometry()
    {
        return Connection.Generate(Startpoint, Endpoint, ConnectionWidth, TipLength);
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        if (DragMode is ConnectorDragMode.None) return;
        _isDragging = true;
        _originalClickOffset = e.GetPosition(this);
        e.Handled = true;
        IsEnabled = false;
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        if (!_isDragging || _originalClickOffset is not { } originalClickOffset) return;

        switch (DragMode)
        {
            case ConnectorDragMode.MoveEnd:
                Endpoint = e.GetPosition(this) - originalClickOffset;
                PseudoClasses.Add(DragActivePseudoclass);
                break;
            case ConnectorDragMode.MoveStart:
                Startpoint = e.GetPosition(this) - originalClickOffset;
                PseudoClasses.Add(DragActivePseudoclass);
                break;
            case ConnectorDragMode.None:
                Startpoint = new Point(0, 0);
                Endpoint = new Point(0, 0);
                PseudoClasses.Remove(DragActivePseudoclass);
                break;
        }
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        if (!_isDragging) return;
        _isDragging = false;
        PseudoClasses.Remove(DragActivePseudoclass);
        IsEnabled = true;
        e.Handled = true;
        
        Transitions ??= [];
        Transitions.Add(_startpointAnimateHomeTransition);
        Transitions.Add(_endpointAnimateHomeTransition);
        Startpoint = new Point(0, 0);
        Endpoint = new Point(0, 0);
        Dispatcher.InvokeAsync(async () =>
        {
            await Task.Delay(_startpointAnimateHomeTransition.Duration);
            Transitions.Remove(_endpointAnimateHomeTransition);
            Transitions.Remove(_startpointAnimateHomeTransition);
        });
    }
}