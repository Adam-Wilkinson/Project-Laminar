using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Laminar.Avalonia.Markup;

namespace Laminar.Avalonia.ToolSystem;

public class QuickAccessExtension : MarkupExtension
{
    public const string QuickAccessRepositoryKey = "QuickAccessRepository";
    
    private readonly QuickAccessInstancerConverter _converter = new();
    private readonly BindingBase _valueKeyBinding;
    
    public QuickAccessExtension(string valueKey)
    {
        _valueKeyBinding = valueKey.AsStaticBinding();
    }

    public QuickAccessExtension(BindingBase valueKeyBinding)
    {
        _valueKeyBinding = valueKeyBinding;
    }

    public override BindingBase ProvideValue(IServiceProvider serviceProvider)
        => serviceProvider.UsingStaticResource<QuickAccessRepository>(QuickAccessRepositoryKey, 
            repository => new MultiBinding
            {
                Bindings = [new CompiledBinding(), _valueKeyBinding ],
                Converter = _converter,
                ConverterParameter = repository
            });
    
    private class QuickAccessInstancerConverter : IMultiValueConverter
    {
        public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
        {
            if (values[0] is not { } target || values[1] is not string key || parameter is not QuickAccessRepository repository)
                return new BindingNotification(new InvalidCastException(), BindingErrorType.Error);

            return repository.FromKey(key).Build(target)?.ChildTools;
        }
    }
}