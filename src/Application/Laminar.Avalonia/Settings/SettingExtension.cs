using System;
using Avalonia.Data;
using Avalonia.Markup.Xaml;
using Avalonia.Metadata;
using Laminar.Avalonia.Markup;

namespace Laminar.Avalonia.Settings;

public class SettingExtension(string settingKey) : MarkupExtension
{
    public override BindingBase ProvideValue(IServiceProvider serviceProvider)
        => serviceProvider.UsingStaticResource<Setting>($"SettingsRoot.{settingKey}", setting =>
        {
            DataType = setting.Value.GetType();
            return setting.CreateValueBinding();
        });

    [DataType]
    public Type? DataType { get; set; }
}