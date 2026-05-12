using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls.Shapes;
using Avalonia.Media;

namespace Laminar.Avalonia.Shapes;

public class Connection : Shape
{
    public static readonly StyledProperty<Point> StartpointProperty = AvaloniaProperty.Register<Connection, Point>(nameof(Startpoint));
    
    public static readonly StyledProperty<Point> EndpointGeometry = AvaloniaProperty.Register<Connection, Point>(nameof(Endpoint));

    static Connection()
    {
        AffectsGeometry<Connection>(StartpointProperty, EndpointGeometry);
    }
    
    public Point Startpoint
    {
        get => GetValue(StartpointProperty);
        set => SetValue(StartpointProperty, value);
    }
    
    public Point Endpoint
    {
        get => GetValue(EndpointGeometry);
        set => SetValue(EndpointGeometry, value);
    }

    protected override Geometry CreateDefiningGeometry()
    {
        return Generate(Startpoint, Endpoint, 10, 10);
    }
    
    public static Geometry Generate(Point startPoint, Point endPoint, double width, double tipLength)
    {
        Point shiftedEnd = endPoint + new Vector(6, 0);
        
        StreamGeometry connectionLine = new();
        using (StreamGeometryContext connectionLineContext = connectionLine.Open())
        {
            connectionLineContext.BeginFigure(startPoint);
            connectionLineContext.LineTo(shiftedEnd);
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