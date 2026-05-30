using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Laminar.Contracts.Scripting.Connection;
using Laminar.Contracts.Scripting.Execution;
using Laminar.Contracts.Scripting.NodeWrapping;
using Laminar.Domain.Notification;
using Laminar.Implementation.Scripting.Connections;
using Laminar.PluginFramework.NodeSystem.Components;
using Laminar.PluginFramework.NodeSystem.Connectors;

namespace Laminar.Implementation.Scripting.Execution;

internal class NodeTree : INodeTree
{
    private readonly ConditionalWeakTable<IConnector, ConnectorInformation> _treeInformation = [];
    private readonly Dictionary<IWrappedNode, EventHandler<ItemAddedEventArgs<INodeRow>>> _nodeRowAddedDelegates = new();
    private readonly Dictionary<IWrappedNode, EventHandler<ItemRemovedEventArgs<INodeRow>>> _nodeRowRemovedDelegates = new();
    private readonly ObservableCollection<IWrappedNode> _nodes = [];
    private readonly ObservableCollection<IConnection> _connections = [];
    
    public event EventHandler? Changed;
    
    public IReadOnlyCollection<ConnectorConnectionInfo> GetConnectionsTo(IConnector connector) => GetConnectorInformation(connector).Connections.Values;

    public IReadOnlyObservableCollection<IWrappedNode> Nodes => new Domain.Notification.ReadOnlyObservableCollection<IWrappedNode>(_nodes);

    public IReadOnlyObservableCollection<IConnection> Connections => new Domain.Notification.ReadOnlyObservableCollection<IConnection>(_connections);
    
    public void AddNode(IWrappedNode node)
    {
        _nodeRowAddedDelegates[node] = (_, newRow) => RowAdded(node, newRow.Item);
        node.Rows.HelperInstance().ItemAdded += _nodeRowAddedDelegates[node];
        _nodeRowRemovedDelegates[node] = (_, removedRow) => RowRemoved(node, removedRow.Item);
        node.Rows.HelperInstance().ItemRemoved += _nodeRowRemovedDelegates[node];

        foreach (INodeRow row in node.Rows)
        {
            RowAdded(node, row);
        }

        _nodes.Add(node);
        Changed?.Invoke(this, EventArgs.Empty);
    }

    public bool DeleteNode(IWrappedNode node)
    {
        if (!_nodes.Contains(node)) return false;
        
        node.Rows.HelperInstance().ItemAdded -= _nodeRowAddedDelegates[node];
        _nodeRowAddedDelegates.Remove(node);
        node.Rows.HelperInstance().ItemRemoved -= _nodeRowRemovedDelegates[node];
        _nodeRowRemovedDelegates.Remove(node);

        foreach (INodeRow row in node.Rows)
        {
            RowRemoved(node, row);
        }
        
        _nodes.Remove(node);
        Changed?.Invoke(this, EventArgs.Empty);
        return true;
    }

    public bool TryConnect(IOutputConnector outputConnector, IInputConnector inputConnector, [NotNullWhen(true)] out IConnection? connection)
    {
        if (!outputConnector.TryConnectTo(inputConnector) && !inputConnector.TryConnectTo(outputConnector))
        {
            connection = null;
            return false;
        }
        
        Connection newConnection = new()
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
        Changed?.Invoke(this, EventArgs.Empty);

        connection = newConnection;
        return true;
    }

    public bool ConnectionExists(IOutputConnector outputConnector, IInputConnector inputConnector)
        => GetConnectorInformation(outputConnector).Connections.ContainsKey(inputConnector) 
           || GetConnectorInformation(inputConnector).Connections.ContainsKey(outputConnector);

    public bool SeverConnection(IOutputConnector outputConnector, IInputConnector inputConnector)
    {
        if (!ConnectionExists(outputConnector, inputConnector)) return false;

        var inputInfo = GetConnectorInformation(inputConnector);
        var outputInfo = GetConnectorInformation(outputConnector);
        
        Debug.Assert(Equals(inputInfo.Connections[outputConnector].Connection, outputInfo.Connections[inputConnector].Connection));
        IConnection brokenConnector = inputInfo.Connections[outputConnector].Connection;

        inputInfo.Connections.Remove(outputConnector);
        outputInfo.Connections.Remove(inputConnector);

        inputConnector.OnConnectionSevered();
        outputConnector.OnConnectionSevered();
        
        _connections.Remove(brokenConnector);
        Changed?.Invoke(this, EventArgs.Empty);
        return true;
    }

    private void RowAdded(IWrappedNode node, INodeRow row)
    {
        if (row.InputConnector is not null)
        {
            _treeInformation.Add(row.InputConnector, new ConnectorInformation(node, []));
        }

        if (row.OutputConnector is not null)
        {
            _treeInformation.Add(row.OutputConnector, new ConnectorInformation(node, []));
        }
    }

    private void RowRemoved(IWrappedNode node, INodeRow row)
    {
        if (row.InputConnector is not null)
        {
            _treeInformation.Remove(row.InputConnector);
        }

        if (row.OutputConnector is not null)
        {
            _treeInformation.Remove(row.OutputConnector);
        }
    }

    private ConnectorInformation GetConnectorInformation(IConnector connector) => _treeInformation.GetValue(connector,
        _ => throw new InvalidOperationException("Connector not found"));
    
    private record ConnectorInformation(
        IWrappedNode Owner,
        Dictionary<IConnector, ConnectorConnectionInfo> Connections);
}
