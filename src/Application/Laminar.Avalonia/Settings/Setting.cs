using System.Globalization;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Media;
using Avalonia.Metadata;
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

    public override CompiledBinding CreateValueBinding() =>
        CompiledBinding.Create<Setting<T>, T>(x => x.Value, source: this);

    public static void OnChange(IResourceHost resourceHost, string settingKey, Action<T> onChange)
    {
        resourceHost.GetResourceObservable(settingKey).Subscribe(
            new Domain.Notification.Value.AnonymousObserver<object?>(o =>
            {
                if (o is not Setting<T> setting) return;

                onChange(setting.Value);
                setting.GetPropertyChangedObservable(ValueProperty)
                    .Subscribe(new Domain.Notification.Value.AnonymousObserver<AvaloniaPropertyChangedEventArgs>(changedArgs =>
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
    public static readonly StyledProperty<ICommand> OnChangedCommandProperty = AvaloniaProperty.Register<Setting, ICommand>(nameof(OnChangedCommand));

    static Setting()
    {
        ValueProperty.Changed.AddClassHandler<Setting>((setting, _) => setting?.OnChangedCommand?.Execute(null));
    }
        
    public ICommand ResetCommand
    {
        get => GetValue(ResetCommandProperty);
        set => SetValue(ResetCommandProperty, value);
    }

    public ICommand OnChangedCommand
    {
        get => GetValue(OnChangedCommandProperty);
        set => SetValue(OnChangedCommandProperty, value);
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

    public virtual CompiledBinding CreateValueBinding() 
        => CompiledBinding.Create<Setting, object>(x => x.Value, source: this);

    [Content]
    public IUserInterfaceDefinition? Definition
    {
        get => GetValue(DefinitionProperty);
        set => SetValue(DefinitionProperty, value);
    }

    public void SetValue(object newValue) => ((IInterfaceData)this).Value = newValue;

    object IInterfaceData.Value
    {
        get => DisplayValueConverter is null ? Value : DisplayValueConverter.Convert(Value, typeof(object), null, CultureInfo.CurrentCulture)!;
        set => Value = DisplayValueConverter is null ? value : DisplayValueConverter.ConvertBack(value, typeof(object), null, CultureInfo.CurrentCulture)!;
    }
}