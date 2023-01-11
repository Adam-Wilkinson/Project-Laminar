using Avalonia;
using Avalonia.Media;
using Avalonia.Platform;
using Laminar.Contracts.Scripting;
using Laminar.Contracts.Scripting.Connection;

namespace Laminar.Avalonia.NodeDisplaySystem;

internal class ConnectionGeometry : Geometry
{
    public static readonly StyledProperty<Point> StartPointProperty = AvaloniaProperty.Register<ConnectionGeometry, Point>(nameof(StartPoint));
    public static readonly StyledProperty<Point> EndPointProperty = AvaloniaProperty.Register<ConnectionGeometry, Point>(nameof(EndPoint));
    public static readonly StyledProperty<IPen> PenProperty = AvaloniaProperty.Register<ConnectionGeometry, IPen>(nameof(Pen));

    static ConnectionGeometry()
    {
        AffectsGeometry(StartPointProperty, EndPointProperty, Selection.SelectedProperty);
    }

    public IConnection CoreConnection { get; set; }

    public IPen Pen
    {
        get => GetValue(PenProperty);
        set => SetValue(PenProperty, value);
    }

    public Point StartPoint
    {
        get => GetValue(StartPointProperty);
        set => SetValue(StartPointProperty, value);
    }

    public Point EndPoint
    {
        get => GetValue(EndPointProperty);
        set => SetValue(EndPointProperty, value);
    }

    public override Geometry Clone()
    {
        return new ConnectionGeometry { StartPoint = StartPoint, EndPoint = EndPoint };
    }

    protected override IGeometryImpl CreateDefiningGeometry()
    {
        IStreamGeometryImpl streamGeometryImpl = AvaloniaLocator.Current.GetService<IPlatformRenderInterface>()!.CreateStreamGeometry();
        using IStreamGeometryContextImpl context = streamGeometryImpl.Open();
        context.BeginFigure(StartPoint, false);
        Point midpoint = (EndPoint + StartPoint) / 2;
        context.ArcTo(midpoint, new Size(StartPoint.X - midpoint.X, StartPoint.Y - midpoint.Y), 0, false, EndPoint.Y > StartPoint.Y ? SweepDirection.Clockwise : SweepDirection.CounterClockwise);
        context.ArcTo(EndPoint, new Size(midpoint.X - EndPoint.X, midpoint.Y - EndPoint.Y), 0, false, EndPoint.Y > StartPoint.Y ? SweepDirection.CounterClockwise : SweepDirection.Clockwise);
        context.EndFigure(false);

        return streamGeometryImpl;
    }
}
