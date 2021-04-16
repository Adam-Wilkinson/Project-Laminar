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
                KeyPresser.PressVirtualKey(Convert.ToByte(VkKeyScan(character)));
            }
        }
    }
}
