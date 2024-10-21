namespace Laminar.Domain.Exceptions;

public class TypeNotRegisteredException : Exception
{
    public TypeNotRegisteredException(Type type) : base($"The type {type} is not registered with Project: Laminar")
    {
    }
}