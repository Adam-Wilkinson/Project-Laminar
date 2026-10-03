using Laminar.Contracts.Base.ActionSystem;
using Laminar.Domain.Exceptions;
using Laminar.PluginFramework.NodeSystem.Connectors;

namespace Laminar.Implementation.Scripting.UserActions;

internal readonly struct SeverConnectionAction(
    IOutputConnector outputConnector,
    IInputConnector inputConnector,
    IWritableNodeGraph writableNodeGraph)
    : IUserAction
{
    public IOutputConnector OutputConnector { get; } = outputConnector;
    
    public IInputConnector InputConnector { get; } = inputConnector;
    
    public Task<IUserActionExecutionOutcome> Execute()
    {
        if (!writableNodeGraph.ConnectionExists(OutputConnector, InputConnector, out _))
        {
            return Task.FromResult(IUserActionExecutionOutcome.Error(new ConnectionDoesNotExistException(OutputConnector, InputConnector)));
        }
        
        writableNodeGraph.SeverConnection(OutputConnector, InputConnector);
        return Task.FromResult(IUserActionExecutionOutcome.Success(new EstablishConnectionAction(OutputConnector, InputConnector, writableNodeGraph)));
    }

    public override string ToString() => $"Sever connection between {OutputConnector} and {InputConnector}";
}
