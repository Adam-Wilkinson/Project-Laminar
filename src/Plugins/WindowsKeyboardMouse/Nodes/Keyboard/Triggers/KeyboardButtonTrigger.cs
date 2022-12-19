using Laminar_PluginFramework;
using Laminar_PluginFramework.NodeSystem.NodeComponents;
using Laminar_PluginFramework.NodeSystem.NodeComponents.Visuals;
using Laminar_PluginFramework.NodeSystem.Nodes;
using System;
using System.Collections.Generic;
using System.Timers;
using WindowsKeyboardMouse.Primitives;

namespace WindowsKeyboardMouse.Nodes.Keyboard.Triggers;

public class KeyboardButtonTrigger : ITriggerNode
{
    // private static readonly Dictionary<Keys, KeyboardButtonTrigger> AllTriggers = new();
    private static readonly Timer CooldownTimer = new(100) { AutoReset = false };
    private static bool Subscribed;
    private static bool KeyIsDown;

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
        /*
        RemoveSelfFromAllTriggers();

        if (Subscribed && AllTriggers.Count == 0)
        {
            Hook.GlobalEvents().KeyDown -= KeyboardButtonTrigger_KeyDown;
            CooldownTimer.Elapsed -= CooldownTimer_Elapsed;
            Subscribed = false;
        }

        KeyField.GetValue(INodeField.InputKey).OnChange -= TriggerKeyChanged;
        */
    }

    public void HookupTriggers()
    {
        /*
        if (AllTriggers.Count == 0)
        {
            Hook.GlobalEvents().KeyDown += KeyboardButtonTrigger_KeyDown;
            CooldownTimer.Elapsed += CooldownTimer_Elapsed;
            Subscribed = true;
        }

        AllTriggers[KeyField.GetInput<KeyboardKey>().HookKey] = this;

        KeyField.GetValue(INodeField.InputKey).OnChange += TriggerKeyChanged;
        */
    }

    private void TriggerKeyChanged(object sender, object key)
    {
        // RemoveSelfFromAllTriggers();

        // AllTriggers[((KeyboardKey)key).HookKey] = this;
    }

    public void Evaluate()
    {
    }
}
