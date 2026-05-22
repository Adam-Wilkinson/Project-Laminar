using Laminar.Contracts.Base.ActionSystem;

namespace Laminar.Implementation.Base.ActionSystem;

public class CompoundAction(IEnumerable<IUserAction> actions) : IUserAction
{
    private readonly List<IUserAction> _actions = [.. actions];
    private bool _hasExecuted;
    
    public CompoundAction(params IUserAction[] actions) : this((IEnumerable<IUserAction>)actions)
    {
    }

    public void Add(IUserAction action)
    {
        if (_hasExecuted)
            throw new InvalidOperationException("Cannot modify a compound action that has already executed");
        
        _actions.Add(action);
    }
    
    public bool CanExecute => _actions.All(x => x.CanExecute);

    public async Task<IUserActionResult> Execute()
    {
        _hasExecuted = true;
        if (_actions.Any(userAction => !userAction.CanExecute))
        {
            return IUserActionResult.Invalid();
        }
        
        var executedUndoActions = new Stack<IUserAction>();

        foreach (var userAction in _actions)
        {
            var result = await userAction.Execute();

            if (result is not UserActionSuccess success)
            {
                while (executedUndoActions.Count > 0)
                {
                    await executedUndoActions.Pop().Execute();
                }

                return result;
            }

            executedUndoActions.Push(success.InverseAction);
        }

        return IUserActionResult.Success(new CompoundAction(executedUndoActions));
    }

    public bool IsInverseOf(IUserAction action)
    {
        if (action is not CompoundAction compoundAction)
        {
            return _actions.Count == 1 && _actions[0].IsInverseOf(action);
        }

        if (_actions.Count != compoundAction._actions.Count)
        {
            return false;
        }
        
        for (int i = 0; i < _actions.Count; i++)
        {
            if (!_actions[i].IsInverseOf(compoundAction._actions[compoundAction._actions.Count - 1 - i]))
            {
                return false;
            }
        }

        return true;
    }
}