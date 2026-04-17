namespace Laminar.Domain.Exceptions;

public class ValueNotInitializedException(string valueName) : Exception($"The value {valueName} is not initialized");