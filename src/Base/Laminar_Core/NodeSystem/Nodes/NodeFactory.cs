﻿using Laminar_Core.NodeSystem.Nodes.NodeTypes;
using Laminar_Core.NodeSystem.NodeTreeSystem;
using Laminar_Core.Scripting.Advanced.Editing;
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

        public INodeContainer Get<T>(T node) where T : INode, new()
        {
            INodeContainer output = PrivateGet(node);

            output.Update(null);

            return output;
        }

        public INodeContainer Get<T>() where T : INode, new()
            => Get(_factory.CreateInstance<T>());

        public INodeContainer Get<T>(T node, Guid guid) where T : INode, new()
        {
            NodeContainer<T> output = PrivateGet(node);
            output.Guid = guid;
            return output;
        }

        private NodeContainer<T> PrivateGet<T>(T node) where T : INode, new()
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

            if (typeof(T) == typeof(InputNode))
            {
                return Make<InputNodeContainer<T>, T>(node);
            }

            return Make<NodeContainer<T>, T>(node);
        }

        private NodeContainer<TNode> Make<TContainer, TNode>(TNode node) where TNode : INode, new() where TContainer : NodeContainer<TNode>
        {
            NodeContainer<TNode> output = _factory.CreateInstance<TContainer>();

            output.BaseNode = node;

            return output;
        }
    }
}
