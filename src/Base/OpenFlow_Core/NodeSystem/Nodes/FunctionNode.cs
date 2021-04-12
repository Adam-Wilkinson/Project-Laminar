﻿using OpenFlow_Core.NodeSystem.NodeComponents.Visuals;
using OpenFlow_PluginFramework.NodeSystem.Nodes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenFlow_Core.NodeSystem.Nodes
{
    public class FunctionNode<T> : NodeBase<T> where T : INode
    {
        readonly T _baseNode;
        bool _updating;

        public FunctionNode(T baseNode, NodeDependencyAggregate dependencies) 
            : base(baseNode, dependencies)
        {
            _baseNode = baseNode;
        }

        // public override INodeBase DuplicateNode() => new FunctionNode((IFunctionNode)Activator.CreateInstance(_baseNode.GetType()), Instance.Factory.CreateInstance<NodeDependencyAggregate>());

        public override void Update()
        {
            if (_updating)
            {
                return;
            }

            _updating = true;

            foreach (IVisualNodeComponentContainer component in (IList)Fields)
            {
                component.InputConnector.Activate();
            }

            try
            {
                (_baseNode as IFunctionNode).Evaluate();
                ErrorState.Value = false;
            }
            catch
            {
                ErrorState.Value = true;
            }

            foreach (IVisualNodeComponentContainer component in (IList)Fields)
            {
                component.OutputConnector.Activate();
            }

            _updating = false;
        }
    }
}