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

    public bool IsCloseTo(Point point, double tolerance)
    {
        return Math.Abs(X - point.X) <= tolerance && Math.Abs(Y - point.Y) <= tolerance;
    }
    
    public double SquaredDistance() => X * X + Y * Y;

    public override string ToString() => $"({X}, {Y})";

    public static Point Parse(string input)
    {
        int comma = input.IndexOf(',');
        return new Point
        {
            X = double.Parse(input.AsSpan(1, comma - 1)),
            Y = double.Parse(input.AsSpan(comma + 2,  input.Length - comma - 3)),
        };
    }
}