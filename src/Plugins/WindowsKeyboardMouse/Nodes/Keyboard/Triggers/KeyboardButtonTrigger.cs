using Laminar_PluginFramework;
using Laminar_PluginFramework.NodeSystem.NodeComponents;
using Laminar_PluginFramework.NodeSystem.NodeComponents.Visuals;
using Laminar_PluginFramework.NodeSystem.Nodes;
using Laminar_PluginFramework.Primitives;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using WindowsHook;

namespace WindowsKeyboardMouse.Nodes.Keyboard.Triggers
{
    public class KeyboardButtonTrigger : ITriggerNode
    {
        private static readonly Dictionary<Keys, EventHandler> AllTriggers = new();

        static KeyboardButtonTrigger()
        {
            Hook.GlobalEvents().KeyUp += KeyboardButtonTrigger_KeyUp;
        }

        private static void KeyboardButtonTrigger_KeyUp(object sender, KeyEventArgs e)
        {
            if (AllTriggers.TryGetValue(e.KeyData, out EventHandler trigger))
            {
                trigger?.Invoke(null, new EventArgs());
            }
        }

        private readonly INodeField KeyField = Constructor.NodeField("Trigger Key").WithInput<Keys>();
        private readonly INodeField SuppressKey = Constructor.NodeField("Suppress Key").WithInput<bool>();

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
            AllTriggers[KeyField.GetInput<Keys>()] = Trigger;

            KeyField.GetValue(INodeField.InputKey).OnChange += TriggerKeyChanged;
        }

        private void TriggerKeyChanged(object key)
        {
            AllTriggers[(Keys)key] = Trigger;
        }
    }
}
