using System;
using System.Transactions;
using Avalonia.Data;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Laminar.Avalonia.Converters;

namespace Laminar.Avalonia.Settings;

public class UiScaledExtension(string valueToScale) : MarkupExtension
{
    private static readonly MultiplyConverter Converter = new();
    
    public override BindingBase ProvideValue(IServiceProvider serviceProvider)
    {
        if (new StaticResourceExtension("SettingsRoot.InterfaceSettings.UiScale").ProvideValue(serviceProvider) is not DoubleSetting uiScaleSetting)
            throw new InvalidOperationException();

        return new ReflectionBinding(nameof(Setting.Value))
        {
            Source = uiScaleSetting,
            Converter = Converter,
            ConverterParameter = valueToScale
        };
         
            //CompiledBinding.Create<DoubleSetting, object>(setting => setting.Value, converter: Converter,
            //converterParameter: valueToScale, source: uiScaleSetting);
    }
}