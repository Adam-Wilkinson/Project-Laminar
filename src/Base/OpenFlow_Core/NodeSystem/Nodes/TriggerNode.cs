using OpenFlow_Core.NodeSystem.NodeComponents.Visuals;
using OpenFlow_PluginFramework;
using OpenFlow_PluginFramework.NodeSystem.NodeComponents.Visuals;
using OpenFlow_PluginFramework.NodeSystem.Nodes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace OpenFlow_Core.NodeSystem.Nodes
{
    public class TriggerNode<T> : NodeBase<T> where T : INode
    {
        private readonly INodeLabel FlowOut = Constructor.NodeLabel("Trigger Flow Out").WithFlowOutput();

        public TriggerNode(NodeDependencyAggregate dependencies) : base(dependencies)
        {
            (BaseNode as ITriggerNode).Trigger += TriggerNode_Trigger;
            FieldList.Insert(0, FlowOut);
        }

        private void TriggerNode_Trigger(object sender, EventArgs e)
        {
            Debug.WriteLine("Outputing");
            ((IList<IVisualNodeComponentContainer>)Fields)[FieldList.IndexOf(FlowOut)].OutputConnector?.Activate();
        }
    }
}
