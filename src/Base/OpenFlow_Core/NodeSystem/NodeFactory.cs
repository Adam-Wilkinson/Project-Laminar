using OpenFlow_Core.NodeSystem.Nodes;
using OpenFlow_Core.Primitives;
using OpenFlow_PluginFramework.NodeSystem.Nodes;
using System;

namespace OpenFlow_Core.NodeSystem
{
    public class NodeFactory : INodeFactory
    {
        public INodeBase Get<T>() where T : INode
        {
            T node = (T)Activator.CreateInstance(typeof(T));
            NodeDependencyAggregate dependencies = Instance.Factory.CreateInstance<NodeDependencyAggregate>();

            if (node is IActionNode)
            {
                return new ActionNode<T>(node, dependencies);
            }

            if (node is IFunctionNode)
            {
                return new FunctionNode<T>(node, dependencies);
            }

            if (node is ITriggerNode)
            {
                return new TriggerNode<T>(node, dependencies);
            }

            return new NodeBase<T>(node, dependencies);
        }
    }
}
