using System;
using System.Collections.Generic;
using System.Linq;
using Laminar.Contracts.Base.ActionSystem;
using Laminar.Contracts.Scripting;
using Laminar.Contracts.Scripting.Connection;
using Laminar.Contracts.Scripting.NodeWrapping;
using Laminar.Domain.ValueObjects;
using Laminar.Implementation.Base.ActionSystem;
using Laminar.Implementation.Scripting.Actions;
using Laminar.PluginFramework.NodeSystem.Components;
using Laminar.PluginFramework.NodeSystem.Connectors;

namespace Laminar.Implementation.Scripting;

internal class ScriptEditor(
    IUserActionManager userActionManager,
    IEnumerable<IConnectionBridger> connectionBridgers)
    : IScriptEditor
{
    public IWrappedNode AddCopyOfNode(IScript script, IWrappedNode node)
    {
        IEditableScript editableScript = MakeEditable(script);
        IWrappedNode newNode = node.Clone(editableScript.ExecutionInstance);
        userActionManager.ExecuteAction(new AddNodeAction(newNode, editableScript.Nodes));
        return newNode;
    }

    public void DeleteNodes(IScript script, IEnumerable<IWrappedNode> nodes)
    {
        IEditableScript editableScript = MakeEditable(script);
        userActionManager.ExecuteAction(
            new CompoundAction(nodes.Select(x => new DeleteNodeAction(x, editableScript.Nodes))));
    }

    public void MoveNodes(IScript script, IEnumerable<IWrappedNode> nodes, Point delta)
    {
        CompoundAction moveNodesAction = new();
        foreach (IWrappedNode node in nodes)
        {
            moveNodesAction.Add(new MoveNodeAction(node, delta));
        }

        userActionManager.ExecuteAction(moveNodesAction);
    }

    public IUserAction? FindBridgeConnectorsAction(IScript script, IIOConnector connectorOne, IIOConnector connectorTwo)
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
        foreach (IConnectionBridger bridger in connectionBridgers)
        {
            if (bridger.TryGetBridgeAction(outputConnector, inputConnector, this, editableScript.Connections) is not
                { } action) continue;
            
            return action;
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
