using System.Collections.Generic;
using Laminar.Contracts.ActionSystem;
using Laminar.Contracts.NodeSystem;
using Laminar.Domain.ValueObjects;
using Laminar.Core.ScriptEditor.Actions;
using Laminar.Contracts.NodeSystem.Connection;
using Laminar.PluginFramework.NodeSystem.Contracts.Connectors;
using System;

namespace Laminar.Core.ScriptEditor;

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

    public INodeWrapper AddCopyOfNode(IScript script, INodeWrapper node)
    {
        if (script is not IEditableScript editableScript)
        {
            return null;
        }

        INodeWrapper newNode = node.Clone(editableScript.ExecutionInstance);
        UserActionManager.ExecuteAction(new AddNodeAction(newNode, editableScript.Nodes));
        return newNode;
    }

    public void DeleteNodes(IScript script, IEnumerable<INodeWrapper> nodes)
    {
        if (script is not IEditableScript editableScript)
        {
            return;
        }

        UserActionManager.BeginCompoundAction();
        foreach (INodeWrapper node in nodes)
        {
            RemoveConnectionsTo(editableScript, node);
            UserActionManager.ExecuteAction(new DeleteNodeAction(node, editableScript.Nodes));
        }

        UserActionManager.EndCompoundAction();
    }

    public void MoveNodes(IScript script, IEnumerable<INodeWrapper> nodes, Point delta)
    {
        CompoundAction moveNodesAction = new();
        foreach (INodeWrapper node in nodes)
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

    private static void RemoveConnectionsTo(IEditableScript script, INodeWrapper node)
    {
        foreach (INodeRowWrapper row in node.Fields)
        {
            script.Connections.RemoveConnectionsTo(row.InputConnector?.NodeIOConnector);
            script.Connections.RemoveConnectionsTo(row.OutputConnector?.NodeIOConnector);
        }
    }
}
