using Laminar.Contracts.Base.ActionSystem;
using Laminar.Contracts.Scripting.Execution;
using Laminar.Domain.Exceptions;
using Laminar.Implementation.Scripting.Execution;
using Laminar.PluginFramework.NodeSystem.Connectors;

namespace Laminar.Implementation.Scripting.Actions;

internal readonly struct SeverConnectionAction(
    IOutputConnector outputConnector,
    IInputConnector inputConnector,
    INodeTree nodeTree)
    : IUserAction
{
    public IOutputConnector OutputConnector { get; } = outputConnector;
    
    public IInputConnector InputConnector { get; } = inputConnector;

    public bool CanExecute => nodeTree.ConnectionExists(OutputConnector, InputConnector);

    public Task<IUserActionResult> Execute()
    {
        if (!nodeTree.ConnectionExists(OutputConnector, InputConnector))
        {
            return Task.FromResult(IUserActionResult.Error(new ConnectionDoesNotExistException(OutputConnector, InputConnector)));
        }
        
        nodeTree.SeverConnection(OutputConnector, InputConnector);
        return Task.FromResult(IUserActionResult.Success(new EstablishConnectionAction(OutputConnector, InputConnector, nodeTree)));
    }

    public override string ToString() => $"Sever connection between {OutputConnector} and {InputConnector}";
}
