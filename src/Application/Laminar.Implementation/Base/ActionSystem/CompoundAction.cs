using Laminar.Contracts.Base.ActionSystem;
using Laminar.Domain.Extensions;

namespace Laminar.Implementation.Base.ActionSystem;

public class CompoundAction : IUserAction
{
    private readonly List<IUserAction> _actions;
    
    public CompoundAction(params IUserAction[] actions) : this((IEnumerable<IUserAction>)actions)
    {
    }

    public CompoundAction(IEnumerable<IUserAction> actions)
    {
        _actions = [.. actions];
        Simplify(_actions);
    }

    private CompoundAction(List<IUserAction> actions, bool skipSimplification)
    {
        _actions = actions;
        if (!skipSimplification)
        {
            Simplify(_actions);
        }
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

    public IUserActionSimplification GetSimplificationAfter(IUserAction previousAction)
    {
        IEnumerable<IUserAction> totalActionEnumerable = previousAction is CompoundAction previousCompoundAction
            ? previousCompoundAction._actions.Concat(_actions)
            : previousAction.Yield().Concat(_actions);

        var totalAction = totalActionEnumerable.ToList();
        Simplify(totalAction);
        return totalAction.Count switch
        {
            0 => IUserActionSimplification.Undoes(),
            1 => IUserActionSimplification.NewEffectiveAction(totalAction[0]),
            _ => IUserActionSimplification.NewEffectiveAction(new CompoundAction(totalAction, skipSimplification: true))
        };
    }

    private static void Simplify(List<IUserAction> actionsList)
    {
        if (actionsList.Count is 0) return;

        var newList = Flatten(actionsList).ToList();
        actionsList.Clear();
        actionsList.AddRange(newList);
        
        if (actionsList.Count is 1) return;
        
        bool actionsListModified = true;
        while (actionsListModified && actionsList.Count >= 2)
        {
            actionsListModified = false;
            for (int i = 0; i < actionsList.Count - 1; i++)
            {
                switch (actionsList[i + 1].GetSimplificationAfter(actionsList[i]))
                {
                    case OverridesAction:
                        actionsList.RemoveAt(i);
                        actionsListModified = true;
                        break;
                    case ReversesAction:
                        actionsList.RemoveRange(i, 2);
                        actionsListModified = true;
                        break;
                    case NewEffectiveAction { NewAction: { } newAction }:
                        actionsList.RemoveAt(i);
                        actionsList[i] = newAction;
                        actionsListModified = true;
                        break;
                }

                if (actionsListModified) break;
            }
        }
    }

    private static IEnumerable<IUserAction> Flatten(IEnumerable<IUserAction> actions)
    {
        foreach (var action in actions)
        {
            if (action is not CompoundAction compound)
            {
                yield return action;
                continue;
            }

            foreach (var child in Flatten(compound.Actions))
            {
                yield return child;
            }
        }
    }
}