using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Microsoft.Extensions.DependencyInjection;

namespace Laminar.Avalonia.Markup;

public static class MarkupExtensionHelpers
{
    extension(IServiceProvider serviceProvider)
    {
        public BindingBase UsingStaticResource<T>(string key, Func<T, BindingBase> bindingProvider)
            => serviceProvider.UsingStaticResource<T>(key, (t, _) => bindingProvider(t));
        
        public BindingBase UsingStaticResource<T>(string key, Func<T, object, BindingBase> bindingProvider)
        {
            var provideValueTarget = serviceProvider.GetRequiredService<IProvideValueTarget>();
            
            if (provideValueTarget.TargetObject is not StyledElement { IsInitialized: false } uninitializedElement)
            {
                return new StaticResourceExtension(key).ProvideValue(serviceProvider) is T correctType
                    ? bindingProvider(correctType, provideValueTarget.TargetObject)
                    : throw new InvalidCastException();
            }

            if (provideValueTarget.TargetProperty is not AvaloniaProperty avaloniaProperty)
            {
                throw new InvalidOperationException("Could not find target property");
            }
            
            uninitializedElement.Initialized += TargetInitialized;
            return AvaloniaProperty.UnsetValue.AsStaticBinding();
            
            void TargetInitialized(object? sender, EventArgs e)
            {
                var resource = uninitializedElement.FindResource(key);

                if (resource is UnsetValueType)
                {
                    throw new InvalidOperationException($"Could not find resource {key}");
                }
                
                if (resource is not T correctType)
                {
                    throw new InvalidCastException();
                }
                
                uninitializedElement.Bind(avaloniaProperty, bindingProvider(correctType, uninitializedElement));
                uninitializedElement.Initialized -= TargetInitialized;
            }
        }
    }
}