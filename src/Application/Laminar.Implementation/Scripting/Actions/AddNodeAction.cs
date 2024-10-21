using System.Collections.Generic;
using Laminar.Contracts.Base.ActionSystem;
using Laminar.Contracts.Base.UserInterface;
using Laminar.Contracts.Scripting.NodeWrapping;
using Laminar.PluginFramework.NodeSystem.Components;

namespace Laminar.Implementation.Scripting.Actions;

public class AddNodeAction : IUserAction
{
    readonly IWrappedNode _node;
    private readonly ICollection<IWrappedNode> _nodeCollection;

    public AddNodeAction(IWrappedNode node, ICollection<IWrappedNode> nodeCollection)
    {
        _node = node;
        _nodeCollection = nodeCollection;
    }

    public bool Execute()
    {
        _nodeCollection.Add(_node);
        return true;
    }

    public IUserAction GetInverse()
    {
        return new DeleteNodeAction(_node, _nodeCollection);
    }
}
