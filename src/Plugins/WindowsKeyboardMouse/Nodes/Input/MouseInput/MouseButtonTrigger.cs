using OpenFlow_PluginFramework;
using OpenFlow_PluginFramework.NodeSystem.NodeComponents;
using OpenFlow_PluginFramework.NodeSystem.NodeComponents.Visuals;
using OpenFlow_PluginFramework.NodeSystem.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsKeyboardMouse.Nodes.Input.MouseInput
{
    public class MouseButtonTrigger : ITriggerNode
    {
        private readonly INodeField MouseButton = Constructor.NodeField("Mouse Button").WithInput<MouseButtonEnum>();

        public string NodeName { get; } = "Mouse Button Trigger";

        public IEnumerable<INodeComponent> Fields
        {
            get
            {
                yield return MouseButton;
            }
        }

        public event EventHandler Trigger;
    }
}
