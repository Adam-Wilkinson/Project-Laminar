namespace Laminar.Domain.Exceptions;

public class CouldNotConnectException(object outputConnector, object inputConnector) : Exception($"Could not connect {outputConnector} to {inputConnector}");