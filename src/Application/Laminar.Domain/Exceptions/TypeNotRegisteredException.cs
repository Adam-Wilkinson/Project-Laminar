namespace Laminar.Domain.Exceptions;

public class TypeNotRegisteredException(Type type) : Exception($"The type {type} is not registered with Project: Laminar");