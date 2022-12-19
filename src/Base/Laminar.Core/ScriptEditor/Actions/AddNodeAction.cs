using Laminar.Contracts.ActionSystem;
using Laminar.Contracts.NodeSystem;
using Laminar.Domain;

namespace Laminar.Core.ScriptEditor.Actions;

public class AddNodeAction : IUserAction
{
    readonly INodeWrapper _node;
    private readonly IObservableCollection<INodeWrapper> _nodeCollection;

    public AddNodeAction(INodeWrapper node, IObservableCollection<INodeWrapper> nodeCollection)
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
