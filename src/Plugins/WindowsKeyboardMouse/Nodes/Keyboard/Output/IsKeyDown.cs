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
using WindowsKeyboardMouse.Primitives;

namespace WindowsKeyboardMouse.Nodes.Keyboard.Output
{
    public class IsKeyDown : IFunctionNode
    {
        private readonly INodeField _keyField = Constructor.NodeField("Key").WithInput<KeyboardKey>().WithOutput<bool>();

        public IEnumerable<INodeComponent> Fields
        {
            get
            {
                yield return _keyField;
            }
        }

        public string NodeName { get; } = "Is Key down";

        public void Evaluate()
        {
            Debug.WriteLine($"Someone asked if the key {_keyField.GetInput()} is down. It got the value {_keyField.GetInput<KeyboardKey>().IsPressed()}");
            _keyField[INodeField.OutputKey] = _keyField.GetInput<KeyboardKey>().IsPressed();
        }
    }
}
