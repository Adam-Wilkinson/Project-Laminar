using Laminar.Contracts.ActionSystem;
using Laminar.Contracts.NodeSystem.Connection;
using Laminar.Domain;
using Laminar.Domain.ValueObjects;
using Laminar.PluginFramework.NodeSystem.Contracts.Connectors;

namespace Laminar.Contracts.NodeSystem;

public interface IScriptEditor
{
    public IUserActionManager UserActionManager { get; }

    public void DeleteNodes(IScript script, IEnumerable<INodeWrapper> nodes);

    public INodeWrapper AddCopyOfNode(IScript script, INodeWrapper node);

    public void MoveNodes(IScript script, IEnumerable<INodeWrapper> nodes, Point delta);

    public bool TryBridgeConnectors(IScript script, IIOConnector connectorOne, IIOConnector connectorTwo);
}
