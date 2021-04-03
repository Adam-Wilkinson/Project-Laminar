using OpenFlow_PluginFramework.NodeSystem.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenFlow_Core.Nodes
{
    public interface INodeFactory
    {
        INodeBase Get<T>() where T : INode;
    }
}
