using Laminar.Contracts.Base.ActionSystem;
using Laminar.Contracts.Scripting;
using Laminar.Contracts.Scripting.Connection;
using Laminar.Contracts.Scripting.NodeWrapping;
using Laminar.Domain.Extensions;
using Laminar.Domain.ValueObjects;
using Laminar.Implementation.Base.ActionSystem;
using Laminar.Implementation.Scripting.Actions;
using Laminar.PluginFramework.NodeSystem.Components;
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
        IEditableScript editableScript = MakeEditable(script);
        IWrappedNode newNode = nodeFactory.CreateMatchingNode(node, editableScript.ExecutionInstance);
        newNode.Location.Value = location;
        return new AddNodeAction(newNode, editableScript.Nodes);
    }

    public void DeleteNodes(IScript script, IEnumerable<IWrappedNode> nodes)
    {
        IEditableScript editableScript = MakeEditable(script);
        userActionManager.ExecuteAction(
            new CompoundAction(nodes.Select(x => (IUserAction)new DeleteNodeAction(x, editableScript.Nodes))));
    }

    public void MoveNodes(IScript script, IEnumerable<IWrappedNode> nodes, Point delta)
    {
        CompoundAction moveNodesAction = new(nodes.Select(x => (IUserAction)new MoveNodeAction(x, delta)));

        userActionManager.ExecuteAction(moveNodesAction);
    }

    public IUserAction? FindBridgeConnectorsAction(IScript script, IConnector connectorOne, IConnector connectorTwo)
    {
        if (script is not IEditableScript editableScript)
        {
            return null;
        }

        if (connectorOne is IInputConnector inputConnectorOne && connectorTwo is IOutputConnector outputConnectorTwo)
        {
            return FindBridgeActionOrdered(editableScript, inputConnectorOne, outputConnectorTwo);
        }

        if (connectorOne is IOutputConnector outputConnectorOne && connectorTwo is IInputConnector inputConnectorTwo)
        {
            return FindBridgeActionOrdered(editableScript, inputConnectorTwo, outputConnectorOne);
        }

        return null;
    }

    public IUserAction DeleteConnectionAction(IScript script, IConnection connection)
        => new SeverConnectionAction(connection, ((IEditableScript)script).Connections);

    public IUserAction DeleteNodeAction(IScript script, IWrappedNode node)
        => new DeleteNodeAction(node, ((IEditableScript)script).Nodes);

    private IUserAction? FindBridgeActionOrdered(IEditableScript editableScript, IInputConnector inputConnector, IOutputConnector outputConnector)
    {
        List<IUserAction>? preparationActions = null;
        if (inputConnector.Status == ConnectorStatus.ConnectionsSaturated)
        {
            preparationActions ??= [];
            preparationActions.Add(new SeverConnectionAction(
                editableScript.NodeTree.GetConnections(inputConnector).First().Connection, editableScript.Connections));
        }

        if (outputConnector.Status == ConnectorStatus.ConnectionsSaturated)
        {
            preparationActions ??= [];
            preparationActions.Add(new SeverConnectionAction(
                editableScript.NodeTree.GetConnections(outputConnector).First().Connection, editableScript.Connections));
        }
        
        foreach (IConnectionBridger bridger in connectionBridgers)
        {
            if (bridger.TryGetBridgeAction(outputConnector, inputConnector, this, editableScript.Connections) is not
                { } action) continue;

            return preparationActions is not null
                ? new CompoundAction(preparationActions.Concat(action.Yield()))
                : action;
        }

        return null;
    }

    private static void RemoveConnectionsTo(IEditableScript script, IWrappedNode node)
    {
        foreach (INodeRow row in node.Rows)
        {
            script.Connections.RemoveConnectionsTo(row.InputConnector);
            script.Connections.RemoveConnectionsTo(row.OutputConnector);
        }
    }

    private static IEditableScript MakeEditable(IScript script)
    {
        if (script is not IEditableScript editableScript)
        {
            throw new ArgumentException("Scripts must also implement IEditableScript to edit them", nameof(script));
        }

        return editableScript;
    }
}
