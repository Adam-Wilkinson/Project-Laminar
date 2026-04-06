using System;
using Avalonia.Data;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Laminar.Avalonia.Converters;

namespace Laminar.Avalonia.Settings;

public class UiScaledExtension(string valueToScale) : MarkupExtension
{
    private static readonly MultiplyConverter Converter = new();
    
    public override IBinding ProvideValue(IServiceProvider serviceProvider)
    {
        return new Binding
        {
            Path = "Value",
            Converter = Converter,
            ConverterParameter = valueToScale,
            Source = new StaticResourceExtension("SettingsRoot.InterfaceSettings.UiScale").ProvideValue(serviceProvider) 
        };
    }
}