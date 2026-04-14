using Laminar.Contracts.Base.ActionSystem;
using Laminar.Contracts.Scripting.NodeWrapping;
using Laminar.Domain.ValueObjects;
using System;
using System.Threading.Tasks;

namespace Laminar.Implementation.Scripting.Actions;

public class MoveNodeAction(IWrappedNode items, Point locationDelta) : IUserAction
{
    public event EventHandler? CanExecuteChanged { add { } remove { } }

    public bool CanExecute => true;

    public Task<IUserActionResult> Execute()
    {
        items.Location.Value += locationDelta;
        return Task.FromResult(IUserActionResult.Success(new MoveNodeAction(items, -locationDelta)));
    }
}