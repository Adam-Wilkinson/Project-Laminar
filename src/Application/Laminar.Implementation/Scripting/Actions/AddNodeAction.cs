using Laminar.Contracts.Base.ActionSystem;
using Laminar.Contracts.Scripting.NodeWrapping;

namespace Laminar.Implementation.Scripting.Actions;

internal readonly struct AddNodeAction(IWrappedNode node, ICollection<IWrappedNode> nodeCollection)
    : IUserAction
{
    public IWrappedNode Node { get; } = node;
    
    public bool CanExecute => true;

    public Task<IUserActionResult> Execute()
    {
        nodeCollection.Add(Node);
        return Task.FromResult(IUserActionResult.Success(new DeleteNodeAction(Node, nodeCollection)));
    }
}
