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

namespace WindowsKeyboardMouse.Nodes.Input.KeyboardInput
{
    public class KeyboardButtonTrigger : ITriggerNode
    {
        private readonly INodeField KeyField = Constructor.NodeField("Trigger Key").WithInput<KeyboardButtonEnum>();
        private readonly INodeField SuppressKey = Constructor.NodeField("Suppress Key").WithInput<bool>();
        private Keys _triggerKey;

        public IEnumerable<INodeComponent> Fields
        {
            get
            {
                yield return KeyField;
                yield return SuppressKey;
            }
        }

        public string NodeName { get; } = "Keyboard Trigger";

        public event EventHandler Trigger;

        public void HookupTriggers()
        {
            KeyField.GetValue(INodeField.InputKey).PropertyChanged += KeyboardButtonTrigger_PropertyChanged;
            KeyboardButtonTrigger_PropertyChanged(null, new PropertyChangedEventArgs(nameof(ILaminarValue.Value)));

            Hook.GlobalEvents().KeyDown += KeyboardButtonTrigger_KeyDown;
        }

        private void KeyboardButtonTrigger_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == _triggerKey)
            {
                e.SuppressKeyPress = SuppressKey.GetInput<bool>();
                Trigger?.Invoke(this, new EventArgs());
            }
        }

        private void KeyboardButtonTrigger_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName is nameof(ILaminarValue.Value))
            {
                _triggerKey = EnumConverters.KeyboardButton(KeyField.GetInput<KeyboardButtonEnum>());
            }
        }
    }
}
