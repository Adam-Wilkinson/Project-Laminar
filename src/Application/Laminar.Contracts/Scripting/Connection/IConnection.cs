using Laminar.PluginFramework.NodeSystem.Connectors;

namespace Laminar.Contracts.Scripting.Connection;

public interface IConnection
{
    public IInputConnector InputConnector { get; }

    public IOutputConnector OutputConnector { get; }
}
