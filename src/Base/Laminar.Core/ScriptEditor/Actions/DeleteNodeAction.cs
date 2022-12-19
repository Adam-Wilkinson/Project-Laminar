using Laminar.Contracts.ActionSystem;
using Laminar.Contracts.NodeSystem;
using Laminar.Domain;

namespace Laminar.Core.ScriptEditor.Actions;

public class DeleteNodeAction : IUserAction
{
    readonly INodeWrapper _node;
    private readonly IObservableCollection<INodeWrapper> _nodeCollection;

    public DeleteNodeAction(INodeWrapper node, IObservableCollection<INodeWrapper> nodeCollection)
    {
        _node = node;
        _nodeCollection = nodeCollection;
    }

    public bool Execute()
    {
        _nodeCollection.Remove(_node);
        _ = _node.NameRow.Display[null];
        foreach (var row in _node.Fields)
        {
            _ = row.Display[null];
        }
        return true;
    }

    public IUserAction GetInverse()
    {
        return new AddNodeAction(_node, _nodeCollection);
    }
}
