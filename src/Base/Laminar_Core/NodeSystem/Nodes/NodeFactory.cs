using Laminar_Core.NodeSystem.Nodes;
using Laminar_Core.NodeSystem.Nodes.NodeTypes;
using Laminar_Core.NodeSystem.NodeTreeSystem;
using Laminar_Core.Primitives;
using Laminar_PluginFramework.NodeSystem.Nodes;
using Laminar_PluginFramework.Primitives;
using System;

namespace Laminar_Core.NodeSystem.Nodes
{
    public class NodeFactory : INodeFactory
    {
        private readonly IObjectFactory _factory;

        public NodeFactory(IObjectFactory factory)
        {
            _factory = factory;
        }

        public INodeBase Get<T>() where T : INode
        {
            INodeBase output = PrivateGet<T>();

            output.Update();

            return output;
        }

        private INodeBase PrivateGet<T>() where T : INode
        {
            if (typeof(IFlowNode).IsAssignableFrom(typeof(T)))
            {
                return _factory.CreateInstance<FlowNode<T>>();
            }

            if (typeof(IActionNode).IsAssignableFrom(typeof(T)))
            {
                return _factory.CreateInstance<ActionNode<T>>();
            }

            if (typeof(IFunctionNode).IsAssignableFrom(typeof(T)))
            {
                return _factory.CreateInstance<FunctionNode<T>>();
            }

            if (typeof(ITriggerNode).IsAssignableFrom(typeof(T)))
            {
                return _factory.CreateInstance<TriggerNode<T>>();
            }

            if (typeof(T) == typeof(InputNode))
            {
                return _factory.CreateInstance<InputNodeBase>();
            }

            return _factory.CreateInstance<NodeBase<T>>();
        }
    }
}
