using Laminar.Contracts.Base.ActionSystem;
using Laminar.PluginFramework.NodeSystem.Contracts.Connectors;

namespace Laminar.Contracts.Scripting.Connection;

public interface IConnectionBridger
{
    public IUserAction? TryBridge(IOutputConnector outputConnector, IInputConnector inputConnector, IScriptEditor scriptEditor, IConnectionCollection connections);
}
