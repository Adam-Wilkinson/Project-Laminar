using Laminar.Contracts.Base.ActionSystem;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Laminar.Implementation.Base.ActionSystem;

internal class UserActionManager(IEnumerable<IUserActionErrorResolver> errorResolvers) : IUserActionManager
{
    private readonly Stack<IUserAction> _undoList = [];
    private readonly Stack<IUserAction> _redoList = [];

    public async Task<IUserActionResult> ExecuteAction(IUserAction action)
    {
        IUserActionResult actionResult = await ResolveExecutionAsync(action);

        if (actionResult is UserActionSuccess { InverseAction: { } inverse })
        {
            _undoList.Push(inverse);
        }

        return actionResult;
    }

    public async Task<IUserActionResult> Undo()
    {
        IUserActionResult actionResult = await ResolveExecutionAsync(_undoList.Pop());

        if (actionResult is UserActionSuccess { InverseAction: { } inverse })
        {
            _redoList.Push(inverse);
        }

        return actionResult;
    }

    public async Task<IUserActionResult> Redo()
    {
        IUserActionResult actionResult = await ResolveExecutionAsync(_redoList.Pop());

        if (actionResult is UserActionSuccess { InverseAction: { } inverse })
        {
            _undoList.Push(inverse);
        }

        return actionResult;
    }

    private async Task<IUserActionResult> ResolveExecutionAsync(IUserAction action)
    {
        if (!action.CanExecute) return IUserActionResult.Invalid();
        var result = await action.Execute();
        if (result is not UserActionError error) return result;

        foreach (var errorResolver in errorResolvers)
        {
            var resolution = await errorResolver.TryResolve(action, error);
            switch (resolution)
            {
                case CancelledByUser:
                    return IUserActionResult.Cancelled();
                case AlternativeActionFound { AlternativeAction: { } alternativeAction }:
                    if (alternativeAction.CanExecute)
                    {
                        return await ResolveExecutionAsync(alternativeAction);
                    }
                    continue;
                default:
                    continue;
            }
        }

        return error;
    }
}
