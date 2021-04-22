using Laminar_Core.NodeSystem.Nodes;
using Laminar_PluginFramework.NodeSystem.Nodes;

namespace Laminar_Core.NodeSystem.Nodes
{
    public interface INodeFactory
    {
        INodeBase Get<T>() where T : INode;

        INodeBase Get<T>(T node) where T : INode;
    }
}
