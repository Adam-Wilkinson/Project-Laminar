using OpenFlow_PluginFramework;
using OpenFlow_PluginFramework.NodeSystem.NodeComponents;
using OpenFlow_PluginFramework.NodeSystem.NodeComponents.Visuals;
using OpenFlow_PluginFramework.NodeSystem.Nodes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowsKeyboardMouse.Primitives;

namespace WindowsKeyboardMouse.Nodes.Mouse.Triggers
{
    public class MouseButton : IFunctionNode
    {
        private readonly INodeField mouseButtonOutput = Constructor.NodeField("Mouse Button ").WithValue<MouseButtonEnum>("Display", true).WithOutput<MouseButtonEnum>();

        public string NodeName => "Mouse Button";

        public IEnumerable<INodeComponent> Fields
        {
            get
            {
                yield return mouseButtonOutput;
            }
        }

        public void Evaluate()
        {
            mouseButtonOutput[INodeField.OutputKey] = mouseButtonOutput["Display"];
        }
    }
}
