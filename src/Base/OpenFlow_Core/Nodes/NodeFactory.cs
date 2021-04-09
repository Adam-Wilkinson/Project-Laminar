using OpenFlow_Core.Primitives;
using OpenFlow_PluginFramework.NodeSystem.Nodes;
using System;

namespace OpenFlow_Core.Nodes
{
    public class NodeFactory : INodeFactory
    {
        public INodeBase Get<T>() where T : INode
        {
            return new NodeBase((T)Activator.CreateInstance(typeof(T)), new ObservableValue<bool>());
        }
    }
}
