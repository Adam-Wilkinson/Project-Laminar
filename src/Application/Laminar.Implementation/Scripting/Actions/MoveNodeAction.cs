using System;
using Laminar.Contracts.Base.ActionSystem;
using Laminar.Contracts.Scripting.NodeWrapping;
using Laminar.Domain.ValueObjects;

namespace Laminar.Implementation.Scripting.Actions;

public class MoveNodeAction(IWrappedNode items, Point locationDelta) : IUserAction
{
    public event EventHandler? CanExecuteChanged;
    
    public bool CanExecute => true;

    public UserActionResult Execute()
    {
        items.Location.Value += locationDelta;
        return UserActionResult.Success(new MoveNodeAction(items, -locationDelta));
    }
}