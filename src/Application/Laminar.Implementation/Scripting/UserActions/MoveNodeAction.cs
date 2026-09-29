using Laminar.Contracts.Base.ActionSystem;
using Laminar.Contracts.Scripting.NodeWrapping;
using Laminar.Domain.ValueObjects;

namespace Laminar.Implementation.Scripting.UserActions;

internal readonly struct MoveNodeAction(INodeContainer nodeContainer, Point locationDelta) : IUserAction
{
    public Point LocationDelta { get; } = locationDelta;

    public INodeContainer NodeContainer => nodeContainer;
    
    public Task<IUserActionResult> Execute()
    {
        nodeContainer.Location.Value += LocationDelta;
        return Task.FromResult(IUserActionResult.Success(new MoveNodeAction(nodeContainer, -LocationDelta)));
    }

    public override string ToString() => $"Move Node {NodeContainer}";
}