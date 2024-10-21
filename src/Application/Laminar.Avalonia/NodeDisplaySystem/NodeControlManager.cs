using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.VisualTree;
using Laminar.Contracts.Scripting.NodeWrapping;
using Laminar.PluginFramework.NodeSystem.Connectors;

namespace Laminar.Avalonia.NodeDisplaySystem;

internal class NodeControlManager
{
    private readonly Dictionary<IWrappedNode, Control> nodes = new();
    private readonly Dictionary<Control, IWrappedNode> controls = new();

    public event EventHandler<PointerPressedEventArgs> NodeClicked;

    public void ForgetNode(IWrappedNode node)
    {
        controls.Remove(nodes[node]);
        nodes.Remove(node);
    }

    public Control GetControl(IWrappedNode node)
    {
        if (nodes.TryGetValue(node, out Control control))
        {
            return control;
        }

        nodes.Add(node, new NodeWrapperDisplay { CoreNode = node });
        nodes[node].PointerPressed += (o, e) => NodeControlManager_PointerPressed(nodes[node], e);

        controls.Add(nodes[node], node);
        return nodes[node];
    }

    public IEnumerable<IWrappedNode> GetSelectedNodes(SelectionManager selectionManager)
    {
        foreach (Control control in selectionManager.GetSelection<Control>())
        {
            if (TryGetNode(control, out IWrappedNode node))
            {
                yield return node;
            }
        }
    }

    public bool TryGetConnector(Control control, out IIOConnector connector)
    {
        if (control is ConnectorControl connectorControl)
        {
            connector = connectorControl.Connector;
            return true;
        }

        connector = null;
        return false;
    }

    public bool TryGetNode(Control control, out IWrappedNode node) => controls.TryGetValue(control, out node);

    private void NodeControlManager_PointerPressed(object sender, PointerPressedEventArgs e)
    {
        NodeClicked?.Invoke(sender, e);
    }
}