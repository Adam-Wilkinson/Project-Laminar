namespace Laminar.Domain.Exceptions;

public class ExistingDataOfIncorrectTypeException(string childName) : ArgumentException("Tried to get child data store, but value already exists and is of wrong type", childName);