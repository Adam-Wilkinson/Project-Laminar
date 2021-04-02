namespace OpenFlow_PluginFramework.NodeSystem.Nodes
{
    using System.Collections.Generic;
    using OpenFlow_PluginFramework.NodeSystem.NodeComponents;
    using OpenFlow_PluginFramework.NodeSystem.NodeComponents.Collections;

    public interface INode
    {
        string NodeName { get; }

        IEnumerable<INodeComponent> Fields { get; }

        void Evaluate();
    }
}
