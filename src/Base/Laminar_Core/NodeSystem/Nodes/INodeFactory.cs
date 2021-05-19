using Laminar_Core.NodeSystem.Nodes;
using Laminar_PluginFramework.NodeSystem.Nodes;
using System;

namespace Laminar_Core.NodeSystem.Nodes
{
    public interface INodeFactory
    {
        INodeContainer Get<T>() where T : INode, new();

        INodeContainer Get<T>(T node) where T : INode, new();

        INodeContainer Get<T>(T node, Guid guid) where T : INode, new();
    }
}
