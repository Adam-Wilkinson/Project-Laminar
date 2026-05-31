using Laminar.Contracts.Base.ActionSystem;
using Laminar.Contracts.Scripting.Execution;
using Laminar.PluginFramework.NodeSystem.Connectors;

namespace Laminar.Contracts.Scripting.Connection;

public interface IConnectionBridger
{
    public IUserAction? TryGetBridgeAction(IOutputConnector outputConnector, IInputConnector inputConnector, INodeTree nodeTree);
}
