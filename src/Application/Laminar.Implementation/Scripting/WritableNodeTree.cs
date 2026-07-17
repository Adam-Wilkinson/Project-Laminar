using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Laminar.Contracts.Scripting;
using Laminar.Contracts.Scripting.Connection;
using Laminar.Contracts.Scripting.NodeWrapping;
using Laminar.Contracts.Storage.PersistentData;
using Laminar.Domain.Notification;
using Laminar.Domain.Notification.Collections;
using Laminar.Implementation.Scripting.Connections;
using Laminar.PluginFramework.NodeSystem;
using Laminar.PluginFramework.NodeSystem.Connectors;
using Microsoft.Extensions.Logging;

namespace Laminar.Implementation.Scripting;

internal class WritableNodeTree : IWritableNodeTree
{
    private readonly ILogger<WritableNodeTree> _logger;
        
    private readonly Dictionary<IConnector, ConnectorInformation> _connectorsInformation = [];
    private readonly Dictionary<IWrappedNode, NodeInformation> _nodesInformation = [];
    private readonly Dictionary<string, IWrappedNode> _nodesDictionary = [];
    private readonly ObservableCollection<Connection> _connections = [];
    private readonly ObservableCollection<IWrappedNode> _nodes = [];
    private readonly IPersistentDictionary _persistentNodes;
    private readonly IPersistentList _persistentConnections;
    
    public WritableNodeTree(
        IPersistentDictionary persistentDictionary, 
        INodeFactory nodeFactory,
        ILogger<WritableNodeTree> logger,
        INotificationClient<LaminarExecutionContext>? userChangedValueClient = null,
        IEnumerable<IWrappedNode>? nodes = null, 
        IEnumerable<IConnection>? connections = null)
    {
        PersistentData = persistentDictionary;
        _logger = logger;

        _persistentNodes = persistentDictionary["Nodes"].GetOrCreateCollection<IPersistentDictionary>();
        foreach (var (key, dataPoint) in _persistentNodes)
        {
            AddNode(nodeFactory.FromPersistentData(dataPoint.GetOrCreateCollection<IPersistentDictionary>(), userChangedValueClient), key);
        }

        if (nodes is not null)
        {
            foreach (var node in nodes)
            {
                AddNode(node);
            }   
        }

        _persistentConnections = persistentDictionary["Connections"].GetOrCreateCollection<IPersistentList>();

        foreach (var dataPoint in _persistentConnections)
        {
            var connection = dataPoint.GetValue<Connection>(deserializationContext: this).Value;
            if (!TryConnectWithoutSerializing(connection.OutputConnector, connection.InputConnector, out _))
            {
                throw new InvalidOperationException($"Could not connect {connection.OutputConnector} to {connection.InputConnector}");
            }
        }
        
        if (connections is not null)
        {
            foreach (var connection in connections)
            {
                TryConnect(connection.OutputConnector, connection.InputConnector, out _);
            }
        }
    }
    
    public event EventHandler? Changed;
    
    public IReadOnlyCollection<ConnectorConnectionInfo> GetConnectionsTo(IConnector connector) => GetConnectorInformation(connector).Connections.Values;

    public IWrappedNode GetParentNode(IConnector connector) => GetConnectorInformation(connector).Owner;
    
    public IWrappedNode GetNodeByKey(string key) => _nodesDictionary[key];
    
    public string GetNodeKey(IWrappedNode node) => _nodesInformation[node].DictionaryKey;

    public INodeUpdates GetUpdates(IWrappedNode node) => _nodesInformation[node].Updates;

    public IReadOnlyObservableCollection<IWrappedNode> Nodes => new Domain.Notification.Collections.ReadOnlyObservableCollection<IWrappedNode>(_nodes);

    public IReadOnlyObservableCollection<IConnection> Connections => _connections.ObservableMap(IConnection (Connection x) => x);
    
    public IEncodableData PersistentData { get; }

    public void AddNode(IWrappedNode node) => AddNode(node, null);

    private void AddNode(IWrappedNode node, string? dictionaryKey)
    {
        if (_nodes.Contains(node)) return;
        
        var rowsChangedSubscription = node.Rows.SubscribeForEach(
            addedRow =>
            {
                if (addedRow.InputConnector is not null) 
                    _connectorsInformation.Add(addedRow.InputConnector, new ConnectorInformation(node, []));

                if (addedRow.OutputConnector is not null) 
                    _connectorsInformation.Add(addedRow.OutputConnector, new ConnectorInformation(node, []));
            }, 
            removedRow =>
            {
                if (removedRow.InputConnector is not null) 
                    _connectorsInformation.Remove(removedRow.InputConnector);

                if (removedRow.OutputConnector is not null) 
                    _connectorsInformation.Remove(removedRow.OutputConnector);
            });
        
        dictionaryKey ??= Guid.NewGuid().ToString();
        
        _nodes.Add(node);
        _nodesDictionary.Add(dictionaryKey, node);
        _nodesInformation[node] = new NodeInformation(rowsChangedSubscription, new NodeUpdates(), dictionaryKey);
        _persistentNodes[dictionaryKey].GetOrCreateCollection(node.PersistentData);
        
        Changed?.Invoke(this, EventArgs.Empty);
    }

