using OpenFlow_PluginFramework;
using OpenFlow_PluginFramework.NodeSystem.NodeComponents;
using OpenFlow_PluginFramework.NodeSystem.NodeComponents.Visuals;
using OpenFlow_PluginFramework.NodeSystem.Nodes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using WindowsHook;

namespace WindowsKeyboardMouse.Nodes.Input.MouseInput
{
    public class MouseButtonTrigger : ITriggerNode
    {
        private readonly INodeField MouseButton = Constructor.NodeField("Mouse Button").WithInput<MouseButtonEnum>();

        public event EventHandler Trigger;

        public MouseButtonTrigger()
        {
            Hook.GlobalEvents().MouseDown += MouseButtonTrigger_MouseDown;
        }

        private void MouseButtonTrigger_MouseDown(object sender, MouseEventArgs e)
        {
            if (ConvertMouseButton.Convert(e.Button) == MouseButton.GetInput<MouseButtonEnum>())
            {
                Trigger?.Invoke(this, new EventArgs());
            }
        }

        public string NodeName { get; } = "Mouse Button Trigger";

        public IEnumerable<INodeComponent> Fields
        {
            get
            {
                yield return MouseButton;
            }
        }
    }
}
