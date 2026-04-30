using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.MarkupExtensions;

namespace Laminar.Avalonia.ToolSystem;

public class QuickAccessExtension : MarkupExtension
{
    public const string QuickAccessRepositoryKey = "QuickAccessRepository";
    
    private readonly QuickAccessInstancerConverter _converter = new();
    private readonly BindingBase _valueKeyBinding;
    
    public QuickAccessExtension(string valueKey)
    {
        _valueKeyBinding = CompiledBinding.Create<object, string>(x => valueKey);
    }

    public QuickAccessExtension(BindingBase valueKeyBinding)
    {
        _valueKeyBinding = valueKeyBinding;
    }

    public override BindingBase ProvideValue(IServiceProvider serviceProvider)
    {
        if (new StaticResourceExtension(QuickAccessRepositoryKey).ProvideValue(serviceProvider) is not QuickAccessRepository repository)
            throw new InvalidOperationException("Quick access markup extension requires a repository");

        return new MultiBinding
        {
            Bindings = [new CompiledBinding(), new CompiledBinding { Source = repository }, _valueKeyBinding ],
            Converter = _converter,
        };
    }
    
    private class QuickAccessInstancerConverter : IMultiValueConverter
    {
        public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
        {
            if (values[0] is not { } target || values[1] is not QuickAccessRepository repository || values[2] is not string key)
                return new BindingNotification(new InvalidCastException(), BindingErrorType.Error);

            return repository.FromKey(key).Build(target)?.ChildTools;
        }
    }
}