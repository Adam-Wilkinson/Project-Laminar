using System;
using Avalonia;
using Avalonia.Controls.Shapes;
using Avalonia.Media;

namespace Laminar.Avalonia.Controls;

public class PointsRectangle : Rectangle
{
    public static readonly StyledProperty<Point> FirstPointProperty = AvaloniaProperty.Register<PointsRectangle, Point>(nameof(FirstPoint));
    public static readonly StyledProperty<Point> SecondPointProperty = AvaloniaProperty.Register<PointsRectangle, Point>(nameof(SecondPoint));

    static PointsRectangle()
    {
        AffectsGeometry<PointsRectangle>(FirstPointProperty, SecondPointProperty);
    }

    public Point FirstPoint
    {
        get => GetValue(FirstPointProperty);
        set => SetValue(FirstPointProperty, value);
    }

    public Point SecondPoint
    {
        get => GetValue(SecondPointProperty);
        set => SetValue(SecondPointProperty, value);
    }

    public Rect Geometry { get; private set; }

    protected override Geometry CreateDefiningGeometry()
    {
        Geometry = new Rect
        (
            Math.Min(FirstPoint.X, SecondPoint.X),
            Math.Min(FirstPoint.Y, SecondPoint.Y),
            Math.Abs(FirstPoint.X - SecondPoint.X),
            Math.Abs(FirstPoint.Y - SecondPoint.Y)
        );
        return new RectangleGeometry(Geometry);
    }
}
