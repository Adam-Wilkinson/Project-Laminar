using System;
using System.Collections.Generic;
using System.Linq;
using Laminar.Contracts.Base.ActionSystem;

namespace Laminar.Implementation.Base.ActionSystem;

public class CompoundAction : IUserAction
{
    private readonly List<IUserAction> _actions;
    
    public CompoundAction(params IUserAction[] actions) : this((IEnumerable<IUserAction>)actions)
    {
    }

    public CompoundAction(IEnumerable<IUserAction> actions)
    {
        _actions = actions.ToList();
        foreach (var action in _actions)
        {
            action.CanExecuteChanged += ChildCanExecuteChanged;
        }
    }

    public void Add(IUserAction action)
    {
        _actions.Add(action);
        action.CanExecuteChanged += ChildCanExecuteChanged;
    }

    public event EventHandler? CanExecuteChanged;
    
    public bool CanExecute { get; private set; }

    public UserActionResult Execute()
    {
        if (_actions.Any(userAction => !userAction.CanExecute))
        {
            return UserActionResult.Failure();
        }
        
        var undoList = new IUserAction[_actions.Count];
        var indexInReverseList = _actions.Count;
        foreach (var userAction in _actions)
        {
            var currentResult = userAction.Execute();
            switch (currentResult.ResultType)
            {
                case UserActionResultType.Failure:
                    return UserActionResult.Failure();
                case UserActionResultType.Error:
                    return UserActionResult.Error(currentResult.Exception!);
                case UserActionResultType.Success:
                    undoList[--indexInReverseList] = currentResult.InverseAction!;
                    break;
            }
        }

        return UserActionResult.Success(new CompoundAction(undoList));
    }

    private void ChildCanExecuteChanged(object? sender, EventArgs args)
    {
        CanExecute = _actions.All(x => x.CanExecute);
        CanExecuteChanged?.Invoke(sender, args);
    }
}
