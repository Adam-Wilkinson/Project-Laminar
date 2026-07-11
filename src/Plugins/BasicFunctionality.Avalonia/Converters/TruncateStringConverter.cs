using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;

namespace BasicFunctionality.Avalonia.Converters;

public class TruncateStringConverter : IMultiValueConverter
{
    public static TruncateStringConverter Instance { get; } = new();
    
    public object Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture) =>
        values.Count switch
        {
            1 
                => values[0]?.ToString() ?? string.Empty,
            2 when values[0] is string stringInput && values[1] is int trunc2 
                => ApplyTruncation(stringInput, trunc2),
            3 when values[0] is { } obj && values[1] is int trunc3 && values[2] is Func<object, string> toString
                => ApplyTruncation(toString(obj), trunc3),
            _ 
                => new BindingNotification(new InvalidCastException(), BindingErrorType.Error)
        };

    private static string ApplyTruncation(string stringInput, int truncationLength) 
        => stringInput.Length <= truncationLength ? stringInput : stringInput[..truncationLength] + "...";
}