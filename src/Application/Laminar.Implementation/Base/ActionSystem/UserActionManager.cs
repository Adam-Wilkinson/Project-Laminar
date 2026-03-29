using System.Collections.Generic;
using Laminar.Contracts.Base.ActionSystem;

namespace Laminar.Implementation.Base.ActionSystem;

internal class UserActionManager : IUserActionManager
{
    private readonly DefaultActionScope _defaultActionScope = new();
    private readonly Dictionary<IActionScope, ActionScopeInfo> _scopeInfos = new();

    public IUserActionResult ExecuteAction(IUserAction action, IActionScope? scope = null)
    {
        if (!action.CanExecute) return IUserActionResult.Failure();
        var actionResult = action.Execute();
        if (actionResult is UserActionSuccess { InverseAction: { } inverse})
        {
            GetScopeInfo(scope).UndoList.Add(inverse);
        }
        
        return actionResult;
    }

    public void Undo(IActionScope? scope)
    {
        var (undoList, redoList) = GetScopeInfo(scope);
        var successfulAction = false;
        while (!successfulAction && undoList.Count > 0)
        {
            if (undoList[^1].CanExecute && undoList[^1].Execute() is UserActionSuccess { InverseAction: { } redoAction })
            {
                redoList.Add(redoAction);
                successfulAction = true;
            }

            undoList.RemoveAt(undoList.Count - 1);
        }
    }

    public void Redo(IActionScope? scope = null)
    {
        var successfulAction = false;
        var (undoList, redoList) = GetScopeInfo(scope);
        while (!successfulAction && redoList.Count > 0)
        {
            if (redoList[^1].CanExecute && redoList[^1].Execute() is UserActionSuccess { InverseAction: { } undoAction })
            {
                undoList.Add(undoAction);
                successfulAction = true;
            }

            redoList.RemoveAt(redoList.Count - 1);
        }
    }

    private ActionScopeInfo GetScopeInfo(IActionScope? scope)
    {
        scope ??= _defaultActionScope;
        if (_scopeInfos.TryGetValue(scope, out var info))
        {
            return info;
        }

        ActionScopeInfo newScopeInfo = new([], []);
        _scopeInfos.Add(scope, newScopeInfo);
        return newScopeInfo;
    }
    
    private record ActionScopeInfo(List<IUserAction> UndoList, List<IUserAction> RedoList);
    private class DefaultActionScope : IActionScope;
}
