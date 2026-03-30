using System;
using Avalonia;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using Avalonia.Visuals.Platform;

namespace Laminar.Avalonia.Shapes;
internal class SettingsCog : Shape
{
    public static readonly StyledProperty<double> SpokeDepthProperty = AvaloniaProperty.Register<SettingsCog, double>(nameof(SpokeDepth), 10);

    public static readonly StyledProperty<int> SpokeCountProperty = AvaloniaProperty.Register<SettingsCog, int>(nameof(SpokeCount), 6);

    static SettingsCog()
    {
        AffectsGeometry<SettingsCog>(SpokeDepthProperty, SpokeCountProperty, BoundsProperty);
    }

    public double SpokeDepth
    {
        get => GetValue(SpokeDepthProperty);
        set => SetValue(SpokeDepthProperty, value);
    }

    public int SpokeCount
    {
        get => GetValue(SpokeCountProperty);
        set => SetValue(SpokeCountProperty, value);
    }

    public static Geometry GetGeometry(double spokeDepth, double spokeCount, Size size)
    {
        PathGeometry pathGeometry = new();

        using PathGeometryContext context = new(pathGeometry);
        double spokeAngleIncrement = 2 * Math.PI / spokeCount;
        double spokeSubtendsAngle = spokeAngleIncrement * 0.4;
        double spokeGapAngle = spokeAngleIncrement - spokeSubtendsAngle;

        double currentAngle = -spokeSubtendsAngle / 2;

        Size innerSize = size.Deflate(new Thickness(spokeDepth / 2));

        context.BeginFigure(FromAngle(currentAngle, 0, size), true);

        for (int i = 0; i < spokeCount; i++)
        {
            currentAngle += spokeSubtendsAngle;
            context.ArcTo(FromAngle(currentAngle, 0, size), size, spokeSubtendsAngle, false,
                SweepDirection.Clockwise);
            context.LineTo(FromAngle(currentAngle + spokeGapAngle / 3, spokeDepth, size));
            currentAngle += spokeGapAngle;
            context.ArcTo(FromAngle(currentAngle - spokeGapAngle / 3, spokeDepth, size), innerSize, spokeGapAngle, false,
                SweepDirection.Clockwise);
            context.LineTo(FromAngle(currentAngle, 0, size));
        }

        context.EndFigure(true);

        return new CombinedGeometry(GeometryCombineMode.Xor, pathGeometry, new EllipseGeometry(new Rect(size).Deflate(new Thickness(spokeDepth * 1.5))));
    }
    
    protected override Geometry? CreateDefiningGeometry()
        => GetGeometry(SpokeDepth, SpokeCount, Bounds.Size);

    protected override Size MeasureOverride(Size availableSize)
    {
        return new Size(StrokeThickness, StrokeThickness);
    }

    private static Point FromAngle(double angle, double shrinkage, Size boundingBox)
    {
        double xRadius = (boundingBox.Width - shrinkage)/ 2;
        double yRadius = (boundingBox.Height - shrinkage) / 2;
        return new Point(
            xRadius * Math.Cos(angle) + boundingBox.Width / 2,
            yRadius * Math.Sin(angle) + boundingBox.Height / 2);
    }
}
