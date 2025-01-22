using Avalonia;

namespace Laminar.Avalonia.Settings;

public class SettingsItem : AvaloniaObject
{
    public static readonly StyledProperty<string> NameProperty = AvaloniaProperty.Register<SettingsItem, string>(nameof(Name));

    public string Name
    {
        get => GetValue(NameProperty);
        set => SetValue(NameProperty, value);
    }
}