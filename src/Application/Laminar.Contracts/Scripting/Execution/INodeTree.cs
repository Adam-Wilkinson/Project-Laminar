using Laminar.Contracts.Scripting.Connection;
using Laminar.Contracts.Scripting.NodeWrapping;
using Laminar.PluginFramework.NodeSystem.Connectors;

namespace Laminar.Contracts.Scripting.Execution;

public interface INodeTree
{
    public event EventHandler? Changed;

    public IReadOnlyCollection<ConnectorConnectionInfo> GetConnections(IIOConnector connector);
}

public record ConnectorConnectionInfo(IConnection Connection, IIOConnector OppositeConnector, IWrappedNode ConnectedNode);