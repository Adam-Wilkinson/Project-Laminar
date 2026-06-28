using Laminar.Contracts.Scripting.Connection;
using Laminar.Contracts.Scripting.NodeWrapping;
using Laminar.Contracts.Storage.PersistentData;
using Laminar.Domain.Notification.Collections;
using Laminar.Domain.ValueObjects;
using Laminar.PluginFramework.NodeSystem.Connectors;

namespace Laminar.Contracts.Scripting;

public interface INodeTree : IDisposable
{
    public event EventHandler? Changed;

    public IReadOnlyCollection<ConnectorConnectionInfo> GetConnectionsTo(IConnector connector);

    public IWrappedNode GetParentNode(IConnector connector);

    public IWrappedNode GetNodeByKey(string key);

    public string GetNodeKey(IWrappedNode node);
    
    public INodeUpdates GetUpdates(IWrappedNode node);
    
    public IReadOnlyObservableCollection<IWrappedNode> Nodes { get; }

    public IReadOnlyObservableCollection<IConnection> Connections { get; }
    
    public IEncodableData PersistentData { get; }
}

public record ConnectorConnectionInfo(IConnection Connection, IConnector OppositeConnector, IWrappedNode ConnectedNode);

public interface INodeUpdates
{
    public event EventHandler? ConnectionsChanged;
}