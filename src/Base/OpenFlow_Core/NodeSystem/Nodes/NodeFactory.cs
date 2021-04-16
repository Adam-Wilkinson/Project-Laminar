using OpenFlow_Core.NodeSystem.Nodes;
using OpenFlow_Core.NodeSystem.Nodes.NodeTypes;
using OpenFlow_Core.Primitives;
using OpenFlow_PluginFramework.NodeSystem.Nodes;
using System;

namespace OpenFlow_Core.NodeSystem.Nodes
{
    public class NodeFactory : INodeFactory
    {
        public INodeBase Get<T>() where T : INode
        {
            NodeDependencyAggregate dependencies = Instance.Factory.CreateInstance<NodeDependencyAggregate>();

            if (typeof(IFlowNode).IsAssignableFrom(typeof(T)))
            {
                return new FlowNode<T>(dependencies);
            }

            if (typeof(IActionNode).IsAssignableFrom(typeof(T)))
            {
                return new ActionNode<T>(dependencies);
            }

            if (typeof(IFunctionNode).IsAssignableFrom(typeof(T)))
            {
                return new FunctionNode<T>(dependencies);
            }

            if (typeof(ITriggerNode).IsAssignableFrom(typeof(T)))
            {
                return new TriggerNode<T>(dependencies);
            }

            return new NodeBase<T>(dependencies);
        }
    }
}
