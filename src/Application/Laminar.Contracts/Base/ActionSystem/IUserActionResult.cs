namespace Laminar.Contracts.Base.ActionSystem;

public interface IUserActionResult
{
    public static IUserActionResult Success(IUserAction inverse) => new UserActionSuccess(inverse);
    
    public static IUserActionResult Success<T>(T returnValue, IUserAction inverseAction) => new UserActionSuccess<T>(returnValue, inverseAction);
    
    public static IUserActionResult Invalid() => new UserActionInvalid();
    
    public static IUserActionResult Error(Exception exception) =>  new UserActionError(exception);

    public static IUserActionResult Cancelled() => new UserActionCancelled();

    public static IUserActionResult Alternative(IUserAction alternative) => new UserActionAlternative(alternative);
}

public interface IResolvableError : IUserActionResult
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


public record UserActionSuccess<T>(T ReturnValue, IUserAction InverseAction) : UserActionSuccess(InverseAction);

public record UserActionSuccess(IUserAction InverseAction) : IUserActionResult;

public record UserActionAlternative(IUserAction AlternativeAction) : IUserActionResult;

public record UserActionInvalid : IUserActionResult;

public record UserActionError(Exception Exception) : IUserActionResult;

public record UserActionCancelled : IUserActionResult;