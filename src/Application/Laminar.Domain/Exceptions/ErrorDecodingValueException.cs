namespace Laminar.Domain.Exceptions;

public class ErrorDecodingValueException(object overrideValue)
    : Exception($"Error decoding value. The value will be reset to {overrideValue}")
{
    public object Value => overrideValue;
}