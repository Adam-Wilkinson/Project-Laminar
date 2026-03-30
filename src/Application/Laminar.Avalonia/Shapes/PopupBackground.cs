using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls.Shapes;
using Avalonia.Media;

namespace Laminar.Avalonia.Shapes;

public class PopupBackground : Shape
{
    public static readonly StyledProperty<double> TeardropSizeProperty 
        = AvaloniaProperty.Register<PopupBackground, double>(nameof(TeardropSize));
    
    static PopupBackground()
    {
        AffectsGeometry<PopupBackground>(BoundsProperty, TeardropSizeProperty);
    }

    public double TeardropSize
    {
        get => GetValue(TeardropSizeProperty);
        set => SetValue(TeardropSizeProperty, value);
    }
    
    protected override Geometry? CreateDefiningGeometry()
    {
        return GetGeometry(Bounds, TeardropSize);
    }

    public static Geometry GetGeometry(Rect bounds, double teardropSize)
        => new PolylineGeometry(GetPoints(bounds, teardropSize), true); 

    private static IEnumerable<Point> GetPoints(Rect bounds, double teardropSize)
    {
        double trueTeardropSize = Math.Min(teardropSize, bounds.Width / 2);   
        yield return new Point(0, trueTeardropSize);
        yield return new Point(trueTeardropSize, 0);
        yield return new Point(trueTeardropSize * 2, trueTeardropSize);
        yield return new Point(bounds.Width, trueTeardropSize);
        yield return new Point(bounds.Width, bounds.Height);
        yield return new Point(0, bounds.Height);
    }
}

public class PopupFlyoutGeometry : PolylineGeometry
{
    
}