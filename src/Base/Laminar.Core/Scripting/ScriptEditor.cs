using System.Collections.Generic;
using Laminar.Contracts.Scripting;
using Laminar.Domain.ValueObjects;
using Laminar.Implementation.Scripting.Actions;
using Laminar.Contracts.Scripting.Connection;
using Laminar.PluginFramework.NodeSystem.Contracts.Connectors;
using System;
using Laminar.Contracts.Base.ActionSystem;
using Laminar.Contracts.Scripting.NodeWrapping;
using Laminar.Implementation.Base.ActionSystem;

namespace Laminar.Implementation.Scripting;

internal class ScriptEditor : IScriptEditor
{
    private readonly IEnumerable<IConnectionBridger> _connectionBridgers;

    public ScriptEditor
        (IUserActionManager userActionManager, 
        IEnumerable<IConnectionBridger> connectionBridgers)
    {
        UserActionManager = userActionManager;
        _connectionBridgers = connectionBridgers;
    }

    public IUserActionManager UserActionManager { get; }

    public IWrappedNode AddCopyOfNode(IScript script, IWrappedNode node)
    {
        if (script is not IEditableScript editableScript)
        {
            return null;
        }

        IWrappedNode newNode = node.Clone(editableScript.ExecutionInstance);
        UserActionManager.ExecuteAction(new AddNodeAction(newNode, editableScript.Nodes));
        return newNode;
    }

    public void DeleteNodes(IScript script, IEnumerable<IWrappedNode> nodes)
    {
        if (script is not IEditableScript editableScript)
        {
            return;
        }

        UserActionManager.BeginCompoundAction();
        foreach (IWrappedNode node in nodes)
        {
            RemoveConnectionsTo(editableScript, node);
            UserActionManager.ExecuteAction(new DeleteNodeAction(node, editableScript.Nodes));
        }

        UserActionManager.EndCompoundAction();
    }

    public void MoveNodes(IScript script, IEnumerable<IWrappedNode> nodes, Point delta)
    {
        CompoundAction moveNodesAction = new();
        foreach (IWrappedNode node in nodes)
        {
            moveNodesAction.Add(new MoveNodeAction(node, delta));
        }

        UserActionManager.ExecuteAction(moveNodesAction);
    }

    public bool TryBridgeConnectors(IScript script, IIOConnector connectorOne, IIOConnector connectorTwo)
    {
        if (script is not IEditableScript editableScript)
        {
            return false;
        }

        if (connectorOne is IInputConnector inputConnectorOne && connectorTwo is IOutputConnector outputConnectorTwo)
        {
            return TryGetBridgeDisposableOrdered(editableScript, inputConnectorOne, outputConnectorTwo);
        }

        if (connectorOne is IOutputConnector outputConnectorOne && connectorTwo is IInputConnector inputConnectorTwo)
        {
            return TryGetBridgeDisposableOrdered(editableScript, inputConnectorTwo, outputConnectorOne);
        }

        return false;
    }

    private bool TryGetBridgeDisposableOrdered(IEditableScript editableScript, IInputConnector inputConnector, IOutputConnector outputConnector)
    {
        foreach (IConnectionBridger bridger in _connectionBridgers)
        {
            if (bridger.TryBridge(outputConnector, inputConnector, this, editableScript.Connections) is IUserAction action)
            {
                UserActionManager.ExecuteAction(action);
                return true;
            }
        }

        return false;
    }

    private static void RemoveConnectionsTo(IEditableScript script, IWrappedNode node)
    {
        foreach (IWrappedNodeRow row in node.Fields)
        {
            script.Connections.RemoveConnectionsTo(row.InputConnector?.NodeIOConnector);
            script.Connections.RemoveConnectionsTo(row.OutputConnector?.NodeIOConnector);
        }
    }
}
