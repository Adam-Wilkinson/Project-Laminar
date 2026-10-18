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
    
    public static implicit operator MayError<T>(T notNull) => new(notNull);
}

public static class MayErrorExtensions
{
    public static MayError<T> ErrorIfNull<T>(this T? mayBeNull, Exception isNullError) where T : notnull 
        => mayBeNull != null ? new MayError<T>(mayBeNull) : new MayError<T>(isNullError);
    
    public static MayError<T> AsSuccess<T>(this T success) where T : notnull => new(success);
    
    public static T? ResultOrNull<T>(this MayError<T> mayError) where T : notnull => mayError.HasError ? default : mayError.Result;
}