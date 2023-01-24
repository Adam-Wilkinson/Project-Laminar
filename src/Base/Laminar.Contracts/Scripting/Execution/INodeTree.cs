using Laminar.Contracts.Scripting.NodeWrapping;
using Laminar.PluginFramework.NodeSystem.Contracts.Connectors;
using Laminar.PluginFramework.NodeSystem.ExecutionFlags;

namespace Laminar.Contracts.Scripting.Execution;

public interface INodeTree
{
    IConditionalExecutionBranch[] GetExecutionBranches(IIOConnector connector, ExecutionFlags flags);
}
