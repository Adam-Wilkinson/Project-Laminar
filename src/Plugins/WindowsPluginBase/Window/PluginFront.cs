using Laminar_PluginFramework.Primitives;
using Laminar_PluginFramework.Registration;
using System;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;

namespace WindowsPluginBase.Window
{
    public class PluginFront : IPlugin
    {
        private IntPtr hook;
        static GCHandle GCSafetyHandle;

        public Platforms Platforms { get; } = Platforms.Windows;

        public string PluginName { get; } = "Windows Base";

        public string PluginDescription { get; } = "A base plugin for all Windows plugins to use";

        public void Register(IPluginHost host)
        {
            Debug.WriteLine("Test");

            WindowHooks.WinEventDelegate onMoveFunction = new(OnMoveFunction);
            GCSafetyHandle = GCHandle.Alloc(onMoveFunction);

            var np = Process.GetProcessesByName("notepad").FirstOrDefault(p => p != null);

            uint targetThreadId = WindowHooks.GetWindowThread(np.MainWindowHandle);

            hook = WindowHooks.WinEventHookOne(NativeMethods.SWEH_Events.EVENT_SYSTEM_MOVESIZESTART, onMoveFunction, 0, 0);
        }

        private void OnMoveFunction(
            IntPtr hWinEventHook,
            NativeMethods.SWEH_Events eventType,
            IntPtr hWnd,
            NativeMethods.SWEH_ObjectId idObject,
            long idChild, uint dwEventThread, uint dwmsEventTime)
        {
            Debug.WriteLine("Window Moved!");
        }

        public void Dispose()
        {
            if (GCSafetyHandle.IsAllocated)
            {
                GCSafetyHandle.Free();
            }
            WindowHooks.WinEventUnhook(hook);
        }
    }


}