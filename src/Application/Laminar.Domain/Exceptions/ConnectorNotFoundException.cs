namespace Laminar.Domain.Exceptions;

public class ConnectorNotFoundException(Type connectorType) : Exception($"Could not find a registered connector implementation for NodeIO {connectorType}");
