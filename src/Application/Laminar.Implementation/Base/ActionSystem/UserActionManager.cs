using Laminar.Contracts.Base.ActionSystem;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Laminar.Implementation.Base.ActionSystem;

internal class UserActionManager(IEnumerable<IUserActionErrorResolver> resolvers) : IUserActionManager
{
    private readonly Stack<IUserAction> _undoList = [];
    private readonly Stack<IUserAction> _redoList = [];
    
    public IUserActionResult ExecuteAction(IUserAction action)
    {
        IUserActionResult actionResult = ResolveExecution(action);

        if (actionResult is UserActionSuccess { InverseAction: { } inverse })
        {
            _undoList.Push(inverse);
        }

        return actionResult;
    }

    public async Task<IUserActionResult> ExecuteActionAsync(IUserAction action)
    {
        IUserActionResult actionResult = await ResolveExecutionAsync(action);

        if (actionResult is UserActionSuccess { InverseAction: { } inverse })
        {
            _undoList.Push(inverse);
        }

        return actionResult;
    }

    public IUserActionResult Undo()
    {
        IUserActionResult actionResult = ResolveExecution(_undoList.Pop());

        if (actionResult is UserActionSuccess { InverseAction: { } inverse })
        {
            _redoList.Push(inverse);
        }

        return actionResult;
    }

    public async Task<IUserActionResult> UndoAsync()
    {
        IUserActionResult actionResult = await ResolveExecutionAsync(_undoList.Pop());

        if (actionResult is UserActionSuccess { InverseAction: { } inverse })
        {
            _redoList.Push(inverse);
        }

        return actionResult;
    }

    public IUserActionResult Redo()
    {
        IUserActionResult actionResult = ResolveExecution(_redoList.Pop());

        if (actionResult is UserActionSuccess { InverseAction: { } inverse })
        {
            _undoList.Push(inverse);
        }

        return actionResult;
    }

    public async Task<IUserActionResult> RedoAsync()
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
        var result = action.Execute();
        if (result is not UserActionError error) return result;

        foreach (var resolver in resolvers)
        {
            var resolution = await resolver.TryResolveAsync(error);
            switch (resolution)
            {
                case CancelledByUser:
                    return IUserActionResult.Cancelled();
                case AlternativeActionFound { AlternativeAction: { } alternativeAction }:
                    if (alternativeAction.CanExecute)
                    {
                        var alternative = await ResolveExecutionAsync(alternativeAction);
                        return alternative;
                    }
                    continue;
                default:
                    continue;
            }
        }

        return error;
    }
    private IUserActionResult ResolveExecution(IUserAction action)
    {
        if (!action.CanExecute) return IUserActionResult.Invalid();
        var result = action.Execute();
        if (result is not UserActionError error) return result;

        foreach (var resolver in resolvers)
        {
            switch (resolver.TryResolve(error))
            {
                case CancelledByUser:
                    return IUserActionResult.Cancelled();
                case AlternativeActionFound { AlternativeAction: { } alternativeAction }:
                    if (alternativeAction.CanExecute)
                    {
                        return ResolveExecution(alternativeAction);
                    }
                    continue;
                default:
                    continue;
            }
        }

        return error;
    }
}
