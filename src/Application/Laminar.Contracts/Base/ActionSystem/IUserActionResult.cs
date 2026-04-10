namespace Laminar.Contracts.Base.ActionSystem;

public interface IUserActionResult
{
    public static IUserActionResult Success(IUserAction inverse) => new UserActionSuccess(inverse);
    
    public static IUserActionResult Invalid() => new UserActionInvalid();
    
    public static IUserActionResult Error(Exception exception) =>  new UserActionError(exception);

    public static IUserActionResult Cancelled() => new UserActionCancelled();
}

public record UserActionSuccess(IUserAction InverseAction) : IUserActionResult;

public record UserActionInvalid : IUserActionResult;

public record UserActionError(Exception Exception) : IUserActionResult;

public record UserActionCancelled : IUserActionResult;