using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Shapes;
using Avalonia.Interactivity;
using Avalonia.Media;
using Laminar.Avalonia.Markup;
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
        ConnectorRegistry.MoveConnectionIndicationEvent.AddClassHandler<Connector>((conn, args) => conn.OnMoveConnectionIndication(args));
        ConnectorRegistry.EndConnectionIndicationEvent.AddClassHandler<Connector>((conn, args) => conn.OnEndConnectionIndication(args));
    }

    private Point? _originalClickOffset;
    
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
    
    private void OnMoveConnectionIndication(MoveConnectionIndicationEventArgs args)
    {
        var relativePoint = args.PointerEvent.GetPosition(this);
        _originalClickOffset ??= relativePoint;
        IsHitTestVisible = false;
        switch (DragMode)
        {
            case ConnectorDragMode.MoveEnd:
                Endpoint = relativePoint - _originalClickOffset.Value;
                PseudoClasses.Add(DragActivePseudoclass);
                break;
            case ConnectorDragMode.MoveStart:
                Startpoint = relativePoint - _originalClickOffset.Value;
                PseudoClasses.Add(DragActivePseudoclass);
                break;
            case ConnectorDragMode.None:
                Startpoint = new Point(0, 0);
                Endpoint = new Point(0, 0);
                PseudoClasses.Remove(DragActivePseudoclass);
                break;
        }
    }

    private void OnEndConnectionIndication(RoutedEventArgs args)
    {
        PseudoClasses.Remove(DragActivePseudoclass);
        IsHitTestVisible = true;
        args.Handled = true;
        _originalClickOffset = null;
        Startpoint = new Point(0, 0);
        Endpoint = new Point(0, 0);
    }
}