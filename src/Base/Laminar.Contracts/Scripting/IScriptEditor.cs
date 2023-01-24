using Laminar.Contracts.Base.ActionSystem;
using Laminar.Contracts.Scripting.NodeWrapping;
using Laminar.Domain.ValueObjects;
using Laminar.PluginFramework.NodeSystem.Contracts.Connectors;

namespace Laminar.Contracts.Scripting;

public interface IScriptEditor
{
    public IUserActionManager UserActionManager { get; }

    public void DeleteNodes(IScript script, IEnumerable<IWrappedNode> nodes);

    public IWrappedNode AddCopyOfNode(IScript script, IWrappedNode node);

    public void MoveNodes(IScript script, IEnumerable<IWrappedNode> nodes, Point delta);

    public bool TryBridgeConnectors(IScript script, IIOConnector connectorOne, IIOConnector connectorTwo);
}
