using System.Diagnostics.CodeAnalysis;
using Laminar.Contracts.Scripting.Connection;
using Laminar.Contracts.Scripting.NodeWrapping;
using Laminar.Contracts.Storage.PersistentData;
using Laminar.Domain.Observables.Collections;
using Laminar.PluginFramework.NodeSystem.Connectors;

namespace Laminar.Contracts.Scripting;

public interface INodeTree : IDisposable
{
    public event EventHandler? Changed;

    public IReadOnlyCollection<ConnectorConnectionInfo> GetConnectionsTo(IConnector connector);

    public INodeContainer GetParentNode(IConnector connector);

    public bool TryGetNodeByKey(string key, [NotNullWhen(true)] out INodeContainer? node);

    public string GetNodeKey(INodeContainer nodeContainer);
    
    public INodeUpdates GetUpdates(INodeContainer nodeContainer);
    
    public IReadOnlyObservableCollection<INodeContainer> Nodes { get; }

    public IReadOnlyObservableCollection<IConnection> Connections { get; }
    
    public bool ConnectionExists(IConnector firstConnector, IConnector secondConnector, [NotNullWhen(true)] out IConnection? existingConnection);
    
    public IEncodableData PersistentData { get; }
}

public record ConnectorConnectionInfo(IConnection Connection, IConnector OppositeConnector, INodeContainer ConnectedNodeContainer);

public interface INodeUpdates
{
    public event EventHandler? ConnectionsChanged;
}