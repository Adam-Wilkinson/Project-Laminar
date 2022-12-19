namespace Laminar.Domain.ValueObjects;

/// <summary>
/// An identifier used throughout Project: Laminar
/// </summary>
/// <typeparam name="T">The parent class of the identifier, so it is strongly typed and can't be mixed up</typeparam>
public class Identifier<T> : IEquatable<Identifier<T>>
{
    private readonly Guid _value;

    private Identifier(Guid guid)
    {
        _value = guid;
    }

    public static Identifier<T> New()
    {
        return new(Guid.NewGuid());
    }

    public static Identifier<T> Empty()
    {
        return new(Guid.Empty);
    }

    public Guid AsGuid() => _value;

    public override bool Equals(object? obj)
    {
        return Equals(obj as Identifier<T>);
    }

    public bool Equals(Identifier<T>? other)
    {
        return ReferenceEquals(this, other) || (other is not null && other.GetType() == this.GetType() && other._value == this._value);
    }

    public override int GetHashCode()
    {
        return _value.GetHashCode();
    }

    public override string ToString()
    {
        return _value.ToString();
    }

    public static bool operator ==(Identifier<T> lhs, Identifier<T> rhs)
    {
        if (lhs is null)
        {
            return rhs is null;
        }

        return lhs.Equals(rhs);
    }

    public static bool operator !=(Identifier<T> lhs, Identifier<T> rhs) => !(lhs == rhs);
}
