using Laminar.Contracts.Base.ActionSystem;
using Laminar.Domain.Exceptions;
using Laminar.Implementation.Base.ActionSystem;
using Laminar.PluginFramework.NodeSystem.Connectors;

namespace Laminar.Implementation.Scripting.Actions;

internal readonly struct EstablishConnectionAction(
    IOutputConnector outputConnector,
    IInputConnector inputConnector,
    IWritableNodeTree writableNodeTree)
    : IUserAction
{
    public IOutputConnector OutputConnector { get; } = outputConnector;

    public IInputConnector InputConnector { get; } = inputConnector;
    
    public bool CanExecute { get; } = outputConnector.CanConnectTo(inputConnector) || inputConnector.CanConnectTo(outputConnector);

    public Task<IUserActionResult> Execute()
    {
        if (!CanExecute)
        {
            return Task.FromResult(IUserActionResult.Error(new CouldNotConnectException(OutputConnector, InputConnector)));
        }
        
        List<IUserAction> totalRequiredActions = [];
        if (InputConnector.Flags.HasFlag(ConnectorFlags.ConnectionsSaturated)
            && writableNodeTree.GetConnectionsTo(InputConnector).FirstOrDefault()?.OppositeConnector is IOutputConnector
                problemOutputConnector)
        {
            totalRequiredActions.Add(new SeverConnectionAction(problemOutputConnector, InputConnector, writableNodeTree));
        }

        if (OutputConnector.Flags.HasFlag(ConnectorFlags.ConnectionsSaturated)
            && writableNodeTree.GetConnectionsTo(OutputConnector).FirstOrDefault()?.OppositeConnector is IInputConnector
                problemInputConnector)
        {
            totalRequiredActions.Add(new SeverConnectionAction(OutputConnector, problemInputConnector, writableNodeTree));
        }

        if (totalRequiredActions.Count == 0)
        {
            return Task.FromResult(writableNodeTree.TryConnect(OutputConnector, InputConnector, out _)
                ? IUserActionResult.Success(new SeverConnectionAction(OutputConnector, InputConnector, writableNodeTree))
                : IUserActionResult.Error(new CouldNotConnectException(OutputConnector, InputConnector)));
        }
        
        totalRequiredActions.Add(this);
        return Task.FromResult(IUserActionResult.Alternative(new CompoundAction(totalRequiredActions)));
    }

    public override string ToString() => $"Establish Connection: {OutputConnector} -> {InputConnector}";
}
