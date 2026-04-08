using System.Collections.Generic;
using Laminar.Contracts.Base.ActionSystem;

namespace Laminar.Implementation.Base.ActionSystem;

internal class UserActionManager : IUserActionManager
{
    private readonly List<IUserAction> _undoList = [];
    private readonly List<IUserAction> _redoList = [];
    
    public IUserActionResult ExecuteAction(IUserAction action)
    {
        if (!action.CanExecute) return IUserActionResult.Failure();
        var actionResult = action.Execute();
        if (actionResult is UserActionSuccess { InverseAction: { } inverse})
        {
            _undoList.Add(inverse);
        }
        
        return actionResult;
    }

    public void Undo()
    {
        var successfulAction = false;
        while (!successfulAction && _undoList.Count > 0)
        {
            if (_undoList[^1].CanExecute && _undoList[^1].Execute() is UserActionSuccess { InverseAction: { } redoAction })
            {
                _redoList.Add(redoAction);
                successfulAction = true;
            }

            _undoList.RemoveAt(_undoList.Count - 1);
        }
    }

    public void Redo()
    {
        var successfulAction = false;
        while (!successfulAction && _redoList.Count > 0)
        {
            if (_redoList[^1].CanExecute && _redoList[^1].Execute() is UserActionSuccess { InverseAction: { } undoAction })
            {
                _undoList.Add(undoAction);
                successfulAction = true;
            }

            _redoList.RemoveAt(_redoList.Count - 1);
        }
    }
}
