using Laminar.Contracts.Base.ActionSystem;
using Laminar.Contracts.Scripting;
using Laminar.Contracts.Scripting.Connection;
using Laminar.Contracts.Scripting.Execution;
using Laminar.Contracts.Scripting.NodeWrapping;
using Laminar.Contracts.Storage.PersistentData;
using Laminar.Domain.Extensions;
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
        IWrappedNode newNode = nodeFactory.FromNodeInfo(node.Info, script.ExecutionInstance);
        newNode.Location.Value = location;
        return new AddNodeAction(newNode, (INodeTree)script.NodeTreeView);
    }

    public IUserAction PasteFromPersistentDataAction(IScript script, IEncodablePersistentData data)
    {
        if (data is not IPersistentDictionary persistentDictionary)
            throw new InvalidOperationException();
        
        var node = nodeFactory.FromPersistentData(persistentDictionary, script.ExecutionInstance);
        node.Location.Value += new Point { X = 50, Y = 50 };
        return new AddNodeAction(node, (INodeTree)script.NodeTreeView);
    }

    public IUserAction? FindBridgeConnectorsAction(IScript script, IConnector connectorOne, IConnector connectorTwo)
    {
        if (connectorOne is IInputConnector inputConnectorOne && connectorTwo is IOutputConnector outputConnectorTwo)
        {
            return FindBridgeActionOrdered((INodeTree)script.NodeTreeView, inputConnectorOne, outputConnectorTwo);
        }

        if (connectorOne is IOutputConnector outputConnectorOne && connectorTwo is IInputConnector inputConnectorTwo)
        {
            return FindBridgeActionOrdered((INodeTree)script.NodeTreeView, inputConnectorTwo, outputConnectorOne);
        }

        return null;
    }

    public IUserAction DeleteConnectionAction(IScript script, IConnection connection)
        => new SeverConnectionAction(connection.OutputConnector, connection.InputConnector, (INodeTree)script.NodeTreeView);

    public IUserAction DeleteNodeAction(IScript script, IWrappedNode node)
        => new DeleteNodeAction(node, (INodeTree)script.NodeTreeView);

    private IUserAction? FindBridgeActionOrdered(INodeTree nodeTree, IInputConnector inputConnector, IOutputConnector outputConnector)
    {
        foreach (var bridger in connectionBridgers)
        {
            if (bridger.TryGetBridgeAction(outputConnector, inputConnector, nodeTree) is not
                { } action) continue;

            return action;
        }

        return null;
    }
}
