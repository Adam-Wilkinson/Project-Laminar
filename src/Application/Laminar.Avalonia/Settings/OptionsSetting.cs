using System.Linq;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Metadata;
using Laminar.PluginFramework.UserInterface;
using Laminar.PluginFramework.UserInterface.UserInterfaceDefinitions;

namespace Laminar.Avalonia.Settings;

public class OptionsSetting : Setting, IInterfaceData<EnumDropdown, object>
{
    private int _valueIndex;
    
    public static readonly StyledProperty<OptionsSettingValue> SelectedOptionProperty = AvaloniaProperty.Register<OptionsSetting, OptionsSettingValue>(nameof(SelectedOption));

    static OptionsSetting()
    {
        SelectedOptionProperty.Changed.AddClassHandler<OptionsSetting>((setting, args) =>
        {
            setting.Value = args.GetNewValue<OptionsSettingValue>().Value!;
        });
        ValueProperty.Changed.AddClassHandler<OptionsSetting>((setting, args) =>
        {
            ((IInterfaceData<object>)setting).Value = setting.Options.FirstOrDefault(x => x.Value == args.GetNewValue<object>())!;
        });
    }
    
    public OptionsSetting()
    {
        Options.CollectionChanged += (_, _) =>
        {
            if (Options.Count > ValueIndex)
            {
                SelectedOption = Options[ValueIndex];
            }
        };

        Definition = new EnumDropdown { DropdownOptions = Options };
    }

    [Content] public AvaloniaList<OptionsSettingValue> Options { get; } = [];
    
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
    
    public OptionsSettingValue SelectedOption
    {
        get => GetValue(SelectedOptionProperty);
        set => SetValue(SelectedOptionProperty, value);
    }
    
    object IInterfaceData<object>.Value
    {
        get => SelectedOption;
        set => SelectedOption = (OptionsSettingValue)value;
    }

    EnumDropdown IInterfaceData<EnumDropdown, object>.Definition => new() { DropdownOptions = Options};
}