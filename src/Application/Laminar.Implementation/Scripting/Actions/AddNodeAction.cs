using Laminar.Contracts.Base.ActionSystem;
using Laminar.Contracts.Scripting.Execution;
using Laminar.Contracts.Scripting.NodeWrapping;
using Laminar.Domain.Exceptions;

namespace Laminar.Implementation.Scripting.Actions;

internal readonly struct AddNodeAction(INodeContainer nodeContainer, IWritableNodeTree writableNodeTree)
    : IUserAction
{
    public INodeContainer NodeContainer { get; } = nodeContainer;
    
    public bool CanExecute => !writableNodeTree.Nodes.Contains(NodeContainer);

    public Task<IUserActionResult> Execute()
    {
        if (writableNodeTree.Nodes.Contains(NodeContainer))
        {
            return Task.FromResult(IUserActionResult.Error(new NodeTreeContainsNodeException(NodeContainer)));
        }
        
        writableNodeTree.AddNode(NodeContainer);
        return Task.FromResult(IUserActionResult.Success(new DeleteNodeAction(NodeContainer, writableNodeTree)));
    }

    public override string ToString()
    {
        return $"Add Node: {NodeContainer}";
    }
}
