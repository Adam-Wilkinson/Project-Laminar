using Laminar.Contracts.Base.ActionSystem;
using Laminar.Contracts.Scripting.Execution;
using Laminar.Contracts.Scripting.NodeWrapping;
using Laminar.Domain.Exceptions;

namespace Laminar.Implementation.Scripting.Actions;

internal readonly struct AddNodeAction(IWrappedNode node, INodeTree nodeTree)
    : IUserAction
{
    public IWrappedNode Node { get; } = node;
    
    public bool CanExecute => !nodeTree.Nodes.Contains(Node);

    public Task<IUserActionResult> Execute()
    {
        if (nodeTree.Nodes.Contains(Node))
        {
            return Task.FromResult(IUserActionResult.Error(new NodeTreeContainsNodeException(Node)));
        }
        
        nodeTree.AddNode(Node);
        return Task.FromResult(IUserActionResult.Success(new DeleteNodeAction(Node, nodeTree)));
    }

    public override string ToString()
    {
        return $"Add Node: {Node}";
    }
}
