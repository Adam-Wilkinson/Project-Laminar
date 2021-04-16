using OpenFlow_PluginFramework.NodeSystem.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenFlow_Core.NodeSystem.Nodes
{
    public class FlowNode<T> : FunctionNode<T> where T : INode
    {
        public FlowNode(NodeDependencyAggregate dep) : base(dep)
        {
        }

        protected override void TriggerEvaluate()
        {
            base.TriggerEvaluate();
            FlowOutContainer = GetContainer((BaseNode as IFlowNode).FlowOutComponent);
        }
    }
}
