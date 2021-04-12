using OpenFlow_PluginFramework;
using OpenFlow_PluginFramework.NodeSystem.NodeComponents.Visuals;
using OpenFlow_PluginFramework.NodeSystem.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenFlow_Core.NodeSystem.Nodes
{
    public class ActionNode<T> : FunctionNode<T> where T : INode
    {
        public ActionNode(T baseNode, NodeDependencyAggregate dependencies) 
            : base(baseNode, dependencies)
        {
            FieldList.Insert(0, Constructor.NodeLabel("Flow").WithFlowInput().WithFlowOutput());
            FieldList.Insert(1, Constructor.Separator());
        }
    }
}
