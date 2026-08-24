using System.Diagnostics.CodeAnalysis;

namespace Laminar.Domain;

public class MayError<T>
{
    public MayError(T success)
    {
        Result = success;
        Exception = null;
        HasError = false;
    }

    public MayError(Exception exception)
    {
        Exception = exception;
        Result = default;
        HasError = true;
    }

    public T Result => !HasError && field != null
        ? field
        : throw new InvalidOperationException("Attempt to access invalid value with exception", Exception);

    public Exception Exception => HasError && field is not null
        ? field
        : throw new InvalidOperationException("This value does not have an error", Exception);

    public bool HasError { get; }

    public bool TryGetResult([NotNullWhen(true)] out T? result, [NotNullWhen(false)] out Exception? exception)
    {
        result = HasError ? default : Result;
        exception = HasError ? Exception : null;
        return !HasError;
    }
}