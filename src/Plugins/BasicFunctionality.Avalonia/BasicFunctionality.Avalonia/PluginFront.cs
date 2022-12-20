using Avalonia.Controls;
using BasicFunctionality.Avalonia.UserControls;
using Laminar.PluginFramework.Registration;
using Laminar_PluginFramework.Primitives;
using Laminar_PluginFramework.Registration;
using Laminar_PluginFramework.UserInterfaces;

[module: TargetFrontend(Frontend.Avalonia)]

namespace BasicFunctionality.Avalonia;

public class PluginFront : IPlugin
{
    public Platforms Platforms => Platforms.All;

    public string PluginName => "Basic Functionality UI";

    public string PluginDescription => "Basic user interface elements for the Avalonia frontend";

    public void Register(IPluginHost host)
    {
        host.RegisterInterface<NumberEntry, NumberEditor, IControl>();
        host.RegisterInterface<DefaultViewer, DefaultDisplay, IControl>();
        host.RegisterInterface<StringViewer, StringDisplay, IControl>();
        host.RegisterInterface<Laminar_PluginFramework.UserInterfaces.StringEditor, UserControls.StringEditor, IControl>();
        host.RegisterInterface<EnumDropdown, EnumEditor, IControl>();
        host.RegisterInterface<Laminar_PluginFramework.UserInterfaces.Slider, SliderEditor, IControl>();
        host.RegisterInterface<Laminar_PluginFramework.UserInterfaces.ToggleSwitch, UserControls.ToggleSwitch, IControl>();
        host.RegisterInterface<Laminar_PluginFramework.UserInterfaces.Checkbox, UserControls.CheckBox, IControl>();
        host.RegisterInterface<EditableLabel, AvaloniaEditableLabel, IControl>();
    }
}
