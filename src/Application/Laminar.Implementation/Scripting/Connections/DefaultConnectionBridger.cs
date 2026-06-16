using Laminar.Contracts.Base.ActionSystem;
using Laminar.Contracts.Scripting;
using Laminar.Contracts.Scripting.Connection;
using Laminar.Implementation.Scripting.Actions;
using Laminar.PluginFramework.NodeSystem.Connectors;

namespace Laminar.Implementation.Scripting.Connections;

internal class DefaultConnectionBridger : IConnectionBridger
{
    public IUserAction TryGetBridgeAction(IOutputConnector outputConnector, IInputConnector inputConnector, INodeTree writableNodeTree)
    {
        return new EstablishConnectionAction(outputConnector, inputConnector, (IWritableNodeTree)writableNodeTree);
    }
}