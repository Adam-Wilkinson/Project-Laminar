using Laminar.Contracts.Base.ActionSystem;
using Laminar.Contracts.Base;
using Laminar.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace Laminar.Implementation.Base.ActionSystem;

internal partial class UserActionManager(
    IEnumerable<IUserActionErrorResolver> errorResolvers,
    IExceptionHandler exceptionHandler,
    IUserActionChainSimplifier chainSimplifier,
    ILogger<UserActionManager> logger) 
    : IUserActionManager, IUserActionSessionHost
{
    private readonly Stack<IUserAction> _undoList = [];
    private readonly Stack<IUserAction> _redoList = [];
    private readonly HashSet<IUserActionSimplifier> _simplifiers = [];
    private readonly IncrementalIdentifier<UserActionManager> _id = IncrementalIdentifier.Next<UserActionManager>();

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
        LogUndoRequested(_id);
        if (_undoList.Count <= 0) return IUserActionResult.Invalid();
        IUserActionResult actionResult = await ResolveExecutionAsync(_undoList.Pop());

        if (actionResult is UserActionSuccess { InverseAction: { } inverse })
        {
            _redoList.Push(inverse);
            LogRedoSuccessful(_id, inverse, _undoList.Count, _redoList.Count);
        }

        return actionResult;
    }

    public async Task<IUserActionResult> Redo()
    {
        LogRedoRequested(_id);
        if (_redoList.Count <= 0) return IUserActionResult.Invalid();
        IUserActionResult actionResult = await ResolveExecutionAsync(_redoList.Pop());

        if (actionResult is UserActionSuccess { InverseAction: { } inverse })
        {
            RegisterUndoAction(inverse);
        }

        return actionResult;
    }
    
    public void RegisterUndoAction(IUserAction action)
    {
        _undoList.Push(action);
        LogUndoSuccessful(_id, action, _undoList.Count, _redoList.Count);
    }

    public IUserActionSession BeginSession() => new UserActionSession(this);

    public IUserActionManager RegisterSimplifier(IUserActionSimplifier simplifier)
    {
        _simplifiers.Add(simplifier);
        return this;
    }

    public void Simplify(List<IUserAction> actions) => chainSimplifier.Simplify(actions, _simplifiers);

    public async Task<IUserActionResult> ResolveExecutionAsync(IUserAction action)
    {
        LogResolvingAction(_id, action);
        if (!action.CanExecute) return IUserActionResult.Invalid();
        var result = await action.Execute();
        if (result is UserActionSuccess success) return success;

        if (result is UserActionAlternative { AlternativeAction: { } alternative })
        {
            return await ResolveExecutionAsync(alternative);
        }
        
        foreach (var errorResolver in errorResolvers)
        {
            var resolution = await errorResolver.TryResolve(result);
            switch (resolution)
            {
                case UserActionCancelledResolution:
                    (result as IResolvableError)?.OnCancelled?.Invoke();
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

    [LoggerMessage(LogLevel.Trace, "[UAM {id}]: Undo action requested")]
    partial void LogUndoRequested(IncrementalIdentifier<UserActionManager> id);

    [LoggerMessage(LogLevel.Trace, "[UAM {id}]: Undo completed successfully. '{inverse}' registered as redo action (Undo height: {undoHeight}, Redo height: {redoHeight})")]
    partial void LogRedoSuccessful(IncrementalIdentifier<UserActionManager> id, IUserAction inverse, int undoHeight, int redoHeight);

    [LoggerMessage(LogLevel.Trace, "[UAM {id}]: Redo action requested")]
    partial void LogRedoRequested(IncrementalIdentifier<UserActionManager> id);

    [LoggerMessage(LogLevel.Trace, "[UAM {id}]: Action completed successfully. '{action}' registered as undo action (Undo height: {undoHeight}, Redo height: {redoHeight})")]
    partial void LogUndoSuccessful(IncrementalIdentifier<UserActionManager> id, IUserAction action, int undoHeight, int redoHeight);

    [LoggerMessage(LogLevel.Trace, "[UAM {id}]: Resolving action: '{action}'")]
    partial void LogResolvingAction(IncrementalIdentifier<UserActionManager> id, IUserAction action);
}
