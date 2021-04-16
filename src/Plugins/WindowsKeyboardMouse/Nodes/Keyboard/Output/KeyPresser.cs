using Laminar_PluginFramework;
using Laminar_PluginFramework.NodeSystem.NodeComponents;
using Laminar_PluginFramework.NodeSystem.NodeComponents.Visuals;
using Laminar_PluginFramework.NodeSystem.Nodes;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using WindowsHook;

namespace WindowsKeyboardMouse.Nodes.Keyboard.Output
{
    public class KeyPresser : IActionNode
    {
        const uint KEYEVENTF_EXTENDEDKEY = 0x0001;

        private readonly INodeField keyField = Constructor.NodeField("Key To Press").WithInput<Keys>();
        private readonly INodeField numberOfPresses = Constructor.NodeField("Number of Presses").WithInput<double>();
        private readonly INodeField delay = Constructor.NodeField("Press Delay").WithInput(10.0);

        public IEnumerable<INodeComponent> Fields
        {
            get
            {
                yield return keyField;
                yield return numberOfPresses;
                yield return delay;
            }
        }

        public string NodeName { get; } = "Key Presser";

        [DllImport("user32.dll")]
        private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, uint dwExtraInfo);

        public static void PressVirtualKey(byte bVk)
        {
            keybd_event(bVk, 0, KEYEVENTF_EXTENDEDKEY | 0, 0);
        }

        public void Evaluate()
        {
            for (int i = 0; i < numberOfPresses.GetInput<double>(); i++)
            {
                PressVirtualKey((byte)keyField.GetInput<Keys>());
                Thread.Sleep((int)delay.GetInput<double>());
            }
        }
    }
}
