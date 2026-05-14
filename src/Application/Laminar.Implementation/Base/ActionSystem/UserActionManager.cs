using Laminar.Contracts.Base.ActionSystem;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Laminar.Contracts.Base;
using Laminar.Domain;

namespace Laminar.Implementation.Base.ActionSystem;

internal class UserActionManager(
    IEnumerable<IUserActionErrorResolver> errorResolvers,
    IExceptionHandler exceptionHandler) : IUserActionManager
{
    private readonly Stack<IUserAction> _undoList = [];
    private readonly Stack<IUserAction> _redoList = [];

    public async Task<IUserActionResult> ExecuteAction(IUserAction action)
    {
        IUserActionResult actionResult = await ResolveExecutionAsync(action);

        if (actionResult is UserActionSuccess { InverseAction: { } inverse })
        {
            RegisterUndoAction(inverse);
        }

        return actionResult;
    }

    public async Task<IUserActionResult> Undo()
    {
        if (_undoList.Count <= 0) return IUserActionResult.Invalid();
        IUserActionResult actionResult = await ResolveExecutionAsync(_undoList.Pop());

        if (actionResult is UserActionSuccess { InverseAction: { } inverse })
        {
            _redoList.Push(inverse);
        }

        return actionResult;
    }

    public async Task<IUserActionResult> Redo()
    {
        if (_redoList.Count <= 0) return IUserActionResult.Invalid();
        IUserActionResult actionResult = await ResolveExecutionAsync(_redoList.Pop());

        if (actionResult is UserActionSuccess { InverseAction: { } inverse })
        {
            _undoList.Push(inverse);
        }

        return actionResult;
    }
    
    public void RegisterUndoAction(IUserAction action)
    {
        _undoList.Push(action);
    }

    public IUserActionSession BeginSession() => new UserActionSession(this);

    public async Task<IUserActionResult> ResolveExecutionAsync(IUserAction action)
    {
        if (!action.CanExecute) return IUserActionResult.Invalid();
        var result = await action.Execute();
        if (result is UserActionSuccess success) return success;

        foreach (var errorResolver in errorResolvers)
        {
            var resolution = await errorResolver.TryResolve(result);
            switch (resolution)
            {
                case UserActionCancelledResolution:
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

        if (result is IResolvableError unresolvedError)
        {
            result = new UserActionError(unresolvedError.Exception);
        }

        if (result is UserActionError error)
        {
            await exceptionHandler.OnExceptionAsync(error.Exception);
        }

        return result;
    }
}
