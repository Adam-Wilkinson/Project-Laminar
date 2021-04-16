using Laminar_PluginFramework.NodeSystem.NodeComponents;
using System.Collections.Generic;

namespace Laminar_PluginFramework.NodeSystem.Nodes
{
    public interface INode
    {
        IEnumerable<INodeComponent> Fields { get; }

        string NodeName { get; }
    }
}