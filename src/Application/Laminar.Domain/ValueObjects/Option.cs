namespace Laminar.Domain.ValueObjects;

public class Option<T> where T : notnull
{
    public static Option<TNullable> FromNullable<TNullable>(TNullable? value) where TNullable : class =>
        new(value, value is not null);

    public Option(T value)
    {
        Value = value;
        HasValue = true;
    }
    
    private Option(T? value, bool hasValue)
    {
        Value = value;
        HasValue = value is not null;
    }
    
    public T? Value { get; }
    
    public bool HasValue { get; }
}