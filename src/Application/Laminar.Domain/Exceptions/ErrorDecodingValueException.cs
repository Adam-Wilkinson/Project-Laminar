namespace Laminar.Domain.Exceptions;

public class ErrorDecodingValueException(string valueName, object overrideValue)
    : Exception($"Error decoding value {valueName}. The value will be reset to {overrideValue}")
{
    public string ValueName => valueName;
    public object Value => overrideValue;
}