using System;
using Avalonia.Data;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.MarkupExtensions;

namespace Laminar.Avalonia.Settings;

public class SettingExtension(string settingKey) : MarkupExtension
{
    public override CompiledBinding ProvideValue(IServiceProvider serviceProvider)
    {
        if (new StaticResourceExtension($"SettingsRoot.{settingKey}").ProvideValue(serviceProvider) is not Setting setting)
            throw new InvalidOperationException();

        DataType = setting.Value.GetType();

        return CompiledBinding.Create<Setting, object>(s => s.Value, source: setting);
    }

    public Type? DataType { get; set; }
}