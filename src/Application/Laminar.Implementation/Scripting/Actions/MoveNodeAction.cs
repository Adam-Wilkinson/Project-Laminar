using Laminar.Contracts.Base.ActionSystem;
using Laminar.Contracts.Scripting.NodeWrapping;
using Laminar.Domain.ValueObjects;

namespace Laminar.Implementation.Scripting.Actions;

internal readonly struct MoveNodeAction(IWrappedNode node, Point locationDelta) : IUserAction
{
    public Point LocationDelta { get; } = locationDelta;
    
    public bool CanExecute => true;

    public Task<IUserActionResult> Execute()
    {
        node.Location.Value += LocationDelta;
        return Task.FromResult(IUserActionResult.Success(new MoveNodeAction(node, -LocationDelta)));
    }

    public IUserActionSimplification GetSimplificationAfter(IUserAction previousAction)
    {
        if (previousAction is not MoveNodeAction previousMoveAction) return IUserActionSimplification.None();

        var totalMove = previousMoveAction.LocationDelta + LocationDelta;
        
        if (totalMove.SquaredDistance() < 1e-10) return IUserActionSimplification.Undoes();

        return IUserActionSimplification.NewEffectiveAction(new MoveNodeAction(node, totalMove));
    }
}