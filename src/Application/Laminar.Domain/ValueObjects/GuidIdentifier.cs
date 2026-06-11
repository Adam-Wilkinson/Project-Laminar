namespace Laminar.Domain.ValueObjects;

/// <summary>
/// An identifier used throughout Project: Laminar
/// </summary>
/// <typeparam name="T">The parent class of the identifier, so it is strongly typed and can't be mixed up</typeparam>
public class GuidIdentifier<T>(Guid guid) : IEquatable<GuidIdentifier<T>>
{
    private readonly Guid _value = guid;

    public static GuidIdentifier<T> New()
    {
        return new(Guid.NewGuid());
    }

    public static GuidIdentifier<T> Empty()
    {
        return new(Guid.Empty);
    }

    public Guid AsGuid() => _value;

    public override bool Equals(object? obj)
    {
        return Equals(obj as GuidIdentifier<T>);
    }

    public bool Equals(GuidIdentifier<T>? other)
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

    public static bool operator ==(GuidIdentifier<T>? lhs, GuidIdentifier<T>? rhs)
    {
        return lhs?.Equals(rhs) ?? rhs is null;
    }

    public static bool operator !=(GuidIdentifier<T> lhs, GuidIdentifier<T> rhs) => !(lhs == rhs);
}
