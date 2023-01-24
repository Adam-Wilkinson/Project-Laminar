namespace Laminar.Domain.Exceptions;

public class ConnectorNotFoundException : Exception
{
    public ConnectorNotFoundException(Type connectorType) : base($"Could not find a registered connector implementation for NodeIO {connectorType}")
    {
    }
}
