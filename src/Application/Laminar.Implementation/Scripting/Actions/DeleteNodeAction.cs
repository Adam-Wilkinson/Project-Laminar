using Laminar.Contracts.Base.ActionSystem;
using Laminar.Contracts.Scripting.Execution;
using Laminar.Contracts.Scripting.NodeWrapping;
using Laminar.Domain.Exceptions;

namespace Laminar.Implementation.Scripting.Actions;

internal readonly struct DeleteNodeAction(INodeContainer nodeContainer, IWritableNodeTree writableNodeTree) : IUserAction
{
    public INodeContainer NodeContainer { get; } = nodeContainer;
    
    public bool CanExecute { get; } = writableNodeTree.Nodes.Contains(nodeContainer);

    public Task<IUserActionResult> Execute()
    {
        if (!writableNodeTree.DeleteNode(NodeContainer))
        {
            return Task.FromResult(IUserActionResult.Error(new NodeTreeDoesNotContainNodeException(NodeContainer)));
        }
        
        return Task.FromResult(IUserActionResult.Success(new AddNodeAction(NodeContainer, writableNodeTree)));
    }
    
    public override string ToString() => $"Delete Node: {NodeContainer}";
}
