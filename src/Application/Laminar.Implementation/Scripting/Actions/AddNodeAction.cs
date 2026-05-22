using Laminar.Contracts.Base.ActionSystem;
using Laminar.Contracts.Scripting.NodeWrapping;

namespace Laminar.Implementation.Scripting.Actions;

public class AddNodeAction(IWrappedNode node, ICollection<IWrappedNode> nodeCollection)
    : IUserAction
{
    public IWrappedNode Node { get; } = node;
    
    public bool CanExecute => true;

    public Task<IUserActionResult> Execute()
    {
        nodeCollection.Add(Node);
        return Task.FromResult(IUserActionResult.Success(new DeleteNodeAction(Node, nodeCollection)));
    }

    public bool IsInverseOf(IUserAction action) => action is DeleteNodeAction deleteAction && deleteAction.Node == Node;
}
