using Laminar.Contracts.Base.ActionSystem;

namespace Laminar.Implementation.Base.ActionSystem;

public class CompoundAction(IEnumerable<IUserAction> actions) : IUserAction
{
    private readonly List<IUserAction> _actions = [.. actions];
    
    public CompoundAction(params IUserAction[] actions) : this((IEnumerable<IUserAction>)actions)
    {
    }

    public IReadOnlyList<IUserAction> Actions => _actions;

    public bool CanExecute => _actions.All(x => x.CanExecute);

    public async Task<IUserActionResult> Execute()
    {
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
}