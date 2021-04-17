using Laminar_PluginFramework;
using Laminar_PluginFramework.NodeSystem.NodeComponents;
using Laminar_PluginFramework.NodeSystem.NodeComponents.Visuals;
using Laminar_PluginFramework.NodeSystem.Nodes;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using WindowsHook;

namespace WindowsKeyboardMouse.Nodes.Keyboard.Output
{
    public class TextTyper : IActionNode
    {
        private readonly INodeField textField = Constructor.NodeField("Text to type").WithInput<string>();

        public IEnumerable<INodeComponent> Fields
        {
            get
            {
                yield return textField;
            }
        }

        public string NodeName { get; } = "Text Typer";

        [DllImport("user32.dll")]
        static extern short VkKeyScan(char ch);

        public void Evaluate()
        {
            foreach (char character in textField.GetInput<string>())
            {
                short keyNumber = VkKeyScan(character);
                Keys key = (Keys)(((keyNumber & 0xFF00) << 8) | (keyNumber & 0xFF));
                if ((key & Keys.Shift) != Keys.None)
                {
                    KeyPresser.PressVirtualKey(unchecked((byte)Keys.LShiftKey), 0);
                }
                KeyPresser.PressVirtualKey((byte)key, 0);
                if ((key & Keys.Shift) != Keys.None)
                {
                    KeyPresser.PressVirtualKey(unchecked((byte)Keys.LShiftKey), KeyPresser.KEYEVENTF_KEYUP);
                }
            }
        }
    }
}
