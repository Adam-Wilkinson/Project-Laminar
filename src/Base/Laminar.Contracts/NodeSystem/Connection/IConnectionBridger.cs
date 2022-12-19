using Laminar.Contracts.ActionSystem;
using Laminar.PluginFramework.NodeSystem.Contracts.Connectors;

namespace Laminar.Contracts.NodeSystem.Connection;

public interface IConnectionBridger
{
    public IUserAction? TryBridge(IOutputConnector outputConnector, IInputConnector inputConnector, IScriptEditor scriptEditor, IConnectionCollection connections);
}
