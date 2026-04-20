namespace Laminar.Domain.Exceptions;

public class DeserializationError(Exception inner) : Exception(null, inner);