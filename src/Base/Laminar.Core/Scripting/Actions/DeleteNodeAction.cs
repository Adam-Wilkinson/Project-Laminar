using Laminar.Contracts.Base.ActionSystem;
using Laminar.Contracts.Scripting.NodeWrapping;
using Laminar.Domain.Notification;

namespace Laminar.Implementation.Scripting.Actions;

public class DeleteNodeAction : IUserAction
{
    readonly IWrappedNode _node;
    private readonly IObservableCollection<IWrappedNode> _nodeCollection;

    public DeleteNodeAction(IWrappedNode node, IObservableCollection<IWrappedNode> nodeCollection)
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
