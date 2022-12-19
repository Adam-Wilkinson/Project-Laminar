using Laminar.Contracts.ActionSystem;
using Laminar.Contracts.NodeSystem;
using Laminar.Contracts.NodeSystem.Connection;
using Laminar.Core.ScriptEditor.Actions;
using Laminar.PluginFramework.NodeSystem.Contracts.Connectors;

namespace Laminar.Core.ScriptEditor.Connections;

internal class DefaultConnectionBridger : IConnectionBridger
{
    public IUserAction TryBridge(IOutputConnector outputConnector, IInputConnector inputConnector, IScriptEditor scriptEditor, IConnectionCollection connections)
    {
        return new EstablishConnectionAction(outputConnector, inputConnector, connections);
    }
}