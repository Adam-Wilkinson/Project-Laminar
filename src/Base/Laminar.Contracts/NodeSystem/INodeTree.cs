using Laminar.Contracts.NodeSystem.Connection;
using Laminar.PluginFramework.NodeSystem.Contracts.Connectors;

namespace Laminar.Contracts.NodeSystem;

public interface INodeTree
{
    IReadOnlyList<INodeWrapper> GetDirectDependents(INodeWrapper node);

    INodeWrapper[] GetExecutionOrder(INodeWrapper node);
}
