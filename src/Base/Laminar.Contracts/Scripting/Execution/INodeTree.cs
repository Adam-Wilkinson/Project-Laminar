using Laminar.Contracts.Scripting.NodeWrapping;
using Laminar.PluginFramework.NodeSystem.Connectors;

namespace Laminar.Contracts.Scripting.Execution;

public interface INodeTree
{
    public event EventHandler? Changed;

    IWrappedNode GetParentNode(IIOConnector connector);

    IReadOnlyList<IWrappedNode> GetConnections(IOutputConnector outputConnector);
}
