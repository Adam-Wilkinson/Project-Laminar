using Avalonia.Controls;
using BasicFunctionality.Avalonia.UserControls;
using Laminar.PluginFramework.Registration;
using Laminar.PluginFramework.UserInterface.UserInterfaceDefinitions;

[module: HasFrontendDependency(FrontendDependency.Avalonia)]

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
        host.RegisterInterface<Laminar.PluginFramework.UserInterface.UserInterfaceDefinitions.StringEditor, UserControls.StringEditor, IControl>();
        host.RegisterInterface<EnumDropdown, EnumEditor, IControl>();
        host.RegisterInterface<Laminar.PluginFramework.UserInterface.UserInterfaceDefinitions.Slider, SliderEditor, IControl>();
        host.RegisterInterface<Laminar.PluginFramework.UserInterface.UserInterfaceDefinitions.ToggleSwitch, UserControls.ToggleSwitch, IControl>();
        host.RegisterInterface<Checkbox, UserControls.CheckBox, IControl>();
        host.RegisterInterface<EditableLabel, AvaloniaEditableLabel, IControl>();
    }
}
