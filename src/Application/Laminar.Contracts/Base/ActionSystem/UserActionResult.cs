namespace Laminar.Contracts.Base.ActionSystem;

public record UserActionValueResult<T>(T Value) : UserActionResult(true, null);

public record UserActionResult
{
    private readonly List<Exception> _exceptions = [];
    
    private protected UserActionResult(bool succeeded, Exception? exception)
    {
        Succeeded = succeeded;
        if (exception is not null)
        {
            _exceptions.Add(exception);
        }
    }

    public static UserActionResult Success() => new(true, null);

    public static UserActionResult Error(Exception exception) => new(false, exception);
    
    public IReadOnlyList<Exception> Exceptions => _exceptions;

    public bool Succeeded { get; private set; }

    public UserActionResult Merge(UserActionResult other)
    {
        Succeeded = Succeeded && other.Succeeded;
        _exceptions.AddRange(other.Exceptions);
        return this;
    }
}