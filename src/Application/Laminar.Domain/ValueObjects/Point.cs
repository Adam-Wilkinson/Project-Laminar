namespace Laminar.Domain.ValueObjects;

public readonly struct Point
{
    public double X { get; init; }
    public double Y { get; init; }

    public static Point operator +(Point pointOne, Point point2)
    {
        return new Point { X = pointOne.X + point2.X, Y = pointOne.Y + point2.Y };
    }

    public static Point operator -(Point pointOne, Point point2)
    {
        return new Point { X = pointOne.X - point2.X, Y = pointOne.Y - point2.Y };
    }

    public static Point operator -(Point point)
    {
        return new Point { X = -point.X, Y = -point.Y };
    }
}