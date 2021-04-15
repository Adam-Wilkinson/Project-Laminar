using OpenFlow_PluginFramework.Primitives;
using OpenFlow_PluginFramework.Registration;
using WindowsKeyboardMouse.Nodes.Mouse.Triggers;
using WindowsKeyboardMouse.Nodes.Keyboard.Triggers;
using WindowsKeyboardMouse.Primitives;
using WindowsKeyboardMouse.Nodes.Keyboard.Output;

namespace WindowsKeyboardMouse
{
    public class PluginFront : IPlugin
    {
        public Platforms Platforms => Platforms.Windows;

        public string PluginName { get; } = "Keyboard and Mouse interface";

        public string PluginDescription { get; } = "Allows for interfacing with the keyboard and mouse, including listening for keyboard and mouse events and sending keyboard and mouse input in Windows";

        public void Register(IPluginHost host)
        {
            host.RegisterType<MouseButtonEnum>("#FFFF00", "Mouse Button", MouseButtonEnum.LeftButton, "EnumEditor", "StringDisplay");
            host.RegisterType<KeyboardButtonEnum>("#00FF00", "Keyboard Button", KeyboardButtonEnum.A, "EnumEditor", "StringDisplay");

            host.AddNodeToMenu<MouseButtonTrigger>("Mouse", "Triggers");

            host.AddNodeToMenu<KeyboardButtonTrigger, KeyCombinationTrigger>("Keyboard", "Triggers");

            host.AddNodeToMenu<KeyPresser, TextTyper>("Keyboard", "Output");
        }
    }
}
