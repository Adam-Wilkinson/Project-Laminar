using Laminar.Contracts.Base.ActionSystem;
using Laminar.Contracts.Scripting;
using Laminar.Contracts.Scripting.NodeWrapping;
using Laminar.Domain.Exceptions;

namespace Laminar.Implementation.Scripting.Actions;

internal readonly struct AddNodeAction(INodeContainer nodeContainer, INodeCollection nodes) : IUserAction
{
    public INodeContainer NodeContainer { get; } = nodeContainer;
    
    public bool CanExecute => !nodes.ContainsNode(NodeContainer);

    public Task<IUserActionResult> Execute()
    {
        if (nodes.ContainsNode(NodeContainer))
        {
            return Task.FromResult(IUserActionResult.Error(new NodeTreeContainsNodeException(NodeContainer)));
        }
        
        nodes.AddNode(NodeContainer);
        return Task.FromResult(IUserActionResult.Success(new DeleteNodeAction(NodeContainer, nodes)));
    }

    public override string ToString()
    {
        return $"Add Node: {NodeContainer}";
    }
}
