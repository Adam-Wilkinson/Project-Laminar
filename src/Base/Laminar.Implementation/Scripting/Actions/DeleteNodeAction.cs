using System.Collections.Generic;
using Laminar.Contracts.Base.ActionSystem;
using Laminar.Contracts.Scripting.NodeWrapping;

namespace Laminar.Implementation.Scripting.Actions;

public class DeleteNodeAction : IUserAction
{
    readonly IWrappedNode _node;
    private readonly ICollection<IWrappedNode> _nodeCollection;

    public DeleteNodeAction(IWrappedNode node, ICollection<IWrappedNode> nodeCollection)
    {
        _node = node;
        _nodeCollection = nodeCollection;
    }

    public bool Execute()
    {
        return _nodeCollection.Remove(_node);
    }

    public IUserAction GetInverse()
    {
        return new AddNodeAction(_node, _nodeCollection);
    }
}
