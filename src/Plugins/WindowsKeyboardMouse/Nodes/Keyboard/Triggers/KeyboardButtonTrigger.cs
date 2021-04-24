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
        private static readonly Dictionary<Keys, KeyboardButtonTrigger> AllTriggers = new();
        private static bool Subscribed;

        private static void KeyboardButtonTrigger_KeyDown(object sender, KeyEventArgs e)
        {
            if (AllTriggers.TryGetValue(e.KeyData, out KeyboardButtonTrigger trigger))
            {
                e.SuppressKeyPress = trigger.SuppressKey.GetInput<bool>();
                trigger.Trigger?.Invoke(null, new EventArgs());
            }
        }

        public readonly INodeField KeyField = Constructor.NodeField("Trigger Key").WithInput<Keys>();
        public readonly INodeField SuppressKey = Constructor.NodeField("Suppress Key").WithInput<bool>();

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

        public void Dispose()
        {
            RemoveSelfFromAllTriggers();

            if (Subscribed && AllTriggers.Count == 0)
            {
                Hook.GlobalEvents().KeyDown -= KeyboardButtonTrigger_KeyDown;
            }
        }

        public void HookupTriggers()
        {
            if (AllTriggers.Count == 0)
            {
                Hook.GlobalEvents().KeyDown += KeyboardButtonTrigger_KeyDown;
                Subscribed = true;
            }

            AllTriggers[KeyField.GetInput<Keys>()] = this;

            KeyField.GetValue(INodeField.InputKey).OnChange += TriggerKeyChanged;
        }

        private void TriggerKeyChanged(object key)
        {
            RemoveSelfFromAllTriggers();

            AllTriggers[(Keys)key] = this;
        }

        private void RemoveSelfFromAllTriggers ()
        {
            var enumer = AllTriggers.GetEnumerator();
            while (enumer.MoveNext() && enumer.Current.Value != this) { }
            AllTriggers.Remove(enumer.Current.Key);
        }
    }
}
