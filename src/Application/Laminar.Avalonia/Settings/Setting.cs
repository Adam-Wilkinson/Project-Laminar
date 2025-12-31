using System;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Media;
using Avalonia.Metadata;
using Avalonia.Reactive;
using Laminar.PluginFramework.UserInterface;
using Laminar.PluginFramework.UserInterface.UserInterfaceDefinitions;

namespace Laminar.Avalonia.Settings;

public class TimeSpanSetting : Setting<TimeSpan>;
public class DoubleSetting : Setting<double>;
public class ColorSetting : Setting<Color>;
public class BoolSetting : Setting<bool>;

public class Setting<T> : Setting where T : notnull
{
    public new T Value
    {
        get => (T)base.Value;
        set => base.Value = value;
    }
    
    public static void OnChange(TopLevel topLevel, string settingKey, Action<T> onChange)
    {
        topLevel.GetResourceObservable(settingKey).Subscribe(
            new AnonymousObserver<object?>(o =>
            {
                if (o is not Setting<T> setting) return;

                onChange(setting.Value);
                setting.GetPropertyChangedObservable(ValueProperty)
                    .Subscribe(new AnonymousObserver<AvaloniaPropertyChangedEventArgs>(changedArgs =>
                    {
                        onChange(setting.Value);
                    }));
            }));
    }
}

public class Setting : SettingsItem, IInterfaceData
{
    public static readonly StyledProperty<object> ValueProperty = AvaloniaProperty.Register<Setting, object>(nameof(Value));
    public static readonly StyledProperty<IUserInterfaceDefinition?> DefinitionProperty = AvaloniaProperty.Register<Setting, IUserInterfaceDefinition?>(nameof(Definition));
    public static readonly StyledProperty<IValueConverter?> DisplayValueConverterProperty = AvaloniaProperty.Register<Setting, IValueConverter?>(nameof(DisplayValueConverter));
    public static readonly StyledProperty<bool> DisplayToUserProperty = AvaloniaProperty.Register<Setting, bool>(nameof(DisplayToUser), defaultValue: true);
    
    public virtual object Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public bool DisplayToUser
    {
        get => GetValue(DisplayToUserProperty);
        set => SetValue(DisplayToUserProperty, value);
    }
    
    public bool IsUserEditable => true;

    public IValueConverter? DisplayValueConverter
    {
        get => GetValue(DisplayValueConverterProperty);
        set => SetValue(DisplayValueConverterProperty, value);
    }
    
    [Content]
    public IUserInterfaceDefinition? Definition
    {
        get => GetValue(DefinitionProperty);
        set => SetValue(DefinitionProperty, value);
    }

    object IInterfaceData.Value
    {
        get => DisplayValueConverter is null ? Value : DisplayValueConverter.Convert(Value, typeof(object), null, CultureInfo.CurrentCulture)!;
        set => Value = DisplayValueConverter is null ? value : DisplayValueConverter.ConvertBack(value, typeof(object), null, CultureInfo.CurrentCulture)!;
    }
}