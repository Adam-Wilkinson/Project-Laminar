using Laminar_PluginFramework.Primitives;
using Laminar_PluginFramework.Registration;
using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using WindowsPluginBase.Nodes;
using WindowsPluginBase.Window;

namespace WindowsPluginBase
{
    public class PluginFront : IPlugin
    {
        public Platforms Platforms { get; } = Platforms.Windows;

        public string PluginName { get; } = "Windows Base";

        public string PluginDescription { get; } = "A base plugin for all Windows plugins to use";

        public void Register(IPluginHost host)
        {
            host.RegisterType<AllWindowsLayout>("#00FF00", "Window Layout", new AllWindowsLayout(), null, null, false, null);
            host.RegisterType<Window.Window>("#00b38c", "Window", new Window.Window { hWnd = IntPtr.Zero }, null, null, false, null);
            host.RegisterType<Rectangle>("#0091bf", "Rectangle", new Rectangle(), null, null, false, null);
            host.RegisterType<Point>("#176931", "Point", null, null, null, true, null);

            host.AddNodeToMenu<WindowLayoutChanged, WindowMoved>("Triggers");
            host.AddNodeToMenu<SetWindowLayout, SetWindowPos, GetWindowPos, CurrentMonitorRect>("Window Management");
            host.AddNodeToMenu<CursorPosition>("Input", "Mouse");
            host.AddNodeToMenu<CustomRectangleNode>("Shapes");
            // host.AddNodeToMenu<NotifyUser>("Interactivity");
        }
    }
}