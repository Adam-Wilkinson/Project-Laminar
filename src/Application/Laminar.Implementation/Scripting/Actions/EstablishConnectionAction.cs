using Laminar.Contracts.Base.ActionSystem;
using Laminar.Contracts.Scripting.Execution;
using Laminar.Domain.Exceptions;
using Laminar.PluginFramework.NodeSystem.Connectors;

namespace Laminar.Implementation.Scripting.Actions;

internal readonly struct EstablishConnectionAction(
    IOutputConnector outputConnector,
    IInputConnector inputConnector,
    INodeTree nodeTree)
    : IUserAction
{
    public IOutputConnector OutputConnector { get; } = outputConnector;

    public IInputConnector InputConnector { get; } = inputConnector;
    
    public bool CanExecute { get; } = outputConnector.CanConnectTo(inputConnector) || inputConnector.CanConnectTo(outputConnector);

    public Task<IUserActionResult> Execute()
    {
        if (!nodeTree.TryConnect(OutputConnector, InputConnector, out _)) 
            return Task.FromResult(IUserActionResult.Error(new CouldNotConnectException(OutputConnector, InputConnector)));
        
        return Task.FromResult(IUserActionResult.Success(new SeverConnectionAction(OutputConnector, InputConnector, nodeTree)));
    }

    public override string ToString() => $"Establish Connection: {OutputConnector} -> {InputConnector}";
}
