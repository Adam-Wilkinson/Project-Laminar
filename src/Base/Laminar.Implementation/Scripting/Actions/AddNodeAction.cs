using Laminar.Contracts.Base.ActionSystem;
using Laminar.Contracts.Scripting.NodeWrapping;
using Laminar.Domain.Notification;

namespace Laminar.Implementation.Scripting.Actions;

public class AddNodeAction : IUserAction
{
    readonly IWrappedNode _node;
    private readonly IObservableCollection<IWrappedNode> _nodeCollection;

    public AddNodeAction(IWrappedNode node, IObservableCollection<IWrappedNode> nodeCollection)
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
