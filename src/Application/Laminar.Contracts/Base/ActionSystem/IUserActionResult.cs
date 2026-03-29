namespace Laminar.Contracts.Base.ActionSystem;

public interface IUserActionResult
{
    public static IUserActionResult Success(IUserAction inverse) => new UserActionSuccess(inverse);
    
    public static IUserActionResult Failure() => new UserActionFailure();
    
    public static IUserActionResult Error(Exception exception) =>  new UserActionError(exception);
}

public record UserActionSuccess(IUserAction InverseAction) : IUserActionResult;

public record UserActionFailure : IUserActionResult;

public record UserActionError(Exception Exception) : IUserActionResult;