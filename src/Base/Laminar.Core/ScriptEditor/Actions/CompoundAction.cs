using System.Collections.Generic;
using System.Linq;
using Laminar.Contracts.ActionSystem;

namespace Laminar.Core.ScriptEditor.Actions;

public class CompoundAction : IUserAction
{
    readonly List<IUserAction> _actions;

    public CompoundAction(params IUserAction[] actions)
    {
        _actions = actions.ToList();
    }

    public void Add(IUserAction action)
    {
        _actions.Add(action);
    }

    public bool Execute()
    {
        foreach (IUserAction userAction in _actions)
        {
            userAction.Execute();
        }

        return true;
    }

    public IUserAction GetInverse()
    {
        IUserAction[] inverseList = new IUserAction[_actions.Count];
        for (int i = 0; i < _actions.Count; i++)
        {
            inverseList[_actions.Count - 1 - i] = _actions[i].GetInverse();
        }

        return new CompoundAction(inverseList);
    }
}