    public bool DeleteNode(IWrappedNode node)
    {
        if (_nodesInformation.Remove(node, out var info))
        {
            info.RowsChangedSubscription.Dispose();
            _nodesDictionary.Remove(info.DictionaryKey);
            _persistentNodes.Remove(info.DictionaryKey);
        }
        
        if (!_nodes.Contains(node)) return false;
        _nodes.Remove(node);
        Changed?.Invoke(this, EventArgs.Empty);
        return true;
    }

    public bool TryConnect(IOutputConnector outputConnector, IInputConnector inputConnector, [NotNullWhen(true)] out IConnection? connection)
    {
        if (!TryConnectWithoutSerializing(outputConnector, inputConnector, out Connection? connectionInternal))
        {
            connection = null;
            return false;
        }

        _persistentConnections.AddNext().GetValueOrInitialize(connectionInternal, deserializationContext: this);
        connection = connectionInternal;
        return true;
    }

    private bool TryConnectWithoutSerializing(IOutputConnector outputConnector, IInputConnector inputConnector,
        [NotNullWhen(true)] out Connection? connection)
    {
        if (ConnectionExists(outputConnector, inputConnector, out var existingConnection) && existingConnection is Connection typedConnection)
        {
            connection = typedConnection;
            return true;
        }
        
        if (!outputConnector.TryConnectTo(inputConnector) && !inputConnector.TryConnectTo(outputConnector))
        {
            connection = null;
            return false;
        }
        
        Connection newConnection = new(this)
        {
            OutputConnector = outputConnector,
            InputConnector = inputConnector,
        };
        
        var inputInfo = GetConnectorInformation(inputConnector);
        var outputInfo = GetConnectorInformation(outputConnector);
        
        inputInfo.Connections.Add(outputConnector, new ConnectorConnectionInfo(newConnection, outputConnector, outputInfo.Owner));
        outputInfo.Connections.Add(inputConnector, new ConnectorConnectionInfo(newConnection, inputConnector, inputInfo.Owner));

        _connections.Add(newConnection);
        outputConnector.OnConnectionEstablished();
        inputConnector.OnConnectionEstablished();
        
        _nodesInformation[inputInfo.Owner].Updates.OnConnectionsChanged();
        _nodesInformation[outputInfo.Owner].Updates.OnConnectionsChanged();
        
        Changed?.Invoke(this, EventArgs.Empty);

        connection = newConnection;
        return true;
    }

    public bool ConnectionExists(IOutputConnector outputConnector, IInputConnector inputConnector,
        [NotNullWhen(true)] out IConnection? existingConnection)
    {
        if (GetConnectorInformation(outputConnector).Connections.TryGetValue(inputConnector, out var info))
        {
            existingConnection = info.Connection;
            return true;
        }

        if (GetConnectorInformation(inputConnector).Connections.TryGetValue(outputConnector, out info))
        {
            _logger.LogWarning("Node tree found connection in one direction but not the other, this script is in an error state.");
            existingConnection = info.Connection;
            return true;
        }
        
        existingConnection = null;
        return false;
    }

    public bool SeverConnection(IOutputConnector outputConnector, IInputConnector inputConnector)
    {
        if (!ConnectionExists(outputConnector, inputConnector, out _)) return false;

        var inputInfo = GetConnectorInformation(inputConnector);
        var outputInfo = GetConnectorInformation(outputConnector);
        
        Debug.Assert(Equals(inputInfo.Connections[outputConnector].Connection, outputInfo.Connections[inputConnector].Connection));
        IConnection brokenConnector = inputInfo.Connections[outputConnector].Connection;

        inputInfo.Connections.Remove(outputConnector);
        outputInfo.Connections.Remove(inputConnector);

        inputConnector.OnConnectionSevered();
        outputConnector.OnConnectionSevered();
        
        _nodesInformation[inputInfo.Owner].Updates.OnConnectionsChanged();
        _nodesInformation[outputInfo.Owner].Updates.OnConnectionsChanged();
        
        _connections.Remove((Connection)brokenConnector);
        Changed?.Invoke(this, EventArgs.Empty);
        return true;
    }

    public void Dispose()
    {
        while (_nodesInformation.Count > 0)
        {
            var firstNode = _nodesInformation.First();
            firstNode.Value.RowsChangedSubscription.Dispose();
            _nodesInformation.Remove(firstNode.Key);
            _nodesDictionary.Remove(firstNode.Value.DictionaryKey);
        }
    }

    private ConnectorInformation GetConnectorInformation(IConnector connector) => _connectorsInformation[connector];
    
    private record ConnectorInformation(IWrappedNode Owner, Dictionary<IConnector, ConnectorConnectionInfo> Connections);

    private record NodeInformation(IDisposable RowsChangedSubscription, NodeUpdates Updates, string DictionaryKey);
    
    private class NodeUpdates : INodeUpdates
    {
        public void OnConnectionsChanged() => ConnectionsChanged?.Invoke(this, EventArgs.Empty);
        
        public event EventHandler? ConnectionsChanged;
    }
}
