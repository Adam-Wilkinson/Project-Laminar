using Laminar_PluginFramework;
using Laminar_PluginFramework.NodeSystem.NodeComponents;
using Laminar_PluginFramework.NodeSystem.NodeComponents.Visuals;
using Laminar_PluginFramework.NodeSystem.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using WindowsPluginBase.Window;

namespace WindowsPluginBase.Nodes
{
    public class WindowLayoutChanged : ITriggerNode
    {
        public readonly INodeField beforeLayout = Constructor.NodeField("Layout Before Change").WithOutput<AllWindowsLayout>();
        public readonly INodeField afterLayout = Constructor.NodeField("Layout After Change").WithOutput<AllWindowsLayout>();

        private IntPtr _hook;
        private IntPtr _hook2;
        private GCHandle _gcSafetyHandle;

        public IEnumerable<INodeComponent> Fields
        {
            get
            {
                yield return beforeLayout;
                yield return afterLayout;
            }
        }

        public string NodeName { get; } = "Window Layout Changed";

        public event EventHandler Trigger;

        public void HookupTriggers()
        {
            afterLayout.SetOutput(AllWindowsLayout.GetCurrentLayout());

            WindowHooks.WinEventDelegate windowMovedDelegate = new(WindowMoved);
            _gcSafetyHandle = GCHandle.Alloc(windowMovedDelegate);
            _hook = WindowHooks.WinEventHookOne(NativeMethods.SWEH_Events.EVENT_SYSTEM_MOVESIZEEND, windowMovedDelegate, 0, 0);
            _hook2 = WindowHooks.WinEventHookOne(NativeMethods.SWEH_Events.EVENT_OBJECT_FOCUS, windowMovedDelegate, 0, 0);
            WindowHooks.WindowArrangementManuallyChanged += ArrangementChanged;
        }

        private void ArrangementChanged()
        {
            beforeLayout.SetOutput(afterLayout.GetOutput());
            afterLayout.SetOutput(AllWindowsLayout.GetCurrentLayout());
        }

        public void RemoveTriggers()
        {
            if (_gcSafetyHandle.IsAllocated)
            {
                _gcSafetyHandle.Free();
            }
            WindowHooks.WinEventUnhook(_hook);
            WindowHooks.WinEventUnhook(_hook2);
            WindowHooks.WindowArrangementManuallyChanged -= ArrangementChanged;

        }

        private void WindowMoved(IntPtr hWndEventHook, NativeMethods.SWEH_Events eventType, IntPtr hWnd, NativeMethods.SWEH_ObjectId objID, long idChild, uint dwEventThread, uint dwmsEventTime)
        {
            ArrangementChanged();
            Trigger?.Invoke(this, new EventArgs());
        }

        public void Evaluate()
        {
        }
    }
}
