using OpenFlow_PluginFramework.NodeSystem.Nodes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenFlow_Core.NodeSystem.Nodes
{
    public class TriggerNode<T> : NodeBase<T> where T : INode
    {
        public TriggerNode(T baseNode, NodeDependencyAggregate dependencies) : base(baseNode, dependencies)
        {
            (baseNode as ITriggerNode).Trigger += (o, e) => { Debug.WriteLine("A trigger node was triggered"); };
        }
    }
}
