using Laminar_PluginFramework;
using Laminar_PluginFramework.NodeSystem.NodeComponents;
using Laminar_PluginFramework.NodeSystem.NodeComponents.Visuals;
using Laminar_PluginFramework.NodeSystem.Nodes;
using Laminar_PluginFramework.Primitives;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowsHook;
using WindowsKeyboardMouse.Nodes.Keyboard.Output;

namespace WindowsKeyboardMouse.Nodes.Keyboard.Triggers
{
    public class TextTypedTrigger : ITriggerNode
    {
        private readonly INodeField textInput = Constructor.NodeField("Text to listen for").WithInput<string>();

        private IKeyboardMouseEvents _globalHook;

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

            TextInputChanged(textInput.GetValue(INodeField.InputKey).Value);
        }

        public void RemoveTriggers()
        {
            _globalHook?.Dispose();
        }

        private void TextInputChanged(object obj)
        {
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
        }

        private void ActivateTriggerOnKeyUp()
        {
            _globalHook.KeyUp += GlobalHook_KeyUp;
        }

        private void GlobalHook_KeyUp(object sender, KeyEventArgs e)
        {
            Trigger?.Invoke(this, e);
            _globalHook.KeyUp -= GlobalHook_KeyUp;
        }
    }
}
