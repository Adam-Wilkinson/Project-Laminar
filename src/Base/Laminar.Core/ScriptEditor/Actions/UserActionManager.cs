using System.Collections.Generic;
using Laminar.Contracts.ActionSystem;

namespace Laminar.Core.ScriptEditor.Actions;

internal class UserActionManager : IUserActionManager
{
    readonly List<IUserAction> _undoList = new();
    readonly List<IUserAction> _redoList = new();

    int _compoundNestDepth = 0;
    CompoundAction? _currentCompoundAction;

    public bool ExecuteAction(IUserAction action)
    {
        if (action.Execute())
        {
            if (_compoundNestDepth > 0)
            {
                _currentCompoundAction.Add(action);
            }
            else
            {
                _undoList.Add(action.GetInverse());
            }
            return true;
        }

        return false;
    }

    public void Undo()
    {
        bool successfulAction = false;
        while (!successfulAction && _undoList.Count > 0)
        {
            successfulAction = _undoList[^1].Execute();
            if (successfulAction)
            {
                _redoList.Add(_undoList[^1].GetInverse());
            }

            _undoList.RemoveAt(_undoList.Count - 1);
        }
    }

    public void Redo()
    {
        bool successfulAction = false;
        while (!successfulAction && _redoList.Count > 0)
        {
            successfulAction = _redoList[^1].Execute();
            if (successfulAction)
            {
                _undoList.Add(_redoList[^1].GetInverse());
            }

            _redoList.RemoveAt(_redoList.Count - 1);
        }
    }

    public void BeginCompoundAction()
    {
        if (_compoundNestDepth == 0)
        {
            _currentCompoundAction = new();
        }

        _compoundNestDepth++;
    }

    public void EndCompoundAction()
    {
        _compoundNestDepth--;
        if (_compoundNestDepth == 0)
        {
            _undoList.Add(_currentCompoundAction.GetInverse());
            _currentCompoundAction = null;
        }
    }

    public void ResetCompountAction()
    {
        _currentCompoundAction.GetInverse().Execute();
        _currentCompoundAction = new();
    }
}
