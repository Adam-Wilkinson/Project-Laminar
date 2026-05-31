using Laminar.Contracts.Base.ActionSystem;
using Laminar.Contracts.Scripting.Connection;
using Laminar.Contracts.Scripting.NodeWrapping;
using Laminar.Domain.ValueObjects;
using Laminar.PluginFramework.NodeSystem.Connectors;

namespace Laminar.Contracts.Scripting;

public interface IScriptEditor
{
    public IUserAction AddMatchingNodeAction(IScript script, IWrappedNode node, Point location);
    
    public IUserAction? FindBridgeConnectorsAction(IScript script, IConnector connectorOne, IConnector connectorTwo);
    
    public IUserAction DeleteConnectionAction(IScript script, IConnection connection);
    
    public IUserAction DeleteNodeAction(IScript script, IWrappedNode node);
}
