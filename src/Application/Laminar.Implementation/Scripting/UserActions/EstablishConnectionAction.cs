using Laminar.Contracts.Base.ActionSystem;
using Laminar.Domain.Exceptions;
using Laminar.Implementation.Base.ActionSystem;
using Laminar.PluginFramework.NodeSystem.Connectors;

namespace Laminar.Implementation.Scripting.UserActions;

internal readonly struct EstablishConnectionAction(
    IOutputConnector outputConnector,
    IInputConnector inputConnector,
    IWritableNodeGraph writableNodeGraph)
    : IUserAction
{
    public IOutputConnector OutputConnector { get; } = outputConnector;

    public IInputConnector InputConnector { get; } = inputConnector;
    
    public Task<IUserActionExecutionOutcome> Execute()
    {
        if (!OutputConnector.CanConnectTo(InputConnector) && !InputConnector.CanConnectTo(OutputConnector))
        {
            return Task.FromResult(IUserActionExecutionOutcome.Error(new CouldNotConnectException(OutputConnector, InputConnector)));
        }
        
        List<IUserAction> totalRequiredActions = [];
        if (InputConnector.Flags.HasFlag(ConnectorFlags.ConnectionsSaturated)
            && writableNodeGraph.GetConnectionsTo(InputConnector).FirstOrDefault()?.OppositeConnector is IOutputConnector
                problemOutputConnector)
        {
            totalRequiredActions.Add(new SeverConnectionAction(problemOutputConnector, InputConnector, writableNodeGraph));
        }

        if (OutputConnector.Flags.HasFlag(ConnectorFlags.ConnectionsSaturated)
            && writableNodeGraph.GetConnectionsTo(OutputConnector).FirstOrDefault()?.OppositeConnector is IInputConnector
                problemInputConnector)
        {
            totalRequiredActions.Add(new SeverConnectionAction(OutputConnector, problemInputConnector, writableNodeGraph));
        }

        if (totalRequiredActions.Count == 0)
        {
            return Task.FromResult(writableNodeGraph.TryConnect(OutputConnector, InputConnector, out _)
                ? IUserActionExecutionOutcome.Success(new SeverConnectionAction(OutputConnector, InputConnector, writableNodeGraph))
                : IUserActionExecutionOutcome.Error(new CouldNotConnectException(OutputConnector, InputConnector)));
        }
        
        totalRequiredActions.Add(this);
        return Task.FromResult(IUserActionExecutionOutcome.Alternative(new CompoundAction(totalRequiredActions)));
    }

    public override string ToString() => $"Establish Connection: {OutputConnector} -> {InputConnector}";
}
