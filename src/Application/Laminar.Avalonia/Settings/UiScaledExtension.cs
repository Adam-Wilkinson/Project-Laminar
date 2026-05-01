using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.MarkupExtensions;

namespace Laminar.Avalonia.Settings;

public class UiScaledExtension : MarkupExtension
{
    private static readonly MultiplyBindingsConverter Converter = new();
    private readonly StaticResourceExtension? _valueToScaleResource;
    private readonly BindingBase? _valueToScaleBinding;
    
    private BindingBase? _uiScaleBinding;

    public UiScaledExtension(object valueToScale)
    {
        switch (valueToScale)
        {
            case BindingBase valueToScaleBinding:
                _valueToScaleBinding = valueToScaleBinding;
                break;
            case StaticResourceExtension valueToScaleResource:
                _valueToScaleResource = valueToScaleResource;
                break;
            default:
                _valueToScaleBinding = CompiledBinding.Create<object, object>(x => x, source: valueToScale);
                break;
        }
    }

    public UiScaledExtension(BindingBase valueToScaleBinding)
    {
        _valueToScaleBinding = valueToScaleBinding;
    }

    public UiScaledExtension(StaticResourceExtension staticResource)
    {
        _valueToScaleResource = staticResource;
    }
    
    public override BindingBase ProvideValue(IServiceProvider serviceProvider)
    {
        if (_uiScaleBinding is null)
        {
            if (new StaticResourceExtension("SettingsRoot.InterfaceSettings.UiScale").ProvideValue(serviceProvider) is not DoubleSetting uiScaleSetting)
                throw new InvalidOperationException();

            _uiScaleBinding = CompiledBinding.Create<Setting, object>(x => x.Value, source: uiScaleSetting);   
        }
        
        BindingBase? valueToScaleBinding = _valueToScaleBinding;
        if (valueToScaleBinding is null)
        {
            ArgumentNullException.ThrowIfNull(_valueToScaleResource);
            valueToScaleBinding = CompiledBinding.Create<object, object>(x => x, source: _valueToScaleResource.ProvideValue(serviceProvider)); 
        }

        return new MultiBinding
        {
            Bindings = [valueToScaleBinding, _uiScaleBinding],
            Converter = Converter
        };
    }

    private class MultiplyBindingsConverter : IMultiValueConverter
    {
        private static readonly BindingNotification NotSupportedNotification
            = new(new NotSupportedException(), BindingErrorType.Error);
        
        public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
        {
            if (targetType == typeof(double))
            {
                return (values[0], values[1]) switch
                {
                    (double d1, double d2) => d1 * d2,
                    (string s, double d) => double.Parse(s) * d,
                    (double d, string s) => double.Parse(s) * d,
                    (string s1, string s2) => double.Parse(s1) * double.Parse(s2),
                    _ => NotSupportedNotification,
                };
            }
            
            if (targetType == typeof(Thickness))
            {
                return (values[0], values[1]) switch
                {
                    (Thickness t, double d) => t * d,
                    (double d, Thickness t) => t * d,
                    (string s, double d) => Thickness.Parse(s) * d,
                    (double d, string s) => Thickness.Parse(s) * d,
                    (Thickness t, string s) => t * double.Parse(s),
                    (string s, Thickness t) => t * double.Parse(s),
                    _ => NotSupportedNotification
                };
            }

            if (targetType == typeof(CornerRadius))
            {
                return (values[0], values[1]) switch
                {
                    (CornerRadius c, double d) => Multiply(c, d),
                    (double d, CornerRadius c) => Multiply(c, d),
                    (string s, double d) => Multiply(CornerRadius.Parse(s), d),
                    (double d, string s) => Multiply(CornerRadius.Parse(s), d),
                    (CornerRadius c, string s) => Multiply(c, double.Parse(s)),
                    (string s, CornerRadius c) => Multiply(c, double.Parse(s)),
                    _ => NotSupportedNotification,
                };
            }

            return NotSupportedNotification;
        }
    }
    
    private static CornerRadius Multiply(CornerRadius radius, double factor) => new(radius.TopLeft * factor,
        radius.TopRight * factor, radius.BottomRight * factor, radius.BottomLeft * factor);
}