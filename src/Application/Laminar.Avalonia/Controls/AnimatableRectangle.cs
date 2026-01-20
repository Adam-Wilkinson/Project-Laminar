using System;
using Avalonia;
using Avalonia.Controls.Shapes;
using Avalonia.Data;
using Avalonia.Media;

namespace Laminar.Avalonia.Controls;

public class AnimatableRectangle : Rectangle
{
    public static readonly StyledProperty<Size> SizeProperty = AvaloniaProperty.Register<AnimatableRectangle, Size>(nameof(Size));

    public static readonly StyledProperty<Vector> TopLeftProperty = AvaloniaProperty.Register<AnimatableRectangle, Vector>(nameof(TopLeft));
    
    public static readonly StyledProperty<Thickness> PaddingProperty = AvaloniaProperty.Register<AnimatableRectangle, Thickness>(nameof(Padding));
    
    static AnimatableRectangle()
    {
        BoundsProperty.Changed.AddClassHandler<AnimatableRectangle>((o, e) => o.BoundsChanged(e));
        AffectsGeometry<AnimatableRectangle>(SizeProperty, TopLeftProperty);
    }
    
    public Size Size
    {
        get => GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }

    public Vector TopLeft
    {
        get => GetValue(TopLeftProperty);
        set => SetValue(TopLeftProperty, value);
    }

    public Thickness Padding
    {
        get => GetValue(PaddingProperty);
        set => SetValue(PaddingProperty, value);
    }

    private void BoundsChanged(AvaloniaPropertyChangedEventArgs e)
    {
        var newBounds = e.GetNewValue<Rect>();
        SetValue(TopLeftProperty, newBounds.TopLeft);
        SetValue(SizeProperty, newBounds.Size);
    }
    
    protected override Geometry CreateDefiningGeometry()
    {
        var rect = new Rect(Size).Deflate(Padding).Deflate(StrokeThickness / 2);
        var radiusX = Math.Min(RadiusX, Size.Height / 2);
        var radiusY = Math.Min(RadiusY, Size.Width / 2);

        return new RectangleGeometry(rect, radiusX, radiusY);
    }
}