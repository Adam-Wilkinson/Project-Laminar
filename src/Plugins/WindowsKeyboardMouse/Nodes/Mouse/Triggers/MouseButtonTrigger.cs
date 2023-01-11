using System;
using System.Collections.Generic;
using System.ComponentModel;
using Laminar.PluginFramework.NodeSystem;

namespace WindowsKeyboardMouse.Nodes.Mouse.Triggers;

public class MouseButtonTrigger : INode
{
    // private readonly INodeField MouseButton = Constructor.NodeField("Mouse Button").WithInput<MouseButtons>();
    // private MouseButtons _buttonToListenFor;

    public event EventHandler Trigger;

    public string NodeName { get; } = "Mouse Button Trigger";

    public void HookupTriggers()
    {
        // MouseButton.GetValue(INodeField.InputKey).PropertyChanged += MouseButtonTrigger_PropertyChanged;
        // MouseButtonTrigger_PropertyChanged(null, new PropertyChangedEventArgs(nameof(ILaminarValue.Value)));

        // Hook.GlobalEvents().MouseDown += MouseButtonTrigger_MouseDown;
    }

    private void MouseButtonTrigger_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        //if (e.PropertyName is nameof(ILaminarValue.Value))
        //{
        //    // _buttonToListenFor = MouseButton.GetInput<MouseButtons>();
        //}
    }

    public void RemoveTriggers()
    {
        // Hook.GlobalEvents().MouseDown -= MouseButtonTrigger_MouseDown;
    }

    public void Evaluate()
    {
    }
}
