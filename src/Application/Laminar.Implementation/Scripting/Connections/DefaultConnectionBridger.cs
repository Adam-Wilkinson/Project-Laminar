using Laminar.Contracts.Base.ActionSystem;
using Laminar.Contracts.Scripting;
using Laminar.Contracts.Scripting.Connection;
using Laminar.Contracts.Scripting.Execution;
using Laminar.Implementation.Scripting.Actions;
using Laminar.Implementation.Scripting.Execution;
using Laminar.PluginFramework.NodeSystem.Connectors;

namespace Laminar.Implementation.Scripting.Connections;

internal class DefaultConnectionBridger : IConnectionBridger
{
    public IUserAction TryGetBridgeAction(IOutputConnector outputConnector, IInputConnector inputConnector, INodeTree nodeTree)
    {
        return new EstablishConnectionAction(outputConnector, inputConnector, nodeTree);
    }
}