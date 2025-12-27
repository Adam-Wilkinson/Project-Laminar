using Avalonia.Controls;
using Avalonia.Media;
using BasicFunctionality.Avalonia.UserControls;
using Laminar.PluginFramework.Registration;
using Laminar.PluginFramework.UserInterface.UserInterfaceDefinitions;
using Slider = Laminar.PluginFramework.UserInterface.UserInterfaceDefinitions.Slider;
using StringEditor = Laminar.PluginFramework.UserInterface.UserInterfaceDefinitions.StringEditor;
using ToggleSwitch = Laminar.PluginFramework.UserInterface.UserInterfaceDefinitions.ToggleSwitch;
using DrawingColor = System.Drawing.Color;
using AvaloniaColor = Avalonia.Media.Color;
using ColorEditor = Laminar.PluginFramework.UserInterface.UserInterfaceDefinitions.ColorEditor;
using ColorViewer = Laminar.PluginFramework.UserInterface.UserInterfaceDefinitions.ColorViewer;

[module: HasFrontendDependency(FrontendDependency.Avalonia)]

namespace BasicFunctionality.Avalonia;

public class PluginFront : IPlugin
{
    public Platforms Platforms => Platforms.All;

    public string PluginName => "Basic Functionality UI";

    public string PluginDescription => "Basic user interface elements for the Avalonia frontend";

    public void Register(IPluginHost host)
    {
        host.RegisterType("#FFFF00", "Color", Colors.White, new ColorEditor(), new ColorViewer(), new AvaloniaColorSerializer());
        
        host.RegisterInterface<NumberEntry, NumberEditor, Control>();
        host.RegisterInterface<DefaultViewer, DefaultDisplay, Control>();
        host.RegisterInterface<StringViewer, StringDisplay, Control>();
        host.RegisterInterface<StringEditor, UserControls.StringEditor, Control>();
        host.RegisterInterface<EnumDropdown, EnumEditor, Control>();
        host.RegisterInterface<Slider, SliderEditor, Control>();
        host.RegisterInterface<ToggleSwitch, UserControls.ToggleSwitch, Control>();
        host.RegisterInterface<Checkbox, UserControls.CheckBox, Control>();
        host.RegisterInterface<EditableLabel, AvaloniaEditableLabel, Control>();
        host.RegisterInterface<ColorViewer, UserControls.ColorViewer, Control>();
        host.RegisterInterface<ColorEditor, UserControls.ColorEditor, Control>();
        
        host.RegisterDataInterface<NumberEntry, double, NumberEditor>();
        host.RegisterDataInterface<DefaultViewer, object, DefaultDisplay>();
        host.RegisterDataInterface<StringViewer, object, StringDisplay>();
        host.RegisterDataInterface<StringEditor, string, UserControls.StringEditor>();
        host.RegisterDataInterface<EnumDropdown, object, EnumEditor>();
        host.RegisterDataInterface<Slider, double, SliderEditor>();
        host.RegisterDataInterface<ToggleSwitch, bool, UserControls.ToggleSwitch>();
        host.RegisterDataInterface<Checkbox, bool, UserControls.CheckBox>();
        host.RegisterDataInterface<BoolTwoButton, bool, BoolEditor>();
        host.RegisterDataInterface<EditableLabel, string, AvaloniaEditableLabel>();
        host.RegisterDataInterface<ColorViewer, DrawingColor, UserControls.ColorViewer>();
        host.RegisterDataInterface<ColorEditor, DrawingColor, UserControls.ColorEditor>();
        host.RegisterDataInterface<ColorViewer, AvaloniaColor, UserControls.ColorViewer>();
        host.RegisterDataInterface<ColorEditor, AvaloniaColor, UserControls.ColorEditor>();
    }
}
