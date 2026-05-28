using System.Runtime.CompilerServices;
using Laminar.Contracts.Scripting;
using Laminar.Contracts.Scripting.Connection;
using Laminar.Contracts.Scripting.Execution;
using Laminar.Contracts.Scripting.NodeWrapping;
using Laminar.Domain.Notification;
using Laminar.PluginFramework.NodeSystem.Components;
using Laminar.PluginFramework.NodeSystem.Connectors;

namespace Laminar.Implementation.Scripting.Execution;

public class NodeTree : INodeTree
{
    private readonly ConditionalWeakTable<IIOConnector, ConnectorInformation> _treeInformation = [];
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

    public IWrappedNode GetConnectorOwner(IIOConnector connector) => GetConnectorInformation(connector).Owner;
    
    public IReadOnlyList<IIOConnector> GetConnections(IIOConnector connector) => GetConnectorInformation(connector).Connected;

    public IReadOnlyList<IWrappedNode> GetConnections(IOutputConnector outputConnector) => GetConnectorInformation(outputConnector).ConnectedNodes;

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
            _treeInformation.Add(row.InputConnector, new ConnectorInformation(node, [], []));
        }

        if (row.OutputConnector is not null)
        {
            _treeInformation.Add(row.OutputConnector, new ConnectorInformation(node, [], []));
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

        inputInfo.Connected.Remove(e.Item.OutputConnector);
        inputInfo.ConnectedNodes.Remove(outputInfo.Owner);

        outputInfo.Connected.Remove(e.Item.InputConnector);
        outputInfo.ConnectedNodes.Remove(inputInfo.Owner);

        Changed?.Invoke(this, EventArgs.Empty);
    }

    private void ConnectionAdded(object? sender, ItemAddedEventArgs<IConnection> e)
    {
        var inputInfo = GetConnectorInformation(e.Item.InputConnector);
        var outputInfo = GetConnectorInformation(e.Item.OutputConnector);

        if (inputInfo.Connected.Contains(e.Item.OutputConnector))
        {
            throw new InvalidOperationException();
        }
        
        inputInfo.Connected.Add(e.Item.OutputConnector);

        if (!inputInfo.ConnectedNodes.Contains(outputInfo.Owner))
        {
            inputInfo.ConnectedNodes.Add(outputInfo.Owner);
        }

        if (outputInfo.Connected.Contains(e.Item.InputConnector))
        {
            throw new InvalidOperationException();
        }
        
        outputInfo.Connected.Add(e.Item.InputConnector);

        if (!outputInfo.ConnectedNodes.Contains(inputInfo.Owner))
        {
            outputInfo.ConnectedNodes.Add(inputInfo.Owner);
        }

        Changed?.Invoke(this, EventArgs.Empty);
    }

    private ConnectorInformation GetConnectorInformation(IIOConnector connector) => _treeInformation.GetValue(connector,
        _ => throw new InvalidOperationException("Connector not found"));
    
    private record ConnectorInformation(
        IWrappedNode Owner,
        List<IIOConnector> Connected,
        List<IWrappedNode> ConnectedNodes);
}
