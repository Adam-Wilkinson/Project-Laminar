using Laminar.Contracts.Base.ActionSystem;

namespace Laminar.Implementation.Base.ActionSystem;

internal class UserActionSession(IUserActionSessionHost owner, IUserActionResolver resolver) : IUserActionSession
{
    private readonly Stack<IUserAction> _undoStack = [];

    public async Task Pop()
    {
        await resolver.Resolve(_undoStack.Pop());
    }

    public async Task<IUserActionExecutionOutcome> ExecuteAction(IUserAction action)
    {
        var result = await resolver.Resolve(action);

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
        if (undoList.Count == 1)
        {
            owner.RegisterUndoAction(undoList[0]);
            return;
        }
        
        owner.RegisterUndoAction(new CompoundAction(undoList));
    }
}