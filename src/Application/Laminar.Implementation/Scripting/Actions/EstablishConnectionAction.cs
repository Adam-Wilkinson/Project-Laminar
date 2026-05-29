using Laminar.Contracts.Base.ActionSystem;
using Laminar.Contracts.Scripting.Connection;
using Laminar.Implementation.Scripting.Connections;
using Laminar.PluginFramework.NodeSystem.Connectors;

namespace Laminar.Implementation.Scripting.Actions;

internal readonly struct  EstablishConnectionAction(
    IOutputConnector outputConnector,
    IInputConnector inputConnector,
    ICollection<IConnection> connectionCollection)
    : IUserAction
{
    public IOutputConnector OutputConnector { get; } = outputConnector;

    public IInputConnector InputConnector { get; } = inputConnector;
    
    public bool CanExecute { get; } = outputConnector.CanConnectTo(inputConnector) || inputConnector.CanConnectTo(outputConnector);

    public Task<IUserActionResult> Execute()
    {
        if (!OutputConnector.TryConnectTo(InputConnector) && !InputConnector.TryConnectTo(OutputConnector)) 
            return Task.FromResult(IUserActionResult.Invalid());
        
        OutputConnector.OnConnectionEstablished();
        InputConnector.OnConnectionEstablished();
        
        Connection connection = new()
        {
            OutputConnector = OutputConnector,
            InputConnector = InputConnector
        };
        
        connectionCollection.Add(connection);
        return Task.FromResult(IUserActionResult.Success(new SeverConnectionAction(connection, connectionCollection)));
    }
}
