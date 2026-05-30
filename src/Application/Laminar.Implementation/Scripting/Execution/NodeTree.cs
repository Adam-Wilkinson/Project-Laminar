using System.Runtime.CompilerServices;
using Laminar.Contracts.Scripting;
using Laminar.Contracts.Scripting.Connection;
using Laminar.Contracts.Scripting.Execution;
using Laminar.Contracts.Scripting.NodeWrapping;
using Laminar.Domain.Notification;
using Laminar.PluginFramework.NodeSystem.Components;
using Laminar.PluginFramework.NodeSystem.Connectors;

namespace Laminar.Implementation.Scripting.Execution;

internal class NodeTree : INodeTree
{
    private readonly ConditionalWeakTable<IConnector, ConnectorInformation> _treeInformation = [];
    private readonly Dictionary<IWrappedNode, EventHandler<ItemAddedEventArgs<INodeRow>>> _nodeRowAddedDelegates = new();
    private readonly Dictionary<IWrappedNode, EventHandler<ItemRemovedEventArgs<INodeRow>>> _nodeRowRemovedDelegates = new();
    
    public event EventHandler? Changed;

    public NodeTree(IScript script)
    { 
        script.Nodes.HelperInstance().ItemAdded += NodeAdded;
        script.Nodes.HelperInstance().ItemRemoved += NodeRemoved;

        script.Connections.HelperInstance().ItemAdded += ConnectionAdded;
        script.Connections.HelperInstance().ItemRemoved += ConnectionRemoved;
    }

    public IWrappedNode GetConnectorOwner(IConnector connector) => GetConnectorInformation(connector).Owner;
    
    public IReadOnlyCollection<ConnectorConnectionInfo> GetConnections(IConnector connector) => GetConnectorInformation(connector).Connections.Values; 

    private void NodeRemoved(object? sender, ItemRemovedEventArgs<IWrappedNode> e)
    {
        e.Item.Rows.HelperInstance().ItemAdded -= _nodeRowAddedDelegates[e.Item];
        _nodeRowAddedDelegates.Remove(e.Item);
        e.Item.Rows.HelperInstance().ItemRemoved -= _nodeRowRemovedDelegates[e.Item];
        _nodeRowRemovedDelegates.Remove(e.Item);

        foreach (INodeRow row in e.Item.Rows)
        {
            RowRemoved(e.Item, row);
        }
    }

    private void NodeAdded(object? sender, ItemAddedEventArgs<IWrappedNode> e)
    {
        _nodeRowAddedDelegates[e.Item] = (_, newRow) => RowAdded(e.Item, newRow.Item);
        e.Item.Rows.HelperInstance().ItemAdded += _nodeRowAddedDelegates[e.Item];
        _nodeRowRemovedDelegates[e.Item] = (_, removedRow) => RowRemoved(e.Item, removedRow.Item);
        e.Item.Rows.HelperInstance().ItemRemoved += _nodeRowRemovedDelegates[e.Item];

        foreach (INodeRow row in e.Item.Rows)
        {
            RowAdded(e.Item, row);
        }
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

    private void ConnectionRemoved(object? sender, ItemRemovedEventArgs<IConnection> e)
    {
        var inputInfo = GetConnectorInformation(e.Item.InputConnector);
        var outputInfo = GetConnectorInformation(e.Item.OutputConnector);

        inputInfo.Connections.Remove(e.Item);
        outputInfo.Connections.Remove(e.Item);

        Changed?.Invoke(this, EventArgs.Empty);
    }

    private void ConnectionAdded(object? sender, ItemAddedEventArgs<IConnection> e)
    {
        var inputInfo = GetConnectorInformation(e.Item.InputConnector);
        var outputInfo = GetConnectorInformation(e.Item.OutputConnector);

        if (inputInfo.Connections.ContainsKey(e.Item))
        {
            throw new InvalidOperationException();
        }
        
        inputInfo.Connections.Add(e.Item, new ConnectorConnectionInfo(e.Item, e.Item.OutputConnector, outputInfo.Owner));

        if (outputInfo.Connections.ContainsKey(e.Item))
        {
            throw new InvalidOperationException();
        }
        
        outputInfo.Connections.Add(e.Item, new ConnectorConnectionInfo(e.Item, e.Item.InputConnector, inputInfo.Owner));

        Changed?.Invoke(this, EventArgs.Empty);
    }

    private ConnectorInformation GetConnectorInformation(IConnector connector) => _treeInformation.GetValue(connector,
        _ => throw new InvalidOperationException("Connector not found"));
    
    private record ConnectorInformation(
        IWrappedNode Owner,
        Dictionary<IConnection, ConnectorConnectionInfo> Connections);
}
