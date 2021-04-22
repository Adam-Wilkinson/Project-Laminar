using Laminar_Core.NodeSystem.Nodes.NodeTypes;
using Laminar_Core.NodeSystem.NodeTreeSystem;
using Laminar_PluginFramework.NodeSystem.Nodes;
using Laminar_PluginFramework.Primitives;
using System;
using System.Collections.Generic;

namespace Laminar_Core.NodeSystem.Nodes
{
    public class NodeFactory : INodeFactory
    {
        private readonly IObjectFactory _factory;

        public NodeFactory(IObjectFactory factory)
        {
            _factory = factory;
        }

        public INodeContainer Get<T>(T node) where T : INode
        {
            INodeContainer output = PrivateGet(node);

            output.Update(null);

            return output;
        }

        public INodeContainer Get<T>() where T : INode
            => Get(_factory.CreateInstance<T>());

        private INodeContainer PrivateGet<T>(T node) where T : INode
        {
            if (typeof(IFlowNode).IsAssignableFrom(typeof(T)))
            {
                return Make<FlowNode<T>, T>(node);
            }

            if (typeof(IActionNode).IsAssignableFrom(typeof(T)))
            {
                return Make<ActionNode<T>, T>(node);
            }

            if (typeof(IFunctionNode).IsAssignableFrom(typeof(T)))
            {
                return Make<FunctionNode<T>, T>(node);
            }

            if (typeof(ITriggerNode).IsAssignableFrom(typeof(T)))
            {
                return Make<TriggerNode<T>, T>(node);
            }

            return Make<NodeContainer<T>, T>(node);
        }

        private INodeContainer Make<TContainer, TNode>(TNode node) where TNode : INode where TContainer : NodeContainer<TNode>
        {
            TContainer output = _factory.CreateInstance<TContainer>();

            output.BaseNode = node;

            return output;
        }
    }
}
