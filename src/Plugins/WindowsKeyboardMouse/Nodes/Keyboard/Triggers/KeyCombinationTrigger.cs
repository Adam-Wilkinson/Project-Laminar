using System;
using System.ComponentModel;
using Laminar.PluginFramework.NodeSystem;

namespace WindowsKeyboardMouse.Nodes.Keyboard.Triggers;

public class KeyCombinationTrigger : INode
{
    //private readonly INodeField NumberOfLetters = Constructor.NodeField("Number of Letters").WithInput(1.0);
    //private readonly INodeComponentList Keys = Constructor.NodeComponentList();

    // private Sequence sequence;
    // private IKeyboardMouseEvents _globalHook;
    private readonly bool live;

    public KeyCombinationTrigger()
    {
        //NumberOfLetters_PropertyChanged(null, new PropertyChangedEventArgs(nameof(ILaminarValue.Value)));
        //live = false;
    }

    public string NodeName { get; } = "Key Sequence Trigger";

    public event EventHandler Trigger;

    public void HookupTriggers()
    {
        //NumberOfLetters.GetValue(INodeField.InputKey).PropertyChanged += NumberOfLetters_PropertyChanged;
        //live = true;
    }

    private void ActivateTrigger()
    {
        if (live)
        {
            Trigger?.Invoke(this, new EventArgs());
        }
    }

    private void AnyKey_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        /*
        if (e.PropertyName is nameof(ILaminarValue.Value))
        {
            Combination[] combinations = new Combination[Keys.VisualComponentList.Count];
            int i = 0;
            foreach (INodeField field in Keys.VisualComponentList)
            {
                combinations[i] = Combination.TriggeredBy(field.GetInput<KeyboardKey>().HookKey);
                i++;
            }
            sequence = Sequence.Of(combinations);

            _globalHook?.Dispose();
            _globalHook = Hook.GlobalEvents();
            _globalHook.OnSequence(new KeyValuePair<Sequence, Action>[] { new KeyValuePair<Sequence, Action>(sequence, ActivateTriggerOnKeyUp) });
        }
        */
    }

    private void ActivateTriggerOnKeyUp()
    {
        // _globalHook.KeyUp += GlobalHook_KeyUp;
    }

    private void NumberOfLetters_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        //if (e.PropertyName is not nameof(ILaminarValue.Value))
        //{
        //    return;
        //}

        //int NewNumberOfKeys = (int)NumberOfLetters.GetInput<double>();
        //if (NewNumberOfKeys > Keys.VisualComponentList.Count)
        //{
        //    for (int i = Keys.Count; i < NewNumberOfKeys; i++)
        //    {
        //        INodeField newField = Constructor.NodeField($"Letter {i + 1}").WithInput<KeyboardKey>();
        //        newField.GetValue(INodeField.InputKey).PropertyChanged += AnyKey_PropertyChanged;
        //        Keys.Add(newField);
        //    }
        //}
        //else if (NewNumberOfKeys < Keys.Count)
        //{
        //    for (int i = Keys.Count; i >= NewNumberOfKeys; i--)
        //    {
        //        Keys.RemoveAt(i);
        //    }
        //}

        //AnyKey_PropertyChanged(null, new PropertyChangedEventArgs(nameof(ILaminarValue.Value)));
    }

    public void RemoveTriggers()
    {
        // _globalHook?.Dispose();
    }

    public void Evaluate()
    {
    }
}
