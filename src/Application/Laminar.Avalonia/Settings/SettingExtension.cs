using System;
using Avalonia.Data;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.MarkupExtensions;

namespace Laminar.Avalonia.Settings;

public class SettingExtension(string settingKey) : MarkupExtension
{
    public override IBinding ProvideValue(IServiceProvider serviceProvider)
    {
        return new StaticResourceExtension($"SettingsRoot.{settingKey}").ProvideValue(serviceProvider) is Setting setting ? setting[!Setting.ValueProperty] : throw new Exception();
    }
}