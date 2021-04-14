using OpenFlow_PluginFramework;
using OpenFlow_PluginFramework.NodeSystem.NodeComponents.Visuals;
using OpenFlow_PluginFramework.NodeSystem.Nodes;
using System.Diagnostics;

namespace OpenFlow_Core.NodeSystem.Nodes
{
    public class TriggerNode<T> : NodeBase<T> where T : INode
    {
        private readonly INodeLabel FlowOut = Constructor.NodeLabel("Trigger Flow Out").WithFlowOutput();

        public TriggerNode(NodeDependencyAggregate dependencies) : base(dependencies)
        {
            (BaseNode as ITriggerNode).Trigger += (o, e) => { Debug.WriteLine("A trigger node was triggered"); };
            FieldList.Insert(0, FlowOut);
        }
    }
}
