using Laminar.Contracts.NodeSystem.Connection;
using Laminar.Contracts.NodeSystem.Execution;
using Laminar.PluginFramework.NodeSystem.Contracts.Connectors;
using Laminar.PluginFramework.NodeSystem.ExecutionFlags;

namespace Laminar.Contracts.NodeSystem;

public interface INodeTree
{
    IConditionalExecutionBranch[] GetExecutionBranches(IIOConnector connector, ExecutionFlags flags);
    INodeWrapper[] GetExecutionOrder(INodeWrapper node);
}
