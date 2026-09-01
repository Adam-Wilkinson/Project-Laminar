using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Laminar.Contracts.Base;
using Laminar.Contracts.Scripting;
using Laminar.Contracts.Scripting.Connection;
using Laminar.Contracts.Scripting.NodeWrapping;
using Laminar.Contracts.Storage.PersistentData;
using Laminar.Domain.Exceptions;
using Laminar.Domain.Observables.Collections;
using Laminar.Implementation.Scripting.Connections;
using Laminar.PluginFramework.NodeSystem.Connectors;
using Microsoft.Extensions.Logging;

namespace Laminar.Implementation.Scripting;

internal class WritableNodeTree : IWritableNodeTree
{
    private readonly ILogger<WritableNodeTree> _logger;
    
    private readonly Dictionary<IConnector, ConnectorInformation> _connectorsInformation = [];
    private readonly Dictionary<INodeContainer, NodeInformation> _nodesInformation = [];
    private readonly Dictionary<string, INodeContainer> _nodesDictionary = [];
    private readonly ObservableCollection<Connection> _connections = [];
    private readonly ObservableCollection<INodeContainer> _nodes = [];
    private readonly IPersistentDictionary _persistentNodes;
    private readonly IPersistentList _persistentConnections;
    
    public WritableNodeTree(
        IPersistentDictionary persistentDictionary, 
        ILoadedNodeManager nodeManager,
        ILogger<WritableNodeTree> logger,
        IExceptionHandler exceptionHandler,
        IEnumerable<INodeContainer>? nodes = null, 
        IEnumerable<IConnection>? connections = null)
    {
        PersistentData = persistentDictionary;
        _logger = logger;

        _persistentNodes = persistentDictionary["Nodes"].GetOrCreateCollection<IPersistentDictionary>();
        foreach (var (key, dataPoint) in _persistentNodes)
        {
            AddNode(nodeManager.CreateNode(dataPoint.GetOrCreateCollection<IPersistentDictionary>()), key);
        }

        if (nodes is not null)
        {
            foreach (var node in nodes)
            {
                AddNode(node);
            }   
        }

        _persistentConnections = persistentDictionary["Connections"].GetOrCreateCollection<IPersistentList>();

        int i = 0;
        while (i < _persistentConnections.Count)
        {
            try
            {
                var connection = _persistentConnections[i].GetValue<Connection>(deserializationContext: this).Value;
                if (!TryConnectWithoutSerializing(connection.OutputConnector, connection.InputConnector, out _))
                {
                    throw new CouldNotConnectException(connection.OutputConnector, connection.InputConnector);
                }

                i++;
            }
            catch (DeserializationError ex) 
            {
                exceptionHandler.OnException(ex);
                _persistentConnections.RemoveAt(i);
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

    public INodeContainer GetParentNode(IConnector connector) => GetConnectorInformation(connector).Owner;
    
    public bool TryGetNodeByKey(string key, [NotNullWhen(true)] out INodeContainer? node) => _nodesDictionary.TryGetValue(key, out node);
    
    public string GetNodeKey(INodeContainer nodeContainer) => _nodesInformation[nodeContainer].DictionaryKey;

    public INodeUpdates GetUpdates(INodeContainer nodeContainer) => _nodesInformation[nodeContainer].Updates;

    public IReadOnlyObservableCollection<INodeContainer> Nodes => new Domain.Observables.Collections.ReadOnlyObservableCollection<INodeContainer>(_nodes);

    public IReadOnlyObservableCollection<IConnection> Connections => _connections.ObservableMap(IConnection (Connection x) => x);
    
    public IEncodableData PersistentData { get; }

    public void AddNode(INodeContainer nodeContainer) => AddNode(nodeContainer, null);

    private void AddNode(INodeContainer nodeContainer, string? dictionaryKey)
    {
        if (_nodes.Contains(nodeContainer)) return;
        
        var rowsChangedSubscription = nodeContainer.Rows.SubscribeForEach(
            addedRow =>
            {
                if (addedRow.InputConnector is not null) 
                    _connectorsInformation.Add(addedRow.InputConnector, new ConnectorInformation(nodeContainer, []));

                if (addedRow.OutputConnector is not null) 
                    _connectorsInformation.Add(addedRow.OutputConnector, new ConnectorInformation(nodeContainer, []));
            }, 
            removedRow =>
            {
                if (removedRow.InputConnector is not null) 
                    _connectorsInformation.Remove(removedRow.InputConnector);

                if (removedRow.OutputConnector is not null) 
                    _connectorsInformation.Remove(removedRow.OutputConnector);
            });
        
        dictionaryKey ??= Guid.NewGuid().ToString();
        
        _nodes.Add(nodeContainer);
        _nodesDictionary.Add(dictionaryKey, nodeContainer);
        _nodesInformation[nodeContainer] = new NodeInformation(rowsChangedSubscription, new NodeUpdates(), dictionaryKey);
        _persistentNodes[dictionaryKey].GetOrCreateCollection(nodeContainer.PersistentData);
        
        Changed?.Invoke(this, EventArgs.Empty);
    }

    public bool DeleteNode(INodeContainer nodeContainer)
    {
        if (_nodesInformation.Remove(nodeContainer, out var info))
        {
            info.RowsChangedSubscription.Dispose();
            _nodesDictionary.Remove(info.DictionaryKey);
            _persistentNodes.Remove(info.DictionaryKey);
        }
        
        if (!_nodes.Contains(nodeContainer)) return false;
        _nodes.Remove(nodeContainer);
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

        _persistentConnections.AddNext().GetValueOrInitialize<Connection>(connectionInternal, deserializationContext: this);
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
        
        if (!outputConnector.CanConnectTo(inputConnector) && !inputConnector.CanConnectTo(outputConnector))
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
        
        _nodesInformation[inputInfo.Owner].Updates.OnConnectionsChanged();
        _nodesInformation[outputInfo.Owner].Updates.OnConnectionsChanged();
        
        Changed?.Invoke(this, EventArgs.Empty);

        connection = newConnection;
        return true;
    }

    public bool ConnectionExists(IConnector firstConnector, IConnector secondConnector,
        [NotNullWhen(true)] out IConnection? existingConnection)
    {
        if (GetConnectorInformation(firstConnector).Connections.TryGetValue(secondConnector, out var info))
        {
            existingConnection = info.Connection;
            return true;
        }

        if (GetConnectorInformation(secondConnector).Connections.TryGetValue(firstConnector, out info))
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
        
        _nodesInformation[inputInfo.Owner].Updates.OnConnectionsChanged();
        _nodesInformation[outputInfo.Owner].Updates.OnConnectionsChanged();

        var persistentValueIndex = _persistentConnections.FirstIndexWhere(x =>
            x.MaterializedValue is IPersistentValue<Connection> { Value: var foundConnection } &&
            Equals(foundConnection.InputConnector, inputConnector) &&
            Equals(foundConnection.OutputConnector, outputConnector));
        
        _persistentConnections.RemoveAt(persistentValueIndex);
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
    
    private record ConnectorInformation(INodeContainer Owner, Dictionary<IConnector, ConnectorConnectionInfo> Connections);

    private record NodeInformation(IDisposable RowsChangedSubscription, NodeUpdates Updates, string DictionaryKey);
    
    private class NodeUpdates : INodeUpdates
    {
        public void OnConnectionsChanged() => ConnectionsChanged?.Invoke(this, EventArgs.Empty);
        
        public event EventHandler? ConnectionsChanged;
    }
}
