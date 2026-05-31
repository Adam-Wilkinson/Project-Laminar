using Laminar.Contracts.Base.ActionSystem;

namespace Laminar.Implementation.Base.ActionSystem;

internal class UserActionSession(IUserActionSessionHost owner) : IUserActionSession
{
    private readonly Stack<IUserAction> _undoStack = [];

    public async Task Pop()
    {
        await owner.ResolveExecutionAsync(_undoStack.Pop());
    }

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
            await Pop();
        }
    }
    
    public void Dispose()
    {
        if (_undoStack.Count == 0) return;

        var undoList = _undoStack.ToList();
        owner.Simplify(undoList);
        if (undoList.Count == 0) return;
        owner.RegisterUndoAction(new CompoundAction(undoList));
    }
}