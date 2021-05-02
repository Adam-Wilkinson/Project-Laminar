using Laminar_PluginFramework.Primitives;
using Laminar_PluginFramework.Registration;
using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using WindowsPluginBase.Nodes;

namespace WindowsPluginBase.Window
{
    public class PluginFront : IPlugin
    {
        public Platforms Platforms { get; } = Platforms.Windows;

        public string PluginName { get; } = "Windows Base";

        public string PluginDescription { get; } = "A base plugin for all Windows plugins to use";

        public void Register(IPluginHost host)
        {
            host.RegisterType<AllWindowsLayout>("#00FF00", "Window Layout", new AllWindowsLayout(), null, null, false);
            host.RegisterType<WindowStub>("#7D3E11", "Window", null, null, null, false);
            host.RegisterType<WindowLayout>("#00b9bc", "Window Position", null, null, null, false);


            host.AddNodeToMenu<WindowLayoutChanged>("Triggers");
            host.AddNodeToMenu<SetWindowLayout, SetWindowPos, GetWindowPos>("Window Management");
            // host.AddNodeToMenu<NotifyUser>("Interactivity");
        }

        public void Dispose()
        {
        }
    }

    public class WindowStub { }
}