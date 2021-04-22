using Laminar_Core.NodeSystem.Nodes;
using Laminar_PluginFramework.NodeSystem.Nodes;

namespace Laminar_Core.NodeSystem.Nodes
{
    public interface INodeFactory
    {
        INodeContainer Get<T>() where T : INode;

        INodeContainer Get<T>(T node) where T : INode;
    }
}
