using OpenFlow_PluginFramework.NodeSystem.Nodes;

namespace OpenFlow_Core.Nodes
{
    public interface INodeFactory
    {
        INodeBase Get<T>() where T : INode;
    }
}
