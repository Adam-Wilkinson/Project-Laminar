namespace Laminar.Contracts.Base.ActionSystem;

public enum UserActionResultType
{
    Success = 0,
    Failure = 1,
    Error = 2,
}

public struct UserActionResult
{
    private UserActionResult(UserActionResultType resultType, IUserAction? inverseAction, Exception? error)
    {
        ResultType = resultType;
        InverseAction = inverseAction;
        Exception = error;
    }
    
    public static UserActionResult Success(IUserAction inverse) 
        => new(UserActionResultType.Success, inverse, null);

    public static UserActionResult Failure()
        => new(UserActionResultType.Failure, null, null);

    public static UserActionResult Error(Exception exception)
        => new(UserActionResultType.Error, null, exception);
    
    public UserActionResultType ResultType { get; }
    
    public IUserAction? InverseAction { get; }
    
    public Exception? Exception { get; }
}