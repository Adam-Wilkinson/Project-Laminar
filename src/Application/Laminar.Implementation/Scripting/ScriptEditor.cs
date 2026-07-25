using Laminar.Contracts.Base.ActionSystem;
using Laminar.Contracts.Scripting;
using Laminar.Contracts.Scripting.Connection;
using Laminar.Contracts.Scripting.NodeWrapping;
using Laminar.Domain.ValueObjects;
using Laminar.Implementation.Base.ActionSystem;
using Laminar.Implementation.Scripting.Actions;
using Laminar.PluginFramework.NodeSystem.Connectors;

namespace Laminar.Implementation.Scripting;

internal class ScriptEditor(
    IUserActionManager userActionManager,
    INodeFactory nodeFactory,
    IEnumerable<IConnectionBridger> connectionBridgers)
    : IScriptEditor
{
    private readonly IUserActionManager _ = userActionManager.RegisterSimplifier(new ScriptActionSimplifier());
    
    public IUserAction AddMatchingNodeAction(IScript script, IWrappedNode node, Point location)
    {
        IWrappedNode newNode = nodeFactory.FromNodeInfo(node.Info);
        newNode.Location.Value = location;
        return new AddNodeAction(newNode, (IWritableNodeTree)script.NodeTree);
    }

    public IUserAction? FindBridgeConnectorsAction(IScript script, IConnector connectorOne, IConnector connectorTwo)
    {
        if (connectorOne is IInputConnector inputConnectorOne && connectorTwo is IOutputConnector outputConnectorTwo)
        {
            return FindBridgeActionOrdered((IWritableNodeTree)script.NodeTree, inputConnectorOne, outputConnectorTwo);
        }

        if (connectorOne is IOutputConnector outputConnectorOne && connectorTwo is IInputConnector inputConnectorTwo)
        {
            return FindBridgeActionOrdered((IWritableNodeTree)script.NodeTree, inputConnectorTwo, outputConnectorOne);
        }

        return null;
    }

    public IUserAction DeleteConnectionAction(IScript script, IConnection connection)
        => new SeverConnectionAction(connection.OutputConnector, connection.InputConnector, (IWritableNodeTree)script.NodeTree);

    public IUserAction DeleteNodeAction(IScript script, IWrappedNode node)
        => new DeleteNodeAction(node, (IWritableNodeTree)script.NodeTree);

    public IUserAction AddSubTree(IScript script, INodeTree subTree)
    {
        List<IUserAction> actions = [];
        actions.AddRange(subTree.Nodes
            .Select(node => new AddNodeAction(node, (IWritableNodeTree)script.NodeTree))
            .Cast<IUserAction>());

        actions.AddRange(subTree.Connections
            .Select(connection => new EstablishConnectionAction(connection.OutputConnector, connection.InputConnector, 
                (IWritableNodeTree)script.NodeTree))
            .Cast<IUserAction>());

        return new CompoundAction(actions);
    }

    private IUserAction? FindBridgeActionOrdered(IWritableNodeTree writableNodeTree, IInputConnector inputConnector, IOutputConnector outputConnector)
    {
        foreach (var bridger in connectionBridgers)
        {
            if (bridger.TryGetBridgeAction(outputConnector, inputConnector, writableNodeTree) is not
                { } action) continue;

            return action;
        }

        return null;
    }
}
