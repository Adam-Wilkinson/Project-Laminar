using Laminar.Contracts.Base.ActionSystem;
using Laminar.Contracts.Scripting;
using Laminar.Contracts.Scripting.Connection;
using Laminar.Implementation.Scripting.Actions;
using Laminar.PluginFramework.NodeSystem.Connectors;

namespace Laminar.Implementation.Scripting.Connections;

internal class DefaultConnectionBridger : IConnectionBridger
{
    public IUserAction TryBridge(IOutputConnector outputConnector, IInputConnector inputConnector, IScriptEditor scriptEditor, IConnectionCollection connections)
    {
        return new EstablishConnectionAction(outputConnector, inputConnector, connections);
    }
}