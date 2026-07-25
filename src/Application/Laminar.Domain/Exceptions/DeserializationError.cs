namespace Laminar.Domain.Exceptions;

public class DeserializationError<T>(Exception inner) : DeserializationError(inner, typeof(T));

public class DeserializationError(Exception inner, Type targetType) : Exception(null, inner)
{
    public Type TargetType { get; } = targetType;
}