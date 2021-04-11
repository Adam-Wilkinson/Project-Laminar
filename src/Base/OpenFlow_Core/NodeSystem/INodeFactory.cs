using OpenFlow_Core.NodeSystem.Nodes;
using OpenFlow_PluginFramework.NodeSystem.Nodes;

namespace OpenFlow_Core.NodeSystem
{
    public interface INodeFactory
    {
        INodeBase Get<T>() where T : INode;
    }
}
