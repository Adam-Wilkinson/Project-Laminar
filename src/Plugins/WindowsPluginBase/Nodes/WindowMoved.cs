using Laminar_PluginFramework;
using Laminar_PluginFramework.NodeSystem.NodeComponents;
using Laminar_PluginFramework.NodeSystem.NodeComponents.Visuals;
using Laminar_PluginFramework.NodeSystem.Nodes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using WindowsPluginBase.Window;

namespace WindowsPluginBase.Nodes
{
    public class WindowMoved : ITriggerNode
    {
        private readonly INodeField movedWindow = Constructor.NodeField("Moved Window").WithOutput<Window.Window>();
        private GCHandle _gcSafetyHandle;
        private IntPtr _hook;

        public IEnumerable<INodeComponent> Fields
        {
            get
            {
                yield return movedWindow;
            }
        }

        public string NodeName { get; } = "Window Moved";

        public event EventHandler Trigger;

        public void HookupTriggers()
        {
            WindowHooks.WinEventDelegate windowMovedDelegate = new(WindowMovedMethod);
            _gcSafetyHandle = GCHandle.Alloc(windowMovedDelegate);
            _hook = WindowHooks.WinEventHookOne(NativeMethods.SWEH_Events.EVENT_SYSTEM_MOVESIZEEND, windowMovedDelegate, 0, 0);
        }

        public void RemoveTriggers()
        {
            if (_gcSafetyHandle.IsAllocated)
            {
                _gcSafetyHandle.Free();
            }
            WindowHooks.WinEventUnhook(_hook);
        }

        private void WindowMovedMethod(IntPtr hWndEventHook, NativeMethods.SWEH_Events eventType, IntPtr hWnd, NativeMethods.SWEH_ObjectId objID, long idChild, uint dwEventThread, uint dwmsEventTime)
        {
            Debug.WriteLine("A window has been moved");
            movedWindow.SetOutput(new Window.Window { hWnd = hWnd });
            Trigger?.Invoke(this, new EventArgs());
        }
    }
}
