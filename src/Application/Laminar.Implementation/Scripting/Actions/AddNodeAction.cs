using Laminar.Contracts.Base.ActionSystem;
using Laminar.Contracts.Scripting.Execution;
using Laminar.Contracts.Scripting.NodeWrapping;
using Laminar.Domain.Exceptions;

namespace Laminar.Implementation.Scripting.Actions;

internal readonly struct AddNodeAction(IWrappedNode node, IWritableNodeTree writableNodeTree)
    : IUserAction
{
    public IWrappedNode Node { get; } = node;
    
    public bool CanExecute => !writableNodeTree.Nodes.Contains(Node);

    public Task<IUserActionResult> Execute()
    {
        if (writableNodeTree.Nodes.Contains(Node))
        {
            return Task.FromResult(IUserActionResult.Error(new NodeTreeContainsNodeException(Node)));
        }
        
        writableNodeTree.AddNode(Node);
        return Task.FromResult(IUserActionResult.Success(new DeleteNodeAction(Node, writableNodeTree)));
    }

    public override string ToString()
    {
        return $"Add Node: {Node}";
    }
}
