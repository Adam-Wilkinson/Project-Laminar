using System;
using Avalonia;
using Laminar.PluginFramework.UserInterface;
using Laminar.PluginFramework.UserInterface.UserInterfaceDefinitions;

namespace Laminar.Avalonia.Settings;

public class TimeSpanSetting : Setting<TimeSpan>;
public class DoubleSetting : Setting<double>;

public class Setting<T> : Setting where T : notnull
{
    public new T Value
    {
        get => (T)base.Value;
        set => base.Value = value;
    }   
}

public class Setting : SettingsItem, IInterfaceData
{
    public static readonly StyledProperty<object> ValueProperty = AvaloniaProperty.Register<SettingsItem, object>(nameof(Value));
    public static readonly StyledProperty<IUserInterfaceDefinition?> DefinitionProperty = AvaloniaProperty.Register<SettingsItem, IUserInterfaceDefinition?>(nameof(Definition));
    
    public virtual object Value
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