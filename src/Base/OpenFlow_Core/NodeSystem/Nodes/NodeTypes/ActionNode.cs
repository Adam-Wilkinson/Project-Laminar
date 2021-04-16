using Laminar_Core.NodeSystem.NodeComponents.Visuals;
using Laminar_PluginFramework;
using Laminar_PluginFramework.NodeSystem.NodeComponents.Visuals;
using Laminar_PluginFramework.NodeSystem.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Laminar_Core.NodeSystem.Nodes.NodeTypes
{
    public class ActionNode<T> : FunctionNode<T> where T : INode
    {
        public ActionNode(NodeDependencyAggregate dependencies) 
            : base(dependencies)
        {
            INodeLabel FlowOut = Constructor.NodeLabel("Flow").WithFlowInput().WithFlowOutput();
            FieldList.Insert(0, FlowOut);
            FieldList.Insert(1, Constructor.Separator());
            FlowOutContainer = GetContainer(FlowOut);
        }
    }
}
