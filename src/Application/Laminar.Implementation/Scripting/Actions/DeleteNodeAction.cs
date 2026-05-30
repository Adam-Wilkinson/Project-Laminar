using Laminar.Contracts.Base.ActionSystem;
using Laminar.Contracts.Scripting.NodeWrapping;

namespace Laminar.Implementation.Scripting.Actions;

internal readonly struct DeleteNodeAction(IWrappedNode node, ICollection<IWrappedNode> nodeCollection)
    : IUserAction
{
    public IWrappedNode Node { get; } = node;
    
    public bool CanExecute { get; } = nodeCollection.Contains(node);

    public Task<IUserActionResult> Execute()
    {
        nodeCollection.Remove(Node);
        return Task.FromResult(IUserActionResult.Success(new AddNodeAction(Node, nodeCollection)));
    }
}
