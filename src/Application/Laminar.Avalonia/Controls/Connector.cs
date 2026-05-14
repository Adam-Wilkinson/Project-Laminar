using System.Diagnostics;
using Avalonia;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
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

public class Connector : Shape
{
    public static readonly StyledProperty<Point> StartpointProperty = Connection.StartpointProperty.AddOwner<Connector>();
    
    public static readonly StyledProperty<Point> EndpointProperty = Connection.EndpointProperty.AddOwner<Connector>();

    public static readonly StyledProperty<double> ConnectionWidthProperty = Connection.ConnectionWidthProperty.AddOwner<Connector>();

    public static readonly StyledProperty<double> TipLengthProperty = Connection.TipLengthProperty.AddOwner<Connector>();
    
    public static readonly StyledProperty<ConnectorDragMode> DragModeProperty = AvaloniaProperty.Register<Connector, ConnectorDragMode>(nameof(DragMode)); 
    
    static Connector()
    {
        AffectsGeometry<Connector>(StartpointProperty, EndpointProperty, ConnectionWidthProperty, TipLengthProperty, DragModeProperty);
    }

    private bool _isDragging;
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
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        if (!_isDragging || _originalClickOffset is not { } originalClickOffset) return;

        switch (DragMode)
        {
            case ConnectorDragMode.MoveEnd:
                Endpoint = e.GetPosition(this) - originalClickOffset;
                break;
            case ConnectorDragMode.MoveStart:
                Startpoint = e.GetPosition(this) - originalClickOffset;
                break;
            case ConnectorDragMode.None:
                Startpoint = new Point(0, 0);
                Endpoint = new Point(0, 0);
                break;
        }
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        if (!_isDragging) return;
        _isDragging = false;
        Startpoint = new Point(0, 0);
        Endpoint = new Point(0, 0);
        e.Handled = true;
    }
}