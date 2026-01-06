using System;
using System.Globalization;
using System.Windows.Input;
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
    
    public static void OnChange(IResourceHost resourceHost, string settingKey, Action<T> onChange)
    {
        resourceHost.GetResourceObservable(settingKey).Subscribe(
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
    public static readonly StyledProperty<ICommand> ResetCommandProperty = AvaloniaProperty.Register<Setting, ICommand>(nameof(ResetCommand));
    
    public ICommand ResetCommand
    {
        get => GetValue(ResetCommandProperty);
        set => SetValue(ResetCommandProperty, value);
    }
    
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