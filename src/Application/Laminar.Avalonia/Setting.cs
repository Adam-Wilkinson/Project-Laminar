using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Metadata;
using Avalonia.Reactive;
using Laminar.Domain.Notification;
using Laminar.PluginFramework.UserInterface;
using Laminar.PluginFramework.UserInterface.UserInterfaceDefinitions;

namespace Laminar.Avalonia;

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

public class OptionsSetting<T> : SettingsItem, IInterfaceData<EnumDropdown, object> where T : notnull
{
    private SettingsOption<T> _selectedOption;
    private int _valueIndex;
    
    public static readonly DirectProperty<OptionsSetting<T>, SettingsOption<T>> ValueProperty = AvaloniaProperty.RegisterDirect<OptionsSetting<T>, SettingsOption<T>>(nameof(SelectedOption), options => options.SelectedOption);
    
    public OptionsSetting()
    {
        Options.CollectionChanged += (_, _) =>
        {
            if (Options.Count > ValueIndex)
            {
                SelectedOption = Options[ValueIndex];
            }
        };
    }

    [Content] public AvaloniaList<SettingsOption<T>> Options { get; } = [];
    
    public int ValueIndex
    {
        get => _valueIndex;
        set
        {
            _valueIndex = value;
            if (Options.Count > ValueIndex)
            {
                SelectedOption = Options[ValueIndex];
            }
        }
    }

    public bool IsUserEditable => true;

    public EnumDropdown Definition => new() { DropdownOptions = Options };
    
    public SettingsOption<T> SelectedOption
    {
        get => _selectedOption;
        set => SetAndRaise(ValueProperty, ref _selectedOption, value);
    }

    object IInterfaceData<object>.Value
    {
        get => SelectedOption;
        set => SelectedOption = (SettingsOption<T>)value;
    }
}

public class SettingsOption<T>
{
    public required string Name { get; set; }

    public required T Value { get; set; }

    public override string ToString() => Name;
}

public class DoubleSetting : Setting<double>;

public class Setting<T> : SettingsItem, IInterfaceData<T> where T : notnull
{
    public static readonly StyledProperty<T> ValueProperty = AvaloniaProperty.Register<SettingsItem, T>(nameof(Value));
    public static readonly StyledProperty<IUserInterfaceDefinition?> DefinitionProperty = AvaloniaProperty.Register<SettingsItem, IUserInterfaceDefinition?>(nameof(Definition));
    
    public static implicit operator T(Setting<T> setting) => setting.Value;
    
    public T Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }
    
    public bool IsUserEditable => true;
    
    public IUserInterfaceDefinition? Definition
    {
        get => GetValue(DefinitionProperty);
        set => SetValue(DefinitionProperty, value);
    }
}

public class SettingsItem : AvaloniaObject
{
    public static readonly StyledProperty<string> NameProperty = AvaloniaProperty.Register<SettingsItem, string>(nameof(Name));

    public string Name
    {
        get => GetValue(NameProperty);
        set => SetValue(NameProperty, value);
    }
}