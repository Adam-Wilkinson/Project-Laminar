using Laminar.Contracts.Scripting.Connection;
using Laminar.Contracts.Scripting.NodeWrapping;
using Laminar.Domain.Notification;
using Laminar.Domain.Notification.Collections;
using Laminar.PluginFramework.NodeSystem.Connectors;

namespace Laminar.Contracts.Scripting.Execution;

public interface INodeTreeView
{
    public event EventHandler? Changed;

    public IReadOnlyCollection<ConnectorConnectionInfo> GetConnectionsTo(IConnector connector);

    public IWrappedNode GetParentNode(IConnector connector);

    public INodeUpdates GetUpdates(IWrappedNode node);
    
    public IReadOnlyObservableCollection<IWrappedNode> Nodes { get; }

    public IReadOnlyObservableCollection<IConnection> Connections { get; }
}

public record ConnectorConnectionInfo(IConnection Connection, IConnector OppositeConnector, IWrappedNode ConnectedNode);

public interface INodeUpdates
{
    public event EventHandler? ConnectionsChanged;
}