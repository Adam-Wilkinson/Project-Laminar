using Laminar.Contracts.Base.ActionSystem;
using Laminar.Contracts.Scripting.Execution;
using Laminar.Domain.Exceptions;
using Laminar.Implementation.Base.ActionSystem;
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
    
    public bool CanExecute { get; } = outputConnector.CouldConnectTo(inputConnector) || inputConnector.CouldConnectTo(outputConnector);

    public Task<IUserActionResult> Execute()
    {
        if (!CanExecute)
        {
            return Task.FromResult(IUserActionResult.Error(new CouldNotConnectException(OutputConnector, InputConnector)));
        }
        
        List<IUserAction> totalRequiredActions = [];
        if (InputConnector.Flags.HasFlag(ConnectorFlags.ConnectionsSaturated)
            && nodeTree.GetConnectionsTo(InputConnector).FirstOrDefault()?.OppositeConnector is IOutputConnector
                problemOutputConnector)
        {
            totalRequiredActions.Add(new SeverConnectionAction(problemOutputConnector, InputConnector, nodeTree));
        }

        if (OutputConnector.Flags.HasFlag(ConnectorFlags.ConnectionsSaturated)
            && nodeTree.GetConnectionsTo(OutputConnector).FirstOrDefault()?.OppositeConnector is IInputConnector
                problemInputConnector)
        {
            totalRequiredActions.Add(new SeverConnectionAction(OutputConnector, problemInputConnector, nodeTree));
        }

        if (totalRequiredActions.Count == 0)
        {
            return Task.FromResult(nodeTree.TryConnect(OutputConnector, InputConnector, out _)
                ? IUserActionResult.Success(new SeverConnectionAction(OutputConnector, InputConnector, nodeTree))
                : IUserActionResult.Error(new CouldNotConnectException(OutputConnector, InputConnector)));
        }
        
        totalRequiredActions.Add(this);
        return Task.FromResult(IUserActionResult.Alternative(new CompoundAction(totalRequiredActions)));
    }

    public override string ToString() => $"Establish Connection: {OutputConnector} -> {InputConnector}";
}
