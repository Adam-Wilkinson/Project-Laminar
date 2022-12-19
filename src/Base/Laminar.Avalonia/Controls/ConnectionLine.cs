using Avalonia.Controls.Shapes;
using Avalonia.Media;
using Avalonia;
using System;
using Laminar.Avalonia.NodeDisplaySystem;

namespace Laminar.Avalonia.Controls;

public class ConnectionLine : Shape
{
    public static readonly StyledProperty<Point> StartPointProperty = AvaloniaProperty.Register<ConnectionLine, Point>(nameof(StartPoint));

    public static readonly StyledProperty<Point> EndPointProperty = AvaloniaProperty.Register<ConnectionLine, Point>(nameof(EndPoint));

    static ConnectionLine()
    {
        StrokeThicknessProperty.OverrideDefaultValue<ConnectionLine>(1);
        AffectsGeometry<ConnectionLine>(StartPointProperty, EndPointProperty);
    }

    public Point StartPoint
    {
        get { return GetValue(StartPointProperty); }
        set { SetValue(StartPointProperty, value); }
    }

    public Point EndPoint
    {
        get { return GetValue(EndPointProperty); }
        set { SetValue(EndPointProperty, value); }
    }

    protected override Geometry CreateDefiningGeometry()
    {
        return new ConnectionGeometry { StartPoint = StartPoint, EndPoint = EndPoint };
    }
}