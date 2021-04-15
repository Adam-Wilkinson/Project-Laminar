using OpenFlow_PluginFramework;
using OpenFlow_PluginFramework.NodeSystem.NodeComponents;
using OpenFlow_PluginFramework.NodeSystem.NodeComponents.Visuals;
using OpenFlow_PluginFramework.NodeSystem.Nodes;
using OpenFlow_PluginFramework.Primitives;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using WindowsHook;
using WindowsKeyboardMouse.Primitives;

namespace WindowsKeyboardMouse.Nodes.Input.MouseInput
{
    public class MouseButtonTrigger : ITriggerNode
    {
        private readonly INodeField MouseButton = Constructor.NodeField("Mouse Button").WithInput<MouseButtonEnum>();
        private MouseButtons _buttonToListenFor;

        public event EventHandler Trigger;

        public string NodeName { get; } = "Mouse Button Trigger";

        public IEnumerable<INodeComponent> Fields
        {
            get
            {
                yield return MouseButton;
            }
        }

        public void HookupTriggers()
        {
            MouseButton.GetValue(INodeField.InputKey).PropertyChanged += MouseButtonTrigger_PropertyChanged;
            MouseButtonTrigger_PropertyChanged(null, new PropertyChangedEventArgs(nameof(ILaminarValue.Value)));

            Hook.GlobalEvents().MouseDown += MouseButtonTrigger_MouseDown;
        }

        private void MouseButtonTrigger_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName is nameof(ILaminarValue.Value))
            {
                _buttonToListenFor = EnumConverters.MouseButton(MouseButton.GetInput<MouseButtonEnum>());
            }
        }

        private void MouseButtonTrigger_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == _buttonToListenFor)
            {
                Trigger?.Invoke(this, new EventArgs());
            }
        }

    }
}
