using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.VisualTree;
using Laminar.Contracts.NodeSystem;
using Laminar.PluginFramework.NodeSystem.Contracts.Connectors;

namespace Laminar.Avalonia.NodeDisplaySystem;

internal class NodeControlManager
{
    private readonly Dictionary<INodeWrapper, Control> nodes = new();
    private readonly Dictionary<Control, INodeWrapper> controls = new();

    private readonly Dictionary<Control, IIOConnector> connectors = new();
    private readonly Dictionary<IIOConnector, Control> connectorControls = new();

    public event EventHandler<PointerPressedEventArgs> NodeClicked;

    public void ForgetNode(INodeWrapper node)
    {
        controls.Remove(nodes[node]);
        nodes.Remove(node);
    }

    public Control GetControl(INodeWrapper node)
    {
        if (nodes.TryGetValue(node, out Control control))
        {
            return control;
        }

        nodes.Add(node, new NodeWrapperDisplay { CoreNode = node });
        nodes[node].PointerPressed += (o, e) =>  NodeControlManager_PointerPressed(nodes[node], e);
        foreach (IVisual visual in nodes[node].GetVisualDescendants())
        {
            if (visual is ConnectorControl connectorControl)
            {
                connectors[connectorControl] = connectorControl.Connector;
                connectorControls[connectorControl.Connector] = connectorControl;
            }
        }

        controls.Add(nodes[node], node);
        return nodes[node];
    }

    public IEnumerable<INodeWrapper> GetSelectedNodes(SelectionManager selectionManager)
    {
        foreach (Control control in selectionManager.GetSelection<Control>())
        {
            if (TryGetNode(control, out INodeWrapper node))
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

    public bool TryGetNode(Control control, out INodeWrapper node) => controls.TryGetValue(control, out node);

    private void NodeControlManager_PointerPressed(object sender, PointerPressedEventArgs e)
    {
        NodeClicked?.Invoke(sender, e);
    }
}