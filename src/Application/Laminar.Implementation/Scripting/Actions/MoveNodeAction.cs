using Laminar.Contracts.Base.ActionSystem;
using Laminar.Contracts.Scripting.NodeWrapping;
using Laminar.Domain.ValueObjects;

namespace Laminar.Implementation.Scripting.Actions;

internal readonly struct MoveNodeAction(IWrappedNode node, Point locationDelta) : IUserAction
{
    public Point LocationDelta { get; } = locationDelta;

    public IWrappedNode Node => node;
    
    public bool CanExecute => true;

    public Task<IUserActionResult> Execute()
    {
        node.Location.Value += LocationDelta;
        return Task.FromResult(IUserActionResult.Success(new MoveNodeAction(node, -LocationDelta)));
    }
}