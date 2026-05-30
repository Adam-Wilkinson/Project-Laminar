using Laminar.Contracts.Base.ActionSystem;
using Laminar.Contracts.Scripting.Connection;
using Laminar.Contracts.Scripting.NodeWrapping;
using Laminar.Domain.ValueObjects;
using Laminar.PluginFramework.NodeSystem.Connectors;

namespace Laminar.Contracts.Scripting;

public interface IScriptEditor
{
    public void DeleteNodes(IScript script, IEnumerable<IWrappedNode> nodes);

    public IUserAction AddMatchingNodeAction(IScript script, IWrappedNode node, Point location);

    public void MoveNodes(IScript script, IEnumerable<IWrappedNode> nodes, Point delta);

    public IUserAction? FindBridgeConnectorsAction(IScript script, IConnector connectorOne, IConnector connectorTwo);
    
    public IUserAction DeleteConnectionAction(IScript script, IConnection connection);
    
    public IUserAction DeleteNodeAction(IScript script, IWrappedNode node);
}
