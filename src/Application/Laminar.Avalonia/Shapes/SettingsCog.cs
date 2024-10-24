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

    protected override Geometry? CreateDefiningGeometry()
    {
        PathGeometry pathGeometry = new();

        using (PathGeometryContext context = new(pathGeometry))
        {
            double spokeAngleIncrement = 2 * Math.PI / SpokeCount;
            double spokeSubtendsAngle = spokeAngleIncrement * 0.4;
            double spokeGapAngle = spokeAngleIncrement - spokeSubtendsAngle;

            double currentAngle = -spokeSubtendsAngle / 2;

            Size innerSize = Bounds.Size.Deflate(new Thickness(SpokeDepth / 2));

            context.BeginFigure(FromAngle(currentAngle, 0), Fill is not null);

            for (int i = 0; i < SpokeCount; i++)
            {
                currentAngle += spokeSubtendsAngle;
                context.ArcTo(FromAngle(currentAngle, 0), Bounds.Size, spokeSubtendsAngle, false, SweepDirection.Clockwise);
                context.LineTo(FromAngle(currentAngle + spokeGapAngle / 3, SpokeDepth));
                currentAngle += spokeGapAngle;
                context.ArcTo(FromAngle(currentAngle - spokeGapAngle / 3, SpokeDepth), innerSize, spokeGapAngle, false, SweepDirection.Clockwise);
                context.LineTo(FromAngle(currentAngle, 0));
            }

            context.EndFigure(true);
        }

        return new CombinedGeometry(GeometryCombineMode.Xor, pathGeometry, new EllipseGeometry(new Rect(Bounds.Size).Deflate(new Thickness(SpokeDepth * 1.5))));
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        return new Size(StrokeThickness, StrokeThickness);
    }

    private Point FromAngle(double angle, double shrinkage)
    {
        double xRadius = (Bounds.Width - shrinkage)/ 2;
        double yRadius = (Bounds.Height - shrinkage) / 2;
        return new Point(
            xRadius * Math.Cos(angle) + Bounds.Width / 2,
            yRadius * Math.Sin(angle) + Bounds.Height / 2);
    }
}
