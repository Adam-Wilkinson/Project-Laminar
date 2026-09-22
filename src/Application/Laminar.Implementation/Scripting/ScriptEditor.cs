using Laminar.Contracts.Base.ActionSystem;
using Laminar.Contracts.Scripting;
using Laminar.Contracts.Scripting.Connection;
using Laminar.Contracts.Scripting.NodeWrapping;
using Laminar.Domain.ValueObjects;
using Laminar.Implementation.Base.ActionSystem;
using Laminar.Implementation.Scripting.Actions;
using Laminar.PluginFramework.NodeSystem.Connectors;

namespace Laminar.Implementation.Scripting;

internal class ScriptEditor(IEnumerable<IConnectionBridger> connectionBridgers) : IScriptEditor
{
    public IUserAction AddMatchingNodeAction(IScript script, INodeContainer nodeContainer, Point location)
    {
        var newNode = script.Runtime.NodeManager.CreateNode(nodeContainer.Descriptor);
        newNode.Location.Value = location;
        return new AddNodeAction(newNode, (IWritableNodeGraph)script.NodeGraph);
    }

    public IUserAction? FindBridgeConnectorsAction(IScript script, IConnector connectorOne, IConnector connectorTwo)
    {
        if (connectorOne is IInputConnector inputConnectorOne && connectorTwo is IOutputConnector outputConnectorTwo)
        {
            return FindBridgeActionOrdered((IWritableNodeGraph)script.NodeGraph, inputConnectorOne, outputConnectorTwo);
        }

        if (connectorOne is IOutputConnector outputConnectorOne && connectorTwo is IInputConnector inputConnectorTwo)
        {
            return FindBridgeActionOrdered((IWritableNodeGraph)script.NodeGraph, inputConnectorTwo, outputConnectorOne);
        }

        return null;
    }

    public IUserAction DeleteConnectionAction(IScript script, IConnection connection)
        => new SeverConnectionAction(connection.OutputConnector, connection.InputConnector, (IWritableNodeGraph)script.NodeGraph);

    public IUserAction DeleteNodeAction(IScript script, INodeContainer nodeContainer)
        => new DeleteNodeAction(nodeContainer, (IWritableNodeGraph)script.NodeGraph);

    public IUserAction AddSubTree(IScript script, INodeGraph subGraph)
    {
        List<IUserAction> actions =
        [
            .. subGraph.Nodes
                .Select(node => new AddNodeAction(node, (IWritableNodeGraph)script.NodeGraph))
                .Cast<IUserAction>(),

            .. subGraph.Connections
                .Select(connection => new EstablishConnectionAction(connection.OutputConnector,
                    connection.InputConnector,
                    (IWritableNodeGraph)script.NodeGraph))
                .Cast<IUserAction>()
        ];

        return new CompoundAction(actions);
    }

    private IUserAction? FindBridgeActionOrdered(IWritableNodeGraph writableNodeGraph, IInputConnector inputConnector, IOutputConnector outputConnector)
    {
        foreach (var bridger in connectionBridgers)
        {
            if (bridger.TryGetBridgeAction(outputConnector, inputConnector, writableNodeGraph) is not
                { } action) continue;

            return action;
        }

        return null;
    }
}
