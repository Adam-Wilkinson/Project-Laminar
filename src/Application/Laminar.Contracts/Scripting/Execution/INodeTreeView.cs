using Laminar.Contracts.Scripting.Connection;
using Laminar.Contracts.Scripting.NodeWrapping;
using Laminar.Domain.Notification;
using Laminar.PluginFramework.NodeSystem.Connectors;

namespace Laminar.Contracts.Scripting.Execution;

public interface INodeTreeView
{
    public event EventHandler? Changed;

    public IReadOnlyCollection<ConnectorConnectionInfo> GetConnectionsTo(IConnector connector);
    
    public IReadOnlyObservableCollection<IWrappedNode> Nodes { get; }

    public IReadOnlyObservableCollection<IConnection> Connections { get; }
}

public record ConnectorConnectionInfo(IConnection Connection, IConnector OppositeConnector, IWrappedNode ConnectedNode);