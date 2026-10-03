using Laminar.Contracts.Base.ActionSystem;

namespace Laminar.Implementation.Base.ActionSystem;

public class UserActionResolver(IEnumerable<IUserActionErrorResolver> errorResolvers) : IUserActionResolver
{
    private readonly List<IUserActionErrorResolver> _errorResolvers = [.. errorResolvers];
    
    public async Task<IUserActionExecutionOutcome> Resolve(IUserAction action)
    {
        IUserActionExecutionOutcome executionOutcome;
        try
        {
            executionOutcome = await action.Execute();
        }
        catch (Exception ex)
        {
            return IUserActionExecutionOutcome.Error(ex);
        }
    
        if (executionOutcome is UserActionSuccess success) return success;

        if (executionOutcome is UserActionAlternative { AlternativeAction: { } alternative })
        {
            return await Resolve(alternative);
        }
    
        foreach (var errorResolver in _errorResolvers)
        {
            var resolution = await errorResolver.TryResolve(executionOutcome);
            switch (resolution)
            {
                case UserActionCancelledResolution:
                    (executionOutcome as IResolvableError)?.OnCancelled?.Invoke();
                    return IUserActionExecutionOutcome.Cancelled();
                case AlternativeActionFound { AlternativeAction: { } alternativeAction }:
                    return await Resolve(alternativeAction);
                default:
                    continue;
            }
        }

        if (executionOutcome is IResolvableError unresolvedError)
        {
            executionOutcome = IUserActionExecutionOutcome.Error(unresolvedError.Exception);
        }

        return executionOutcome;
    }
}