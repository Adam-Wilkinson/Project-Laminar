using Laminar_PluginFramework;
using Laminar_PluginFramework.NodeSystem.NodeComponents;
using Laminar_PluginFramework.NodeSystem.NodeComponents.Visuals;
using Laminar_PluginFramework.NodeSystem.Nodes;
using System;
using System.Collections.Generic;

namespace WindowsKeyboardMouse.Nodes.Keyboard.Triggers;

public class TextTypedTrigger : ITriggerNode
{
    private readonly INodeField textInput = Constructor.NodeField("Text to listen for").WithInput<string>();

    // private IKeyboardMouseEvents _globalHook;

    public IEnumerable<INodeComponent> Fields
    {
        get
        {
            yield return textInput;
        }
    }

    public string NodeName { get; } = "Text Typed Trigger";

    public event EventHandler Trigger;

    public void HookupTriggers()
    {
        textInput.GetValue(INodeField.InputKey).OnChange += TextInputChanged;

        TextInputChanged(null, textInput.GetValue(INodeField.InputKey).Value);
    }

    public void RemoveTriggers()
    {
        // _globalHook?.Dispose();
    }

    private void TextInputChanged(object sender, object obj)
    {
        /*
        if (obj as string is null or "")
        {
            _globalHook?.Dispose();
            return;
        }

        Combination[] combinations = new Combination[((string)obj).Length];
        int i = 0;
        foreach (char ch in (string)obj)
        {
            combinations[i] = Combination.TriggeredBy(TextTyper.ConvertCharToKey(ch));
            i++;
        }
        Sequence sequence = Sequence.Of(combinations);

        _globalHook?.Dispose();
        _globalHook = Hook.GlobalEvents();
        _globalHook.OnSequence(new KeyValuePair<Sequence, Action>[] { new KeyValuePair<Sequence, Action>(sequence, ActivateTriggerOnKeyUp) });
        */
    }

    private void ActivateTriggerOnKeyUp()
    {
        // _globalHook.KeyUp += GlobalHook_KeyUp;
    }

    public void Evaluate()
    {
    }
}
