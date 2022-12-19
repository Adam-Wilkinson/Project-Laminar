using Laminar.Contracts.ActionSystem;
using Laminar.Contracts.NodeSystem;
using Laminar.Domain.ValueObjects;

namespace Laminar.Core.ScriptEditor.Actions;

public class MoveNodeAction : IUserAction
{
    readonly INodeWrapper _items;
    readonly Point _locationDelta;

    public MoveNodeAction(INodeWrapper items, Point locationDelta)
    {
        _items = items;
        _locationDelta = locationDelta;
    }

    public bool Execute()
    {
        if (_locationDelta.X == 0 && _locationDelta.Y == 0)
        {
            return false;
        }

        _items.Location.Value += _locationDelta;
        return true;
    }

    public IUserAction GetInverse()
    {
        return new MoveNodeAction(_items, -_locationDelta);
    }
}