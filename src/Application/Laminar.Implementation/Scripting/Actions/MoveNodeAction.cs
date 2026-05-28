using Laminar.Contracts.Base.ActionSystem;
using Laminar.Contracts.Scripting.NodeWrapping;
using Laminar.Domain.ValueObjects;

namespace Laminar.Implementation.Scripting.Actions;

public class MoveNodeAction(IWrappedNode items, Point locationDelta) : IUserAction
{
    public Point LocationDelta { get; } = locationDelta;
    
    public bool CanExecute => true;

    public Task<IUserActionResult> Execute()
    {
        items.Location.Value += LocationDelta;
        return Task.FromResult(IUserActionResult.Success(new MoveNodeAction(items, -LocationDelta)));
    }

    public bool IsInverseOf(IUserAction action)
        => action is MoveNodeAction moveAction && moveAction.LocationDelta.IsCloseTo(-LocationDelta, 1e-10);
}