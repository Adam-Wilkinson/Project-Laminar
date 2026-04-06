using System;
using System.Globalization;
using System.Linq;
using Avalonia;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Markup.Xaml.MarkupExtensions;

namespace Laminar.Avalonia.Converters;

public class MultiplyConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (parameter is string stringParam)
        {
            if (targetType == typeof(double))
            {
                parameter = double.TryParse(stringParam, out double doubleVal) ? doubleVal : null;
            }
            else if (targetType == typeof(Thickness))
            {
                parameter = Thickness.Parse(stringParam);
            }
            else if (targetType == typeof(CornerRadius))
            {
                parameter = CornerRadius.Parse(stringParam);
            }
        }
        
        return (value, parameter) switch
        {
            (double doubleInput, double doubleParameter) => doubleInput * doubleParameter,
            (Thickness thicknessInput, double doubleParamWithThickness) => thicknessInput * doubleParamWithThickness,
            (double doubleInputWithThickness, Thickness thicknessParam) => thicknessParam * doubleInputWithThickness,
            (CornerRadius cornerRadiusInput, double doubleParamWithCornerRadius) => Multiply(cornerRadiusInput, doubleParamWithCornerRadius),
            (double doubleInputWithCr, CornerRadius crParam) => Multiply(crParam, doubleInputWithCr),
            _ => new BindingNotification(new InvalidCastException(), BindingErrorType.Error),
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (parameter is string stringParam)
        {
            if (targetType == typeof(double))
            {
                parameter = double.TryParse(stringParam, out double doubleVal) ? doubleVal : null;
            }
            else if (targetType == typeof(Thickness))
            {
                parameter = Thickness.Parse(stringParam);
            }
            else if (targetType == typeof(CornerRadius))
            {
                parameter = CornerRadius.Parse(stringParam);
            }
        }
        
        return (value, parameter) switch
        {
            (double doubleInput, double doubleParameter) => doubleInput / doubleParameter,
            (Thickness thicknessInput, double doubleParamWithThickness) => thicknessInput * (1 / doubleParamWithThickness),
            (double doubleInputWithThickness, Thickness thicknessParam) => thicknessParam * (1 / doubleInputWithThickness),
            (CornerRadius cornerRadiusInput, double doubleParamWithCornerRadius) => Multiply(cornerRadiusInput, 1 / doubleParamWithCornerRadius),
            (double doubleInputWithCr, CornerRadius crParam) => Multiply(crParam, 1 / doubleInputWithCr),
            _ => new BindingNotification(new InvalidCastException(), BindingErrorType.Error),
        };
    }

    private static CornerRadius Multiply(CornerRadius radius, double factor) => new(radius.TopLeft * factor,
        radius.TopRight * factor, radius.BottomRight * factor, radius.BottomLeft * factor);
}