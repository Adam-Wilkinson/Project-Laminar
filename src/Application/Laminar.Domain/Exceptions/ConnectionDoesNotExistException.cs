namespace Laminar.Domain.Exceptions;

public class ConnectionDoesNotExistException(object outputConnector, object inputConnector) : Exception($"The connection between {outputConnector} and {inputConnector} does not exist");