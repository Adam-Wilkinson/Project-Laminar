using Laminar.Contracts.Base.ActionSystem;

namespace Laminar.Implementation.Base.ActionSystem;

public class UserActionChainSimplifier : IUserActionChainSimplifier
{
    public bool Simplify(List<IUserAction> chain, ICollection<IUserActionSimplifier> simplifiers)
    {
        if (chain.Count is 0) return false;
        
        var newList = Flatten(chain).ToList();
        bool singleItemFlatten = chain[0] != newList[0];
        chain.Clear();
        chain.AddRange(newList);
        
        if (chain.Count is 1) return singleItemFlatten;
        
        bool actionsListModifiedThisRun = true;
        bool actionsListModified = false;
        while (actionsListModifiedThisRun && chain.Count >= 2)
        {
            actionsListModifiedThisRun = false;
            for (int i = 0; i < chain.Count - 1; i++)
            {
                switch (Simplify(chain[i], chain[i + 1], simplifiers))
                {
                    case OverridesAction:
                        chain.RemoveAt(i);
                        actionsListModifiedThisRun = true;
                        break;
                    case ReversesAction:
                        chain.RemoveRange(i, 2);
                        actionsListModifiedThisRun = true;
                        break;
                    case NewEffectiveAction { NewAction: { } newAction }:
                        chain.RemoveAt(i);
                        chain[i] = newAction;
                        actionsListModifiedThisRun = true;
                        break;
                }

                if (actionsListModifiedThisRun)
                {
                    actionsListModified = true;
                    break;
                }
            }
        }

        return actionsListModified;
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

    private static IUserActionSimplification Simplify(IUserAction first, IUserAction second, IEnumerable<IUserActionSimplifier> simplifiers)
    {
        foreach (var simplifier in simplifiers)
        {
            var simplification = simplifier.Simplify(first, second); 
            if (simplification is not NoSimplification)
            {
                return simplification;
            }
        }

        return IUserActionSimplification.None();
    }
}