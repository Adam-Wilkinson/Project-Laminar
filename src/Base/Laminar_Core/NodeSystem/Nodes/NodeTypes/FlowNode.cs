using Laminar_PluginFramework.NodeSystem.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Laminar_Core.NodeSystem.Nodes.NodeTypes
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
