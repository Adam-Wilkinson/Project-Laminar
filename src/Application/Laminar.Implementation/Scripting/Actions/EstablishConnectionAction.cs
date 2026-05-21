using Laminar.Contracts.Base.ActionSystem;
using Laminar.Contracts.Scripting.Connection;
using Laminar.Implementation.Scripting.Connections;
using Laminar.PluginFramework.NodeSystem.Connectors;

namespace Laminar.Implementation.Scripting.Actions;

public class EstablishConnectionAction(
    IOutputConnector connectorOne,
    IInputConnector connectorTwo,
    ICollection<IConnection> connectionCollection)
    : IUserAction
{
    public event EventHandler? CanExecuteChanged { add { } remove { } }

    public bool CanExecute { get; } = connectorOne.CanConnectTo(connectorTwo) || connectorTwo.CanConnectTo(connectorOne);

    public Task<IUserActionResult> Execute()
    {
        if (!connectorOne.TryConnectTo(connectorTwo) && !connectorTwo.TryConnectTo(connectorOne)) 
            return Task.FromResult(IUserActionResult.Invalid());
        
        connectorOne.OnConnectionEstablished();
        connectorTwo.OnConnectionEstablished();
        
        Connection connection = new()
        {
            OutputConnector = connectorOne,
            InputConnector = connectorTwo
        };
        
        connectionCollection.Add(connection);
        return Task.FromResult(IUserActionResult.Success(new SeverConnectionAction(connection, connectionCollection)));
    }
}
