using System.Collections;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Metadata;

namespace Laminar.Avalonia.Settings;

public class SettingsCategory : SettingsItem, IEnumerable<SettingsItem>
{
    public static readonly StyledProperty<AvaloniaList<SettingsItem>> ChildrenProperty = AvaloniaProperty.Register<SettingsCategory, AvaloniaList<SettingsItem>>(nameof(Children), defaultValue: []);

    [Content]
    public AvaloniaList<SettingsItem> Children
    {
        get => GetValue(ChildrenProperty);
        set => SetValue(ChildrenProperty, value);
    }

    public IEnumerator<SettingsItem> GetEnumerator() => Children.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}