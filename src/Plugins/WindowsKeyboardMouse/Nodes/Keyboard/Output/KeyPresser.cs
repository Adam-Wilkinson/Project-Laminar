using System.Collections.Generic;
using System.Runtime.InteropServices;
using Laminar.PluginFramework.NodeSystem;
using WindowsKeyboardMouse.Primitives;

namespace WindowsKeyboardMouse.Nodes.Keyboard.Output
{
    public class KeyPresser : INode
    {
        public const uint KEYEVENTF_EXTENDEDKEY = 0x0001;
        public const uint KEYEVENTF_KEYUP = 0x0002;

        //private readonly INodeField keyField = Constructor.NodeField("Key To Press").WithInput<KeyboardKey>();
        //private readonly INodeField numberOfPresses = Constructor.NodeField("Number of Presses").WithInput(1.0);
        //private readonly INodeField delay = Constructor.NodeField("Press Delay").WithInput(10.0);

        public string NodeName { get; } = "Key Presser";

        [DllImport("user32.dll")]
#pragma warning disable IDE1006 // Supress warning about lower-case name
        private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, uint dwExtraInfo);

        public static void PressVirtualKey(byte bVk, uint keyEvent)
        {
            keybd_event(bVk, 0, keyEvent, 0);
        }
        
        public void Evaluate()
        {
            /*
            for (int i = 0; i < numberOfPresses.GetInput<double>(); i++)
            {
                PressVirtualKey((byte)keyField.GetInput<KeyboardKey>().HookKey, KEYEVENTF_EXTENDEDKEY);
                Thread.Sleep((int)delay.GetInput<double>());
            }
            */
        }
    }
}
