using Laminar_PluginFramework;
using Laminar_PluginFramework.NodeSystem.NodeComponents;
using Laminar_PluginFramework.NodeSystem.NodeComponents.Visuals;
using Laminar_PluginFramework.NodeSystem.Nodes;
using Laminar_PluginFramework.Primitives;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Timers;
using WindowsHook;
using WindowsKeyboardMouse.Primitives;

namespace WindowsKeyboardMouse.Nodes.Keyboard.Triggers
{
    public class KeyboardButtonTrigger : ITriggerNode
    {
        private static readonly Dictionary<Keys, KeyboardButtonTrigger> AllTriggers = new();
        private static readonly Timer CooldownTimer = new(100) { AutoReset = false };
        private static bool Subscribed;
        private static bool KeyIsDown;

        private static void KeyboardButtonTrigger_KeyDown(object sender, KeyEventArgs e)
        {
            if (AllTriggers.TryGetValue(e.KeyData, out KeyboardButtonTrigger trigger) && !KeyIsDown)
            {
                KeyIsDown = true;
                e.SuppressKeyPress = trigger.SuppressKey.GetInput<bool>();
                trigger.Trigger?.Invoke(null, new EventArgs());
                CooldownTimer.Start();
            }
        }

        private static void CooldownTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            KeyIsDown = false;
        }

        public readonly INodeField KeyField = Constructor.NodeField("Trigger Key").WithInput<KeyboardKey>();
        public readonly INodeField SuppressKey = Constructor.NodeField("Suppress Key").WithInput(Constructor.RigidTypeDefinitionManager(false, "CheckBox", null));

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

        public void RemoveTriggers()
        {
            RemoveSelfFromAllTriggers();

            if (Subscribed && AllTriggers.Count == 0)
            {
                Hook.GlobalEvents().KeyDown -= KeyboardButtonTrigger_KeyDown;
                CooldownTimer.Elapsed -= CooldownTimer_Elapsed;
                Subscribed = false;
            }

            KeyField.GetValue(INodeField.InputKey).OnChange -= TriggerKeyChanged;
        }

        public void HookupTriggers()
        {
            if (AllTriggers.Count == 0)
            {
                Hook.GlobalEvents().KeyDown += KeyboardButtonTrigger_KeyDown;
                CooldownTimer.Elapsed += CooldownTimer_Elapsed;
                Subscribed = true;
            }

            AllTriggers[KeyField.GetInput<KeyboardKey>().HookKey] = this;

            KeyField.GetValue(INodeField.InputKey).OnChange += TriggerKeyChanged;
        }

        private void TriggerKeyChanged(object sender, object key)
        {
            RemoveSelfFromAllTriggers();

            AllTriggers[((KeyboardKey)key).HookKey] = this;
        }

        private void RemoveSelfFromAllTriggers ()
        {
            var enumer = AllTriggers.GetEnumerator();
            while (enumer.MoveNext() && enumer.Current.Value != this) { }
            AllTriggers.Remove(enumer.Current.Key);
        }
    }
}
