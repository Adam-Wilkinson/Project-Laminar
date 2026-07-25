using Laminar.PluginFramework.NodeSystem.Connectors;

namespace Laminar.Domain.Exceptions;

public class CouldNotConnectException(IOutputConnector outputConnector, IInputConnector inputConnector)
    : Exception($"Could not connect {outputConnector} to {inputConnector}")
{
    public IOutputConnector OutputConnector { get; } = outputConnector;
    public IInputConnector InputConnector { get; } = inputConnector;
}