using Laminar_PluginFramework;
using Laminar_PluginFramework.NodeSystem.NodeComponents;
using Laminar_PluginFramework.NodeSystem.NodeComponents.Visuals;
using Laminar_PluginFramework.NodeSystem.Nodes;
using Laminar_PluginFramework.Primitives;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using WindowsHook;

namespace WindowsKeyboardMouse.Nodes.Mouse.Triggers
{
    public class MouseButtonTrigger : ITriggerNode
    {
        private readonly INodeField MouseButton = Constructor.NodeField("Mouse Button").WithInput<MouseButtons>();
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
                _buttonToListenFor = MouseButton.GetInput<MouseButtons>();
            }
        }

        private void MouseButtonTrigger_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == _buttonToListenFor)
            {
                Trigger?.Invoke(this, new EventArgs());
            }
        }

        public void RemoveTriggers()
        {
            Hook.GlobalEvents().MouseDown -= MouseButtonTrigger_MouseDown;
        }
    }
}
