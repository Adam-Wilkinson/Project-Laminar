using Laminar_PluginFramework.Primitives;
using Laminar_PluginFramework.Registration;
using WindowsKeyboardMouse.Nodes.Mouse.Triggers;
using WindowsKeyboardMouse.Nodes.Keyboard.Triggers;
using WindowsKeyboardMouse.Nodes.Keyboard.Output;
using WindowsHook;
using Avalonia.Controls;
using WindowsKeyboardMouse.UserControls;
using WindowsKeyboardMouse.Primitives;
using Avalonia.Input;

namespace WindowsKeyboardMouse
{
    public class PluginFront : IPlugin
    {
        public Platforms Platforms => Platforms.Windows;

        public string PluginName { get; } = "Keyboard and Mouse interface";

        public string PluginDescription { get; } = "Allows for interfacing with the keyboard and mouse, including listening for keyboard and mouse events and sending keyboard and mouse input in Windows";

        public void Dispose()
        {
        }

        public void Register(IPluginHost host)
        {
            host.RegisterType<MouseButtons>("#FFFF00", "Mouse Button", MouseButtons.Left, "EnumEditor", "StringDisplay", true);
            host.RegisterType<KeyboardKey>("#FFA500", "Keyboard Button", new KeyboardKey((int)Keys.A), "KeyboardKeyEditor", "StringDisplay", true);

            host.RegisterEditor<IControl, KeyboardKeyEditor>("KeyboardKeyEditor");

            host.AddNodeToMenu<MouseButtonTrigger, KeyboardButtonTrigger, KeyCombinationTrigger, TextTypedTrigger>("Triggers");

            host.AddNodeToMenu<MouseButtonTrigger>("Mouse", "Triggers");

            host.AddNodeToMenu<KeyPresser, TextTyper>("Keyboard");
        }
    }
}
