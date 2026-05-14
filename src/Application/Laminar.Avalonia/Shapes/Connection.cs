using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls.Shapes;
using Avalonia.Media;

namespace Laminar.Avalonia.Shapes;

public class Connection : Shape
{
    public static readonly StyledProperty<Point> StartpointProperty = AvaloniaProperty.Register<Connection, Point>(nameof(Startpoint));
    
    public static readonly StyledProperty<Point> EndpointProperty = AvaloniaProperty.Register<Connection, Point>(nameof(Endpoint));

    public static readonly StyledProperty<double> ConnectionWidthProperty = AvaloniaProperty.Register<Connection, double>(nameof(ConnectionWidth), 15);

    public static readonly StyledProperty<double> TipLengthProperty = AvaloniaProperty.Register<Connection, double>(nameof(TipLength), 10);
    
    static Connection()
    {
        AffectsGeometry<Connection>(StartpointProperty, EndpointProperty, ConnectionWidthProperty);
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
        return Generate(Startpoint, Endpoint, ConnectionWidth, TipLength);
    }
    
    public static Geometry Generate(Point startPoint, Point endPoint, double width, double tipLength)
    {
        Point shiftedEnd = endPoint + new Vector(6, 0);
        double horizontalMidpoint = (startPoint.X + endPoint.X) / 2;
        
        StreamGeometry connectionLine = new();
        using (StreamGeometryContext connectionLineContext = connectionLine.Open())
        {
            connectionLineContext.BeginFigure(startPoint);
            connectionLineContext.CubicBezierTo(
                new Point(horizontalMidpoint, startPoint.Y),
                new Point(horizontalMidpoint, endPoint.Y),
                shiftedEnd);
            connectionLineContext.EndFigure(false);
        }
        
        var widenedLine = connectionLine.GetWidenedGeometry(new Pen(Brushes.White, width));

        PolylineGeometry tipGeometry = new(GenerateTip(shiftedEnd, width, tipLength), true);
        
        return new CombinedGeometry(GeometryCombineMode.Union, widenedLine, tipGeometry);
    }

    private static IEnumerable<Point> GenerateTip(Point location, double width, double tipLength)
    {
        yield return location + new Vector(0, width / 2);
        yield return location + new Vector(tipLength, 0);
        yield return location - new Vector(0, width / 2);
    }
}