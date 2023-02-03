using System;
using System.Collections.Generic;
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
        IEditableScript editableScript = MakeEditable(script);
        IWrappedNode newNode = node.Clone(editableScript.ExecutionInstance);
        UserActionManager.ExecuteAction(new AddNodeAction(newNode, editableScript.Nodes));
        return newNode;
    }

    public void DeleteNodes(IScript script, IEnumerable<IWrappedNode> nodes)
    {
        IEditableScript editableScript = MakeEditable(script);
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
