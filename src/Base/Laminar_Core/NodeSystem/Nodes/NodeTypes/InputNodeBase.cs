using Laminar_Core.NodeSystem.NodeComponents.Visuals;
using Laminar_Core.NodeSystem.NodeTreeSystem;
using Laminar_Core.Primitives;
using Laminar_PluginFramework.NodeSystem.Nodes;
using Laminar_PluginFramework.Primitives;
using System;
using System.Collections.Specialized;

namespace Laminar_Core.NodeSystem.Nodes.NodeTypes
{
    class InputNodeBase : NodeBase<InputNode>
    {
        public InputNodeBase(NodeDependencyAggregate deps) : base(deps) 
        {
        }

        public void SetType(Type type)
        {
            (BaseNode as InputNode).SetType(type);
        }

        public override INodeBase DuplicateNode()
        {
            return this;
        }
    }
}
