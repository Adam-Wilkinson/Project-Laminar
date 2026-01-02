using System.Collections.Generic;
using Laminar.Contracts.Base.ActionSystem;

namespace Laminar.Implementation.Base.ActionSystem;

internal class UserActionManager : IUserActionManager
{
    private readonly List<IUserAction> _undoList = [];
    private readonly List<IUserAction> _redoList = [];

    public bool ExecuteAction(IUserAction action)
    {
        if (!action.CanExecute) return false;
        if (action.Execute() is not { } undoAction)
        {
            return false;
        }
        _undoList.Add(undoAction);
        return true;
    }

    public void Undo()
    {
        var successfulAction = false;
        while (!successfulAction && _undoList.Count > 0)
        {
            if (_undoList[^1].CanExecute && _undoList[^1].Execute() is { } redoAction)
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
            if (_redoList[^1].CanExecute && _redoList[^1].Execute() is { } undoAction)
            {
                _undoList.Add(undoAction);
                successfulAction = true;
            }

            _redoList.RemoveAt(_redoList.Count - 1);
        }
    }
}
