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
    public class ActionNode<T> : FunctionNode<T> where T : INode, new()
    {
        public ActionNode(NodeDependencyAggregate dependencies)
            : base(dependencies)
        {
        }

        public override T BaseNode 
        {
            set
            {
                base.BaseNode = value;
                Name.Child.FlowInput.Exists = true;
                Name.Child.FlowOutput.Exists = true;
            }
        }
    }
}
