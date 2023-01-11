using Laminar.Contracts.Base.ActionSystem;
using Laminar.Contracts.Scripting.NodeWrapping;
using Laminar.Domain.ValueObjects;

namespace Laminar.Implementation.Scripting.Actions;

public class MoveNodeAction : IUserAction
{
    readonly IWrappedNode _items;
    readonly Point _locationDelta;

    public MoveNodeAction(IWrappedNode items, Point locationDelta)
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