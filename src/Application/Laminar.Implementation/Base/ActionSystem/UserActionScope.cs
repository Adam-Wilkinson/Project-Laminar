using Laminar.Contracts.Base;
using Laminar.Contracts.Base.ActionSystem;
using Microsoft.Extensions.DependencyInjection;

namespace Laminar.Implementation.Base.ActionSystem;

internal class UserActionScope(
    IUserActionSimplifier[] simplifiers,
    IUserActionResolver resolver,
    IUserActionChainSimplifier chainSimplifier,
    IExceptionHandler exceptionHandler,
    IServiceProvider serviceProvider)
    : IUserActionScope, IUserActionSessionHost
{
    private readonly ActionHistory _history = new(resolver);
    private readonly List<ActionHistory> _children = [];

    public IUserActionScope CreateChild(params IUserActionSimplifier[] childSimplifiers)
    {
         var newChild = ActivatorUtilities.CreateInstance<UserActionScope>(serviceProvider, (object)childSimplifiers);
         _children.Add(newChild._history);
         return newChild;
    }

    public Task<UserActionResult> ExecuteAction(IUserAction action)
    {
        _history.AddEventAfterCursor(new ActionHistoryEvent(action, ActionHistoryTraversalType.Redo, _children));
        return NotifyIfError(_history.ShiftCursor(ActionHistoryTraversalType.Redo));
    }

    public Task<UserActionResult> Undo() 
        => NotifyIfError(_history.ExecuteNextValid(ActionHistoryTraversalType.Undo));

    public Task<UserActionResult> Redo() 
        => NotifyIfError(_history.ExecuteNextValid(ActionHistoryTraversalType.Redo));

    public IUserActionSession BeginSession() => new UserActionSession(this, resolver);

    public void Simplify(List<IUserAction> actions) => chainSimplifier.Simplify(actions, simplifiers);
    
    public void RegisterUndoAction(IUserAction action)
        => _history.RegisterCompletedEvent(new ActionHistoryEvent(action, ActionHistoryTraversalType.Undo, _children));

    private async Task<UserActionResult> NotifyIfError(Task<UserActionResult> resultTask)
    {
        var result = await resultTask;
        foreach (var exception in result.Exceptions)
        {
            await exceptionHandler.OnExceptionAsync(exception);
        }
        
        return result;
    }
}