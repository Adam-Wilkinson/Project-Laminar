namespace Laminar.Domain.ValueObjects;

public static class IncrementalIdentifier
{
    private static readonly Dictionary<Type, int> MaxValues = [];

    public static IncrementalIdentifier<T> Next<T>()
    {
        if (!MaxValues.TryGetValue(typeof(T), out var value))
        {
            int newValue = 0;
            MaxValues.Add(typeof(T), newValue);
            value = newValue;
        }

        MaxValues[typeof(T)] = value + 1;
        return new IncrementalIdentifier<T>(value);
    }
}

public class IncrementalIdentifier<T> : IEquatable<IncrementalIdentifier<T>>
{
    private readonly int _value;
    
    internal IncrementalIdentifier(int value)
    {
        _value = value;
    }

    public bool Equals(IncrementalIdentifier<T>? other) => _value == other?._value;

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((IncrementalIdentifier<T>)obj);
    }

    public override int GetHashCode() => _value.GetHashCode();
    
    public override string ToString() => _value.ToString();
}