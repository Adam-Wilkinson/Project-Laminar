using Laminar.Contracts.Scripting.NodeWrapping;
using Laminar.PluginFramework.NodeSystem.Connectors;

namespace Laminar.Contracts.Scripting.Execution;

public interface INodeTree
{
    public event EventHandler? Changed;

    IWrappedNode GetConnectorOwner(IIOConnector connector);

    IReadOnlyList<IIOConnector> GetConnections(IIOConnector connector);
    
    IReadOnlyList<IWrappedNode> GetConnections(IOutputConnector outputConnector);
}
