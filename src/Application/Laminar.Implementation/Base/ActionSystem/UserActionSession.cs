using Laminar.Contracts.Base.ActionSystem;

namespace Laminar.Implementation.Base.ActionSystem;

internal class UserActionSession(UserActionManager owner) : IUserActionSession
{
    private readonly Stack<IUserAction> _undoStack = [];
    
    public async Task<IUserActionResult> ExecuteAction(IUserAction action)
    {
        var result = await owner.ResolveExecutionAsync(action);

        if (result is UserActionSuccess { InverseAction: { } inverse })
        {
            _undoStack.Push(inverse);
        }

        return result;
    }

    public async Task Reset()
    {
        while (_undoStack.Count > 0)
        {
            await owner.ResolveExecutionAsync(_undoStack.Pop());
        }
    }
    
    public void Dispose()
    {
        if (_undoStack.Count == 0) return;

        var undoAction = new CompoundAction(_undoStack.ToArray());

        if (undoAction.Actions.Count == 0) return;
        
        owner.RegisterUndoAction(undoAction);
    }
}