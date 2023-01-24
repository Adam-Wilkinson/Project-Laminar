using Avalonia;

namespace Laminar.Avalonia.Utils;

internal static class ValueObjectConverter
{
    public static Domain.ValueObjects.Point Point(Point avaloniaPoint) => new() { X = avaloniaPoint.X, Y = avaloniaPoint.Y };

    public static Point PointConverter(Domain.ValueObjects.Point domainPoint) => new(domainPoint.X, domainPoint.Y);
}
