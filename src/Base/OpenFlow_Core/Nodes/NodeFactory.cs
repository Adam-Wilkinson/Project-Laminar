using OpenFlow_Core.Primitives;
using OpenFlow_PluginFramework.NodeSystem.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
