namespace Laminar.Contracts.Base.ActionSystem;

public interface IUserActionExecutionOutcome
{
    public static IUserActionExecutionOutcome Success(IUserAction inverse) => new UserActionSuccess(inverse);
    
    public static IUserActionExecutionOutcome Success<T>(T returnValue, IUserAction inverseAction) => new UserActionSuccess<T>(returnValue, inverseAction);
    
    public static IUserActionExecutionOutcome Ineffectual() => new UserActionIneffectual();
    
    public static IUserActionExecutionOutcome Error(Exception exception) =>  new UserActionError(exception);

    public static IUserActionExecutionOutcome Cancelled() => new UserActionCancelled();

    public static IUserActionExecutionOutcome Alternative(IUserAction alternative) => new UserActionAlternative(alternative);
}

public interface IResolvableError : IUserActionExecutionOutcome
{
    public Exception Exception { get; }
    
    public Action? OnCancelled { get; init; }
}

public class ResolvableError<TParam> : IResolvableError
{
    public required Func<TParam, IUserActionErrorResolution> Resolve { get; init; }

    public Action? OnCancelled { get; init; }

    public required Exception Exception { get; init; }
}

public record UserActionSuccess<T>(T ReturnValue, IUserAction InverseAction) : UserActionSuccess(InverseAction)
{
    public override UserActionResult ToActionResult() => new UserActionValueResult<T>(ReturnValue);
}

public record UserActionSuccess(IUserAction InverseAction) : IUserActionExecutionOutcome
{
    public virtual UserActionResult ToActionResult() => UserActionResult.Success();
}

public record UserActionAlternative(IUserAction AlternativeAction) : IUserActionExecutionOutcome;

public record UserActionIneffectual : IUserActionExecutionOutcome;

public record UserActionError(Exception Exception) : IUserActionExecutionOutcome;

public record UserActionCancelled : IUserActionExecutionOutcome;